using System;
using System.Windows.Forms;

namespace gdtools {
    namespace Pages {
        public class Home : TableLayoutPanel {
            public Home() {
                this.Name = "Home";
                this.Dock = DockStyle.Fill;

                this.Controls.Add(new Elem.Text($"Welcome to {Settings.AppName}!"));
                this.Controls.Add(new Elem.BigNewLine());
                this.Controls.Add(new Elem.Text($"Current version: {Settings.AppBuild} {Settings.AppVersion} ({Settings.AppVersionNum})"));
                this.Controls.Add(new Elem.BigNewLine());
                this.Controls.Add(new Elem.Text($"Credits:\r\nDeveloped by {Settings.Developers}"));
            }
        }
    }
}