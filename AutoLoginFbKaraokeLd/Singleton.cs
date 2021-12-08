using AutoLoginFbKaraokeLd.DataClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;

namespace AutoLoginFbKaraokeLd
{
    internal static class Singleton
    {
        static Singleton()
        {
            Directory.CreateDirectory(LogDir);
        }
        internal static string ExeDir { get; } = Directory.GetCurrentDirectory();
        internal static string LogDir { get; } = ExeDir + "\\Logs";
        internal static string AppDataDir { get; } = ExeDir + "\\AppData";
        internal static string ImageDir { get; } = AppDataDir + "\\Images";
        internal static string LibFolder { get; } = AppDataDir + "\\" + (Environment.Is64BitProcess ? "x64" : "x86");


        internal static SaveSettingData<SettingData> Setting { get; } = new SaveSettingData<SettingData>(ExeDir + "\\Setting.json");
    }
}
