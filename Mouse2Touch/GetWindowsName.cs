using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mouse2Touch {

    /// <summary>
    /// Get the name of the focused window
    /// </summary>
    public class GetWindowsName {


        [DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow(); // Get the IntPtr of the currently focused window

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count); // Get window title

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);


        //HANDLE WINAPI OpenProcess(
        //  __in  DWORD dwDesiredAccess,
        //  __in  BOOL bInheritHandle,
        //  __in  DWORD dwProcessId
        //);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        //  DWORD WINAPI GetModuleBaseName(
        //      __in      HANDLE hProcess,
        //      __in_opt  HMODULE hModule,
        //      __out     LPTSTR lpBaseName,
        //      __in      DWORD nSize
        //  );


        [DllImport("psapi.dll", CharSet = CharSet.Ansi)]
        private static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder lpBaseName, uint nSize);

        //  DWORD WINAPI GetModuleFileNameEx(
        //      __in      HANDLE hProcess,
        //      __in_opt  HMODULE hModule,
        //      __out     LPTSTR lpFilename,
        //      __in      DWORD nSize
        //  );
        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);


        /// <summary>
        /// Get the window title of the focused window
        /// </summary>
        /// <returns></returns>
        public static String GetTopWindowText() {
            IntPtr hWnd = GetForegroundWindow();
            int length = GetWindowTextLength(hWnd);
            StringBuilder text = new StringBuilder(length + 1);
            GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }


        /// <summary>
        /// Get the full path of the focused window
        /// </summary>
        public static String GetTopWindowName() {
            IntPtr hWnd = GetForegroundWindow();
            uint lpdwProcessId;
            GetWindowThreadProcessId(hWnd, out lpdwProcessId);

            IntPtr hProcess = OpenProcess(0x0410, false, lpdwProcessId);

            StringBuilder text = new StringBuilder(10000);
            //GetModuleBaseName(hProcess, IntPtr.Zero, text, text.Capacity); // May produce garbled text
            GetModuleFileNameEx(hProcess, IntPtr.Zero, text, text.Capacity);

            CloseHandle(hProcess);

            return text.ToString();
        }


        /// <summary>
        /// Get the name of the focused window
        /// </summary>
        /// <returns></returns>
        public static String GetName() {
            String path = GetTopWindowName();
            String name = Path.GetFileName(path);

            // Cannot get the name of UWP apps, so use window title instead for UWP
            if (name == "ApplicationFrameHost.exe") {
                return Path.GetFileName(GetTopWindowText());
            } else {
                return Path.GetFileName(path);
            }
        }


    }
}
