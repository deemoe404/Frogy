using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frogy.Resources.Theme
{
    class ThemeHelper
    {
        static private Dictionary<int, string> themeSets = new Dictionary<int, string>()
        {
            { 0, "Bright" },
            { 1, "Dark" }
        };
        static public Dictionary<int, string> ThemeSets
        {
            get { return themeSets; }
        }
    }
}
