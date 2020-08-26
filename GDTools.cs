using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdtools {
    public partial class GDTools : Form {
        public GDTools() {
            InitializeComponent();
        }

        public int SidebarSize = 150;

        private void InitializeComponent() {
            Size = Settings.DefaultSize;

            SplitContainer Base = new SplitContainer();
            Base.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left);
            Base.AutoSize = true;
            Base.Size = new Size(ClientSize.Width, ClientSize.Height);
            Base.BackColor = Color.Black;
            Base.SplitterIncrement = 10;
            Base.SplitterWidth = Style.DraggerWidth;
            Base.Panel1MinSize = 40;
            Base.Panel2MinSize = 60;
            Base.SplitterDistance = this.SidebarSize;

            Panel Main = new Panel();
            Main.Size = new Size(ClientSize.Width - this.SidebarSize, ClientSize.Height);
            Main.AutoSize = true;
            Main.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left);
            Main.BackColor = Color.Green;
            Main.Location = new Point(this.SidebarSize, 0);

            Label txt = new Label();
            txt.Text = "wow";
            Main.Controls.Add(txt);

            Panel Sidebar = new Panel();
            Sidebar.Size = new Size(this.SidebarSize, ClientSize.Height);
            Sidebar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left);
            Sidebar.BackColor = Color.Red;
            Sidebar.Location = new Point(0,0);

            Controls.Add(Sidebar);
            Controls.Add(Main);
        }
    }
}
