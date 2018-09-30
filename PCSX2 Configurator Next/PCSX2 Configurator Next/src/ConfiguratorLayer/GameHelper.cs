using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next.ConfiguratorLayer
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
            var gameConfigDirName = !IsGameUsingRocketLauncher(game) ? GetSafeGameTitle(game) : Path.GetFileNameWithoutExtension(game.ApplicationPath);
            var gameConfigDir = $"{Settings.Model.GameConfigsDir}\\{gameConfigDirName}";
            return gameConfigDir;
        }

        public static string GetRemoteConfigPath(IGame game)
        {
            var svnProcess = Configurator.Model.SvnProcess;
            svnProcess.StartInfo.WorkingDirectory = Configurator.Model.RemoteConfigsDir;
            svnProcess.StartInfo.Arguments = $"list {Configurator.Model.RemoteConfigsUrl}";

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
