using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

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

                this.EditPanel.Controls.Add(new Elem.But("Generate T4 glow", (s, e) => {
                    Elem.BasicForm c = new Elem.BasicForm();
                    c.Text = "Generate T4 glow";
                    c.Size = new Size(Meth._S(350),Meth._S(350));

                    string newData = this.SelectedLevelContent.Data;
                    
                    TableLayoutPanel con = new TableLayoutPanel();
                    con.AutoSize = true;

                    con.ColumnCount = 3;
                    con.RowCount = 7;

                    con.Controls.Add(new Elem.Text("X position: "), 0, 0);
                    con.Controls.Add(new Elem.Input("__L_X", "INT", "", "0"), 1, 0);

                    con.Controls.Add(new Elem.Text("Y position: "), 0, 1);
                    con.Controls.Add(new Elem.Input("__L_Y", "INT", "", "0"), 1, 1);

                    con.Controls.Add(new Elem.Text("Scale: "), 0, 2);
                    con.Controls.Add(new Elem.Input("__L_S", "FLT", "", "1"), 1, 2);

                    con.Controls.Add(new Elem.Text(""), 0, 4);

                    con.Controls.Add(new Elem.But("Generate", (s, e) => {
                        string obj = GDTools.DecodeLevelData(GDTools.GetKey(newData, "k4"));

                        obj += $"1,1888,2,{con.Controls.Find("__L_X", true)[0].Text},3,{con.Controls.Find("__L_Y", true)[0].Text},24,10,32,{con.Controls.Find("__L_S", true)[0].Text};";

                        newData = GDTools.SetKey(newData, "k4", GDTools.EncodeLevelData(obj));

                        GDTools.UpdateLevel(newData);

                        c.Dispose();

                        Program.MainForm.FullReload();
                    }), 0, 5);

                    con.Controls.Add(new Elem.Text("T4 glow is Blending glow that can be put above T3 solid objects.\r\n\r\nYou have to give the glow a color that has Blending enabled.", new Size(150, 0)), 0, 6);

                    c.Controls.Add(con);

                    c.Show();
                }, "Generates a piece of T4 glow at the start of the level."));

                this.EditPanel.Controls.Add(new Elem.But("Edit level data", (s, e) => {
                    Elem.BasicForm DataEditor = new Elem.BasicForm();
                    DataEditor.Size = new Size(800, 600);

                    int ControlsHeight = 50;

                    TableLayoutPanel DataEditorMain = new TableLayoutPanel();
                    DataEditorMain.AutoSize = true;
                    DataEditorMain.Dock = DockStyle.Fill;

                    TableLayoutPanel DataEditorEditor = new TableLayoutPanel();
                    DataEditorEditor.ColumnCount = 2;
                    DataEditorEditor.Dock = DockStyle.Fill;
                    DataEditorEditor.AutoScroll = true;
                    DataEditorEditor.Size = new Size(DataEditor.Size.Width - 30, DataEditor.Size.Height - ControlsHeight - 100);

                    FlowLayoutPanel DataEditorControls = new FlowLayoutPanel();
                    DataEditorControls.AutoSize = true;
                    DataEditorControls.Dock = DockStyle.Bottom;
                    DataEditorControls.Height = ControlsHeight;

                    string[] objs = this.SelectedLevelContent.k4.Split(";");
                    int page = 0;
                    int show = 20;

                    Elem.Text PageNum = new Elem.Text();
                    Elem.Text PageAmt = new Elem.Text();

                    void PageUpdate() {
                        DataEditorEditor.Controls.Clear(true);

                        PageNum.Text = $"Page: {page / show + 1} / {objs.Length / show + 1}";
                        PageAmt.Text = $"Amount of object(s) shown on page: {show}";
                        
                        int i = 0;
                        foreach (string obj in objs.Red(page, show)) {
                            if (obj == null) continue;
                            DataEditorEditor.Controls.Add(new Elem.Text($"{page + i}:"), 0, i);
                            DataEditorEditor.Controls.Add(new Elem.Text($"{obj}"), 1, i);

                            i++;
                        }
                    };

                    PageUpdate();

                    DataEditorControls.Controls.Add(PageNum);
                    DataEditorControls.Controls.Add(PageAmt);
                    DataEditorControls.Controls.Add(new Elem.But("Next", (s, e) => {
                        if (page < objs.Length) { page += show; PageUpdate(); };
                    }));
                    DataEditorControls.Controls.Add(new Elem.But("Previous", (s, e) => {
                        if (page > 0) { page -= show; PageUpdate(); };
                    }));
                    DataEditorControls.Controls.Add(new Elem.But("Go to page", (s, e) => {
                        Elem.ChooseForm c = new Elem.ChooseForm( "Go to page", 
                            new string[] { "IS-INPUT::INT", "::Go", "Cancel" }
                        );

                        c.Show();

                        c.FinishStr += resc => {
                            if (resc != "") {
                                int np = Int32.Parse(resc) - 1;
                                if (np >= 0 && np <= objs.Length / show)
                                    page = np * show;

                                PageUpdate();
                                c.Dispose();
                            }
                        };
                    }));

                    DataEditorMain.Controls.Add(DataEditorEditor);
                    DataEditorMain.Controls.Add(DataEditorControls);
                    DataEditor.Controls.Add(DataEditorMain);

                    DataEditor.Show();
                }));

                this.EditPanel.Controls.Add(new Elem.But("Rearrange Groups And Colours", (s, e) => {
                    Elem.BasicForm c = new Elem.BasicForm();
                    c.Text = "Rearrange Groups And Colours";
                    c.Size = new Size(Meth._S(350),Meth._S(250));

                    string newData = this.SelectedLevelContent.k4;
                    
                    TableLayoutPanel con = new TableLayoutPanel();
                    con.AutoSize = true;

                    con.ColumnCount = 3;
                    con.RowCount = 7;

                    con.Controls.Add(new Elem.Text("Group Range"), 0, 0);
                    con.Controls.Add(new Elem.Input("__G_RANGE", "ANY", "", "x - y", false, false), 0, 1);

                    con.Controls.Add(new Elem.Text("Offset"), 1, 0);
                    con.Controls.Add(new Elem.Input("__G_OFF", "INN", "", "+0", false, false), 1, 1);

                    con.Controls.Add(new Elem.Text(""), 0, 2);

                    con.Controls.Add(new Elem.Text("Color Range"), 0, 3);
                    con.Controls.Add(new Elem.Input("__C_RANGE", "ANY", "", "x - y", false, false), 0, 4);

                    con.Controls.Add(new Elem.Text("Offset"), 1, 3);
                    con.Controls.Add(new Elem.Input("__C_OFF", "INN", "", "+0", false, false), 1, 4);

                    con.Controls.Add(new Elem.Text(""), 0, 5);

                    con.Controls.Add(new Elem.But("Apply changes", (s, e) => {
                        try {
                            string grang = con.Controls.Find("__G_RANGE", true)[0].Text.Trim();
                            if (!Regex.IsMatch(grang, @"\d+\s*\-\s*\d+")) throw new Exception("Invalid input in group range!");

                            string goff = con.Controls.Find("__G_OFF", true)[0].Text.Trim();
                            if (!Regex.IsMatch(goff, @"[+|-]?\s*\d+")) throw new Exception("Invalid input in group offset!");

                            string crang = con.Controls.Find("__C_RANGE", true)[0].Text.Trim();
                            if (!Regex.IsMatch(crang, @"\d+\s*\-\s*\d+")) throw new Exception("Invalid input in color range!");

                            string coff = con.Controls.Find("__C_OFF", true)[0].Text.Trim();
                            if (!Regex.IsMatch(coff, @"[+|-]?\s*\d+")) throw new Exception("Invalid input in color offset!");

                            // groups are separated like a.b.c.d.e
                            // colors are just the color id

                            // things you need to affect: color triggers, pulse triggers, literally every trigger that takes groups

                            foreach (dynamic obj in GDTools.GetObjectsByKey(newData, "ARR_21_22_57"))
                                Console.WriteLine(obj.Data);
                        } catch (Exception err) {
                            MessageBox.Show($"Error: {err}", "Error");
                        }
                    }), 0, 6);

                    con.Controls.Add(new Elem.But("Cancel", (s, e) => c.Dispose()), 1, 6);

                    c.Controls.Add(con);

                    c.Show();
                }));

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