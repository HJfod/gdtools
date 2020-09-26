using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;

namespace gdtools {
    namespace Pages {
        public class LevelEdit : TableLayoutPanel {
            private TableLayoutPanel SelectPanel;
            private TableLayoutPanel EditPanel;
            private Elem.Text LevelName;
            private Elem.Select SelectLevel;
            public string SelectedLevel;
            private dynamic SelectedLevelContent;

            public LevelEdit() {
                this.Name = "Level";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                this.SelectPanel = new TableLayoutPanel();
                this.SelectPanel.AutoSize = true;

                this.SelectLevel = new Elem.Select(false);
                this.SelectLevel.DoubleClick += (s, e) => this.SelectLevelToEdit();

                foreach (dynamic lvl in GDTools.GetLevelList(null, null, true))
                    this.SelectLevel.AddItem(lvl.Name);
                
                this.SelectPanel.Controls.Add(SelectLevel);
                this.SelectPanel.Controls.Add(new Elem.But("Select", (s, e) => this.SelectLevelToEdit(), "Select a level to edit"));



                this.EditPanel = new TableLayoutPanel();
                this.EditPanel.Visible = false;
                this.EditPanel.AutoSize = true;

                this.EditPanel.Controls.Add(new Elem.But("⟵", (s, e) => {
                    this.SelectedLevelContent = new {};
                    this.SelectPanel.Visible = true;
                    this.EditPanel.Visible = false;
                }));

                this.EditPanel.Controls.Add(new Elem.NewLine());

                this.LevelName = new Elem.Text();
                this.EditPanel.Controls.Add(LevelName);
                this.EditPanel.Controls.Add(new Elem.BigNewLine());

                CheckBox ExportCompressed = new CheckBox();
                ExportCompressed.Text = "Export compressed (.gmdc)";
                ExportCompressed.AutoSize = true;

                this.EditPanel.Controls.Add(new Elem.But("Create guidelines from BPM", (s, e) => {
                    Form c = new Form();
                    c.Text = "Enter BPM";
                    Meth.HandleTheme(c);
                    
                    TableLayoutPanel con = new TableLayoutPanel();
                    con.AutoSize = true;

                    con.Controls.Add(new Elem.Input("__i_BPM", "INT", "BPM"));
                    con.Controls.Add(new Elem.Input("__i_OFFSET", "INT", "Offset (ms)"));
                    con.Controls.Add(new Elem.Input("__i_TIMESIG", "INT", "Time signature (x/4)"));
                    con.Controls.Add(new Elem.Input("__i_LENGTH", "INT", "Length (s)"));
                    
                    CheckBox KeepOldGuidelines = new CheckBox();
                    KeepOldGuidelines.Text = "Keep old guidelines";
                    KeepOldGuidelines.AutoSize = true;
                    KeepOldGuidelines.Checked = true;

                    con.Controls.Add(KeepOldGuidelines);

                    con.Controls.Add(new Elem.Div(new Control[] {
                        new Elem.But("Create", (s, e) => {
                            string iBPM = con.Controls.Find("__i_BPM", false)[0].Text;
                            string iOFF = con.Controls.Find("__i_OFFSET", false)[0].Text;
                            string iSIG = con.Controls.Find("__i_TIMESIG", false)[0].Text;
                            string iLGT = con.Controls.Find("__i_LENGTH", false)[0].Text;

                            if (iBPM.Length == 0 || iOFF.Length == 0 || iSIG.Length == 0 || iLGT.Length == 0) return;

                            float BPMmultiplier = 60F / Int32.Parse(iBPM);

                            int LineCount = Int32.Parse(iLGT) * Int16.Parse(iSIG);
                            int TimeSignature = Int16.Parse(iSIG);
                            float offset = Int32.Parse(iOFF) / 1000F;
                            string bpmData = "";

                            for (int i = 0; i < LineCount; i++)
                                bpmData += $"{Math.Round((float)i * BPMmultiplier + offset, 2)}~{(i % TimeSignature == 0 ? "1" : "0")}~";

                            string ndat = this.SelectedLevelContent.Data;
                            string dat = this.SelectedLevelContent.k4;

                            if (KeepOldGuidelines.Checked)
                                bpmData += $"{GDTools.GetStartKey(dat, "kA14")}~";

                            ndat = GDTools.SetKey(ndat, "k4", GDTools.SetStartKey(dat, "kA14", bpmData.Substring(0, bpmData.Length - 1)));

                            GDTools.UpdateLevel(ndat);

                            c.Dispose();

                            Program.MainForm.FullReload();
                        }),
                        new Elem.But("Cancel", (s, e) => c.Dispose())
                    }));

                    c.Controls.Add(con);

                    c.Show();
                }, "Used to generate automatic guidelines in the level out of the song's BPM to ease syncing."));

                this.EditPanel.Controls.Add(new Elem.But("Edit level properties", (s, e) => {
                    Elem.BasicForm c = new Elem.BasicForm();
                    c.Text = "Edit level properties";
                    c.Size = new Size(Meth._S(350),Meth._S(350));

                    string newData = this.SelectedLevelContent.Data;
                    
                    TableLayoutPanel con = new TableLayoutPanel();
                    con.AutoSize = true;

                    con.ColumnCount = 3;
                    con.RowCount = 6;

                    con.Controls.Add(new Elem.Text("Name: "), 0, 0);
                    con.Controls.Add(new Elem.Input("__L_NAME", "ANY", "", GDTools.GetKey(newData, "k2"), true), 1, 0);

                    con.Controls.Add(new Elem.Text("Creator: "), 0, 1);
                    con.Controls.Add(new Elem.Input("__L_CREATOR", "ANY", "", GDTools.GetKey(newData, "k5"), true), 1, 1);

                    con.Controls.Add(new Elem.Text("Password: "), 0, 2);
                    con.Controls.Add(new Elem.Input("__L_PASSWORD", "INT", "", GDTools.GetKey(newData, "k41"), true), 1, 2);

                    string desc = GDTools.GetKey(newData, "k3");
                    try { desc = Encoding.UTF8.GetString(GDTools.DecryptBase64(desc)); } catch (Exception) {};

                    con.Controls.Add(new Elem.Text("Description: "), 0, 3);
                    con.Controls.Add(new Elem.Input("__L_DESC", "ANY", "", desc, true), 1, 3);

                    con.Controls.Add(new Elem.Text(""), 0, 4);

                    con.Controls.Add(new Elem.But("Apply changes", (s, e) => {
                        newData = GDTools.SetKey(newData, "k2",     con.Controls.Find("__L_NAME", true)[0].Text);
                        newData = GDTools.SetKey(newData, "k5",     con.Controls.Find("__L_CREATOR", true)[0].Text);
                        newData = GDTools.SetKey(newData, "k41",    con.Controls.Find("__L_PASSWORD", true)[0].Text);
                        newData = GDTools.SetKey(newData, "k3",     GDTools.EncryptBase64(Encoding.UTF8.GetBytes(con.Controls.Find("__L_DESC", true)[0].Text)));

                        GDTools.UpdateLevel(newData.Replace(System.Environment.NewLine, ""));

                        c.Dispose();

                        Program.MainForm.FullReload();
                    }), 0, 5);

                    c.Controls.Add(con);

                    c.Show();
                }, "Edit level properties such as name, description, etc. (Hint: You can add newlines to descriptions!)"));

                this.EditPanel.Controls.Add(new Elem.But("View level info", (s, e) => {
                    dynamic LevelInfo = GDTools.GetLevelInfo(this.SelectedLevel);
                        
                    string Info = "";

                    foreach (PropertyInfo i in LevelInfo.GetType().GetProperties()) 
                        Info += $"{i.Name.Replace("_", " ")}: {i.GetValue(LevelInfo)}\n";

                    MessageBox.Show(Info, $"Info for {this.SelectedLevel}", MessageBoxButtons.OK, MessageBoxIcon.None);
                }, "View info about the selected level."));

                this.EditPanel.Controls.Add(new Elem.But("Export this level", (s, e) => {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    DialogResult dr = fbd.ShowDialog();

                    if (dr == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                        string ExportTry = GDTools.ExportLevel(this.SelectedLevel, fbd.SelectedPath, ExportCompressed.Checked);
                        if (ExportTry != null) {
                            MessageBox.Show($"Error: {ExportTry}");
                        } else {
                            MessageBox.Show("Successfully exported!");
                        }
                    } else if (dr != DialogResult.Cancel) {
                        MessageBox.Show("Selected path not accepted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }, "Export the selected level"));

                this.EditPanel.Controls.Add(ExportCompressed);

                this.Controls.Add(SelectPanel);
                this.Controls.Add(EditPanel);
            }

            private bool SelectLevelToEdit() {
                if (this.SelectLevel.SelectedItem == null) return false;
                this.SelectedLevel = ((Elem.Select.SelectItem)this.SelectLevel.SelectedItem).Text;

                string dat = GDTools.GetLevelData(this.SelectedLevel);
                this.SelectedLevelContent = new {
                    Data = dat,
                    k4 = GDTools.DecodeLevelData(GDTools.GetKey(dat, "k4"))
                };
                
                this.LevelName.Text = $"Editing {this.SelectedLevel}";

                this.SelectPanel.Visible = false;
                this.EditPanel.Visible = true;

                return true;
            }
        }
    }
}