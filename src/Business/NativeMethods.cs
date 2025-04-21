using System;
using System.Runtime.InteropServices;

namespace APIRunner.Business
{
  internal static class NativeMethods
  {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    internal static extern bool IsIconic(IntPtr hWnd);

    // Constantes para uso com ShowWindow
    internal const int SW_RESTORE = 9;
    internal const int SW_SHOW = 5;
    internal const int SW_SHOWNOACTIVATE = 4;
  }
}