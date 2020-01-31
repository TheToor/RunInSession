using System;
using System.Runtime.InteropServices;

namespace RunInSession.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SECURITY_ATTRIBUTES
    {
        public Int32 Length;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }
}
