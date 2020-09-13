using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdtools {
    public partial class Main : Form {
        public Main() {
            this.Text = $"{Settings.AppName} {Settings.AppVersion}";
            this.Size = Settings.DefaultSize;
            this.Icon = new Icon(Settings.IconPath);

            this.CenterToScreen();

            if (GDTools.CheckIfGDIsOpen()) {
                Elem.PauseForm PauseForm = new Elem.PauseForm();

                PauseForm.Show();

                Task.Run<bool>(() => {
                    while (GDTools.CheckIfGDIsOpen()) {
                        Thread.Sleep(GDTools._GDCheckLoopTime);
                    }

                    PauseForm.Close();
                    PauseForm.Dispose();

                    this.DoLoad();

                    return true;
                });
            } else {
                this.DoLoad();
            }
        }

        public void DoLoad() {
            Elem.MsgBox LoadInfo = new Elem.MsgBox("Loading...");
            LoadInfo.Show();
            GDTools.DecodeCCFile(GDTools.GetCCPath("LocalLevels"), (string msg, int prog) => LoadInfo.Txt($"{msg} ({prog}%)"));
            GDTools.DecodeCCFile(GDTools.GetCCPath("GameManager"), (string msg, int prog) => LoadInfo.Txt($"{msg} ({prog}%)"));
            GDTools.LoadUserData();
            LoadInfo.Close();
            LoadInfo.Dispose();

            this.Init();
        }

        public void AwaitGDClose() {
            this.BeginInvoke((Action)(() => {
                this.Controls.Clear();

                this.Controls.Add(new Elem.Text("This app can not be used while GD is open.\r\n\r\nIt will automatically boot up once you close the game."));
            }));

            Task.Run<bool>(() => {
                while (GDTools.CheckIfGDIsOpen()) {
                    Thread.Sleep(GDTools._GDCheckLoopTime);
                }
                
                this.BeginInvoke((Action)(() => {
                    this.FullReload();
                }));

                return true;
            });
        }

        public void FullReload() {
            this.Controls.Clear();
            this.DoLoad();
        }

        public void Reload() {
            this.Controls.Clear();
            this.Init();
        }

        private void Init() {
            this.BeginInvoke((Action)(() => {
                Meth.HandleTheme(this);

                Dotnetrix_Samples.TabControl Tabs = new Dotnetrix_Samples.TabControl();
                Tabs.Dock = DockStyle.Fill;
                Tabs.AutoSize = true;
                Meth.HandleTheme(Tabs);
                
                foreach (Panel Page in new Panel[] {
                    new Pages.Home(),
                    new Pages.User(),
                    new Pages.Share(),
                    new Pages.Backups(),
                    new Pages.SettingPage()
                }) {
                    TabPage Tab = new TabPage();
                    Tab.Controls.Add(Page);
                    Tab.Text = Page.Name;
                    Meth.HandleTheme(Tab);
                    Tabs.Controls.Add(Tab);
                }

                this.Controls.Add(Tabs);

                Task.Run<bool>(() => {
                    while (!GDTools.CheckIfGDIsOpen()) {
                        Thread.Sleep(GDTools._GDCheckLoopTime);
                    }

                    this.AwaitGDClose();

                    return true;
                });
            }));
        }
    }
}
