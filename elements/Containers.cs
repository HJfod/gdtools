using System.Windows.Forms;

namespace gdtools {
    namespace Elements {
        public class Container : FlowLayoutPanel {
            public Container() {
                this.AutoSize = true;
                this.Padding = Style.Padding;
            }
        }
    }
}