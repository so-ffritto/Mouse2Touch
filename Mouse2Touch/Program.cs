using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mouse2Touch {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            // Restart as administrator if not already elevated
            if (!IsAdministrator()) {
                var startInfo = new ProcessStartInfo {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                try {
                    Process.Start(startInfo);
                } catch {
                    // User cancelled the UAC prompt — exit silently
                }
                return;
            }

            // Prevent duplicate execution
            string ProcessName = Process.GetCurrentProcess().ProcessName;
            var p = Process.GetProcessesByName(ProcessName);
            if (p.Length > 1) {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static bool IsAdministrator() {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
