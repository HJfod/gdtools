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
    }
}