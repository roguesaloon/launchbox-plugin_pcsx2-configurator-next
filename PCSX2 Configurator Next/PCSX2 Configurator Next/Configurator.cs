using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    public class Configurator
    {
        public static string PluginDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string LaunchBoxDircetory => Application.StartupPath;

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
