using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace gdtools {
    namespace Pages {
        public class SettingPage : TableLayoutPanel {
            public SettingPage() {
                this.Name = "Settings";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                CheckBox ToggleDarkMode = new CheckBox();
                if (Settings.DarkTheme) ToggleDarkMode.Checked = true;
                ToggleDarkMode.Text = "Enable Dark Mode";
                ToggleDarkMode.AutoSize = true;
                ToggleDarkMode.Click += (s, e) => {
                    Settings.DarkTheme = ToggleDarkMode.Checked;
                    GDTools.SaveKeyToUserData("dark-mode", ToggleDarkMode.Checked ? "1" : "0");
                    Program.MainForm.Reload();
                };

                CheckBox ToggleBackupCompression = new CheckBox();
                if (Settings.CompressBackups) ToggleBackupCompression.Checked = true;
                ToggleBackupCompression.Text = "Compress backups";
                ToggleBackupCompression.AutoSize = true;
                ToggleBackupCompression.Click += (s, e) => {
                    Settings.CompressBackups = ToggleBackupCompression.Checked;
                    GDTools.SaveKeyToUserData("compress-backups", ToggleBackupCompression.Checked ? "1" : "0");
                };

                this.Controls.Add(ToggleDarkMode);
                this.Controls.Add(ToggleBackupCompression);
            }
        }
    }
}