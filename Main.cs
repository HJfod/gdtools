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
using System.IO;

namespace gdtools {
    public static class ExtensionMethods {
        public static void Clear(this Control.ControlCollection controls, bool dispose) {
            for (int ix = controls.Count - 1; ix >= 0; --ix)
                if (dispose) controls[ix].Dispose(); else controls.RemoveAt(ix);
        }

        public static T[] Red<T>(this T[] data, int index, int length) {
            T[] result = new T[length];
            if (data.Length < index + length)
                Array.Copy(data, index, result, 0, length - (data.Length - index));
            else
                Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static IEnumerable<TSource> SortLike<TSource,TKey>(this ICollection<TSource> source, IEnumerable<TKey> sortOrder) {
            var cloned = sortOrder.ToArray();
            var sourceArr = source.ToArray();
            Array.Sort(cloned, sourceArr);
            return sourceArr;
        }
    }

    public partial class Main : Form {
        public Main() {
            Settings.AppScale = ( (new Elem.NewLine()).CreateGraphics().DpiX / 96 );

            this.Text = $"{Settings.AppName} {Settings.AppVersion}";
            this.Size = new Size(
                Meth._S(Settings.DefaultSize.Width),
                Meth._S(Settings.DefaultSize.Height)
            );
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
            GDTools.LoadUserData();
            GDTools.DecodeCCFile(GDTools.GetCCPath("LocalLevels"), (string msg, int prog) => LoadInfo.Txt($"{msg} ({prog}%)"));
            GDTools.DecodeCCFile(GDTools.GetCCPath("GameManager"), (string msg, int prog) => LoadInfo.Txt($"{msg} ({prog}%)"));
            LoadInfo.Close();
            LoadInfo.Dispose();

            Program.CheckForUpdates(true);

            this.Init();
        }

        public void AwaitGDClose() {
            this.BeginInvoke((Action)(() => {
                this.Controls.Clear(true);

                this.Controls.Add(new Elem.Text("This app can not be used while GD is open.\r\n\r\nIt will automatically boot up once you close the game."));
            }));

            Task.Run<bool>(() => {
                while (GDTools.CheckIfGDIsOpen())
                    Thread.Sleep(GDTools._GDCheckLoopTime);
                
                this.BeginInvoke((Action)(() => this.FullReload() ));

                return true;
            });
        }

        public void FullReload() {
            this.Controls.Clear(true);
            this.DoLoad();
        }

        public void Reload() {
            this.Controls.Clear(true);
            this.Init();
        }

        private void Init() {
            this.BeginInvoke((Action)(() => {
                Meth.HandleTheme(this);

                Dotnetrix_Samples.TabControl Tabs = new Dotnetrix_Samples.TabControl();
                Tabs.Dock = DockStyle.Fill;
                Tabs.AutoSize = true;
                Meth.HandleTheme(Tabs);

                Panel[] Pages = new Panel[] {
                    new Pages.Home(),
                    new Pages.User(),
                    new Pages.Share(),
                    new Pages.Backups(),
                    new Pages.Collabs(),
                    new Pages.LevelEdit(),
                    new Pages.SettingPage()
                };
                
                foreach (Panel Page in Pages ) {
                    TabPage Tab = new TabPage();
                    Tab.Controls.Add(Page);
                    Tab.Text = Page.Name;
                    Meth.HandleTheme(Tab);
                    Tabs.Controls.Add(Tab);
                }

                TableLayoutPanel FileDropOverlay = new TableLayoutPanel();
                FileDropOverlay.Size = this.Size;
                FileDropOverlay.Location = new Point(0, 0);
                FileDropOverlay.Controls.Add(new Elem.Text("Drop imports / backups here!"));
                FileDropOverlay.Controls.Add(new Elem.BigNewLine());
                FileDropOverlay.Controls.Add(new Elem.Text($"Supported level formats: {GDTools.Ext.LevelList}\r\nSupported backup formats: Folder, .zip, {GDTools.Ext.Backup}"));
                FileDropOverlay.Visible = false;
                FileDropOverlay.BringToFront();

                this.Controls.Add(FileDropOverlay);
                this.Controls.Add(Tabs);

                this.DragEnter += (s, e) => {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                        e.Effect = DragDropEffects.All;
                        FileDropOverlay.Visible = true;
                    }
                };
                this.DragLeave += (s, e) => {
                    FileDropOverlay.Visible = false;
                };
                this.DragDrop += (s, e) => {
                    string[] drops = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                    foreach (string drop in drops) {
                        if (File.GetAttributes(drop).HasFlag(FileAttributes.Directory)) {
                            ((Pages.Backups)Pages[Array.FindIndex(Pages, x => x.Name == "Backups")]).ImportBackup(drop);
                        } else {
                            foreach (string Ext in GDTools.Ext.ExtArray.Levels)
                                if (drop.EndsWith(Ext))
                                    ((Pages.Share)Pages[Array.FindIndex(Pages, x => x.Name == "Sharing")]).AddImport(drop);
                            foreach (string Ext in GDTools.Ext.ExtArray.Backups)
                                if (drop.EndsWith(Ext))
                                    ((Pages.Backups)Pages[Array.FindIndex(Pages, x => x.Name == "Backups")]).ImportBackup(drop);
                        }
                    }
                    FileDropOverlay.Visible = false;
                };
                AllowDrop = true;

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
