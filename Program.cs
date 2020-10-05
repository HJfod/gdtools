using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Net.Http;

namespace gdtools {
    public static class Settings {
        public static Size DefaultSize = new Size(450, 550);
        public static string AppName = "GDTools";
        public static string AppVersion = "v0.1.1";
        public static string AppBuild = "DEV-BUILD";
        public static string Developers = "HJfod";
        public static string IconPath = "gdtools.ico";
        public static int AppVersionNum = 2;
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