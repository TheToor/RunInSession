using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RunInSession.Structs;

namespace RunInSession
{
    public static class ProcessUtil
    {
        private const int GENERIC_ALL_ACCESS = 0x10000000;

        public static void StartProcessForAllSession(string path, string arguments, string workingDirectory, string desktop = @"winsta0\default")
        {
            var tokens = GetTokensForProcess("explorer");
            if(tokens == null || tokens.Count() == 0)
            {
                Console.WriteLine("No active sessions found");
                return;
            }

            foreach(var token in tokens)
            {
                StartProcessInSession(token, path, arguments, workingDirectory, desktop);
            }

            return;
        }

        public static void StartProcessInLockScreen(string path, string arguments, string workingDirectory)
        {
            var tokens = GetTokensForProcess("winlogon");
            if(tokens == null || tokens.Count() == 0)
            {
                Console.WriteLine("Failed to find winlogon session");
                return;
            }

            var token = tokens[0];
            StartProcessInSession(token, path, arguments, workingDirectory, @"winsta0\winlogon");
        }

        private static List<IntPtr> GetTokensForProcess(string processName)
        {
            var result = new List<IntPtr>();

            var process = Process.GetProcessesByName(processName);
            if (process == null || process.Count() == 0)
            {
                Console.WriteLine($"No instance of {processName} found");
                return result;
            }

            var grouped = process.GroupBy((p) => p.SessionId);
            foreach (var group in grouped)
            {
                var sessionId = group.Key;
                var handle = group.First().Handle;
                var token = IntPtr.Zero;

                if (NativeMethod.OpenProcessToken(handle, TokenAdjuster.TOKEN_READ | TokenAdjuster.TOKEN_QUERY | TokenAdjuster.TOKEN_DUPLICATE | TokenAdjuster.TOKEN_ASSIGN_PRIMARY, ref token) == 0)
                {
                    Console.WriteLine($"Failed to open token from {sessionId} ({Marshal.GetLastWin32Error()}). Skipping ...");
                    continue;
                }

                result.Add(token);
            }

            return result;
        }

        private static bool StartProcessInSession(IntPtr sessionToken, string path, string arguments, string workingDirectory, string desktop)
        {
            if (sessionToken == null)
                throw new ArgumentNullException(nameof(sessionToken));

            // Adjust token
            Console.WriteLine("Adjusting token ...");
            if (!TokenAdjuster.EnablePrivilege("SeAssignPrimaryTokenPrivilege", true))
            {
                Console.WriteLine("Failed to enable required privilege (SeAssignPrimaryTokenPrivilege)");
                return false;
            }

            Console.WriteLine($"Trying to start '{path}' with arguments '{arguments}' in working directory '{workingDirectory}' ...");

            var duplicatedToken = IntPtr.Zero;
            var environment = IntPtr.Zero;
            var processInformation = new PROCESS_INFORMATION();

            try
            {
                var securityAttributes = new SECURITY_ATTRIBUTES();
                securityAttributes.Length = Marshal.SizeOf(securityAttributes);

                if (!NativeMethod.DuplicateTokenEx(
                        sessionToken,
                        GENERIC_ALL_ACCESS,
                        ref securityAttributes,
                        (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                        (int)TOKEN_TYPE.TokenPrimary,
                        ref duplicatedToken
                    )
                )
                {
                    Console.WriteLine($"Failed to duplicate token ({Marshal.GetLastWin32Error()})");
                    return false;
                }

                if (!NativeMethod.CreateEnvironmentBlock(out environment, duplicatedToken, false))
                {
                    Console.WriteLine($"Failed to get environment ({Marshal.GetLastWin32Error()})");
                    return false;
                }

                var startupInfo = new STARTUPINFO();
                startupInfo.cb = Marshal.SizeOf(startupInfo);
                startupInfo.lpDesktop = desktop;
                startupInfo.wShowWindow = 5; // SW_SHOW

                if (!NativeMethod.CreateProcessAsUser(
                        duplicatedToken,
                        path,
                        arguments,
                        ref securityAttributes,
                        ref securityAttributes,
                        false,
                        ProcessCreationFlags.NORMAL_PRIORITY_CLASS | ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT | ProcessCreationFlags.CREATE_NEW_CONSOLE | ProcessCreationFlags.CREATE_BREAKAWAY_FROM_JOB,
                        environment,
                        workingDirectory,
                        ref startupInfo,
                        ref processInformation
                    )
                )
                {
                    Console.WriteLine($"Failed to start process ({Marshal.GetLastWin32Error()})");
                    return false;
                }

                Console.WriteLine($"Process started as {processInformation.dwProcessID} ({Marshal.GetLastWin32Error()})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while trying to start process as user");
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                if (processInformation.hProcess != IntPtr.Zero)
                    NativeMethod.CloseHandle(processInformation.hProcess);
                if (processInformation.hThread != IntPtr.Zero)
                    NativeMethod.CloseHandle(processInformation.hThread);
                if (duplicatedToken != IntPtr.Zero)
                    NativeMethod.CloseHandle(duplicatedToken);
                if (environment != IntPtr.Zero)
                    NativeMethod.DestroyEnvironmentBlock(environment);
                if (sessionToken != IntPtr.Zero)
                    NativeMethod.CloseHandle(sessionToken);
            }
        }
    }
}