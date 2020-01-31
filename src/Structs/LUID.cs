using System.Runtime.InteropServices;

namespace RunInSession.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID
    {
        internal int LowPart;
        internal int HighPart;
    }
}
