using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next.Core
{
    public static class GameHelper
    {
        public static string GetSafeGameTitle(IGame game)
        {
            var safeTitle = Path.GetInvalidFileNameChars().Aggregate(game.Title, (s, c) => s.Replace(c.ToString(), ""));
            return safeTitle;
        }

        public static string GetGameConfigDir(IGame game, bool relativeToPcsx2 = false)
        {
            var gameConfigsDir = !relativeToPcsx2
                ? Utils.Pcsx2RelativePathToAbsolute(Settings.Model.GameConfigsDir)
                : Settings.Model.GameConfigsDir;

            var gameConfigDirName = !IsGameUsingRocketLauncher(game)
                ? GetSafeGameTitle(game)
                : Path.GetFileNameWithoutExtension(game.ApplicationPath);
            var gameConfigDir = $"{gameConfigsDir}\\{gameConfigDirName}";
            return gameConfigDir;
        }

        public static string GetRemoteConfigPath(IGame game)
        {
            var remotePath = Configurator.Model.RemoteConfigsUrl;
            bool WithGameId(string str) => str.Contains($"id#{game.LaunchBoxDbId}");

            var selectedGamePath = Utils.SvnFindPathInRemote(remotePath, WithGameId);
            return selectedGamePath;
        }

        public static bool IsValidForGame(IGame game)
        {
            return game.Platform.ToLower() == "sony playstation 2";
        }

        public static bool IsGameConfigured(IGame game)
        {
            var gameConfigDir = GetGameConfigDir(game);
            var uiConfigFile = $"{gameConfigDir}\\{Configurator.Model.Pcsx2UiFileName}";

            return File.Exists(uiConfigFile);
        }

        public static bool IsGameUsingRemoteConfig(IGame game)
        {
            if (!IsGameConfigured(game)) return false;
            var gameConfigDir = GetGameConfigDir(game);
            var remoteFile = $"{gameConfigDir}\\{Configurator.Model.RemoteConfigDummyFileName}";

            return File.Exists(remoteFile);
        }

        public static bool IsGameUsingRocketLauncher(IGame game)
        {
            var emulator = PluginHelper.DataManager.GetEmulatorById(game.EmulatorId);
            var ret = Regex.IsMatch(emulator.Title, "rocket.*launcher", RegexOptions.IgnoreCase);
            return ret;
        }
    }
}
