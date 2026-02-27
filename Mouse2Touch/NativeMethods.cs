using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mouse2Touch {

    /// <summary>
    /// Mouse simulation
    /// </summary>
    class NativeMethods {


        #region #####Mouse#####
        [STAThread]
        [DllImport("User32")]
        public extern static void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);


        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);
        [DllImport("User32")]
        public extern static bool GetCursorPos(out Point p);
        [DllImport("User32")]
        public extern static int ShowCursor(bool bShow);
        [DllImport("User32")]
        public extern static IntPtr WindowFromPoint(int x, int y);
        [DllImport("User32")]
        public extern static IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);
        #endregion



        #region Mouse Simulation

        /// <summary>
        /// Simulate left mouse button press.
        /// </summary>
        public static void LeftDown() {
            NativeMethods.mouse_event(NativeContansts.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Simulate left mouse button release.
        /// </summary>
        public static void LeftUp() {
            NativeMethods.mouse_event(NativeContansts.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Simulate left mouse button click.
        /// </summary>
        public static void LeftClick() {
            LeftDown();
            LeftUp();
        }

        /// <summary>
        /// Simulate right mouse button press.
        /// </summary>
        public static void RightDown() {
            NativeMethods.mouse_event(NativeContansts.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Simulate right mouse button release.
        /// </summary>
        public static void RightUp() {
            NativeMethods.mouse_event(NativeContansts.MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Simulate right mouse button click.
        /// </summary>
        public static void RightClick() {
            RightDown();
            RightUp();
        }

        /// <summary>
        /// Simulate middle mouse button press.
        /// </summary>
        public static void MiddleDown() {
            NativeMethods.mouse_event(NativeContansts.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Simulate middle mouse button release.
        /// </summary>
        public static void MiddleUp() {
            NativeMethods.mouse_event(NativeContansts.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Simulate middle mouse button click.
        /// </summary>
        public static void MiddleClick() {
            MiddleDown();
            MiddleUp();
        }

        #endregion



    }


    public class NativeContansts {

        // Left mouse button
        public static int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public static int MOUSEEVENTF_LEFTUP = 0x0004;

        // Right mouse button
        public static int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public static int MOUSEEVENTF_RIGHTUP = 0x0010;

        // Middle mouse button
        public static int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public static int MOUSEEVENTF_MIDDLEUP = 0x0040;

        // Side mouse button
        public static int MOUSEEVENTF_XDOWN = 0x0080;
        public static int MOUSEEVENTF_XUP = 0x0100;

        // Scroll wheel
        public static int MOUSEEVENTF_WHEEL = 0x0800;
        public static int MOUSEEVENTF_HWHEEL = 0x01000;


    }

}
