using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using System.Dynamic;

namespace gdtools {
    public static class Style {
        public static int DraggerWidth = 4;
        public static int TitlebarSize = 30;
        public static int ResizeDrag = 15;
        public static int TabSize = 40;
        public static int TabSideSize = 6;
        public static int PaddingSize = 15;
        public static Padding Padding = new Padding(PaddingSize);

        public static dynamic Colors = new ExpandoObject();

        public static void LoadStyle(dynamic list) {
                foreach (PropertyInfo i in list.GetType().GetProperties()) {
                    ((IDictionary<String, Object>)Colors)[i.Name] = (Color)ColorTranslator.FromHtml(i.GetValue(list));
                }
        }

        public static void Init() {
            LoadStyle(DefaultStyle.list);
        }

        public static Color Color(string hex) {
            return ColorTranslator.FromHtml(hex);
        }
    }
}