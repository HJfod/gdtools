using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace gdtools {
    public static class Settings {
        public static Size DefaultSize = new Size(450, 550);
        public static string AppName = "GDTools";
        public static string AppVersion = "v0.1.2";
        public static string AppBuild = "DEV-BUILD";
        public static string Developers = "HJfod";
        public static string IconPath = "gdtools.ico";
        public static int AppVersionNum = 3;
        public static float AppScale = 1;
        public static bool DarkTheme = false;
        public static bool CompressBackups = true;
        public static bool DevMode = false;
    }

    public static class Program {
        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( int dwProcessId );

        public static Main MainForm = new gdtools.Main();
        
        public static readonly HttpClient HClient = new HttpClient();

        private static int ConvertVer(string ver) {
            string res = "";
            foreach (Match m in Regex.Matches(ver, "[0-9]+", RegexOptions.None, Regex.InfiniteMatchTimeout))
                res += m.Value;
            return Int32.Parse(res);
        }

        public static void CheckForUpdates(bool _startup = false) {
            try {
                string url = "https://api.github.com/repos/HJfod/gdtools/releases/latest";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "request";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK) {
                    throw new Exception("Can't connect to Github! HTTPS Response Code: " + response.StatusCode);
                }

                Stream resStream = response.GetResponseStream();
                StreamReader streamRead = new StreamReader( resStream );

                string msg = "";

                Char[] buffer = new Char[256];
                int count = streamRead.Read( buffer, 0, 256 );

                while (count > 0) {
                    msg += new String(buffer, 0, count);
                    count = streamRead.Read(buffer, 0, 256);
                }

                streamRead.Close();
                resStream.Close();
                response.Close();

                string tag = Regex.Match(msg, $"\"tag_name\":\".*?\"", RegexOptions.None, Regex.InfiniteMatchTimeout).Value;
                if (tag == null || tag == "") throw new Exception("Could not find tag_name!");
                tag = tag.Substring(tag.IndexOf(":"));
                tag = tag.Substring(tag.IndexOf('"') + 1, tag.LastIndexOf('"') - 1);
                
                int vern = ConvertVer(tag);
                int vero = ConvertVer(Settings.AppVersion);
                if (vero == vern) {
                    if (!_startup)
                        MessageBox.Show("You are up to date!", "Version check");
                } else if (vero < vern) {
                    MessageBox.Show($"A new version {(_startup ? $"of {Settings.AppName} " : "")}is available!", "Version check");
                    Process.Start("explorer.exe", "https://github.com/HJfod/gdtools/releases/latest");
                } else if (vero > vern) {
                    if (!_startup)
                        MessageBox.Show("You are using a newer versin than last stable release.", "Version check");
                }
            } catch (Exception err) {
                MessageBox.Show($"Error: {err}", "Error");
            }
        }

        [STAThread]
        static void Main() {
            AttachConsole(-1);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(MainForm);
        }
    }
}