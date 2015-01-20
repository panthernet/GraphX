using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace WindowGlows
{
    enum HT
    {
        BORDER = 18,
        BOTTOM = 15,
        BOTTOMLEFT = 16,
        BOTTOMRIGHT = 17,
        LEFT = 10,
        RIGHT = 11,
        TOP = 12,
        TOPLEFT = 13,
        TOPRIGHT = 14
    }

    enum WM
    {
        NCLBUTTONDOWN = 0x00A1,
        MOUSEACTIVATE = 0x0021,
        LBUTTONDOWN = 0x0201,
        NCHITTEST = 0x0084
    }

    enum GWL
    {
        EXSTYLE = -20
    }

    enum WS_EX
    {
        TOOLWINDOW = 0x00000080
    }

    class NativeMethods
    {
        public static Func<short, short, IntPtr> MAKELPARAM = ( wLow, wHigh ) =>
        {
            return new IntPtr( ( wHigh << 16 ) | ( wLow & 0xFFFF ) );
        };

        public static Func<IntPtr, Point> LPARAMTOPOINT = lParam =>
        {
            return new Point( (int)lParam & 0xFFFF, ( (int)lParam >> 16 ) & 0xFFFF );
        };

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendNotifyMessage( IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos( out POINT lpPoint );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong( IntPtr hWnd, int nIndex );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong( IntPtr hWnd, int nIndex, int dwNewLong );
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x;
        public int y;
    }
}
