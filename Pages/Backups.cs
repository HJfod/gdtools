using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;

namespace gdtools {
    namespace Pages {
        public class Backups : TableLayoutPanel {
            Elem.Select BackupSelect;
            Elem.Text BackupPath;

            public Backups() {
                this.Name = "Backups";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                EventHandler ViewBackup = (s, e) => {
                    if (BackupSelect.SelectedItem != null) {
                        Elem.Select.SelectItem backup = (Elem.Select.SelectItem)BackupSelect.SelectedItem;
                        
                        Form Info = new Form();
                        Info.Size = new Size(350, 400);
                        Info.Text = $"Viewing {backup.Text}";
                        Info.Icon = new Icon(Settings.IconPath);
                        Info.FormClosed += (s, e) => Info.Dispose();

                        Meth.HandleTheme(Info);

                        Elem.Text Load = new Elem.Text("Loading...");

                        Info.Show();

                        Info.Controls.Add(Load);

                        dynamic BackupInfo = GDTools.Backups.GetBackupInfo(backup.Text);

                        string User = "";

                        Elem.Select Levels = new Elem.Select(false);
                        Levels.DoubleClick += (s, e) => {
                            if (Levels.SelectedItem != null) {
                                Elem.Select.SelectItem lvl = (Elem.Select.SelectItem)Levels.SelectedItem;
                                dynamic LevelInfo = GDTools.GetLevelInfo(lvl.Text, BackupInfo.Levels);
                                
                                string Info = "";

                                foreach (PropertyInfo i in LevelInfo.GetType().GetProperties()) {
                                    if (i.Name != "Name" && i.Name != "Creator" && i.Name != "Description")
                                        Info += $"{i.Name.Replace("_", " ")}: {i.GetValue(LevelInfo)}\n";
                                }

                                MessageBox.Show(Info, $"Info for {lvl.Text}", MessageBoxButtons.OK, MessageBoxIcon.None);
                            }
                        };

                        foreach (PropertyInfo i in BackupInfo.User.Stats.GetType().GetProperties()) {
                            User += $"{i.Name.Replace("_", " ")}: {i.GetValue(BackupInfo.User.Stats)}\r\n";
                        }

                        foreach (dynamic lvl in BackupInfo.Levels) {
                            Levels.AddItem(lvl.Name);
                        }

                        Info.Controls.Remove(Load);

                        FlowLayoutPanel Contain = new FlowLayoutPanel();
                        Contain.Dock = DockStyle.Fill;
                        Contain.AutoSize = true;

                        Contain.Controls.Add(new Elem.Text(User));
                        Contain.Controls.Add(new Elem.BigNewLine());
                        Contain.Controls.Add(Levels);
                        Contain.Controls.Add(new Elem.Text("Double-click to view level"));

                        Info.Controls.Add(Contain);
                    }
                };

                GDTools.Backups.InitBackups();

                BackupSelect = new Elem.Select(false);
                BackupSelect.DoubleClick += ViewBackup;

                BackupPath = new Elem.Text();

                RefreshBackupList();

                ContextMenuStrip CM = new ContextMenuStrip();
                CM.Items.Add(new ToolStripMenuItem("View selected backup", null, ViewBackup));
                BackupSelect.ContextMenuStrip = CM;

                FlowLayoutPanel BackupControls = new FlowLayoutPanel();
                BackupControls.AutoSize = true;
                BackupControls.Dock = DockStyle.Fill;
                BackupControls.Controls.Add(new Elem.But("View", ViewBackup, "This button shows info about the backup. (You can do the same by double-clicking a backup on the list)"));
                BackupControls.Controls.Add(new Elem.But("Load", (s, e) => {
                    Elem.ChooseForm BackupCurrent = new Elem.ChooseForm(
                        "Backup current progress?",
                        new string[] {"Yes", "No", "Cancel"},
                        "Would you like to backup your current GD progress before loading?"
                    );

                    BackupCurrent.Show();

                    BackupCurrent.Finish += (s) => {
                        if (s == 2) return;
                        if (BackupSelect.SelectedItem != null) {
                            if (s == 0)
                                GDTools.Backups.CreateNewBackup();
                        
                            GDTools.Backups.SwitchToBackup(((Elem.Select.SelectItem)BackupSelect.SelectedItem).Text);

                            Program.MainForm.FullReload();
                        }
                    };
                }, "Switches your current GD progress to that of the backup. Asks you before switching if you'd like to save your current progress."));
                BackupControls.Controls.Add(new Elem.But("Delete", (s, e) => {
                    if (BackupSelect.SelectedItem != null) {
                        Elem.ChooseForm Y = new Elem.ChooseForm("Are you sure?", new string[] { "Yes", "Cancel" }, "Are you sure you want to delete this backup?");

                        Y.Show();

                        Y.Finish += s => {
                            if (s != 0) return;

                            GDTools.Backups.DeleteBackup(((Elem.Select.SelectItem)BackupSelect.SelectedItem).Text);
                            
                            RefreshBackupList();
                        };
                    }
                }, "Deletes the selected backup permanently."));
                BackupControls.Controls.Add(new Elem.NewLine());
                BackupControls.Controls.Add(new Elem.But("New", (s, e) => {
                    GDTools.Backups.CreateNewBackup();
                    RefreshBackupList();
                }, "Creates a new backup of your current GD progress."));
                BackupControls.Controls.Add(new Elem.But("Import Backup", (s, e) => {
                    Elem.ChooseForm FileOrFolder = new Elem.ChooseForm("Select backup type", new string[] {"Folder", $"Compressed file (.zip / .{GDTools.Ext.Backup})"});

                    FileOrFolder.Show();

                    FileOrFolder.Finish += (s) => {
                        if (s == 0) {
                            using (FolderBrowserDialog ofd = new FolderBrowserDialog()) {
                                ofd.Description = "Select a backup folder";

                                if (ofd.ShowDialog() == DialogResult.OK) {
                                    GDTools.Backups.ImportBackup(ofd.SelectedPath);
                                    RefreshBackupList();
                                }
                            }
                        } else {
                            using (OpenFileDialog ofd = new OpenFileDialog()) {
                                ofd.InitialDirectory = "c:\\";
                                ofd.Filter = GDTools.Ext.BackupFilter;
                                ofd.FilterIndex = 1;
                                ofd.RestoreDirectory = true;
                                ofd.Multiselect = true;

                                if (ofd.ShowDialog() == DialogResult.OK) {
                                    foreach (string file in ofd.FileNames)
                                        ImportBackup(file);
                                }
                            }
                        }
                    };
                }, "This button lets you import backups you've made before."));
                BackupControls.Controls.Add(new Elem.BigNewLine());
                BackupControls.Controls.Add(BackupPath);
                BackupControls.Controls.Add(new Elem.NewLine());
                BackupControls.Controls.Add(new Elem.But("Change Folder", (s, e) => {
                    using (FolderBrowserDialog ofd = new FolderBrowserDialog()) {
                        ofd.Description = "Select backup directory";

                        if (ofd.ShowDialog() == DialogResult.OK) {
                            GDTools.Backups.SetBackupLocation(ofd.SelectedPath);
                            RefreshBackupList();
                        }
                    }
                }, "Change the folder where backups are saved. All backups are automatically moved to the new location."));
                BackupControls.Controls.Add(new Elem.But("Open Folder", (s, e) => Process.Start("explorer.exe", GDTools._BackupDirectory), "Opens the backup folder in File Explorer."));
                BackupControls.Controls.Add(new Elem.But("Refresh Folder", (s, e) => RefreshBackupList(), "Reloads the backup list."));
                BackupControls.Controls.Add(new Elem.BigNewLine());
                BackupControls.Controls.Add(new Elem.But("Help", (s, e) => Pages.SettingPage.ShowHelp("backups")));

                this.Controls.Add(BackupSelect);
                this.Controls.Add(BackupControls);
            }

            public void ImportBackup(string file) {
                Elem.MsgBox LoadInfo = new Elem.MsgBox("Importing...");
                LoadInfo.Show();
                GDTools.Backups.ImportBackup(file);
                RefreshBackupList();
                LoadInfo.Close();
                LoadInfo.Dispose();
            }

            public void RefreshBackupList() {
                BackupSelect.Items.Clear();

                foreach (dynamic lvl in GDTools.Backups.GetBackups()) {
                    BackupSelect.AddItem(lvl.Name);
                }

                BackupPath.Text = $"Current folder: {GDTools._BackupDirectory}";
            }
        }
    }
}