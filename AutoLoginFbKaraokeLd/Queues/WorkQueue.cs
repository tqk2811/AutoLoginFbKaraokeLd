using AutoLoginFbKaraokeLd.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.Media.Images;
using TqkLibrary.WinApi;
using AutoLoginFbKaraokeLd.DataClass;
using System.Threading;
using System.Drawing;
using TqkLibrary.AdbDotNet.LdPlayer;
using TwoFactorAuthNet;

namespace AutoLoginFbKaraokeLd.Queues
{
    internal class WorkQueue : IQueue
    {
        SettingData Setting { get { return Singleton.Setting.Setting; } }

        static readonly ImageTemplateHelper imageTemplateHelper = new ImageTemplateHelper(Singleton.ImageDir);
        static readonly ImageCropHelper cropHelper = new ImageCropHelper(new Dictionary<string, Rectangle>()
        {
            { "abc", new Rectangle() }
        });
        static readonly TwoFactorAuth twoFactorAuth = new TwoFactorAuth();

        readonly LdProfileVM LdProfile;
        readonly Action<string> logCallback;
        readonly WaitImageHelper waiter;
        readonly LdProfileVM baseClone;
        readonly LdPlayer ldPlayer;

        LdPlayerHelper ldPlayerHelper;
        public WorkQueue(LdProfileVM ldProfile, LdProfileVM baseClone, Action<string> logCallback)
        {
            this.LdProfile = ldProfile ?? throw new ArgumentNullException(nameof(ldProfile));
            this.logCallback = logCallback ?? throw new ArgumentNullException(nameof(logCallback));
            this.baseClone = baseClone ?? throw new ArgumentNullException(nameof(baseClone));

            ldPlayer = new LdPlayer(ldProfile.Title);
            waiter = new WaitImageHelper(
                () => ldPlayerHelper?.ScreenShot(Setting.ScreenShotType),
                (name, index) => imageTemplateHelper.GetImage(name,index),
                (name) => cropHelper.GetCrop(name),
                () => Setting.Percent,
                () => Setting.FindImageTimeout);
            waiter.LogCallback += WriteLog;
        }
        ~WorkQueue()
        {
            waiter.Dispose();
            ldPlayer.Dispose();
        }

        void WriteLog(string text)
        {
            logCallback.Invoke($"{DateTime.Now:HH:mm:ss.fff} {LdProfile.Title}: {text}");
        }

        static readonly object lock_copy = new object();
        static readonly object lock_start = new object();



        public async Task DoWork()
        {
            using var register = ldPlayer.Adb.CancellationToken.Register(() => waiter.CancellationTokenSource.Cancel());
            try
            {
                LdProfile.IsRunning = true;
                var ldlist2 = LdPlayer.List2().FirstOrDefault(x => x.Title.Equals(LdProfile.Title));
                if(ldlist2 == null)//clone
                {
                    lock (lock_copy)
                    {
                        WriteLog($"Clone: {baseClone.Title} -> {LdProfile.Title}");
                        LdPlayer.Copy(baseClone.Title, LdProfile.Title, CancellationToken.None, ldPlayer.Adb.CancellationToken);
                        WriteLog($"Clone {LdProfile.Title} Done");
                    }
                    await ldPlayer.Adb.DelayAsync(3000);
                }

                if(LdPlayer.List2().FirstOrDefault(x => x.Title.Equals(LdProfile.Title))?.AndroidStarted != true)
                {
                    lock (lock_start)
                    {
                        WriteLog($"Launch {LdProfile.Title}");
                        ldPlayer.Launch();
                        while(LdPlayer.List2().FirstOrDefault(x => x.Title.Equals(LdProfile.Title))?.AndroidStarted != true)
                        {
                            ldPlayer.Adb.Delay(1000);
                        }
                    }
                    await ldPlayer.Adb.DelayAsync(Setting.DelayAfterStart);
                }

               
                ldlist2 = LdPlayer.List2().FirstOrDefault(x => x.Title.Equals(LdProfile.Title));
                ldPlayerHelper = new LdPlayerHelper(ldlist2.TopWindowHandle, ldlist2.BindWindowHandle);
                ldPlayerHelper.Resize(new Size(540 + 40, 960 + 34));

                ldPlayer.RunApp("com.km.karaoke");

                waiter.WaitUntil("kara_fbLogo", "android_allow").AndTapFirst((index, point, finds) =>
                {
                    switch (finds[index])
                    {
                        case "kara_fbLogo":
                            ldPlayerHelper.Tap(point);
                            return false;

                        default:
                            ldPlayerHelper.Tap(point);
                            return true;
                    }
                }).WithThrow().Build();
                waiter.WaitUntil("fb_phoneOrEmail").AndTapFirst(Tap).WithThrow().Build();
                await ldPlayer.Adb.DelayAsync(Setting.DelayBeforeWriteText);
                ldPlayerHelper.SendText(LdProfile.AccountName, Setting.DelayStepWriteText);

                waiter.WaitUntil("fb_password").AndTapFirst(Tap).WithThrow().Build();
                await ldPlayer.Adb.DelayAsync(Setting.DelayBeforeWriteText);
                ldPlayerHelper.SendText(LdProfile.Data.FacebookAccount.Password, Setting.DelayStepWriteText);

                waiter.WaitUntil("fb_loginBtn").AndTapFirst(Tap).WithThrow().Build();
                waiter.WaitUntil("fb_okPopup").AndTapFirst(Tap).WithThrow().Build();

                waiter.WaitUntil("fb_twoFaInput").AndTapFirst(Tap).WithThrow().Build();
                await ldPlayer.Adb.DelayAsync(Setting.DelayBeforeWriteText);
                string facode = twoFactorAuth.GetCode(LdProfile.Data.FacebookAccount.TwoFA);
                ldPlayerHelper.SendText(facode, Setting.DelayStepWriteText);

                waiter.WaitUntil("fb_authContinue").AndTapFirst(Tap).WithThrow().Build();
                waiter.WaitUntil("kara_home", "fb_authContinue").AndTapFirst((index,point,finds) =>
                {
                    switch(finds[index])
                    {
                        case "kara_home":
                            return false;


                        default:
                            ldPlayerHelper.Tap(point);
                            break;
                    }
                    return true;
                }).WithThrow().Build();
                LdProfile.IsLogined = true;
                ldPlayer.Adb.Delay(2000);
            }
            catch(Exception ex)
            {
                if (ex is AggregateException ae) ex = ae.InnerException;
                if(ex is OperationCanceledException oce)
                {
                    WriteLog("Cancel");
                    return;
                }
                WriteLog($"{ex.GetType().FullName}: {ex.Message}, {ex.StackTrace}");
            }
            finally
            {
                ldPlayer?.Quit();
                LdProfile.IsRunning = false;
            }
        }

        bool Tap(int index,Point point,string[] finds)
        {
            ldPlayerHelper.Tap(point);
            return false;
        }
























        public bool IsPrioritize => false;

        public bool ReQueue => false;

        public bool ReQueueAfterRunComplete => false;

        public void Cancel()
        {
            ldPlayer.Adb.Stop();
        }

        public void Dispose()
        {
            waiter.Dispose();
            ldPlayer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
