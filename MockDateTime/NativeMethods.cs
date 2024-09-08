using System.Runtime.InteropServices;

namespace MockDateTime;

public class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FileTime
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern void GetSystemTimeAsFileTime(out FileTime lpSystemTimeAsFileTime);

}