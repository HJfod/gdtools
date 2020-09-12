using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace gdtools {
    namespace Pages {
        public class Share : TableLayoutPanel {
            Elem.Select ExportSelect;

            FlowLayoutPanel ImportLeveLArea;

            public Share() {
                this.Name = "Sharing";
                this.Dock = DockStyle.Fill;

                ImportLeveLArea = new FlowLayoutPanel();

                ExportSelect = new Elem.Select();

                foreach (dynamic lvl in GDTools.GetLevelList()) {
                    ExportSelect.AddItem(lvl.Name);
                }

                this.Controls.Add(ExportSelect);
                this.Controls.Add(new Elem.But("Export Selected", (s, e) => ExportLevel()));
                this.Controls.Add(new Elem.NewLine());
                this.Controls.Add(new Elem.SectionBreak());
                this.Controls.Add(new Elem.NewLine());
                this.Controls.Add(new Elem.But("Import", (s, e) => ExportLevel()));
                this.Controls.Add(ImportLeveLArea);
            }

            public void AddImport(string file) {
                Elem.BorderPanel Level = new Elem.BorderPanel();

                EventHandler ImportThis = (object s, EventArgs e) => {
                    string res = GDTools.ImportLevel(file);
                    if (res != null) MessageBox.Show($"Error: {res}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); else Level.Dispose();
                };

                EventHandler CloseThis = (object s, EventArgs e) => Level.Dispose();

                ContextMenuStrip CM = new ContextMenuStrip();
                CM.Items.Add(new ToolStripMenuItem("Import", null, ImportThis ));
                CM.Items.Add(new ToolStripMenuItem("Close", null, CloseThis ));

                Level.ContextMenuStrip = CM;

                dynamic LevelInfo = GDTools.GetLevelInfo(file);
                
                string Info = "";

                foreach (PropertyInfo i in LevelInfo.GetType().GetProperties()) {
                    if (i.Name != "Name" && i.Name != "Creator" && i.Name != "Description")
                        Info += $"{i.Name.Replace("_", " ")}: {i.GetValue(LevelInfo)}\n";
                }

                Level.Controls.Add(new Elem.Text(LevelInfo.Name));
                Level.Controls.Add(new Elem.Text($"by {LevelInfo.Creator}"));
                Level.Controls.Add(new Elem.Text($"\"{LevelInfo.Description}\""));
                Level.Controls.Add(new Elem.Text($"{Info}"));

                Elem.But ImportButton = new Elem.But("Import");
                ImportButton.Click += ImportThis;

                Elem.But CloseButton = new Elem.But("Close");
                CloseButton.Click += CloseThis;

                Level.Controls.Add(ImportButton);
                Level.Controls.Add(CloseButton);

                ImportLeveLArea.Controls.Add(Level);
            }

            private void OpenImport(object sender, EventArgs e) {
                using (OpenFileDialog ofd = new OpenFileDialog()) {
                    ofd.InitialDirectory = "c:\\";
                    ofd.Filter = $"Level files (*.{GDTools.Ext.LevelAlt};*.{GDTools.Ext.Level})|*.{GDTools.Ext.LevelAlt};*.{GDTools.Ext.Level}|All files (*.*)|*.*";
                    ofd.FilterIndex = 1;
                    ofd.RestoreDirectory = true;
                    ofd.Multiselect = true;

                    if (ofd.ShowDialog() == DialogResult.OK) {
                        foreach (string file in ofd.FileNames) {
                            AddImport(file);
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
                        string ExportTry = GDTools.ExportLevel(i.Text, fbd.SelectedPath);
                        if (ExportTry != null) {
                            failure += ExportTry;
                        } else {
                            success += $"{i.Text}, ";
                        }
                    }
                    MessageBox.Show($"Succesfully exported {success.Substring(0,success.Length-2)}{(failure.Length > 0 ? $"; Failed: {failure}" : "")}");
                } else if (dr != DialogResult.Cancel) {
                    MessageBox.Show("Selected path not accepted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}