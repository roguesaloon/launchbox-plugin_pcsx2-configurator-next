using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

                if (downloadConfigResult != "Failed")
                {
                    var remoteConfigDir = $"{ConfiguratorModel.RemoteConfigsDir}\\{remoteConfigPath}";
                    ApplyRemoteConfig(game, remoteConfigDir);
                    return true;
                }
            }

            return false;
        }

        public static bool CheckForConfigUpdates(string remoteConfigPath)
        {
            var remoteConfigDir = $"{ConfiguratorModel.RemoteConfigsDir}\\{remoteConfigPath}";
            return DoesRemoteConfigDirNeedUpdate(remoteConfigDir);
        }

        public static void UpdateGameConfig(IGame game, string remoteConfigPath)
        {
            var remoteConfigDir = $"{ConfiguratorModel.RemoteConfigsDir}\\{remoteConfigPath}";

            if (DownloadConfigFromRemote(remoteConfigPath) == "Updated")
            {
                ApplyRemoteConfig(game, remoteConfigDir);
            }
        }

        public static void RemoveConfig(IGame game)
        {
            DeleteConfigDir(game);
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
            var pcsx2AppPath = ConfiguratorModel.Pcsx2RelativeAppPath;
            var pcsx2CommandLine = ConfiguratorModel.Pcsx2CommandLine;
            var gameConfigDir = GameHelper.GetGameConfigDir(game);

            var configCommandLine = $"--cfgpath \"{gameConfigDir}\"";

            game.CommandLine = string.IsNullOrEmpty(game.CommandLine) ? $"{pcsx2CommandLine} {configCommandLine}" : game.CommandLine;
            game.ConfigurationPath = pcsx2AppPath;
            game.ConfigurationCommandLine = configCommandLine;
        }

        private static void ClearGameConfigParams(IGame game)
        {
            game.CommandLine = string.Empty;
            game.ConfigurationPath = string.Empty;
            game.ConfigurationCommandLine = string.Empty;
        }

        private static void DeleteConfigDir(IGame game)
        {
            var gameConfigDir = GameHelper.GetGameConfigDir(game);
            if (!Directory.Exists(gameConfigDir)) return;
            foreach (var file in new DirectoryInfo(gameConfigDir).GetFiles())
            {
                file.Delete();
            }
        }

        private static void CreateUiConfigFile(string targetConfigDir, IGame game)
        {
            var iniParser = new FileIniDataParser();
            var baseUiConfig = iniParser.ReadFile($"{ConfiguratorModel.Pcsx2InisDir}\\{ConfiguratorModel.Pcsx2UiFileName}");
            var targetUiConfig = new IniData();

            if (SettingsModel.CopyLogSettings) targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if (SettingsModel.CopyFolderSettings) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if (SettingsModel.CopyFileSettings) targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if (SettingsModel.CopyWindowSettings) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (SettingsModel.UseIndependantMemCards)
            {
                var safeGameTitle = GameHelper.GetSafeGameTitle(game);
                var memCardFileName = $"{safeGameTitle.Replace(" ", "")}.ps2";

                targetUiConfig["MemoryCards"]["Slot1_Enable"] = "enabled";
                targetUiConfig["MemoryCards"]["Slot1_Filename"] = memCardFileName;

                ExtractFormattedMemoryCard(baseUiConfig, memCardFileName);
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

            iniParser.WriteFile($"{targetConfigDir}\\{ConfiguratorModel.Pcsx2UiFileName}", targetUiConfig, Encoding.UTF8);
        }

        private static void ExtractFormattedMemoryCard(IniData baseUiConfig, string memCardFileName)
        {
            var memCardsDir = baseUiConfig["Folders"]["MemoryCards"];
            memCardsDir = !Path.IsPathRooted(memCardsDir)
                ? $"{ConfiguratorModel.Pcsx2AbsoluteDir}\\{memCardsDir}"
                : memCardsDir;

            if (File.Exists($"{memCardsDir}\\{memCardFileName}")) return;
            var sevenZipProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = $"{ConfiguratorModel.LaunchBoxDir}\\7-Zip\\7z.exe",
                    Arguments = $"e \"{ConfiguratorModel.PluginDir}\\Assets\\Mcd.7z\" -o\"{memCardsDir}\""
                }
            };

            sevenZipProcess.Start();
            sevenZipProcess.WaitForExit();
            File.Move($"{memCardsDir}\\Mcd.ps2", $"{memCardsDir}\\{memCardFileName}");
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

        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        private static string DownloadConfigFromRemote(string remoteConfigPath)
        {
            var remoteConfigDir = $"{ConfiguratorModel.RemoteConfigsDir}\\{remoteConfigPath}";
            if (Directory.Exists(remoteConfigDir))
            {
                Directory.Delete(remoteConfigDir);
                while (Directory.Exists(remoteConfigDir)) continue;
            }

            var svnProcess = ConfiguratorModel.SvnProcess;
            svnProcess.StartInfo.WorkingDirectory = ConfiguratorModel.RemoteConfigsDir;

            svnProcess.StartInfo.Arguments = $"checkout \"{ConfiguratorModel.RemoteConfigsUrl}/{remoteConfigPath}\"";
            svnProcess.Start();
            var svnStdOut = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            return 
                svnStdOut.StartsWith("Checked out revision") ? "No Update" :
                svnStdOut.Contains("UU   ") ? "Updated" : 
                svnStdOut.Contains("svn: E") ? "Failed" : 
                "Downloaded";
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

            var baseUiConfig = iniParser.ReadFile($"{ConfiguratorModel.Pcsx2InisDir}\\{ConfiguratorModel.Pcsx2UiFileName}");
            var cheatsDir = baseUiConfig["Folders"]["Cheats"];
            cheatsDir = !Path.IsPathRooted(cheatsDir)
                ? $"{ConfiguratorModel.Pcsx2AbsoluteDir}\\{cheatsDir}"
                : cheatsDir;
            foreach (var file in Directory.GetFiles(targetGameConfigDir, "*.pnach"))
            {
                File.Move(file, $"{cheatsDir}\\{Path.GetFileName(file)}");
            }

            iniParser.WriteFile(targetUiConfigFilePath, targetUiConfig, Encoding.UTF8);
        }

        private static bool DoesRemoteConfigDirNeedUpdate(string remoteConfigDir)
        {
            var svnProcess = ConfiguratorModel.SvnProcess;
            svnProcess.StartInfo.WorkingDirectory = remoteConfigDir;

            svnProcess.StartInfo.Arguments = "info -r HEAD";
            svnProcess.Start();
            var svnHeadInfo = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            svnProcess.StartInfo.Arguments = "info";
            svnProcess.Start();
            var svnInfo = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            svnHeadInfo = GetLastChangedRev(svnHeadInfo);
            svnInfo = GetLastChangedRev(svnInfo);

            return svnHeadInfo != svnInfo;

            string GetLastChangedRev(string output)
            {
                var arr = output.Replace("\r\n", "\n").Split('\n');
                var ret = arr.FirstOrDefault(_ => _.StartsWith("Last Changed Rev"));
                return ret;
            }
        }
    }
}
