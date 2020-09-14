using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace gdtools {
    namespace Pages {
        public class Share : TableLayoutPanel {
            Elem.Select ExportSelect;

            FlowLayoutPanel ImportLeveLArea;
            CheckBox ExportCompressed;
            TableLayoutPanel RefreshDiv;

            public Share() {
                this.Name = "Sharing";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                ImportLeveLArea = new FlowLayoutPanel();
                ImportLeveLArea.Dock = DockStyle.Fill;
                ImportLeveLArea.AutoSize = true;

                ExportSelect = new Elem.Select();
                ExportSelect.DoubleClick += (s, e) => ViewInfo();

                foreach (dynamic lvl in GDTools.GetLevelList()) {
                    ExportSelect.AddItem(lvl.Name);
                }

                ExportCompressed = new CheckBox();
                ExportCompressed.Text = "Export compressed (.gmdc)";
                ExportCompressed.AutoSize = true;

                RefreshDiv = new TableLayoutPanel();
                RefreshDiv.AutoSize = true;
                RefreshDiv.Visible = false;

                RefreshDiv.Controls.Add(new Elem.Text("After importing all you want, make sure to refresh data in order to use the app further."));
                RefreshDiv.Controls.Add(new Elem.But("Refresh", (s, e) => Program.MainForm.FullReload()));

                FlowLayoutPanel ExportControls = new FlowLayoutPanel();
                ExportControls.AutoSize = true;

                ExportControls.Controls.Add(new Elem.But("Export", (s, e) => ExportLevel(), "Exports all the level(s) you've selected on the list"));
                ExportControls.Controls.Add(new Elem.But("View", (s, e) => ViewInfo(), "Shows info for the topmost level you've selected on the list."));

                this.Controls.Add(ExportSelect);
                this.Controls.Add(ExportControls);
                this.Controls.Add(ExportCompressed);
                this.Controls.Add(new Elem.NewLine());
                this.Controls.Add(new Elem.But("Help", (s, e) => Pages.SettingPage.ShowHelp("share")));
                this.Controls.Add(new Elem.SectionBreak());
                this.Controls.Add(new Elem.NewLine());
                this.Controls.Add(new Elem.But("Import", (s, e) => OpenImport(), "Opens a file browsing dialog where you can select level(s) to add to imports."));
                this.Controls.Add(RefreshDiv);
                this.Controls.Add(ImportLeveLArea);
            }

            private void ViewInfo() {
                try {
                    if (ExportSelect.SelectedItems.Count > 0) {
                        Elem.Select.SelectItem lvl = (Elem.Select.SelectItem)ExportSelect.SelectedItems[0];
                        dynamic LevelInfo = GDTools.GetLevelInfo(lvl.Text);
                        
                        string Info = "";

                        foreach (PropertyInfo i in LevelInfo.GetType().GetProperties()) {
                            if (i.Name != "Name" && i.Name != "Creator" && i.Name != "Description")
                                Info += $"{i.Name.Replace("_", " ")}: {i.GetValue(LevelInfo)}\n";
                        }

                        MessageBox.Show(Info, $"Info for {lvl.Text}", MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                } catch(Exception err) {
                    MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            public void AddImport(string file) {
                Elem.BorderPanel Level = new Elem.BorderPanel();

                EventHandler ImportThis = (object s, EventArgs e) => {
                    RefreshDiv.Visible = true;
                    string res = GDTools.ImportLevel(file);
                    if (res != null) MessageBox.Show($"Error: {res}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); else Level.Dispose();
                };

                EventHandler CloseThis = (object s, EventArgs e) => Level.Dispose();

                ContextMenuStrip CM = new ContextMenuStrip();
                CM.Items.Add(new ToolStripMenuItem("Import", null, ImportThis ));
                CM.Items.Add(new ToolStripMenuItem("Cancel", null, CloseThis ));

                Level.ContextMenuStrip = CM;

                dynamic LevelInfo = GDTools.GetLevelInfo(file);
                
                string Info = "";

                foreach (PropertyInfo i in LevelInfo.GetType().GetProperties()) {
                    if (i.Name != "Name" && i.Name != "Creator")
                        Info += $"{i.Name.Replace("_", " ")}: {i.GetValue(LevelInfo)}\n";
                }

                Level.Controls.Add(new Elem.Text($"{LevelInfo.Name} by {LevelInfo.Creator}"));

                FlowLayoutPanel x = new FlowLayoutPanel();
                x.AutoSize = true;
                x.Controls.Add(new Elem.But("Info", (s, e) => {
                    MessageBox.Show(Info, $"Info for {LevelInfo.Name}");
                }));
                x.Controls.Add(new Elem.But("Import", ImportThis));
                x.Controls.Add(new Elem.But("Close", CloseThis));

                Level.Controls.Add(x);

                ImportLeveLArea.Controls.Add(Level);
            }

            private void OpenImport() {
                using (OpenFileDialog ofd = new OpenFileDialog()) {
                    ofd.InitialDirectory = "c:\\";
                    ofd.Filter = GDTools.Ext.Filter;
                    ofd.FilterIndex = 1;
                    ofd.RestoreDirectory = true;
                    ofd.Multiselect = true;

                    if (ofd.ShowDialog() == DialogResult.OK) {
                        foreach (string file in ofd.FileNames) {
                            Elem.MsgBox LoadInfo = new Elem.MsgBox("Loading...");
                            LoadInfo.Show();
                            AddImport(file);
                            LoadInfo.Close();
                            LoadInfo.Dispose();
                        }
                    }
                }
            }

            private void ExportLevel() {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                DialogResult dr = fbd.ShowDialog();

                if (dr == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    string failure = "";
                    string success = "";
                    foreach (Elem.Select.SelectItem i in ExportSelect.SelectedItems) {
                        string ExportTry = GDTools.ExportLevel(i.Text, fbd.SelectedPath, ExportCompressed.Checked);
                        if (ExportTry != null) {
                            failure += ExportTry;
                        } else {
                            success += $"{i.Text}, ";
                        }
                    }
                    MessageBox.Show($"Succesfully exported {(success.Length > 0 ? success.Substring(0,success.Length-2) : "")}{(failure.Length > 0 ? $"; Failed: {failure}" : "")}");
                } else if (dr != DialogResult.Cancel) {
                    MessageBox.Show("Selected path not accepted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}