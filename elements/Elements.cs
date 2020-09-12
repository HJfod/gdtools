using System;
using System.Drawing;
using System.Windows.Forms;

namespace gdtools {
    namespace Elem {
        public class MsgBox : Form {
            private Elem.Text Loading;

            public MsgBox(string _msg = "") {
                this.Text = _msg;
                this.Size = new Size(120,80);
                this.MinimizeBox = false;
                this.MaximizeBox = false;

                Loading = new Elem.Text();

                this.Controls.Add(Loading);

                this.CenterToScreen();
            }

            public void Txt(string _t = "") {
                Loading.Text = _t;
            }
        }

        public class Text : Label {
            public Text(string _Text = "") {
                this.AutoSize = true;
                if (_Text != "") this.Text = _Text;
            }
        }

        public class But : Button {
            public But(string _Text = "", EventHandler _Click = null) {
                this.AutoSize = true;
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
        
        public class BigNewLine : Panel {
            public BigNewLine() {
                this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                this.Width = Settings.DefaultSize.Width;
                this.Height = 30;
            }
        }
    }
}