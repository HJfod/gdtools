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
                    Elem.ChooseForm c = new Elem.ChooseForm("Enter BPM", new string[] { "IS-INPUT::INT", "::Set", "Cancel" });

                    c.Show();

                    c.FinishStr += res => {
                        Console.WriteLine(res);
/*
                        string dat = GDTools.DecodeLevelData(
                            GDTools.GetKey(
                                GDTools.GetLevelData(this.SelectedLevel), "k4")
                        );
                        string ndat = GDTools.GetLevelData(this.SelectedLevel);

                        ndat = GDTools.SetKey(GDTools.SetKey(ndat, "k4", GDTools.SetStartKey(dat, "kA14", "0.00~0~1.00~0~2.00~0~3.00~1")), "k2", "@@TEST@@");

                        ndat = Regex.Replace(ndat, @"<k>k_\d+<\/k>", "");

                        Console.WriteLine(ndat);

                        GDTools.ImportLevel(ndat, true); //*/
                    };
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