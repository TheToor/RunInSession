using System.Runtime.InteropServices;

namespace RunInSession.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID_AND_ATTRIBUTES
    {
        private LUID Luid;
        private int Attributes;
    }
}
