using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gdtools {
    namespace Pages {
        public class Collabs : TableLayoutPanel {
            public Elem.Select MergeList;
            public Elem.Text MergeBase;
            public bool MergeLink = false;
            public bool AutoReassignGroups = true;

            public Collabs() {
                this.Name = "Collab";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                TableLayoutPanel con = new TableLayoutPanel();
                con.AutoSize = true;
                con.Visible = Settings.DevMode;

                MergeList = new Elem.Select(false);
                MergeBase = new Elem.Text("Base not selected");

                FlowLayoutPanel MergeControls = new FlowLayoutPanel();
                MergeControls.AutoSize = true;

                MergeControls.Controls.Add(new Elem.But("Set base", (s, e) => {
                    Elem.ChooseForm Select = new Elem.ChooseForm("Select base source",
                    new string[] { "Set base from file", "Set base from local levels", "Set base by ID" });

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
                        } else if (res == 1) {
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
                        } else if (res == 2) {
                            Elem.ChooseForm c = new Elem.ChooseForm( "Type Level ID", 
                                new string[] { "IS-INPUT::INT", "::Add", "Cancel" },
                                "Type in level ID"
                            );

                            c.Show();

                            c.FinishStr += resc => {
                                if (resc != "") {
                                    Elem.MsgBox LoadInfo = new Elem.MsgBox("Loading...");
                                    LoadInfo.Show();

                                    string r = GDTools.RequestGDLevel(resc);

                                    if (r.StartsWith("-"))
                                        MessageBox.Show($"Error: {GDTools.VerifyRequest(r)}", "Could not get level!");
                                    
                                    AddBase($"{resc} ({GDTools.GetRequestKey(r, "2")})");

                                    LoadInfo.Dispose();
                                }
                            };
                        }
                    };
                }));
                MergeControls.Controls.Add(new Elem.But("Add part(s)", (s, e) => {
                    Elem.ChooseForm Select = new Elem.ChooseForm("Select part source",
                    new string[] { "Add part(s) from file", "Add part(s) from local levels", "Add part(s) by ID" });

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
                        } else if (res == 1) {
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

                            Choose.ShowDialog(Select);
                        } else if (res == 2) {
                            Elem.ChooseForm c = new Elem.ChooseForm( "Type Level ID(s)", 
                                new string[] { "IS-INPUT-BIG::INS", "::Add", "Cancel" },
                                "Type in level ID(s) (Separated by spaces)"
                            );

                            c.Show();

                            c.FinishStr += resc => {
                                if (resc != "") {
                                    int i = 0;
                                    string[] rescs = resc.Split(" ");
                                    foreach (string ress in rescs) {
                                        i++;
                                        Elem.MsgBox LoadInfo = new Elem.MsgBox($"Loading ({i}/{rescs.Length})...");
                                        LoadInfo.Show();

                                        if (ress.Length < 3) {
                                            LoadInfo.Dispose();
                                            continue;
                                        }

                                        string r = GDTools.RequestGDLevel(ress);

                                        if (r.StartsWith("-")) {
                                            MessageBox.Show($"Error with {ress}: {GDTools.VerifyRequest(r)}", "Could not get level!");
                                            LoadInfo.Dispose();
                                            continue;
                                        }
                                        
                                        AddMerge($"{ress} ({GDTools.GetRequestKey(r, "2")})");

                                        LoadInfo.Dispose();
                                    }
                                }
                            };
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
                    
                CheckBox AutoReassignToggle = new CheckBox();
                AutoReassignToggle.Text = "Reassign groups & colours";
                AutoReassignToggle.AutoSize = true;
                AutoReassignToggle.Checked = AutoReassignGroups;
                AutoReassignToggle.Click += (s, e) =>
                    AutoReassignGroups = AutoReassignToggle.Checked;

                con.Controls.Add(new Elem.Text("Part merging"));
                con.Controls.Add(MergeList);
                con.Controls.Add(MergeBase);
                con.Controls.Add(MergeControls);
                con.Controls.Add(MergeLinkToggle);
                con.Controls.Add(AutoReassignToggle);

                this.Controls.Add(con);
                if (!Settings.DevMode) this.Controls.Add(new Elem.DevToolWarning((s, e) => { con.Visible = true; }));
            }

            public string MergeParts() {
                try {
                    if (MergeBase.Text == "Base not selected") return "No base selected!";

                    if (MergeList.Items.Count == 0) return "No parts to merge!";

                    List<string> parts = new List<string> {};
                    foreach (Elem.Select.SelectItem x in MergeList.Items) parts.Add(x.Text);
                    string err = GDTools.Merge(MergeBase.Text.Substring("Base: ".Length), parts, MergeLink, AutoReassignGroups);
                    if (err.Length > 0) return err;

                    MessageBox.Show("Succesfully merged! :)");

                    Program.MainForm.FullReload();

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