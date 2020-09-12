using System;
using System.Threading;
using System.Windows.Forms;

namespace gdtools {
    namespace Pages {
        public class Share : FlowLayoutPanel {
            public Share() {
                this.Name = "Sharing";
                this.Dock = DockStyle.Fill;

                Elem.Select ExportSelect = new Elem.Select();

                foreach (dynamic lvl in GDTools.GetLevelList()) {
                    ExportSelect.AddItem(lvl.Name);
                }

                this.Controls.Add(ExportSelect);
            }
        }
    }
}