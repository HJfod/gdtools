using System;
using System.Drawing;
using System.Windows.Forms;

namespace gdtools {
    namespace Elements {
        public class TabButton : Button {
            private Color _Hover = Style.Colors.BG;
            private Color _BG = Style.Colors.BGDark;
            private Color _Side = Style.Colors.TitlebarBG;
            private Color _Current;
            private Color _CurrentSide;
            private Color _Select;
            public int _ID;
            public bool _Selected = false;

            public TabButton(int _width = 50, string _text = null, Color? BG = null, Color? side = null, int? id = null) {
                this.Text = _text;
                this.ForeColor = Style.Colors.Text;
                this.Height = Style.TabSize;
                this.Width = _width;
                this.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                this.DoubleBuffered = true;
                this._ID = id != null ? (int)id : -1;

                if (BG is Color) {
                    this._BG = (Color)BG;
                    this._Hover = ControlPaint.Light((Color)BG, 0.5F);
                }

                if (side is Color) {
                    this._Side = (Color)side;
                }

                this._Current = this._BG;
                this._CurrentSide = this._Hover;
                this._Select = Style.Colors.Light;
                this.Invalidate();

                this.MouseEnter += (object s, EventArgs e) => {
                    this._Current = _Hover;
                    this._CurrentSide = _Side;
                };
                this.MouseLeave += (object s, EventArgs e) => {
                    this._Current = _BG;
                    this._CurrentSide = _Hover;
                };
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);
                using (Brush b = new SolidBrush(this._Selected ? this._Select : this._Current)) {
                    e.Graphics.FillRectangle(b, this.ClientRectangle);
                    e.Graphics.FillRectangle(new SolidBrush(this._CurrentSide), new Rectangle(0, 0, Style.TabSideSize, this.ClientRectangle.Height));
                    TextRenderer.DrawText(
                        e.Graphics,
                        this.Text,
                        this.Font,
                        new Rectangle(Style.TabSideSize * 2, 0, this.ClientRectangle.Width - Style.TabSideSize * 2, this.ClientRectangle.Height),
                        this.ForeColor,
                        TextFormatFlags.VerticalCenter
                    );
                }
            }
        }
    }
}