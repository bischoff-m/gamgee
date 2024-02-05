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

namespace GamgeeCompanion
{
    internal class Program
    {
        static void Main()
        {
            HideOnStartup.Main();
        }
    }
}