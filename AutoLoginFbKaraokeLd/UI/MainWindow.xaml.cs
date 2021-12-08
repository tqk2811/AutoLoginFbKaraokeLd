using AutoLoginFbKaraokeLd.Queues;
using AutoLoginFbKaraokeLd.UI.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TqkLibrary.AdbDotNet.LdPlayer;
using TqkLibrary.Queues.TaskQueues;

namespace AutoLoginFbKaraokeLd.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Timer timer = new Timer(1000);
        readonly TaskQueue<WorkQueue> WorkQueues = new TaskQueue<WorkQueue>();
        readonly MainWVM mainWVM;
        public MainWindow()
        {
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + Singleton.LibFolder);
            InitializeComponent();
            this.mainWVM = this.DataContext as MainWVM;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = false;
            timer.Start();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Singleton.Setting.Setting.LdConsolePath))
                LdPlayer.LdConsolePath = Singleton.Setting.Setting.LdConsolePath;
            else
            {
                MessageBox.Show("Không tìm thấy ldconsole, hãy chọn đường dẫn", "Lỗi");
                SettingWindow settingWindow = new SettingWindow();
                settingWindow.Owner = this;
                settingWindow.ShowDialog();
            }
        }


        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if(File.Exists(LdPlayer.LdConsolePath))
                {
                    var list2 = LdPlayer.List2().ToList();
                    var current = mainWVM.LdProfiles.ToList();

                    var online = current.Where(x => list2.Where(y => y.AndroidStarted).Select(y => y.Title).Contains(x.Title)).ToList();
                    var offline = current.Except(online).ToList();

                    var newItem = list2.Where(x => !current.Any(y => x.Title.Equals(y.Title))).ToList();
                    var removeItem = current.Where(x => !x.IsBaseClone && string.IsNullOrEmpty(x.AccountName) && !list2.Select(y => y.Title).Contains(x.Title)).ToList();


                    online.ForEach(x => x.IsOpen = true);
                    offline.ForEach(x => x.IsOpen = false);

                    await Dispatcher.InvokeAsync(() =>
                    {
                        newItem.ForEach(x => mainWVM.LdProfiles.Add(new LdProfileVM(new LdProfileData() { Title = x.Title })));
                    });

                    await Dispatcher.InvokeAsync(() =>
                    {
                        removeItem.ForEach(x => mainWVM.LdProfiles.Remove(x));
                    });
                }
            }
            catch(Exception ex)
            {
                mainWVM.Logs.Add($"{ex.GetType().FullName}: {ex.Message}, {ex.StackTrace}");
            }
            finally
            {
                timer.Start();
            }
        }
        private void LV_Group_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                MenuVM menuViewModel = menuItem.DataContext as MenuVM;
                var selectedItems = lv_LdProfiles.SelectedItems.Cast<LdProfileVM>().ToList();
                switch (menuViewModel.Action)
                {
                    case MenuAction.ImportAccount:
                        {
                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Multiselect = false;
                            openFileDialog.Filter = "txt|*.txt|all file|*.*";
                            openFileDialog.InitialDirectory = Singleton.ExeDir;
                            if(openFileDialog.ShowDialog() == true)
                            {
                                var lines = File.ReadAllLines(openFileDialog.FileName)
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Select(x => x.Trim().Split('|'))
                                    .Where(x => x.Length == 3)
                                    .Where(x => !mainWVM.LdProfiles.Any(y => x[0].Equals(y.AccountName)))
                                    .Select(x => new FacebookAccount() { UserName = x[0], Password = x[1], TwoFA = x[2] })
                                    .ToList();
                                lines.ForEach(x => mainWVM.LdProfiles.Add(new LdProfileVM(new LdProfileData() { FacebookAccount = x, Title = $"Kara_{x.UserName}" })));
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
            }
        }
        private void btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.Owner = this;
            settingWindow.ShowDialog();
        }

        private void cb_IsBaseClone_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            LdProfileVM ldProfileVM = checkBox.DataContext as LdProfileVM;
            foreach (var profile in mainWVM.LdProfiles.Where(x => x != ldProfileVM)) profile.IsBaseClone = false;
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!WorkQueues.IsRunning)
                {
                    var baseClone = mainWVM.LdProfiles.FirstOrDefault(x => x.IsBaseClone);
                    if(baseClone == null)
                    {
                        MessageBox.Show("Chưa chọn ld gốc để clone");
                        return;
                    }

                    WorkQueues.ShutDown();

                    WorkQueues.AddRange(mainWVM.LdProfiles
                        .Where(x => x.IsChecked && !x.IsLogined && !x.IsBaseClone && !string.IsNullOrWhiteSpace(x.AccountName))
                        .Select(x => new WorkQueue(x, baseClone, (x) => mainWVM.Logs.Add(x))));

                    WorkQueues.MaxRun = Singleton.Setting.Setting.ThreadCount;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WorkQueues.ShutDown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
            }
        }
    }
}
