using System.Windows.Forms;
using System.Drawing;

namespace gdtools {
    namespace Pages {
        public class Home : Page {
            public Home() {
                this._Color = Style.Colors.TabHome;
                this._Name = "Home";

                Elements.Container C = new Elements.Container();

                Label t = new Label();
                t.Text = "Welcome to GDTools!";

                C.Controls.Add(t);

                this.Controls.Add(C);
            }
        }
    }
}