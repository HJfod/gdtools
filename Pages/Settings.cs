using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;

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
                this.Controls.Add(new Elem.But("Help", (s, e) => {
                    Elem.ChooseForm Help = new Elem.ChooseForm("Help",
                    new string[] { "Exporting / Importing levels", "Backups"},
                    "What would you like help with?");

                    Help.Show();

                    Help.Finish += s => {
                        switch (s) {
                            case "Exporting / Importing levels": ShowHelp("share"); break;
                            case "Backups": ShowHelp("backups"); break;
                        }
                    };
                }));
                this.Controls.Add(new Elem.Text("In case you need specific help,\r\nmessage HJfod on Discord at HJfod#1795.\r\n"));

                LinkLabel Discord = new LinkLabel();
                Discord.AutoSize = true;
                Discord.Text = "Support server";
                Discord.LinkClicked += (s, e) => Process.Start("explorer.exe", "https://discord.gg/ZvV7zjj");

                this.Controls.Add(Discord);
            }

            public static void ShowHelp(string _For) {
                switch (_For) {
                    case "share":
                        MessageBox.Show(
                            "To export a level, select it from the list and click Export.\n\n"+
                            "If you'd like to see what stats the level has before exporting, click View / double click the list.\n\n\n\n" + 
                            "To import a level, drag and drop it on the app / click Import to select it from a file dialog.\n\n" +
                            "Once you'd selected the level, it will appear below the Import button. Now you can choose to Import it, View info about it or Cancel the import.",
                            "GDTools Help"
                        );
                        break;
                    case "backups":
                        MessageBox.Show(
                            "The backup controls are organized as such: First there is a list of backups," + 
                            "then three controls (View, Switch and Delete) that relate to the currently selected backup," +
                            "then two controls for adding new backups (New & Import Backup)," +
                            "then finally controls for the folder the backups are stored in.\n\n" +
                            "Right-click a button for specific help on it!",
                            "GDTools Help"
                        ); break;
                }
            }
        }
    }
}