using AutoLoginFbKaraokeLd.DataClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.AdbDotNet.LdPlayer;
using TqkLibrary.WinApi;
using TqkLibrary.WpfUi;

namespace AutoLoginFbKaraokeLd.UI.ViewModels
{
    internal class SettingWVM : BaseViewModel
    {
        SettingData Setting { get { return Singleton.Setting.Setting; } }
        void Save() => Singleton.Setting.Save();

        public string LdConsolePath
        {
            get { return Setting.LdConsolePath; }
            set
            {
                Setting.LdConsolePath = value;
                NotifyPropertyChange();
                if (File.Exists(value)) LdPlayer.LdConsolePath = value;
                Save();
            }
        }

        public double Percent
        {
            get { return Setting.Percent; }
            set { Setting.Percent = value; NotifyPropertyChange(); Save(); }
        }

        public int FindImageTimeout
        {
            get { return Setting.FindImageTimeout; }
            set { Setting.FindImageTimeout = value; NotifyPropertyChange(); Save(); }
        }
        public int DelayAfterStart
        {
            get { return Setting.DelayAfterStart; }
            set { Setting.DelayAfterStart = value; NotifyPropertyChange(); Save(); }
        }
        public ScreenShotType ScreenShotType
        {
            get { return Setting.ScreenShotType; }
            set { Setting.ScreenShotType = value; NotifyPropertyChange(); Save(); }
        }
        public List<ScreenShotType> ScreenShotTypes { get; } = new List<ScreenShotType>()
        {
            ScreenShotType.BitBlt,
            ScreenShotType.PrintWindow
        };

    }
}
