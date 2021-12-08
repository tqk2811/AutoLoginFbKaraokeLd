using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WinApi;

namespace AutoLoginFbKaraokeLd.DataClass
{
    internal class SettingData
    {
        public string LdConsolePath { get; set; }
        public double Percent { get; set; } = 0.8;
        public int FindImageTimeout { get; set; } = 40000;
        public int ThreadCount { get; set; } = 2;
        public ScreenShotType ScreenShotType { get; set; } = ScreenShotType.PrintWindow;
        public int DelayAfterStart { get; set; } = 6000;
    }
}
