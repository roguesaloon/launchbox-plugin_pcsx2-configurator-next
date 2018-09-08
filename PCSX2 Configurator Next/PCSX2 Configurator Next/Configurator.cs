using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using IniParser;
using IniParser.Model;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    public static class Configurator
    {
        public static string PluginDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string LaunchBoxDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static string RemoteConfigsDir => "https://github.com/Zombeaver/PCSX2-Configs/trunk/Game%20Configs";
        public static string LocalRemoteConfigsDir => $"{PluginDirectory}\\remote";

        public static bool GetIsValidForGame(IGame game)
        {
            return game.Platform == "Sony Playstation 2";
        }

        public static IEmulator GetPcsx2Emulator()
        {
            var emulators = PluginHelper.DataManager.GetAllEmulators();
            foreach (var emulator in emulators)
            {
                if (!emulator.Title.ToLower().Contains("pcsx2")) continue;

                return emulator;
            }

            return null;
        }

        public static string GetPcsx2AppPath(bool absolutePath = true)
        {
            var pcsx2Emulator = GetPcsx2Emulator();

            var appPath = pcsx2Emulator.ApplicationPath;
            var absolutAppPath = LaunchBoxDirectory + "\\" + appPath;

            appPath = (!Path.IsPathRooted(appPath) && absolutePath) ? absolutAppPath : appPath;

            return File.Exists(absolutAppPath) ? appPath : null;
        }

        public static string GetPcsx2CommandLine()
        {
            var pcsx2Emulator = GetPcsx2Emulator();
            return pcsx2Emulator.CommandLine;
        }

        public static string GetPcsx2Dir()
        {
            var pcsx2AppPath = GetPcsx2AppPath();
            return Path.GetDirectoryName(pcsx2AppPath);
        }

        public static string GetBaseConfigDir()
        {
            // TODO: Expand this by checking validity with possible fallbacks
            var pcsx2Dir = GetPcsx2Dir();
            var baseConfigPath = pcsx2Dir + "\\inis";

            return baseConfigPath;
        }

        public static string GetMemCardsDir()
        {
            // TODO?: Expand this by checking against actual folder settings
            var pcsx2Dir = GetPcsx2Dir();
            var memCardsDir = pcsx2Dir + "\\memcards";

            return memCardsDir;
        }

        public static string GetSafeGameTitle(IGame game)
        {
            var safeTitle = Path.GetInvalidFileNameChars().Aggregate(game.Title, (s, c) => s.Replace(c.ToString(), ""));
            return safeTitle;
        }

        public static string GetGameConfigDir(IGame game)
        {
            var safeGameTitle = GetSafeGameTitle(game);
            var gameConfigDir = SettingsModel.GameConfigsDir + "\\" + safeGameTitle;
            return gameConfigDir;
        }

        public static bool IsGameConfigured(IGame game)
        {
            var gameConfigDir = GetGameConfigDir(game);
            var uiConfigFile = gameConfigDir + "\\PCSX2_ui.ini";

            return File.Exists(uiConfigFile);
        }

        public static bool IsGameUsingRemoteConfig(IGame game)
        {
            if (!IsGameConfigured(game)) return false;
            var gameConfigDir = GetGameConfigDir(game);
            var remoteFile = gameConfigDir + "\\remote";

            return File.Exists(remoteFile);
        }

        public static void DeleteConfigDir(IGame game)
        {
            var gameConfigDir = GetGameConfigDir(game);
            if (Directory.Exists(gameConfigDir))
            {
                Directory.Delete(gameConfigDir, true);
            }
        }

        public static void RemoveConfig(IGame game)
        {
            DeleteConfigDir(game);
            ClearGameConfigParams(game);
        }

        public static void CreateConfig(IGame game)
        {
            var gameConfigDir = GetGameConfigDir(game);

            DeleteConfigDir(game);
            Directory.CreateDirectory(gameConfigDir);

            CreateUiConfigFile(gameConfigDir, game);
            CopyOtherSettings(gameConfigDir);

            SetGameConfigParams(game);
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        public static bool DownloadConfig(IGame game)
        {
            var downloadConfigResult = DownloadConfigFromRemote(game.LaunchBoxDbId);

            if (downloadConfigResult.Status == "Downloaded")
            {
                ApplyRemoteConfig(game, downloadConfigResult.GameConfigRemoteDir);
                return true;
            }

            return false;
        }

        private static void CreateUiConfigFile(string targetConfigDir, IGame game)
        {
            var baseConfigDir = GetBaseConfigDir();
            var uiConfigFileName = "PCSX2_ui.ini";

            var iniParser = new FileIniDataParser();
            var baseUiConfig = iniParser.ReadFile(baseConfigDir + "\\" + uiConfigFileName);
            var targetUiConfig = new IniData();

            if(SettingsModel.CopyLogSettings)    targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if(SettingsModel.CopyFolderSettings) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if(SettingsModel.CopyFileSettings)   targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if(SettingsModel.CopyWindowSettings) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (SettingsModel.UseIndependantMemCards)
            {
                // TODO: Extract Formatted Memory Card

                var memCardsDir = GetMemCardsDir();
                var safeGameTitle = GetSafeGameTitle(game);
                var memCardFilePath = $"{memCardsDir}\\{safeGameTitle.Replace(" ", "")}.ps2";

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
                ? $"{LaunchBoxDirectory}\\{game.ApplicationPath}"
                : game.ApplicationPath;

            targetUiConfig.Global["CurrentIso"] = isoPath.Replace("\\", "\\\\");
            targetUiConfig.Global["AskOnBoot"] = "disabled";

            iniParser.WriteFile(targetConfigDir + "\\" + uiConfigFileName, targetUiConfig, Encoding.UTF8);
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private static void CopyOtherSettings(string targetConfigDir)
        {
            var baseConfigDir = GetBaseConfigDir();

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

        public static void SetGameConfigParams(IGame game)
        {
            var pcsx2AppPath = GetPcsx2AppPath(absolutePath: false);
            var pcsx2CommandLine = GetPcsx2CommandLine();
            var gameConfigDir = GetGameConfigDir(game);

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

        public struct DownloadStatus
        {
            public string Status;
            public string GameConfigRemoteDir;
        }

        private static DownloadStatus DownloadConfigFromRemote(int? launchBoxDbId)
        {
            if (launchBoxDbId == null) return new DownloadStatus { Status = "Invalid"};

            if (!Directory.Exists(LocalRemoteConfigsDir))
            {
                Directory.CreateDirectory(LocalRemoteConfigsDir).Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            var svnProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = $"{LaunchBoxDirectory}\\SVN\\bin\\svn.exe",
                    WorkingDirectory = LocalRemoteConfigsDir
                }
            };

            svnProcess.StartInfo.Arguments = $"list {RemoteConfigsDir}";
            svnProcess.Start();
            var svnStdOut = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            var gameList = svnStdOut.Replace("\r\n", "\n").Split('\n');
            var selectedGamePath = gameList.FirstOrDefault(_ => _.Contains($"id#{launchBoxDbId}"));

            if (selectedGamePath == null) return new DownloadStatus { Status = "Not Found" };

            selectedGamePath = selectedGamePath.Substring(0, selectedGamePath.Length - 1);
            svnProcess.StartInfo.Arguments = $"checkout \"{RemoteConfigsDir}/{selectedGamePath}\"";
            svnProcess.Start();
            svnProcess.WaitForExit();

            return new DownloadStatus { Status = "Downloaded", GameConfigRemoteDir = $"{LocalRemoteConfigsDir}\\{selectedGamePath}" };
        }

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        private static void ApplyRemoteConfig(IGame game, string gameConfigRemoteDir)
        {
            CreateConfig(game);

            var targetGameConfigDir = GetGameConfigDir(game);

            foreach (var file in Directory.GetFiles(gameConfigRemoteDir))
            {
                if (file.ToLower().Contains("readme")) continue;
                File.Copy(file, $"{targetGameConfigDir}\\{Path.GetFileName(file)}", overwrite: true);
            }

            var remoteFile = $"{targetGameConfigDir}\\remote";
            if (!File.Exists(remoteFile))
            {
                File.Create($"{targetGameConfigDir}\\remote").Dispose();
            }

            var iniParser = new FileIniDataParser();
            var targetUiConfigFilePath = $"{targetGameConfigDir}\\PCSX2_ui.ini";
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
