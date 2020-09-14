using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace gdtools {
    public static class Settings {
        public static Size DefaultSize = new Size(400, 450);
        public static string AppName = "GDTools";
        public static string AppVersion = "v0.1.0";
        public static string AppBuild = "REL-BUILD";
        public static string Developers = "HJfod";
        public static string IconPath = "gdtools.ico";
        public static int AppVersionNum = 1;
        public static bool DarkTheme = false;
        public static bool CompressBackups = true;
    }

    public static class Program {
        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( int dwProcessId );

        public static Main MainForm = new gdtools.Main();

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