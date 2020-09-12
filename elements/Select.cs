using System.Windows.Forms;

namespace gdtools {
    namespace Elem {
        public partial class Select : ListBox {
            public class SelectItem {
                public string Text { get; set; }
                public int Index { get; set; }
            }

            public Select(ContextMenuStrip _Menu = null) {
                this.SelectionMode = SelectionMode.MultiSimple;
                this.DisplayMember = "Text";
                this.ValueMember = "Text";
                this.ContextMenuStrip = _Menu;
                this.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            }

            public bool AddItem(string Text) {
                this.Items.Add(new SelectItem() {
                    Text = Text,
                    Index = this.Items.Count
                });

                return true;
            }
        }
    }
}