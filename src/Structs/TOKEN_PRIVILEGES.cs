using System.Runtime.InteropServices;

namespace RunInSession.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TOKEN_PRIVILEGES
    {
        internal int PrivilegeCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal int[] Privileges;
    }
}
