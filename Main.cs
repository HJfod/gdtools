using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdtools {
    public partial class Main : Form {
        public static (bool, bool) HasDecodedCC = (false, false);

        public Main() {
            Elem.MsgBox LoadInfo = new Elem.MsgBox("Loading...");
            LoadInfo.Show();
            GDTools.DecodeCCFile(GDTools.GetCCPath("LocalLevels"), (string msg, int prog) => LoadInfo.Txt($"{msg} ({prog}%)"));
            GDTools.DecodeCCFile(GDTools.GetCCPath("GameManager"), (string msg, int prog) => LoadInfo.Txt($"{msg} ({prog}%)"));
            GDTools.LoadUserData();
            LoadInfo.Close();
            LoadInfo.Dispose();
            
            this.Text = $"{Settings.AppName} {Settings.AppVersion}";
            this.Size = Settings.DefaultSize;

            TabControl Tabs = new TabControl();
            Tabs.Dock = DockStyle.Fill;
            Tabs.AutoSize = true;

            foreach (Panel Page in new Panel[] {
                new Pages.Home(),
                new Pages.Share(),
                new Pages.Backups()
            }) {
                TabPage Tab = new TabPage();
                Tab.Controls.Add(Page);
                Tab.Text = Page.Name;
                Tabs.Controls.Add(Tab);
            }

            this.Controls.Add(Tabs);
            this.CenterToScreen();
        }
    }
}
