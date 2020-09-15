using System;
using System.Drawing;
using System.Windows.Forms;

namespace gdtools {
    namespace Pages {
        public class Collabs : TableLayoutPanel {
            public Elem.Select MergeList;
            public Elem.Text MergeBase;

            public Collabs() {
                this.Name = "Collabs";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                MergeList = new Elem.Select(false);
                MergeBase = new Elem.Text("Base not selected");

                FlowLayoutPanel MergeControls = new FlowLayoutPanel();
                MergeControls.AutoSize = true;

                MergeControls.Controls.Add(new Elem.But("Set base", (s, e) => {
                    Elem.ChooseForm Select = new Elem.ChooseForm("Select base source",
                    new string[] { "Add base from file", "Add base from local levels" });

                    Select.Show();

                    Select.Finish += res => {
                        if (res == 0) {
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
                                        AddBase(file);
                                        LoadInfo.Close();
                                        LoadInfo.Dispose();
                                    }
                                }
                            }
                        } else {
                            Form Choose = new Form();
                            Choose.Text = "Double-click to select";
                            Choose.Size = new Size(400, 300);
                            Meth.HandleTheme(Choose);

                            Elem.Select ExportSelect = new Elem.Select(false);

                            EventHandler Pick = (s, e) => {
                                if (ExportSelect.SelectedItem == null) return;
                                AddBase(((Elem.Select.SelectItem)ExportSelect.SelectedItem).Text);
                                Choose.Close();
                                Choose.Dispose();
                            };

                            ExportSelect.DoubleClick += Pick;

                            foreach (dynamic lvl in GDTools.GetLevelList())
                                ExportSelect.AddItem(lvl.Name);

                            Choose.Controls.Add(ExportSelect);

                            Choose.Show();
                        }
                    };
                }));
                MergeControls.Controls.Add(new Elem.But("Add part", (s, e) => {
                    Elem.ChooseForm Select = new Elem.ChooseForm("Select part source",
                    new string[] { "Add part from file", "Add part from local levels" });

                    Select.Show();

                    Select.Finish += res => {
                        if (res == 0) {
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
                                        AddMerge(file);
                                        LoadInfo.Close();
                                        LoadInfo.Dispose();
                                    }
                                }
                            }
                        } else {
                            Form Choose = new Form();
                            Choose.Text = "Double-click to select";
                            Choose.Size = new Size(400, 300);
                            Meth.HandleTheme(Choose);

                            Elem.Select ExportSelect = new Elem.Select(false);

                            EventHandler Pick = (s, e) => {
                                if (ExportSelect.SelectedItem == null) return;
                                AddMerge(((Elem.Select.SelectItem)ExportSelect.SelectedItem).Text);
                                Choose.Close();
                                Choose.Dispose();
                            };

                            ExportSelect.DoubleClick += Pick;

                            foreach (dynamic lvl in GDTools.GetLevelList())
                                ExportSelect.AddItem(lvl.Name);

                            Choose.Controls.Add(ExportSelect);

                            Choose.Show();
                        }
                    };
                }));
                MergeControls.Controls.Add(new Elem.But("Merge"));

                this.Controls.Add(new Elem.Text($"Collabing tools"));
                this.Controls.Add(MergeList);
                this.Controls.Add(MergeBase);
                this.Controls.Add(MergeControls);
            }

            public void AddMerge(string _LevelName) {
                MergeList.AddItem(_LevelName);
            }

            public void AddBase(string _LevelName) {
                MergeBase.Text = $"Base: {_LevelName}";
            }
        }
    }
}