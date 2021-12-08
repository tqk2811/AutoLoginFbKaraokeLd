using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.ObservableCollection;

namespace AutoLoginFbKaraokeLd.UI.ViewModels
{
    class FacebookAccount
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string TwoFA { get; set; }
    }
    class LdProfileData
    {
        public string Title { get; set; }

        public FacebookAccount FacebookAccount { get; set; }

        public bool IsLogined { get; set; } = false;
        public bool IsBaseClone { get; set; } = false;
        public bool IsChecked { get; set; } = true;
    }
    internal class LdProfileVM : BaseViewModel, IViewModel<LdProfileData>
    {
        public LdProfileVM(LdProfileData data)
        {
            this.Data = data;
        }
        public LdProfileData Data { get; }

        public event ChangeCallBack<LdProfileData> Change;

        public void Refresh()
        {
            NotifyPropertyChange(nameof(Title));
        }

        public string Title { get { return Data.Title; } }

        public string AccountName { get { return Data.FacebookAccount?.UserName; } }
        public bool IsChecked
        {
            get { return Data.IsChecked; }
            set { Data.IsChecked = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }
        public bool IsLogined
        {
            get { return Data.IsLogined; }
            set { Data.IsLogined = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }
        public bool IsBaseClone
        {
            get { return Data.IsBaseClone; }
            set { Data.IsBaseClone = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }



        bool _IsOpen = false;
        public bool IsOpen
        {
            get { return _IsOpen; }
            set { _IsOpen = value; NotifyPropertyChange(); }
        }


        bool _IsRunning = false;
        public bool IsRunning
        {
            get { return _IsRunning; }
            set { _IsRunning = value; NotifyPropertyChange(); }
        }

        string _Info = string.Empty;
        public string Info
        {
            get { return _Info; }
            set { _Info = value; NotifyPropertyChange(); }
        }

    }
}
