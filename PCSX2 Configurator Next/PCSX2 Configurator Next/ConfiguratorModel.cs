using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    public static class ConfiguratorModel
    {
        public static string PluginDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string LaunchBoxDir => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static string RemoteConfigsUrl => "https://github.com/Zombeaver/PCSX2-Configs/trunk/Game%20Configs";
        public static string RemoteConfigsDir => _remoteConfigsDir ?? (_remoteConfigsDir = GetRemoteConfigDir());
        public static string RemoteConfigDummyFileName => "remote";
        public static string Pcsx2UiFileName => "PCSX2_ui.ini";
        public static string Pcsx2CommandLine => Pcsx2Emulator.CommandLine;
        public static string Pcsx2RelativeAppPath => _pcsx2RelativeAppPath ?? (_pcsx2RelativeAppPath = GetPcsx2AppPath(absolutePath: false));
        public static string Pcsx2AbsoluteAppPath => _pcsx2AbsoluteAppPath ?? (_pcsx2AbsoluteAppPath = GetPcsx2AppPath(absolutePath: true));
        public static string Pcsx2RelativeDir => _pcsx2RelativeDir ?? (_pcsx2RelativeDir = Path.GetDirectoryName(Pcsx2RelativeAppPath));
        public static string Pcsx2AbsoluteDir => _pcsx2AbsoluteDir ?? (_pcsx2AbsoluteDir = Path.GetDirectoryName(Pcsx2AbsoluteAppPath));
        public static string Pcsx2InisDir => _pcsx2InisDir ?? (_pcsx2InisDir = GetPcsx2InisDir());
        public static string Pcsx2BaseUiFilePath => _pcsx2BaseUiFilePath ?? (_pcsx2BaseUiFilePath = $"{Pcsx2InisDir}\\{Pcsx2UiFileName}");
        public static Process SvnProcess => _svnProcess ?? (_svnProcess = GetSvnProcess());

        private static IEmulator _pcsx2Emulator;
        [SuppressMessage("ReSharper", "InvertIf")]
        private static IEmulator Pcsx2Emulator
        {
            get
            {
                if (_pcsx2Emulator == null)
                {
                    var emulators = PluginHelper.DataManager.GetAllEmulators();
                    _pcsx2Emulator = emulators.First(_ => _.Title.ToLower().Contains("pcsx2"));
                }

                return _pcsx2Emulator;
            }
        }

        private static string _remoteConfigsDir;
        private static string GetRemoteConfigDir()
        {
            var remoteConfigDir = $"{PluginDir}\\remote";

            if (!Directory.Exists(remoteConfigDir))
            {
                Directory.CreateDirectory(remoteConfigDir);
            }

            return remoteConfigDir;
        }

        private static string _pcsx2RelativeAppPath;
        private static string _pcsx2AbsoluteAppPath;
        private static string _pcsx2RelativeDir;
        private static string _pcsx2AbsoluteDir;
        private static string GetPcsx2AppPath(bool absolutePath)
        {
            var appPath = Pcsx2Emulator.ApplicationPath;
            var absolutAppPath = $"{LaunchBoxDir}\\{appPath}";

            appPath = (!Path.IsPathRooted(appPath) && absolutePath) ? absolutAppPath : appPath;

            return File.Exists(absolutAppPath) ? appPath : null;
        }

        private static string _pcsx2InisDir;
        private static string _pcsx2BaseUiFilePath;
        private static string GetPcsx2InisDir()
        {
            var pcsx2InisDir = File.Exists($"{Pcsx2AbsoluteDir}\\portable.ini")
                ? $"{Pcsx2AbsoluteDir}\\inis"
                : Registry.GetValue("HKCU\\Software\\PCSX2", "SettingsFolder", null).ToString();

            if (string.IsNullOrEmpty(pcsx2InisDir))
            {
                pcsx2InisDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\PCSX2\\inis";
            }

            return pcsx2InisDir;
        }

        private static Process _svnProcess;
        private static Process GetSvnProcess()
        {
            var svnProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = $"{LaunchBoxDir}\\SVN\\bin\\svn.exe",
                    WorkingDirectory = RemoteConfigsDir
                }
            };

            return svnProcess;
        }
    }
}
