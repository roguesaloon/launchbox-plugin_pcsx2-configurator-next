using System.IO;
using System.Linq;
using System.Reflection;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    public class Configurator
    {
        public static string PluginDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string LaunchBoxDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public static string GetPcsx2Path()
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
    }
}
