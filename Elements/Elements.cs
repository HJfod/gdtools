using System;
using System.Drawing;
using System.Windows.Forms;

namespace gdtools {
    public static class Meth {
        public static void HandleTheme(Control obj, bool _noback = false) {
            if (Settings.DarkTheme) {
                obj.ForeColor = Color.White;
                if (!_noback) obj.BackColor = Color.Black;
            } else {
                obj.ForeColor = Color.Black;
                if (!_noback) obj.BackColor = Color.White;
            }
        }
    }
    namespace Elem {
        public partial class Select : ListBox {
            public class SelectItem {
                public string Text { get; set; }
                public int Index { get; set; }
            }

            public Select(bool _Multi = true) {
                this.SelectionMode = _Multi ? SelectionMode.MultiSimple : SelectionMode.One;
                this.DisplayMember = "Text";
                this.ValueMember = "Text";
                this.Width = 300;
                this.Height = 100;
                Meth.HandleTheme(this);
            }

            public bool AddItem(string Text) {
                this.Items.Add(new SelectItem() {
                    Text = Text,
                    Index = this.Items.Count
                });

                return true;
            }
        }

        public class MsgBox : Form {
            private Elem.Text Loading;

            public MsgBox(string _msg = "") {
                this.Text = _msg;
                this.Size = new Size(120,80);
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.Icon = new Icon(Settings.IconPath);

                Loading = new Elem.Text();

                this.Controls.Add(Loading);

                this.CenterToScreen();
            }

            public void Txt(string _t = "") {
                Loading.Text = _t;
            }
        }

        public class PauseForm : Form {
            public PauseForm() {
                this.Text = "Program Paused";
                this.Size = new Size(400, 200);
                this.Icon = new Icon(Settings.IconPath);
                
                Elem.Text InfoText = new Elem.Text("This app can not be used while GD is open.\r\n\r\nIt will automatically boot up once you close the game.");

                this.Controls.Add(InfoText);
            }
        }

        public class ChooseForm : Form {
            public event Action<string> Finish;

            private void ChooseFinishEventHandler(string Fin) {}

            public ChooseForm(string _Name, string[] _Buttons) {
                Meth.HandleTheme(this);

                Finish += new Action<string>(ChooseFinishEventHandler);

                this.Text = _Name;
                this.Size = new Size(250, 200);
                this.Icon = new Icon(Settings.IconPath);

                TableLayoutPanel p = new TableLayoutPanel();
                p.Dock = DockStyle.Fill;
                p.AutoSize = true;

                foreach (string Button in _Buttons) {
                    But n = new But(Button);
                    n.Click += (s, e) => {
                        Finish(Button);
                        this.Close();
                    };
                    p.Controls.Add(n);
                }

                this.Controls.Add(p);
                this.CenterToScreen();
            }
        }

        public class BorderPanel : TableLayoutPanel {
            public BorderPanel() {
                this.AutoSize = true;
                this.BorderStyle = BorderStyle.FixedSingle;
                Meth.HandleTheme(this);
            }
        }

        public class Text : Label {
            public Text(string _Text = "") {
                this.AutoSize = true;
                Meth.HandleTheme(this, true);
                if (_Text != "") this.Text = _Text;
            }
        }

        public class But : Button {
            public But(string _Text = "", EventHandler _Click = null) {
                Meth.HandleTheme(this);
                this.AutoSize = true;
                this.FlatStyle = FlatStyle.Flat;
                if (_Text != "") this.Text = _Text;
                if (_Click != null) this.Click += _Click;
            }
        }
        
        public class NewLine : Panel {
            public NewLine() {
                this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                this.Width = Settings.DefaultSize.Width;
                this.Height = 1;
            }
        }
        
        public class SectionBreak : Panel {
            public SectionBreak() {
                this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                this.Width = Settings.DefaultSize.Width;
                this.Height = 1;
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);
                this.BackColor = this.ForeColor;
            }
        }
        
        public class BigNewLine : Panel {
            public BigNewLine() {
                this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                this.Width = Settings.DefaultSize.Width;
                this.Height = 30;
            }
        }
    }
}