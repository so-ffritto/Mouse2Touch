using Gma.UserActivityMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Mouse2Touch {
    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
        }


        public static bool bool_stop = false; // Focused window is in exclusion list, temporarily disable touch
        public static bool bool_m_handled = true; // Intercept original signal when right-clicking
        public static Mtype mtype = Mtype.MouseSideButton; // Operation type

        private System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon(); // System tray icon

        bool bool_exe_run = true; // Whether the program is running
        bool bool_down = false;

        bool bool_down_b1 = false; // Side button pressed
        bool bool_down_ml = false; // Left mouse button pressed
        bool bool_down_mr = false; // Right mouse button pressed
        bool bool_down_mm = false; // Middle mouse button pressed

        bool bool_down_mr_moved = false; // Right button pressed and movement has been triggered
        float f_touch_speed = 1; // Touch speed

        List<String> ar_ad = new List<String>(); // Exclusion list

        // Touch simulation
        Point pp = new Point(); // Last moved position (to determine move distance)
        Point pp_start = new Point(); // Position when pressed
        PointerTouchInfo contact;
        PointerFlags oFlags;



        /// <summary>
        /// Operation type
        /// </summary>
        public enum Mtype {
            //stop = -1,
            none = 0,
            MouseSideButton = 1,
            MouseLeft = 2,
            MouseRight = 3, // Long press right button to open context menu
            MouseRight2 = 4, // Click in place to open context menu
            MouseMiddle = 5,
            Keyboard = 6
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender2, EventArgs e2) {


            var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            String html_path = Path.Combine(baseDir, "main_ui.html");

            if (!File.Exists(html_path)) {
                var candidates = new[] {
                    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "main_ui.html")),
                    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "main_ui.html")),
                    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "main_ui.html"))
                };

                var found = candidates.FirstOrDefault(File.Exists);
                if (!String.IsNullOrEmpty(found)) {
                    html_path = found;
                }
            }

            if (File.Exists(html_path)) {

                webBrowser1.Navigate(html_path);
                webBrowser1.ObjectForScripting = new C_js_to_net(this); // Allow web page to access C#
                    webBrowser1.AllowNavigation = false; // Prevent loading other web pages
                    //webBrowser1.IsWebBrowserContextMenuEnabled = false; // Disable right-click

            } else {
                webBrowser1.DocumentText = $"Can't find 「{html_path}」";
            }

            TouchInjector.InitializeTouchInjection(); // Initialize touch API


            //
            var tim = new System.Windows.Forms.Timer();
            tim.Interval = 200;
            tim.Tick += (sender, e) => {

                String gn = GetWindowsName.GetName(); // Get the name of the currently focused window

                // Compare with the exclusion list
                foreach (var item in ar_ad) {
                    if (gn.IndexOf(item, StringComparison.CurrentCultureIgnoreCase) > -1) {
                        bool_stop = true;
                        return;
                    }
                }
                bool_stop = false;
            };
            tim.Start();



            new Thread(() => {


                while (bool_exe_run) {

                    if (mtype == Mtype.MouseRight2) {

                        bool bool_d = bool_down_mr;

                        // Touch Down Simulate
                        if ((bool_down == false) && bool_d) {
                            bool_down = true;
                            pp = getPos();
                            pp_start = pp;
                            bool_down_mr_moved = false;
                        }

                        // Touch Up Simulate
                        if (bool_down && (bool_d == false)) {
                            bool_down = false;

                            if (bool_down_mr_moved) {
                                    // Touch simulation - end
                                        contact.PointerInfo.PointerFlags = PointerFlags.UP;
                                        TouchInjector.InjectTouchInput(1, new[] { contact });

                                    } else {

                                        RightClick();
                                    }
                                    bool_down_mr_moved = false;
                                    //upFull();
                                } else

                                // Touch Move Simulate
                                if (bool_down) {
                                    var p2 = getPos();

                                    if (bool_down_mr_moved) {

                                        // Touch simulation - move
                                        int nMoveIntervalX = (int)((p2.X - pp.X) * f_touch_speed);
                                        int nMoveIntervalY = (int)((p2.Y - pp.Y) * f_touch_speed);
                                contact.Move(nMoveIntervalX, nMoveIntervalY);
                                oFlags = PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;
                                contact.PointerInfo.PointerFlags = oFlags;
                                TouchInjector.InjectTouchInput(1, new[] { contact });

                                // Make the mouse follow the touch position
                                if (f_touch_speed != 1) {
                                    NativeMethods.SetCursorPos(pp.X + nMoveIntervalX, pp.Y + nMoveIntervalY);
                                    p2 = getPos();
                                }

                                pp = p2;

                                if (overd_sc(pp)) { // Auto-stop touch when exceeding screen bounds
                                    upFull();
                                }

                            } else {

                                if (HasMoved(pp_start, p2)) {
                                    bool_down_mr_moved = true;

                                    // Touch simulation - start
                                    contact = MakePointerTouchInfo(pp_start.X, pp_start.Y);
                                    oFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                                    contact.PointerInfo.PointerFlags = oFlags;
                                    bool bIsSuccess = TouchInjector.InjectTouchInput(1, new[] { contact });
                                }

                            }

                        }

                    } else {

                        bool bool_d = bool_down_b1 || bool_down_ml || bool_down_mr || bool_down_mm;

                        // Touch Down Simulate
                        if ((bool_down == false) && bool_d) {
                            bool_down = true;
                            pp = getPos();

                            if (mtype == Mtype.MouseRight) {
                                    bool_m_handled = false;
                                }

                                // Touch simulation - start
                            contact = MakePointerTouchInfo(pp.X, pp.Y);
                            oFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                            contact.PointerInfo.PointerFlags = oFlags;
                            bool bIsSuccess = TouchInjector.InjectTouchInput(1, new[] { contact });

                            if (mtype == Mtype.MouseRight) {
                                Thread.Sleep(1);
                                bool_m_handled = true;
                            }
                        }

                        // Touch Up Simulate
                        if (bool_down && (bool_d == false)) {
                            bool_down = false;

                            if (mtype == Mtype.MouseRight) { bool_m_handled = false; }

                            // Touch simulation - end
                            contact.PointerInfo.PointerFlags = PointerFlags.UP;
                            TouchInjector.InjectTouchInput(1, new[] { contact });

                            if (mtype == Mtype.MouseRight) {
                                Thread.Sleep(1);
                                bool_m_handled = true;
                            }
                        }

                        // Touch Move Simulate
                        if (bool_down) {

                            var p2 = getPos();
                            int nMoveIntervalX = (int)((p2.X - pp.X) * f_touch_speed);
                            int nMoveIntervalY = (int)((p2.Y - pp.Y) * f_touch_speed);

                            // Touch simulation - move
                            contact.Move(nMoveIntervalX, nMoveIntervalY);
                            oFlags = PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;
                            contact.PointerInfo.PointerFlags = oFlags;
                            TouchInjector.InjectTouchInput(1, new[] { contact });

                            // Make the mouse follow the touch position
                            if (f_touch_speed != 1) {
                                NativeMethods.SetCursorPos(pp.X + nMoveIntervalX, pp.Y + nMoveIntervalY);
                                p2 = getPos();
                            }

                            pp = p2;
                            if (overd_sc(pp)) { // Auto-stop touch when exceeding screen bounds
                                upFull();
                            }


                        }

                    }


                    Thread.Sleep(11);

                }

            }).Start();


            HookManager.MouseDown += HookManager_MouseDown;
            HookManager.MouseUp += HookManager_MouseUp;
            //HookManager.MouseMove += HookManager_MouseMove;

            this.FormClosing += Form1_FormClosing;
            init_nIcon();


        }



        /// <summary>
        /// When the program is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            bool_exe_run = false;
            HookManager.MouseDown -= HookManager_MouseDown;
            HookManager.MouseUp -= HookManager_MouseUp;
        }



        /// <summary>
        /// Check if touch point has reached screen edge
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool overd_sc(Point p) {

            //var sc = Screen.GetWorkingArea(p); // Excludes taskbar
            var sc = Screen.FromPoint(p).Bounds; // Includes taskbar

            if (p.X <= sc.X + 1 || p.Y <= sc.Y + 1 || (p.X >= sc.X + sc.Width - 1) || (p.Y >= sc.Y + sc.Height - 1)) {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Set touch speed
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public String set_touch_speed(String t) {
            try {
                f_touch_speed = float.Parse(t);
                if (f_touch_speed < 1) { f_touch_speed = 1f; }
                if (f_touch_speed > 5) { f_touch_speed = 5; }
                //System.Console.WriteLine(t);
                return "true";
            } catch (Exception) {
                f_touch_speed = 1;
                return "false";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public String set_Mtype(String t) {
            if (t == "none") { mtype = Mtype.none; return "true"; }
            if (t == "m_b1") { mtype = Mtype.MouseSideButton; return "true"; }
            if (t == "m_l") { mtype = Mtype.MouseLeft; return "true"; }
            if (t == "m_mm") { mtype = Mtype.MouseMiddle; return "true"; }
            if (t == "m_r") { mtype = Mtype.MouseRight; return "true"; }
            if (t == "m_r_2") { mtype = Mtype.MouseRight2; return "true"; }

            return "false";
        }


        /// <summary>
        /// Set exclusion list
        /// </summary>
        /// <param name="s"></param>
        public void set_ad(String s) {


            if (s == null) { s = ""; }
            s = s.Trim();
            var ar = s.Split('\n');

            ar_ad = new List<string>();

            foreach (var item in ar) {
                String s2 = item.Trim();
                if (s2.Length > 0 && ar_ad.Contains(s2) == false) {
                    ar_ad.Add(s2);
                }
            }

        }


        /// <summary>
        /// Get config
        /// </summary>
        /// <returns></returns>
        public String get_config() {
            String json_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");
            String s = "";

            if (File.Exists(json_path)) {
                using (StreamReader sr = new StreamReader(json_path, Encoding.UTF8)) {
                    s = sr.ReadToEnd();
                    sr.Close();
                }
            }
            return s;
        }

        /// <summary>
        /// Set config
        /// </summary>
        public void set_config(String s) {
            String json_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");

            using (FileStream fs = new FileStream(json_path, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                    sw.WriteLine(s);
                }
            }

        }





        /// <summary>
        /// Check if the mouse has moved
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        bool HasMoved(Point p1, Point p2) {
            int x = Math.Abs(p1.X) - Math.Abs(p2.X);
            int y = Math.Abs(p1.Y) - Math.Abs(p2.Y);
            bool b = (Math.Abs(x) + Math.Abs(y) >= 5); // If the two points are more than 5 pixels apart, consider it moved
            return b;
        }




        /// <summary>
        /// Mouse button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HookManager_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {

            if (bool_stop) { // Force stop all operations
                return;
            }

            if (mtype == Mtype.MouseSideButton && e.Button == MouseButtons.XButton1) {
                bool_down_b1 = true;
                return;
            }

            if (mtype == Mtype.MouseLeft && e.Button == MouseButtons.Left) {
                bool_down_ml = true;
                return;
            }

            if (mtype == Mtype.MouseRight && e.Button == MouseButtons.Right) {
                bool_down_mr = true;
                return;
            }

            if (mtype == Mtype.MouseRight2 && e.Button == MouseButtons.Right) {
                bool_down_mr = true;
                return;
            }

            if (mtype == Mtype.MouseMiddle && e.Button == MouseButtons.Middle) {
                bool_down_mm = true;
                return;
            }
        }

        /// <summary>
        /// Mouse button released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HookManager_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            upFull();
        }



        /// <summary>
        /// Called when keys are released
        /// </summary>
        private void upFull() {
            bool_down_b1 = false;
            bool_down_ml = false;
            bool_down_mr = false;
            bool_down_mm = false;
            bool_m_handled = true; // Intercept original signal when right-clicking
        }



        /// <summary>
        /// Simulate right mouse click. Open context menu
        /// </summary>
        private void RightClick() {
            var mt = mtype;
            mtype = Mtype.none;
            NativeMethods.RightDown();
            NativeMethods.RightUp();
            Thread.Sleep(1);
            mtype = mt;
        }



        /// <summary>
        /// Initialize system tray icon
        /// </summary>
        public void init_nIcon() {

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            nIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            nIcon.Text = "Mouse2Touch";
            nIcon.Visible = false; // Hide tray icon

            var cm = new System.Windows.Forms.ContextMenu();
            cm.MenuItems.Add("Show", new EventHandler((sender2, e2) => {
                this.Visible = true; // Show window
                nIcon.Visible = false; // Hide tray icon
            }));

            cm.MenuItems.Add("Close", new EventHandler((sender2, e2) => {
                this.Close(); // Close program
            }));

            nIcon.ContextMenu = cm;

            nIcon.DoubleClick += (sender, e) => {
                this.Visible = true; // Show window
                nIcon.Visible = false; // Hide tray icon
            };

        }


        /// <summary>
        /// Minimize to system tray
        /// </summary>
        public void hide_window() {
            this.Visible = false;
            nIcon.Visible = true; // Show tray icon
        }



        /// <summary>
        /// Get current mouse position
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Point getPos() {

            return System.Windows.Forms.Cursor.Position;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public void print(String s) {
            System.Console.WriteLine(s);
        }



        /// <summary>
        /// Create touch contact info
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public PointerTouchInfo MakePointerTouchInfo(int x, int y) {

            uint orientation = 87; // Angle
            uint pressure = 256; // Pressure
            int radius = 1; // Touch radius

            PointerTouchInfo contact = new PointerTouchInfo();
            contact.PointerInfo.pointerType = PointerInputType.TOUCH;
            contact.TouchFlags = TouchFlags.NONE;
            contact.Orientation = orientation;
            contact.Pressure = pressure;
            contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contact.PointerInfo.PtPixelLocation.X = x;
            contact.PointerInfo.PtPixelLocation.Y = y;
            uint unPointerId = IdGenerator.GetUinqueUInt();
            //Console.WriteLine("PointerId    " + unPointerId);
            contact.PointerInfo.PointerId = unPointerId;
            contact.ContactArea.left = x - radius;
            contact.ContactArea.right = x + radius;
            contact.ContactArea.top = y - radius;
            contact.ContactArea.bottom = y + radius;
            return contact;
        }



    }





    [ComVisible(true)]
    public class C_js_to_net {

        Form1 M;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public C_js_to_net(Form1 f) {
            this.M = f;
        }

        /// <summary>
        /// Set touch speed
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public String set_touch_speed(String s) {
            return M.set_touch_speed(s);
        }

        /// <summary>
        /// Set operation type
        /// </summary>
        /// <param name="s"></param>
        public void set_Mtype(String s) {
            M.set_Mtype(s);
        }

        /// <summary>
        /// Hide window
        /// </summary>
        public void hide_window() {
            M.hide_window();
        }

        /// <summary>
        /// Set exclusion list
        /// </summary>
        /// <param name="s"></param>
        public void set_ad(String s) {
            M.set_ad(s);
        }

        /// <summary>
        /// Read config file
        /// </summary>
        /// <returns></returns>
        public String get_config() {
            return M.get_config();
        }

        /// <summary>
        /// Save config file
        /// </summary>
        /// <param name="s"></param>
        public void set_config(String s) {
            M.set_config(s);
        }

        /// <summary>
        /// Open URL
        /// </summary>
        /// <param name="url"></param>
        public void open_url(String url) {
            // Open in system default browser
            System.Diagnostics.Process.Start(url);
        }

        /// <summary>
        /// Print text, used for debugging
        /// </summary>
        /// <param name="s"></param>
        public void print(String s) {
            M.print(s);
        }
    }

}
