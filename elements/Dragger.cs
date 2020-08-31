using System;
using System.Drawing;
using System.Windows.Forms;

namespace gdtools {
    namespace Elements {
        class Dragger : PictureBox {
            public Dragger(string direction, Control before, Control after, int size, int min = 50, int max = 500, bool offset = false) {
                InitializeComponent(direction, before, after, size, min, max, offset);
            }

            private bool Resizing;

            private void InitializeComponent(string direction, Control before, Control after, int size, int min, int max, bool offset) {
                this.Cursor = (direction == "ew" || direction == "we") ? Cursors.SizeWE : Cursors.SizeNS;
                this.Resizing = false;

                if (direction == "ew" || direction == "we") {
                    this.Size = new Size(Style.DraggerWidth, size);
                    this.Left = before.Width - this.Width / (offset ? 1 : 2);
                } else {
                    this.Size = new Size(size, Style.DraggerWidth);
                    this.Top = before.Height - this.Height / (offset ? 1 : 2);
                }
                this.BringToFront();

                this.MouseDown += (object s, MouseEventArgs e) => this.Resizing = true;
                this.MouseUp += (object s, MouseEventArgs e) => this.Resizing = false;
                this.MouseMove += (object s, MouseEventArgs e) => {
                    if (this.Resizing) {
                        if (direction == "ew" || direction == "we") {
                            if (before.Width + e.X > min && before.Width + e.X < max) {
                                before.Width = before.Width + e.X;
                                after.Width = after.Width - e.X;
                                after.Left = after.Left + e.X;
                                this.Left = before.Width - this.Width / (offset ? 1 : 2);
                            }
                        } else {
                            if (before.Height + e.Y > min && before.Height + e.Y < max) {
                                before.Height = before.Height + e.Y;
                                after.Height = after.Height - e.Y;
                                after.Top = after.Top + e.Y;
                                this.Top = before.Height - this.Height / (offset ? 1 : 2);
                            }
                        }
                    }
                };
            }
        }
    }
}