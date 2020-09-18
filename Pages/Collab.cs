using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace gdtools {
    namespace Pages {
        public class Collabs : TableLayoutPanel {
            public Elem.Select MergeList;
            public Elem.Text MergeBase;
            public bool MergeLink = false;

            public Collabs() {
                this.Name = "Collab";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                MergeList = new Elem.Select(false);
                MergeBase = new Elem.Text("Base not selected");

                FlowLayoutPanel MergeControls = new FlowLayoutPanel();
                MergeControls.AutoSize = true;

                MergeControls.Controls.Add(new Elem.But("Set base", (s, e) => {
                    Elem.ChooseForm Select = new Elem.ChooseForm("Select base source",
                    new string[] { "Set base from file", "Set base from local levels" });

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
                MergeControls.Controls.Add(new Elem.But("Add part(s)", (s, e) => {
                    Elem.ChooseForm Select = new Elem.ChooseForm("Select part source",
                    new string[] { "Add part(s) from file", "Add part(s) from local levels" });

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

                            Elem.Select ExportSelect = new Elem.Select();

                            EventHandler Pick = (s, e) => {
                                if (ExportSelect.SelectedItems[0] == null) return;
                                foreach (Elem.Select.SelectItem x in ExportSelect.SelectedItems)
                                    AddMerge(x.Text);
                                Choose.Close();
                                Choose.Dispose();
                            };

                            ExportSelect.DoubleClick += Pick;

                            foreach (dynamic lvl in GDTools.GetLevelList())
                                ExportSelect.AddItem(lvl.Name);

                            Choose.Controls.Add(new Elem.Div(new Control[] {
                                ExportSelect,
                                new Elem.But("Select", Pick)
                            }));

                            Choose.Show();
                        }
                    };
                }));
                MergeControls.Controls.Add(new Elem.But("Remove part", (s, e) => {
                    if (MergeList.SelectedItem == null) return;
                    MergeList.Items.Remove(MergeList.SelectedItem);
                }));
                MergeControls.Controls.Add(new Elem.But("Merge", (s, e) => {
                    string err = MergeParts();
                    if (err.Length > 0) MessageBox.Show(err, "Error merging");
                }));

                CheckBox MergeLinkToggle = new CheckBox();
                MergeLinkToggle.Text = "Link part objects";
                MergeLinkToggle.AutoSize = true;
                MergeLinkToggle.Click += (s, e) =>
                    MergeLink = MergeLinkToggle.Checked;

                this.Controls.Add(new Elem.Text("Part merging"));
                this.Controls.Add(MergeList);
                this.Controls.Add(MergeBase);
                this.Controls.Add(MergeControls);
                this.Controls.Add(MergeLinkToggle);
            }

            public string MergeParts() {
                try {
                    if (MergeBase.Text == "Base not selected") return "No base selected!";

                    if (MergeList.Items.Count == 0) return "No parts to merge!";

                    List<string> parts = new List<string> {};
                    foreach (Elem.Select.SelectItem x in MergeList.Items) parts.Add(x.Text);
                    string err = GDTools.Merge(MergeBase.Text.Substring("Base: ".Length), parts, MergeLink);
                    if (err.Length > 0) return err;

                    MessageBox.Show("Succesfully merged! :)");

                    return "";
                } catch (Exception e) { MessageBox.Show(e.ToString(), "Error"); return e.ToString(); }
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