using AutoLoginFbKaraokeLd.UI.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoLoginFbKaraokeLd.UI
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        readonly SettingWVM settingWVM;
        public SettingWindow()
        {
            InitializeComponent();
            settingWVM = this.DataContext as SettingWVM;
        }

        private void btn_loadLdConsole_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "ldconsole.exe|ldconsole.exe|all file|*.*",
                Multiselect = false,
                InitialDirectory = Singleton.ExeDir,

            };
            if(openFileDialog.ShowDialog() == true)
            {
                settingWVM.LdConsolePath = openFileDialog.FileName;
            }
        }
    }
}
