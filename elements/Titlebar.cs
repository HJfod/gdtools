using System;
using System.Drawing;
using System.Windows.Forms;

namespace gdtools {
    namespace Elements {
        public class TitlebarButton : Button {
            public TitlebarButton(string _text = null, EventHandler _click = null) {
                this.Text = _text;
                this.Anchor = (AnchorStyles.Right);
                this.Dock = DockStyle.Right;
                this.ForeColor = Style.Colors.Text;
                this.Size = new Size((int)((double)Style.TitlebarSize * 1.5), Style.TitlebarSize);
                this.Font = new Font("Segoe UI", Style.TitlebarSize / (int)(3F * Style.Scale));
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;

                if (_click != null) this.Click += _click;
            }
        }

        public class Titlebar : Panel {
            public Titlebar(int width, int height, Form OG, string title) {
                this.Size = new Size(width, height);
                this.BackColor = Style.Colors.TitlebarBG;
                this.Location = new Point(0,0);
                this.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left);

                Label Title = new Label();
                Title.Text = title;
                Title.ForeColor = Style.Colors.Text;
                Title.Location = new Point(0,0);
                Title.Font = new Font("Segoe UI", Style.TitlebarSize / (int)(3F * Style.Scale));
                Title.Height = Style.TitlebarSize;
                Title.Width = (int)(100F * Style.Scale);
                Title.TextAlign = ContentAlignment.MiddleCenter;

                this.Controls.Add(Title);
                this.Controls.Add(new Elements.TitlebarButton("─", (object s, EventArgs e) => OG.WindowState = FormWindowState.Minimized ));
                this.Controls.Add(new Elements.TitlebarButton("☐", (object s, EventArgs e) => MaxNom(OG) ));
                this.Controls.Add(new Elements.TitlebarButton("✕", (object s, EventArgs e) => OG.Close() ));

                ContextMenuStrip CM = new ContextMenuStrip();
                CM.Items.Add(new ToolStripMenuItem("Minimize", null, (object s, EventArgs e) => OG.WindowState = FormWindowState.Minimized ));
                CM.Items.Add(new ToolStripMenuItem("Maximize", null, (object s, EventArgs e) => MaxNom(OG) ));

                CM.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem qi = new ToolStripMenuItem("Quit", null, (object s, EventArgs e) => OG.Close() );
                qi.ShortcutKeys = (Keys.Alt | Keys.F4);
                CM.Items.Add(qi);

                
                this.MouseDown += (object s, MouseEventArgs e) => this.Moving = new Point(e.X, e.Y);
                this.MouseUp += (object s, MouseEventArgs e) => this.Moving = null;
                this.MouseMove += (object s, MouseEventArgs e) => {
                    if (this.Moving is Point) {
                        Point p2 = OG.PointToScreen(new Point(e.X, e.Y));
                        OG.Location = new Point(p2.X - ((Point)this.Moving).X, p2.Y - ((Point)this.Moving).Y);
                    }
                };
                this.DoubleClick += (object s, EventArgs e) => MaxNom(OG);

                this.ContextMenuStrip = CM;
            }

            private void MaxNom(Form OG) {
                OG.WindowState = OG.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            }

            public Point? Moving = null;
        }
    }
}