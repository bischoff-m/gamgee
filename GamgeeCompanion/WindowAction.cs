// See https://aka.ms/new-console-template for more information
// See https://learn.microsoft.com/en-us/windows/win32/api/winuser for documentation of
// API used
// 
// - Window Stations and Desktops
//     https://learn.microsoft.com/en-us/windows/win32/api/_winstation/
// - Windows and Messages
//     https://learn.microsoft.com/en-us/windows/win32/api/_winmsg/
// 
// Use Spy++ in Visual Studio to find the window handles of the windows you want to close

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GamgeeCompanion
{
    internal class WindowAction
    {
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsDelegate lpfn, IntPtr lParam);
        delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        const uint WM_CLOSE = 0x0010;
        const uint WM_SHOWWINDOW = 0x0018;
        const uint WM_ENABLE = 0x000A;
        const uint WM_CHILDACTIVATE = 0x0022;
        const uint WM_ACTIVATE = 0x0006;
        const uint WM_ACTIVATEAPP = 0x001C;
        const uint WM_CREATE = 0x0001;

        const int SW_HIDE = 0;
        const int SW_NORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_SHOWMINNOACTIVE = 7;
        const int SW_SHOWNA = 8;
        const int SW_RESTORE = 9;
        const int SW_SHOWDEFAULT = 10;
        const int SW_FORCEMINIMIZE = 11;

        // Programs to display information for
        static readonly List<string> processNames = ["Spotify"];

        public static void Main()
        {
            Console.WriteLine("Program started.");
            EnumDesktopWindows(IntPtr.Zero, EnumDesktopWindowsCallback, IntPtr.Zero); // This is synchronous
        }

        // This is called for each window found
        // If the process name matches the description, the window is shown and the
        // enumeration stops
        // Returns true to continue enumeration, false to stop
        static bool EnumDesktopWindowsCallback(IntPtr hWnd, int lParam)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            Process process = Process.GetProcessById((int)pid);

            if (!processNames.Contains(process.ProcessName))
                return true;

            Console.WriteLine("Found window of process: {0}", string.Join("\n", [
                "ProcessName: " + process.ProcessName,
                "Id: " + process.Id,
                "MainWindowHandle: " + hWnd.ToInt32().ToString("X"),
                "MainModuleFileName: " + process.MainModule?.FileName,
                "\n\n"
            ]));

            // This unfortunately doesn't work
            // Spotifys window shows but is frozen
            // Does not respond to SendMessage at all
            List<uint> messages = [WM_CREATE, WM_ACTIVATE];
            List<int> showTasks = [];

            foreach (var message in messages)
            {
                Console.WriteLine(
                    "Sending message {0} to window {1}",
                    message.ToString("X"),
                    hWnd.ToInt32().ToString("X")
                );
                SendMessage(hWnd, message, IntPtr.Zero, IntPtr.Zero);
            }
            foreach (var showTask in showTasks)
            {
                Console.WriteLine(
                    "Sending task {0} to window {1}",
                    showTask,
                    hWnd.ToInt32().ToString("X")
                );
                ShowWindow(hWnd, showTask);
            }
            return false;
        }
    }
}