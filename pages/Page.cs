using System.Windows.Forms;
using System.Drawing;

namespace gdtools {
    namespace Pages {
        public class Page : Panel {
            public Color _Color;
            public string _Name;

            public Page() {
                this.AutoSize = true;
            }
        }

        public class Settings : Page {
            public Settings() {
                this._Color = Style.Colors.Light;
                this._Name = "Settings";

                Elements.Container C = new Elements.Container();

                Label t = new Label();
                t.Text = "wwwww";

                C.Controls.Add(t);

                this.Controls.Add(C);
            }
        }
    }
}