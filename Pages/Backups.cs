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
                BackupControls.Controls.Add(new Elem.But("New", (s, e) => {
                    GDTools.Backups.CreateNewBackup();
                    RefreshBackupList();
                }));
                BackupControls.Controls.Add(new Elem.But("Import Backup", (s, e) => {
                    Elem.ChooseForm FileOrFolder = new Elem.ChooseForm("Select backup type", new string[] {"Folder", $"Compressed file (.zip / .{GDTools.Ext.Backup})"});

                    FileOrFolder.Show();

                    FileOrFolder.Finish += (s) => {
                        if (s == "Folder") {
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
                                    foreach (string file in ofd.FileNames) {
                                        Elem.MsgBox LoadInfo = new Elem.MsgBox("Importing...");
                                        LoadInfo.Show();
                                        GDTools.Backups.ImportBackup(file);
                                        RefreshBackupList();
                                        LoadInfo.Close();
                                        LoadInfo.Dispose();
                                    }
                                }
                            }
                        }
                    };
                }));
                BackupControls.Controls.Add(new Elem.But("View", ViewBackup));
                BackupControls.Controls.Add(new Elem.But("Delete", (s, e) => {
                    if (BackupSelect.SelectedItem != null) {
                        GDTools.Backups.DeleteBackup(((Elem.Select.SelectItem)BackupSelect.SelectedItem).Text);
                        
                        RefreshBackupList();
                    }
                }));
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
                }));
                BackupControls.Controls.Add(new Elem.But("Open Folder", (s, e) => Process.Start("explorer.exe", GDTools._BackupDirectory)));
                BackupControls.Controls.Add(new Elem.But("Refresh Folder", (s, e) => RefreshBackupList()));

                this.Controls.Add(BackupSelect);
                this.Controls.Add(BackupControls);
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