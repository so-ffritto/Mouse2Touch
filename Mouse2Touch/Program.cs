using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mouse2Touch {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            // Prevent duplicate execution
            string ProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            var p = System.Diagnostics.Process.GetProcessesByName(ProcessName);
            if (p.Length > 1) {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);        
            Application.Run(new Form1());

           
        }
    }
}
