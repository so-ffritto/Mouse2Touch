using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Mouse2Touch {
    internal class OsdForm : Form {
        private readonly Timer _timer;

        private string _text = "";
        private Color _accent = Color.FromArgb(26, 115, 232); // blue for ON
        private int _phase = 0;
        private int _ticks = 0;
        private int _holdTicks = 0;

        private static readonly int PillW = 320;
        private static readonly int PillH = 80;
        private static readonly int Radius = 40;

        public OsdForm() {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.Magenta;       // used as transparency key
            TransparencyKey = Color.Magenta;
            Opacity = 0;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(PillW, PillH);
            DoubleBuffered = true;

            _timer = new Timer();
            _timer.Interval = 16;
            _timer.Tick += (s, e) => TickAnimation();
        }

        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                // WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT
                cp.ExStyle |= 0x00000080;
                cp.ExStyle |= 0x08000000;
                cp.ExStyle |= 0x00000020;
                return cp;
            }
        }

        public void ShowMessage(string text, Rectangle screenBounds, bool isOn, int durationMs = 950) {
            _text = text;
            _accent = isOn
                ? Color.FromArgb(26, 115, 232)   // blue
                : Color.FromArgb(95, 99, 104);   // grey

            // Center pill, slightly above middle (like Windows OSD)
            int x = screenBounds.X + (screenBounds.Width - PillW) / 2;
            int y = screenBounds.Y + (screenBounds.Height - PillH) / 2 - screenBounds.Height / 8;
            Location = new Point(x, y);
            Size = new Size(PillW, PillH);

            UpdatePillRegion();

            _phase = 0;
            _ticks = 0;
            _holdTicks = Math.Max(1, durationMs / _timer.Interval);

            if (!Visible) {
                Show();
            }

            TopMost = true;
            Opacity = 0;
            Invalidate();

            _timer.Stop();
            _timer.Start();
        }

        private void UpdatePillRegion() {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, Radius * 2, Radius * 2, 180, 90);
            path.AddArc(PillW - Radius * 2, 0, Radius * 2, Radius * 2, 270, 90);
            path.AddArc(PillW - Radius * 2, PillH - Radius * 2, Radius * 2, Radius * 2, 0, 90);
            path.AddArc(0, PillH - Radius * 2, Radius * 2, Radius * 2, 90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e) {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // Background: dark pill
            using (var bgBrush = new SolidBrush(Color.FromArgb(38, 38, 38))) {
                g.FillRectangle(bgBrush, 0, 0, Width, Height);
            }

            // Icon circle
            bool isOn = _accent.B > 100;
            int iconR = 36;
            int iconX = (Height - iconR) / 2;
            int iconY = (Height - iconR) / 2;
            using (var iconBrush = new SolidBrush(_accent)) {
                g.FillEllipse(iconBrush, iconX, iconY, iconR, iconR);
            }

            // Icon symbol
            using (var pen = new Pen(Color.White, 2.8f) { LineJoin = LineJoin.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round }) {
                float cx = iconX + iconR / 2f;
                float cy = iconY + iconR / 2f;
                float sz = iconR * 0.27f;

                if (isOn) {
                    // Checkmark
                    g.DrawLines(pen, new[] {
                        new PointF(cx - sz, cy),
                        new PointF(cx - sz * 0.1f, cy + sz),
                        new PointF(cx + sz, cy - sz * 0.85f)
                    });
                } else {
                    // X
                    g.DrawLine(pen, cx - sz, cy - sz, cx + sz, cy + sz);
                    g.DrawLine(pen, cx + sz, cy - sz, cx - sz, cy + sz);
                }
            }

            // Text
            int textX = iconX + iconR + 14;
            var textRect = new RectangleF(textX, 0, Width - textX - 12, Height);
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near })
            using (var font = new Font("Segoe UI", 17f, FontStyle.Bold, GraphicsUnit.Point))
            using (var brush = new SolidBrush(Color.White)) {
                g.DrawString(_text, font, brush, textRect, sf);
            }
        }

        private void TickAnimation() {
            const double maxOpacity = 0.92;

            if (_phase == 0) {
                _ticks++;
                var t = Math.Min(1.0, _ticks / 9.0);
                Opacity = maxOpacity * t;
                if (t >= 1.0) { _phase = 1; _ticks = 0; }
                return;
            }

            if (_phase == 1) {
                _ticks++;
                if (_ticks >= _holdTicks) { _phase = 2; _ticks = 0; }
                return;
            }

            if (_phase == 2) {
                _ticks++;
                var t = Math.Min(1.0, _ticks / 14.0);
                Opacity = maxOpacity * (1.0 - t);
                if (t >= 1.0) { _timer.Stop(); Hide(); }
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                try { _timer.Dispose(); } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
