using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace gdtools {
    public partial class GDTools : Form {
        public GDTools() {
            InitializeComponent();
        }

        public int SidebarSize = (int)(150F * Style.Scale);
        public int SidebarMaxSize = (int)(350F * Style.Scale);
        public int SidebarMinSize = 100;
        public int SelectedTab = 0;

        public dynamic TabList = new {};

        protected override CreateParams CreateParams {
            get {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void InitializeComponent() {
            this.Size = Settings.DefaultSize;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.ForeColor = Style.Colors.Text;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            Elements.Titlebar Titlebar = new Elements.Titlebar(ClientSize.Width, Style.TitlebarSize, this, Settings.AppName);
            Titlebar.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left);

            Panel Base = new Panel();
            Base.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left);
            Base.AutoSize = true;
            Base.Size = new Size(ClientSize.Width - 64, ClientSize.Height - Style.TitlebarSize);
            Base.Location = new Point(0, Style.TitlebarSize);
            Base.BackColor = Style.Colors.BG;

            Panel Main = new Panel();
            Main.AutoSize = true;
            Main.Size = new Size(Base.Width - this.SidebarSize, Base.Height);
            Main.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left);
            Main.BackColor = Style.Colors.BG;
            Main.Location = new Point(this.SidebarSize, 0);

            Panel Sidebar = new Panel();
            Sidebar.Size = new Size(this.SidebarSize, Base.Height);
            Sidebar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left);
            Sidebar.BackColor = Style.Colors.BGDark;
            Sidebar.Location = new Point(0, 0);

            this.TabList = new {
                Home = new Pages.Home(),
                Settings = new Pages.Settings()
            };

            int ix = 0;
            foreach (PropertyInfo pi in TabList.GetType().GetProperties()) {
                Pages.Page i = pi.GetValue(TabList);

                Elements.TabButton tab = new Elements.TabButton(Sidebar.Width, i._Name, null, i._Color, ix);
                if (ix < TabList.GetType().GetProperties().Length - 1) {
                    tab.Top = ix * Style.TabSize;
                } else {
                    tab.Top = Sidebar.Height - Style.TabSize;
                    tab.Anchor = ( AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right );
                }
                tab.Click += (object s, EventArgs e) => {
                    foreach (Elements.TabButton i in Sidebar.Controls) {
                        i._Selected = false;
                        i.Invalidate();
                    }
                    tab._Selected = true;
                };

                if (ix == 0) tab._Selected = true;

                Sidebar.Controls.Add(tab);

                ix++;
            }

            Elements.Dragger Dragger = new Elements.Dragger("ew", Sidebar, Main, Base.Height, SidebarMinSize, SidebarMaxSize, true);
            Dragger.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left);
            Dragger.BackColor = Style.Colors.BGDark;

            Base.Controls.Add(Sidebar);
            Base.Controls.Add(Dragger);
            Base.Controls.Add(Main);

            Dragger.BringToFront();

            this.Controls.Add(Base);
            this.Controls.Add(Titlebar);

            Titlebar.BringToFront();
            CenterToScreen();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Rectangle rc = new Rectangle(this.ClientSize.Width - Style.ResizeDrag, this.ClientSize.Height - Style.ResizeDrag, Style.ResizeDrag, Style.ResizeDrag);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);
            rc = new Rectangle(0, 0, this.ClientSize.Width, Style.TitlebarSize);
            e.Graphics.FillRectangle(Brushes.DarkBlue, rc);
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x84) {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < Style.TitlebarSize) {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - Style.ResizeDrag && pos.Y >= this.ClientSize.Height - Style.ResizeDrag) {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
