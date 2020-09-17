using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace gdtools {
    namespace Pages {
        public class SettingPage : TableLayoutPanel {
            private int ConvertVer(string ver) {
                string res = "";
                foreach (Match m in Regex.Matches(ver, "[0-9]+", RegexOptions.None, Regex.InfiniteMatchTimeout))
                    res += m.Value;
                return Int32.Parse(res);
            }

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

                this.Controls.Add(new Elem.BigNewLine());

                this.Controls.Add(new Elem.But("Check for updates", (s, e) => {
                    try {
                        string url = "https://api.github.com/repos/HJfod/gdtools/releases/latest";

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.UserAgent = "request";

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode != HttpStatusCode.OK) {
                            throw new Exception("Can't connect to Github! HTTPS Response Code: " + response.StatusCode);
                        }

                        Stream resStream = response.GetResponseStream();
                        StreamReader streamRead = new StreamReader( resStream );

                        string msg = "";

                        Char[] buffer = new Char[256];
                        int count = streamRead.Read( buffer, 0, 256 );

                        while (count > 0) {
                            msg += new String(buffer, 0, count);
                            count = streamRead.Read(buffer, 0, 256);
                        }

                        streamRead.Close();
                        resStream.Close();
                        response.Close();

                        string tag = Regex.Match(msg, $"\"tag_name\":\".*?\"", RegexOptions.None, Regex.InfiniteMatchTimeout).Value;
                        if (tag == null || tag == "") throw new Exception("Could not find tag_name!");
                        tag = tag.Substring(tag.IndexOf(":"));
                        tag = tag.Substring(tag.IndexOf('"') + 1, tag.LastIndexOf('"') - 1);
                        
                        int vern = ConvertVer(tag);
                        int vero = ConvertVer(Settings.AppVersion);
                        if (vero == vern) {
                            MessageBox.Show("You are up to date!", "Version check");
                        } else if (vero < vern) {
                            MessageBox.Show("A new version is available!", "Version check");
                            Process.Start("explorer.exe", "https://github.com/HJfod/gdtools/releases/latest");
                        } else if (vero > vern) {
                            MessageBox.Show("You are using a newer versin than last stable release.", "Version check");
                        }
                    } catch (Exception err) {
                        MessageBox.Show($"Error: {err}", "Error");
                    }
                }));
                this.Controls.Add(new Elem.But("Help", (s, e) => {
                    Elem.ChooseForm Help = new Elem.ChooseForm("Help",
                    new string[] { "Exporting / Importing levels", "Backups"},
                    "What would you like help with?");

                    Help.Show();

                    Help.Finish += s => {
                        switch (s) {
                            case 0: ShowHelp("share"); break;
                            case 1: ShowHelp("backups"); break;
                        }
                    };
                }));
                this.Controls.Add(new Elem.But("Reload app", (s, e) => Program.MainForm.FullReload()));
                
                this.Controls.Add(new Elem.BigNewLine());
                
                this.Controls.Add(new Elem.Text("In case you need specific help,\r\nmessage HJfod on Discord at HJfod#1795.\r\n"));

                this.Controls.Add(new Elem.Link("Support server", "https://discord.gg/ZvV7zjj"));
                this.Controls.Add(new Elem.Link("Github repository", "https://github.com/HJfod/gdtools"));
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