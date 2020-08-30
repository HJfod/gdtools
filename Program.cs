using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace gdtools {
    public static class Settings {
        public static Size DefaultSize = new Size(1280, 720);
        public static string AppName = "GDTools";
    }

    static class Program {
        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( int dwProcessId );

        [STAThread]
        static void Main() {
            AttachConsole( -1 );
            
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Style.Init();

            Application.Run(new GDTools());
        }
    }
}
