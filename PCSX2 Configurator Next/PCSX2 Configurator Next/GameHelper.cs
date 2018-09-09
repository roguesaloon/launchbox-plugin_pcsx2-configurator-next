using System.IO;
using System.Linq;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    public static class GameHelper
    {
        public static string GetSafeGameTitle(IGame game)
        {
            var safeTitle = Path.GetInvalidFileNameChars().Aggregate(game.Title, (s, c) => s.Replace(c.ToString(), ""));
            return safeTitle;
        }

        public static string GetGameConfigDir(IGame game)
        {
            var safeGameTitle = GetSafeGameTitle(game);
            var gameConfigDir = $"{SettingsModel.GameConfigsDir}\\{safeGameTitle}";
            return gameConfigDir;
        }

        public static string GetRemoteConfigPath(IGame game)
        {
            var svnProcess = ConfiguratorModel.SvnProcess;
            svnProcess.StartInfo.Arguments = $"list {ConfiguratorModel.RemoteConfigsUrl}";

            svnProcess.Start();
            var svnStdOut = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            var gameList = svnStdOut.Replace("\r\n", "\n").Split('\n');
            var selectedGamePath = gameList.FirstOrDefault(_ => _.Contains($"id#{game.LaunchBoxDbId}"));
            selectedGamePath = selectedGamePath?.Substring(0, selectedGamePath.Length - 1);

            return selectedGamePath;
        }

        public static bool IsValidForGame(IGame game)
        {
            // TODO: Expand This
            return game.Platform == "Sony Playstation 2";
        }

        public static bool IsGameConfigured(IGame game)
        {
            var gameConfigDir = GetGameConfigDir(game);
            var uiConfigFile = $"{gameConfigDir}\\{ConfiguratorModel.Pcsx2UiFileName}";

            return File.Exists(uiConfigFile);
        }

        public static bool IsGameUsingRemoteConfig(IGame game)
        {
            if (!IsGameConfigured(game)) return false;
            var gameConfigDir = GetGameConfigDir(game);
            var remoteFile = $"{gameConfigDir}\\{ConfiguratorModel.RemoteConfigDummyFileName}";

            return File.Exists(remoteFile);
        }
    }
}
