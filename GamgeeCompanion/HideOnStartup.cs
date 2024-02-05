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
using System.Timers;
using CommunityToolkit.WinUI.Notifications;
using Timer = System.Timers.Timer;

namespace GamgeeCompanion
{
    internal class HideOnStartup
    {
        [DllImport("user32.dll")] static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsDelegate lpfn, IntPtr lParam);
        delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);
        [DllImport("User32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("kernel32.dll")] static extern bool FreeConsole();



        enum WindowAction
        {
            SENDCLOSE,
            MINIMIZE,
            SHOW
        };

        const uint WM_CLOSE = 0x0010;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;

        // Programs of which to close or minimize windows
        // Keys:    Process.ProcessName
        // Values:  Action to take
        static Dictionary<string, WindowAction> objectives = new Dictionary<string, WindowAction>(){
            {"Surfshark", WindowAction.SENDCLOSE},
            {"Spotify", WindowAction.SENDCLOSE},
            {"Discord", WindowAction.SENDCLOSE},
            {"OUTLOOK", WindowAction.MINIMIZE}
        };
        static bool success = false;
        static Timer? pollingTimer;
        static ManualResetEvent exitEvent = new ManualResetEvent(false);

        public static void Main()
        {
            FreeConsole();
            pollingTimer = new Timer(500);
            pollingTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            pollingTimer.Start();

            Console.WriteLine("Program started. Waiting for all programs to be running.");

            // Wait for timeout
            Task.Delay(20000).ContinueWith(t => exitEvent.Set());
            exitEvent.WaitOne(); // Block the thread until timeoutEvent is set

            exitEvent.Close();
            pollingTimer.Stop();

            if (!success)
            {
                // Not sure if this still works
                // Could be broken by migration to .NET Core
                new ToastContentBuilder()
                    .AddText("Timeout")
                    .AddText("Es gab ein Problem beim Schließen der Fenster.");
                // .Show();
                Console.WriteLine("Program was stopped because of a timeout.");
            }
        }

        static async void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            Console.WriteLine("Checking if all programs are running. Timestamp: {0}", e.SignalTime);
            // Wait for all processes to be running 
            if (AreAllObjectivesRunning())
            {
                pollingTimer?.Stop();
                await Task.Delay(3000);

                EnumDesktopWindows(IntPtr.Zero, EnumDesktopWindowsCallback, IntPtr.Zero); // This is synchronous

                Console.WriteLine("All tasks successful. Exiting.");
                success = true;
                exitEvent.Set();
            }
        }

        static bool AreAllObjectivesRunning()
        {
            Process[] processes = Process.GetProcesses();
            string[] processNames = processes.Select(p => p.ProcessName).ToArray();
            HashSet<string> keys = new HashSet<string>(objectives.Keys);
            return keys.IsSubsetOf(processNames);
        }

        static bool EnumDesktopWindowsCallback(IntPtr hWnd, int lParam)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            Process process = Process.GetProcessById((int)pid);

            if (!objectives.ContainsKey(process.ProcessName))
                return true;
            if (!IsWindowVisible(hWnd))
                return true;

            switch (objectives[process.ProcessName])
            {
                case WindowAction.SENDCLOSE:
                    SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    break;
                case WindowAction.MINIMIZE:
                    ShowWindowAsync(hWnd, SW_MINIMIZE);
                    break;
                case WindowAction.SHOW:
                    ShowWindowAsync(hWnd, SW_SHOW);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        // Prints the names of all processes that start with the given string (case insesitive)
        // Can be used to find the names of processes that should also be considered by this program
        static void PrintRunningProcesses(string startsWith)
        {
            HashSet<string> found = new HashSet<string>();
            EnumDesktopWindowsDelegate callback = delegate (IntPtr hWnd, int lParam)
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                Process process = Process.GetProcessById((int)pid);
                if (process.ProcessName.ToLower().StartsWith(startsWith.ToLower()))
                    found.Add(process.ProcessName);
                return true;
            };
            EnumDesktopWindows(IntPtr.Zero, callback, IntPtr.Zero);
            Console.WriteLine("Found process names: {{{0}}}", string.Join("", found));
        }
    }
}