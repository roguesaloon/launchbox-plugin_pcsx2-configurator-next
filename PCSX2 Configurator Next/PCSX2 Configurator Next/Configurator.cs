using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static string GetPcsx2AppPath()
        {
            var emulators = PluginHelper.DataManager.GetAllEmulators();
            foreach (var emulator in emulators)
            {
                if (!emulator.Title.ToLower().Contains("pcsx2")) continue;

                var appPath = emulator.ApplicationPath;
                appPath = (!Path.IsPathRooted(appPath)) ? LaunchBoxDirectory + "\\" + appPath : appPath;

                return File.Exists(appPath) ? appPath : null;
            }

            return null;
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

        public static string GetSafeGameName(string gameName)
        {
            var safeName = Path.GetInvalidFileNameChars().Aggregate(gameName, (s, c) => s.Replace(c.ToString(), ""));
            return safeName;
        }

        public static string GetGameConfigDir(IGame game)
        {
            var safeGameName = GetSafeGameName(game.Title);
            var gameConfigDir = SettingsModel.GameConfigsDir + "\\" + safeGameName;
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

            // TODO: Clear Command Line and Config Path
        }

        public static void CreateConfig(IGame game)
        {
            var gameConfigDir = GetGameConfigDir(game);

            DeleteConfigDir(game);
            Directory.CreateDirectory(gameConfigDir);

            CreateUiConfigFile(gameConfigDir, game);
            CopyOtherSettings(gameConfigDir);

            // TODO: Set Command Line and Config Path
        }

        private static void CreateUiConfigFile(string targetConfigDir, IGame game)
        {
            var baseConfigDir = GetBaseConfigDir();
            var uiConfigFileName = "PCSX2_ui.ini";

            File.CreateText(targetConfigDir + "\\" + uiConfigFileName).Dispose();
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
                var safeGameName = GetSafeGameName(game.Title);
                var memCardFilePath = $"{memCardsDir}\\{safeGameName.Replace(" ", "")}.ps2";

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

            iniParser.WriteFile(targetConfigDir + "\\" + uiConfigFileName, targetUiConfig);
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
    }
}
