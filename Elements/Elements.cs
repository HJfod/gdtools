using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

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
        public static int _S(int Size) {
            return (int)((float)Size * Settings.AppScale);
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
                this.Width = Meth._S(300);
                this.Height = Meth._S(100);
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
                this.Size = new Size(Meth._S(120),Meth._S(80));
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
                this.Size = new Size(Meth._S(400), Meth._S(200));
                this.Icon = new Icon(Settings.IconPath);
                
                Elem.Text InfoText = new Elem.Text("This app can not be used while GD is open.\r\n\r\nIt will automatically boot up once you close the game.");

                this.Controls.Add(InfoText);
            }
        }

        public class ChooseForm : Form {
            public event Action<int> Finish;
            public event Action<string> FinishStr;

            private void ChooseFinishEventHandler(int Fin) {}
            private void ChooseFinishStrEventHandler(string Fin) {}

            public ChooseForm(string _Name, string[] _Buttons, string _Text = null) {
                Meth.HandleTheme(this);

                Finish += new Action<int>(ChooseFinishEventHandler);
                FinishStr += new Action<string>(ChooseFinishStrEventHandler);

                this.Text = _Name;
                this.Size = new Size(Meth._S(250), Meth._S(200));
                this.Icon = new Icon(Settings.IconPath);

                FlowLayoutPanel p = new FlowLayoutPanel();
                p.Dock = DockStyle.Fill;
                p.AutoSize = true;

                if (_Text != null) 
                    p.Controls.Add(new Elem.Text(_Text));

                int i = 0;
                int rtype = 0;
                Input inp = new Input();
                foreach (string Button in _Buttons) {
                    if (Button.StartsWith("IS-INPUT")) {
                        switch (Button.Contains("::") ? Button.Substring(Button.IndexOf("::") + 2) : "") {
                            case "INT":
                                inp.SetType("INT");
                                break;
                        }
                        rtype = 1;
                        p.Controls.Add(inp);
                        p.Controls.Add(new Elem.NewLine());
                    } else {
                        bool ret = false;
                        string bt;
                        if (Button.StartsWith("::")) {
                            ret = true;
                            bt = Button.Substring(2);
                        } else bt = Button;
                        But n = new But(bt);
                        int ix = i;
                        n.Click += (s, e) => {
                            if (rtype == 1) { if (ret) FinishStr(inp.Text); else FinishStr(""); } else Finish(ix);
                            this.Close();
                            this.Dispose();
                        };
                        p.Controls.Add(n);
                    }
                    i++;
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
            public But(string _Text = "", EventHandler _Click = null, string _HelpText = null) {
                Meth.HandleTheme(this);
                this.AutoSize = true;
                this.FlatStyle = FlatStyle.Flat;
                if (_Text != "") this.Text = _Text;
                if (_Click != null) this.Click += _Click;
                if (_HelpText != null) {
                    ContextMenuStrip CM = new ContextMenuStrip();
                    CM.Items.Add(new ToolStripMenuItem("What does this button do?", null, (s, e) => {
                        MessageBox.Show(_HelpText, "Button Help");
                    }));
                    this.ContextMenuStrip = CM;
                }
            }
        }

        public class Input : TextBox {
            private bool onlyNum = false;

            public Input(string _name = "__input", string _type = "ANY", string _desc = "") {
                Meth.HandleTheme(this);

                this.Name = _name;
                this.SetType(_type);

                this.Text = _desc;

                this.MouseEnter += (s, e) => { if (this.Text == _desc) this.Text = ""; };
                this.MouseLeave += (s, e) => { if (this.Text == "" && !this.Focused) this.Text = _desc; };
                this.GotFocus += (s, e) => { if (this.Text == _desc) this.Text = ""; };

                this.KeyPress += (o, e) => 
                    e.Handled = this.onlyNum ? !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) : false;
            }

            public void SetType(string _t) {
                onlyNum = _t == "INT";
            }
        }

        public class Link : LinkLabel {
            public Link(string _Text, string _Link) {
                this.AutoSize = true;
                this.Text = _Text;
                this.LinkClicked += (s, e) => Process.Start("explorer.exe", _Link);
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

        public class Div : FlowLayoutPanel {
            public Div(Control[] _Controls) {
                this.AutoSize = true;
                this.Dock = DockStyle.Fill;

                foreach (Control c in _Controls) {
                    this.Controls.Add(c);
                }
            }
        }
    }
}