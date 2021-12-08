using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.ObservableCollection;

namespace AutoLoginFbKaraokeLd.UI.ViewModels
{
    internal class MainWVM : BaseViewModel
    {
        public int ThreadCount
        {
            get { return Singleton.Setting.Setting.ThreadCount; }
            set { Singleton.Setting.Setting.ThreadCount = value; NotifyPropertyChange(); Singleton.Setting.Save(); }
        }

        public List<MenuVM> LdProfilesMenu { get; } = new List<MenuVM>()
        {
            new MenuVM(MenuAction.ImportAccount)
        };
        public SaveObservableCollection<LdProfileData, LdProfileVM> LdProfiles { get; }
            = new SaveObservableCollection<LdProfileData, LdProfileVM>(Singleton.ExeDir + "\\LdProfiles.json", x => new LdProfileVM(x));
        public LimitObservableCollection<string> Logs { get; } 
            = new LimitObservableCollection<string>(() => Singleton.LogDir + $"\\{DateTime.Now:yyyy-MM-dd}.log");

        bool _IsCheckedAll = false;
        public bool IsCheckedAll
        {
            get { return _IsCheckedAll; }
            set
            {
                _IsCheckedAll = value;
                NotifyPropertyChange();
                if(value)
                    foreach (var profile in LdProfiles.Where(x => !x.IsBaseClone && !string.IsNullOrWhiteSpace(x.AccountName))) profile.IsChecked = value;
                else
                    foreach (var profile in LdProfiles) profile.IsChecked = value;
            }
        }
    }
}
