﻿using System;
using System.Runtime.InteropServices;

namespace RunInSession.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public Int32 dwProcessID;
        public Int32 dwThreadID;
    }
}
