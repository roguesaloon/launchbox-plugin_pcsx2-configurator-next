using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    public static class Configurator
    {
        public static void CreateConfig(IGame game)
        {
            var gameConfigDir = GameHelper.GetGameConfigDir(game);

            DeleteConfigDir(game);
            Directory.CreateDirectory(gameConfigDir);

            CreateUiConfigFile(gameConfigDir, game);
            CopyOtherSettings(gameConfigDir);

            SetGameConfigParams(game);
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        public static bool DownloadConfig(IGame game, string remoteConfigPath)
        {
            if (remoteConfigPath != null)
            {
                var downloadConfigResult = DownloadConfigFromRemote(remoteConfigPath);

                if (downloadConfigResult == "Downloaded")
                {
                    var remoteConfigDir = $"{ConfiguratorModel.RemoteConfigsDir}\\{remoteConfigPath}";
                    ApplyRemoteConfig(game, remoteConfigDir);
                    return true;
                }
            }

            return false;
        }

        public static void RemoveConfig(IGame game)
        {
            DeleteConfigDir(game);
            ClearGameConfigParams(game);
        }

        public static void SetGameConfigParams(IGame game)
        {
            var pcsx2AppPath = ConfiguratorModel.Pcsx2RelativeAppPath;
            var pcsx2CommandLine = ConfiguratorModel.Pcsx2CommandLine;
            var gameConfigDir = GameHelper.GetGameConfigDir(game);

            var configCommandLine = $"--cfgpath \"{gameConfigDir}\"";

            game.CommandLine = $"{pcsx2CommandLine} {configCommandLine}";
            game.ConfigurationPath = pcsx2AppPath;
            game.ConfigurationCommandLine = configCommandLine;
        }

        public static void ClearGameConfigParams(IGame game)
        {
            game.CommandLine = string.Empty;
            game.ConfigurationPath = string.Empty;
            game.ConfigurationCommandLine = string.Empty;
        }

        private static void DeleteConfigDir(IGame game)
        {
            var gameConfigDir = GameHelper.GetGameConfigDir(game);
            if (Directory.Exists(gameConfigDir))
            {
                Directory.Delete(gameConfigDir, true);
            }
        }

        private static void CreateUiConfigFile(string targetConfigDir, IGame game)
        {
            var baseConfigDir = ConfiguratorModel.Pcsx2InisDir;
            var uiConfigFileName = "PCSX2_ui.ini";

            var iniParser = new FileIniDataParser();
            var baseUiConfig = iniParser.ReadFile(baseConfigDir + "\\" + uiConfigFileName);
            var targetUiConfig = new IniData();

            if (SettingsModel.CopyLogSettings) targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if (SettingsModel.CopyFolderSettings) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if (SettingsModel.CopyFileSettings) targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if (SettingsModel.CopyWindowSettings) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (SettingsModel.UseIndependantMemCards)
            {
                // TODO: Extract Formatted Memory Card

                var safeGameTitle = GameHelper.GetSafeGameTitle(game);
                var memCardFilePath = $"{safeGameTitle.Replace(" ", "")}.ps2";

                targetUiConfig["MemoryCards"]["Slot1_Enable"] = "enabled";
                targetUiConfig["MemoryCards"]["Slot1_Filename"] = Path.GetFileName(memCardFilePath);
            }

            if (SettingsModel.ExposeAllConfigSettings)
            {
                targetUiConfig.Global["EnablePresets"] = "disabled";
                targetUiConfig.Global["EnableGameFixes"] = "enabled";
                targetUiConfig.Global["EnableSpeedHacks"] = "enabled";
            }

            var isoPath = !Path.IsPathRooted(game.ApplicationPath)
                ? $"{ConfiguratorModel.LaunchBoxDir}\\{game.ApplicationPath}"
                : game.ApplicationPath;

            targetUiConfig.Global["CurrentIso"] = isoPath.Replace("\\", "\\\\");
            targetUiConfig.Global["AskOnBoot"] = "disabled";

            iniParser.WriteFile(targetConfigDir + "\\" + uiConfigFileName, targetUiConfig, Encoding.UTF8);
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private static void CopyOtherSettings(string targetConfigDir)
        {
            var baseConfigDir = ConfiguratorModel.Pcsx2InisDir;

            if (SettingsModel.CopyVmSettingsFile)
            {
                var vmSettingsFileName = "PCSX2_vm.ini";
                File.Copy($"{baseConfigDir}\\{vmSettingsFileName}", $"{targetConfigDir}\\{vmSettingsFileName}", true);
            }

            if (SettingsModel.CopyGsdxSettingsFile)
            {
                var gsdxSettingsFileName = "GSdx.ini";
                File.Copy($"{baseConfigDir}\\{gsdxSettingsFileName}", $"{targetConfigDir}\\{gsdxSettingsFileName}", true);
            }

            if (SettingsModel.CopySpu2XSettingsFile)
            {
                var spu2XSetiingsFileName = "SPU2-X.ini";
                File.Copy($"{baseConfigDir}\\{spu2XSetiingsFileName}", $"{targetConfigDir}\\{spu2XSetiingsFileName}", true);
            }

            if (SettingsModel.CopyLilyPadSettingsFile)
            {
                var lilyPadSettingsFileName = "LilyPad.ini";
                File.Copy($"{baseConfigDir}\\{lilyPadSettingsFileName}", $"{targetConfigDir}\\{lilyPadSettingsFileName}", true);
            }
        }

        private static string DownloadConfigFromRemote(string remoteConfigPath)
        {
            var remoteConfigsDir = ConfiguratorModel.RemoteConfigsDir;
            var svnProcess = ConfiguratorModel.SvnProcess;

            svnProcess.StartInfo.Arguments = $"checkout \"{ConfiguratorModel.RemoteConfigsUrl}/{remoteConfigPath}\"";
            svnProcess.Start();
            svnProcess.WaitForExit();

            return "Downloaded";
        }

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        private static void ApplyRemoteConfig(IGame game, string remoteConfigDir)
        {
            CreateConfig(game);

            var targetGameConfigDir = GameHelper.GetGameConfigDir(game);

            foreach (var file in Directory.GetFiles(remoteConfigDir))
            {
                if (file.ToLower().Contains("readme")) continue;
                File.Copy(file, $"{targetGameConfigDir}\\{Path.GetFileName(file)}", overwrite: true);
            }

            var remoteFile = $"{targetGameConfigDir}\\{ConfiguratorModel.RemoteConfigDummyFileName}";
            if (!File.Exists(remoteFile))
            {
                File.Create($"{targetGameConfigDir}\\{ConfiguratorModel.RemoteConfigDummyFileName}").Dispose();
            }

            var iniParser = new FileIniDataParser();
            var targetUiConfigFilePath = $"{targetGameConfigDir}\\{ConfiguratorModel.Pcsx2UiFileName}";
            var uiTweaksFilePath = $"{targetGameConfigDir}\\PCSX2_ui-tweak.ini";

            var targetUiConfig = iniParser.ReadFile(targetUiConfigFilePath);

            targetUiConfig.Global["EnablePresets"] = "disabled";
            targetUiConfig.Global["EnableGameFixes"] = "enabled";
            targetUiConfig.Global["EnableSpeedHacks"] = "enabled";
            targetUiConfig["GSWindow"]["Zoom"] = "101.00";

            if (File.Exists(uiTweaksFilePath))
            {
                var uiTweakConfig = iniParser.ReadFile(uiTweaksFilePath);
                targetUiConfig.Merge(uiTweakConfig);
            }

            iniParser.WriteFile(targetUiConfigFilePath, targetUiConfig, Encoding.UTF8);
        }
    }
}
