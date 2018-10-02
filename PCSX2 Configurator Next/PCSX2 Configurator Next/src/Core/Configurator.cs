using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next.Core
{
    public static class Configurator
    {
        public static ConfiguratorModel Model { get; } = new ConfiguratorModel();

        public static void CreateConfig(IGame game)
        {
            var gameConfigDir = GameHelper.GetGameConfigDir(game);

            Utils.SystemRemoveDir(gameConfigDir);
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

                if (downloadConfigResult != "Failed")
                {
                    var remoteConfigDir = $"{Model.RemoteConfigsDir}\\{remoteConfigPath}";
                    ApplyRemoteConfig(game, remoteConfigDir);
                    return true;
                }
            }

            return false;
        }

        public static bool CheckForConfigUpdates(string remoteConfigPath)
        {
            var remoteConfigDir = $"{Model.RemoteConfigsDir}\\{remoteConfigPath}";
            return Utils.SvnDirNeedsUpdate(remoteConfigDir);
        }

        public static void UpdateGameConfig(IGame game, string remoteConfigPath)
        {
            var remoteConfigDir = $"{Model.RemoteConfigsDir}\\{remoteConfigPath}";

            if (DownloadConfigFromRemote(remoteConfigPath) == "Updated")
            {
                ApplyRemoteConfig(game, remoteConfigDir);
            }
        }

        public static void RemoveConfig(IGame game)
        {
            var gameConfigDir = GameHelper.GetGameConfigDir(game);
            Utils.SystemRemoveDir(gameConfigDir);
            ClearGameConfigParams(game);
        }

        public static void ApplyGameConfigParams(IGame game)
        {
            if (!GameHelper.IsValidForGame(game)) return;

            if (GameHelper.IsGameConfigured(game))
            {
                SetGameConfigParams(game);
            }

            if (!GameHelper.IsGameConfigured(game))
            {
                ClearGameConfigParams(game);
            }
        }

        private static void SetGameConfigParams(IGame game)
        {
            var pcsx2AppPath = Model.Pcsx2RelativeAppPath;
            var pcsx2CommandLine = Model.Pcsx2CommandLine;
            var gameConfigDir = GameHelper.GetGameConfigDir(game);

            var configCommandLine = $"--cfgpath \"{gameConfigDir}\"";

            if (!GameHelper.IsGameUsingRocketLauncher(game))
            {
                game.CommandLine = string.IsNullOrEmpty(game.CommandLine)
                    ? $"{pcsx2CommandLine} {configCommandLine}"
                    : game.CommandLine;
            }
            else
            {
                Task.Run(() =>
                {
                    var rocketLauncherPath = PluginHelper.DataManager.GetEmulatorById(game.EmulatorId).ApplicationPath;
                    var rocketLauncherDir = Utils.LaunchBoxRelativePathToAbsolute(rocketLauncherPath);

                    var iniParser = new FileIniDataParser();
                    var rocketLauncherPcsx2ConfigPath = $"{rocketLauncherDir}\\Modules\\PCSX2\\PCSX2.ini";
                    var rocketLauncherPcsx2Config = File.Exists(rocketLauncherPcsx2ConfigPath) ? iniParser.ReadFile(rocketLauncherPcsx2ConfigPath) : new IniData();

                    rocketLauncherPcsx2Config["Settings"]["cfgpath"] = Settings.Model.GameConfigsDir;

                    iniParser.WriteFile(rocketLauncherPcsx2ConfigPath, rocketLauncherPcsx2Config, Encoding.UTF8);
                });

            }

            game.ConfigurationPath = pcsx2AppPath;
            game.ConfigurationCommandLine = configCommandLine;
        }

        private static void ClearGameConfigParams(IGame game)
        {
            game.CommandLine = string.Empty;
            game.ConfigurationPath = string.Empty;
            game.ConfigurationCommandLine = string.Empty;
        }

        private static void CreateUiConfigFile(string targetConfigDir, IGame game)
        {
            var iniParser = new FileIniDataParser();
            var baseUiConfig = iniParser.ReadFile($"{Model.Pcsx2InisDir}\\{Model.Pcsx2UiFileName}");
            var targetUiConfig = new IniData();

            if (Settings.Model.CopyLogSettings) targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if (Settings.Model.CopyFolderSettings) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if (Settings.Model.CopyFileSettings) targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if (Settings.Model.CopyWindowSettings) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (Settings.Model.UseIndependantMemCards)
            {
                var safeGameTitle = GameHelper.GetSafeGameTitle(game);
                var memCardFileName = $"{safeGameTitle.Replace(" ", "")}.ps2";

                targetUiConfig["MemoryCards"]["Slot1_Enable"] = "enabled";
                targetUiConfig["MemoryCards"]["Slot1_Filename"] = memCardFileName;

                ExtractFormattedMemoryCard(baseUiConfig, memCardFileName);
            }

            if (Settings.Model.ExposeAllConfigSettings)
            {
                targetUiConfig.Global["EnablePresets"] = "disabled";
                targetUiConfig.Global["EnableGameFixes"] = "enabled";
                targetUiConfig.Global["EnableSpeedHacks"] = "enabled";
            }

            var isoPath = Utils.LaunchBoxRelativePathToAbsolute(game.ApplicationPath);

            targetUiConfig.Global["CurrentIso"] = isoPath.Replace("\\", "\\\\");
            targetUiConfig.Global["AskOnBoot"] = "disabled";

            iniParser.WriteFile($"{targetConfigDir}\\{Model.Pcsx2UiFileName}", targetUiConfig, Encoding.UTF8);
        }

        private static void ExtractFormattedMemoryCard(IniData baseUiConfig, string memCardFileName)
        {
            var memCardsDir = baseUiConfig["Folders"]["MemoryCards"];
            memCardsDir = Utils.Pcsx2RelativePathToAbsolute(memCardsDir);

            if (File.Exists($"{memCardsDir}\\{memCardFileName}")) return;

            var memCardArchive = $"{Model.PluginDir}\\Assets\\Mcd.7z";
            Utils.SevenZipExtract(memCardArchive, memCardsDir);

            File.Move($"{memCardsDir}\\Mcd.ps2", $"{memCardsDir}\\{memCardFileName}");
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private static void CopyOtherSettings(string targetConfigDir)
        {
            var baseConfigDir = Model.Pcsx2InisDir;

            if (Settings.Model.CopyVmSettingsFile)
            {
                var vmSettingsFileName = "PCSX2_vm.ini";
                try { File.Copy($"{baseConfigDir}\\{vmSettingsFileName}", $"{targetConfigDir}\\{vmSettingsFileName}", true); }
                catch { /*ignored*/ }
            }

            if (Settings.Model.CopyGsdxSettingsFile)
            {
                var gsdxSettingsFileName = "GSdx.ini";
                try { File.Copy($"{baseConfigDir}\\{gsdxSettingsFileName}", $"{targetConfigDir}\\{gsdxSettingsFileName}", true); }
                catch { /*ignored*/ }
            }

            if (Settings.Model.CopySpu2XSettingsFile)
            {
                var spu2XSetiingsFileName = "SPU2-X.ini";
                try { File.Copy($"{baseConfigDir}\\{spu2XSetiingsFileName}", $"{targetConfigDir}\\{spu2XSetiingsFileName}", true); }
                catch { /*ignored*/ }
            }

            if (Settings.Model.CopyLilyPadSettingsFile)
            {
                var lilyPadSettingsFileName = "LilyPad.ini";
                try { File.Copy($"{baseConfigDir}\\{lilyPadSettingsFileName}", $"{targetConfigDir}\\{lilyPadSettingsFileName}", true); }
                catch { /*ignored*/ }
            }
        }

        private static string DownloadConfigFromRemote(string remoteConfigPath)
        {
            var remoteConfigDir = $"{Model.RemoteConfigsDir}\\{remoteConfigPath}";
            Utils.SystemRemoveDir(remoteConfigDir);

            var remotePath = $"{Model.RemoteConfigsUrl}/{remoteConfigPath}";
            var workingDir = Model.RemoteConfigsDir;
            var output = Utils.SvnCheckout(remotePath, workingDir);

            return 
                output.StartsWith("Checked out revision") ? "No Update" :
                output.Contains("UU   ") ? "Updated" :
                output.Contains("svn: E") ? "Failed" : 
                "Downloaded";
        }

        private static void ApplyRemoteConfig(IGame game, string remoteConfigDir)
        {
            CreateConfig(game);

            var targetGameConfigDir = GameHelper.GetGameConfigDir(game);

            foreach (var file in Directory.GetFiles(remoteConfigDir))
            {
                if (file.ToLower().Contains("readme")) continue;
                File.Copy(file, $"{targetGameConfigDir}\\{Path.GetFileName(file)}", overwrite: true);
            }

            var remoteFile = $"{targetGameConfigDir}\\{Model.RemoteConfigDummyFileName}";
            if (!File.Exists(remoteFile))
            {
                File.Create($"{targetGameConfigDir}\\{Model.RemoteConfigDummyFileName}").Dispose();
            }

            var iniParser = new FileIniDataParser();
            var targetUiConfigFilePath = $"{targetGameConfigDir}\\{Model.Pcsx2UiFileName}";
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

            var baseUiConfig = iniParser.ReadFile($"{Model.Pcsx2InisDir}\\{Model.Pcsx2UiFileName}");
            var cheatsDir = baseUiConfig["Folders"]["Cheats"];
            cheatsDir = Utils.Pcsx2RelativePathToAbsolute(cheatsDir);

            foreach (var file in Directory.GetFiles(targetGameConfigDir, "*.pnach"))
            {
                File.Move(file, $"{cheatsDir}\\{Path.GetFileName(file)}");
            }

            iniParser.WriteFile(targetUiConfigFilePath, targetUiConfig, Encoding.UTF8);
        }
    }
}
