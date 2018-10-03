using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next.Core
{
    public class ConfiguratorModel
    {
        public string PluginDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string LaunchBoxDir => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public string RemoteConfigsUrl => "https://github.com/Zombeaver/PCSX2-Configs/trunk/Game%20Configs";
        public string RemoteConfigsDir => _remoteConfigsDir ?? (_remoteConfigsDir = GetRemoteConfigsDir());
        public string RemoteConfigDummyFileName => "remote";
        public string Pcsx2UiFileName => "PCSX2_ui.ini";
        public string SvnDir => $"{LaunchBoxDir}\\SVN";
        public string Pcsx2CommandLine => Pcsx2Emulator.CommandLine;
        public string Pcsx2RelativeAppPath => _pcsx2RelativeAppPath ?? (_pcsx2RelativeAppPath = GetPcsx2AppPath(absolutePath: false));
        public string Pcsx2AbsoluteAppPath => _pcsx2AbsoluteAppPath ?? (_pcsx2AbsoluteAppPath = GetPcsx2AppPath(absolutePath: true));
        public string Pcsx2RelativeDir => _pcsx2RelativeDir ?? (_pcsx2RelativeDir = Path.GetDirectoryName(Pcsx2RelativeAppPath));
        public string Pcsx2AbsoluteDir => _pcsx2AbsoluteDir ?? (_pcsx2AbsoluteDir = Path.GetDirectoryName(Pcsx2AbsoluteAppPath));
        public string Pcsx2InisDir => _pcsx2InisDir ?? (_pcsx2InisDir = GetPcsx2InisDir());
        public string Pcsx2BaseUiFilePath => _pcsx2BaseUiFilePath ?? (_pcsx2BaseUiFilePath = $"{Pcsx2InisDir}\\{Pcsx2UiFileName}");

        private IEmulator _pcsx2Emulator;
        [SuppressMessage("ReSharper", "InvertIf")]
        private IEmulator Pcsx2Emulator
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

        private string _remoteConfigsDir;
        private string GetRemoteConfigsDir()
        {
            var remoteConfigDir = $"{PluginDir}\\remote";

            if (!Directory.Exists(remoteConfigDir))
            {
                Directory.CreateDirectory(remoteConfigDir).Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            return remoteConfigDir;
        }

        private string _pcsx2RelativeAppPath;
        private string _pcsx2AbsoluteAppPath;
        private string _pcsx2RelativeDir;
        private string _pcsx2AbsoluteDir;
        private string GetPcsx2AppPath(bool absolutePath)
        {
            var appPath = Pcsx2Emulator.ApplicationPath;
            var absolutAppPath = Utils.LaunchBoxRelativePathToAbsolute(appPath);

            appPath = absolutePath ? absolutAppPath : appPath;

            return File.Exists(absolutAppPath) ? appPath : null;
        }

        private string _pcsx2InisDir;
        private string _pcsx2BaseUiFilePath;
        private string GetPcsx2InisDir()
        {
            var pcsx2InisDir = File.Exists($"{Pcsx2AbsoluteDir}\\portable.ini")
                ? $"{Pcsx2AbsoluteDir}\\inis"
                : Registry.GetValue("HKEY_CURRENT_USER\\Software\\PCSX2", "SettingsFolder", null).ToString();

            if (string.IsNullOrEmpty(pcsx2InisDir))
            {
                pcsx2InisDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\PCSX2\\inis";
            }

            return pcsx2InisDir;
        }
    }
}
