using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;

namespace gdtools {
    namespace Pages {
        public class LevelEdit : TableLayoutPanel {
            private TableLayoutPanel SelectPanel;
            private TableLayoutPanel EditPanel;
            private Elem.Text LevelName;
            private Elem.Select SelectLevel;
            public string SelectedLevel;

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
                this.SelectPanel.Controls.Add(new Elem.But("Select", (s, e) => this.SelectLevelToEdit()));



                this.EditPanel = new TableLayoutPanel();
                this.EditPanel.Visible = false;
                this.EditPanel.AutoSize = true;

                this.EditPanel.Controls.Add(new Elem.But("Back", (s, e) => {
                    SelectPanel.Visible = true;
                    EditPanel.Visible = false;
                }));

                this.EditPanel.Controls.Add(new Elem.NewLine());

                this.LevelName = new Elem.Text();
                this.EditPanel.Controls.Add(LevelName);
                
                this.EditPanel.Controls.Add(new Elem.But("View level code", (s, e) => {
                    MessageBox.Show(
                        GDTools.GetKey(
                            GDTools.GetLevelData(this.SelectedLevel),
                        "k4")
                    );
                }));

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

                            string ndat = GDTools.GetLevelData(this.SelectedLevel);
                            string dat = GDTools.DecodeLevelData(GDTools.GetKey(ndat, "k4"));

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
                }));

                this.EditPanel.Controls.Add(new Elem.But("Edit level properties", (s, e) => {
                    // edit name, creator, etc.
                }));

                this.Controls.Add(SelectPanel);
                this.Controls.Add(EditPanel);
            }

            private bool SelectLevelToEdit() {
                if (SelectLevel.SelectedItem == null) return false;
                this.SelectedLevel = ((Elem.Select.SelectItem)this.SelectLevel.SelectedItem).Text;
                
                this.LevelName.Text = $"Editing {this.SelectedLevel}";

                SelectPanel.Visible = false;
                EditPanel.Visible = true;

                return true;
            }
        }
    }
}