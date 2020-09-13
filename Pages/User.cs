using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace gdtools {
    namespace Pages {
        public class User : TableLayoutPanel {
            public User() {
                this.Name = "User";
                this.Dock = DockStyle.Fill;
                Meth.HandleTheme(this);

                dynamic UserInfo = GDTools.GetGDUserInfo(null);

                string Stats = "";

                foreach (PropertyInfo i in UserInfo.Stats.GetType().GetProperties())
                    Stats += $"{i.Name.Replace("_", " ")}: {i.GetValue(UserInfo.Stats)}\n";

                this.Controls.Add(new Elem.Text($"Logged in as {UserInfo.Name} | UserID {UserInfo.UserID}"));
                this.Controls.Add(new Elem.BigNewLine());
                this.Controls.Add(new Elem.Text(Stats));
            }
        }
    }
}