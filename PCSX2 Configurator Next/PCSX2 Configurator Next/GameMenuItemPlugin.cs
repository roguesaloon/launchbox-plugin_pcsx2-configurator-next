using System.Drawing;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    internal class GameMenuItemPlugin : IGameMenuItemPlugin
    {
        public bool SupportsMultipleGames => false;

        public string Caption => "PCSX2 Configurator";

        public Image IconImage => null;

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => false;

        public bool GetIsValidForGame(IGame selectedGame)
        {
            return selectedGame.Platform == "Sony Playstation 2";
        }

        public bool GetIsValidForGames(IGame[] selectedGames)
        {
            return SupportsMultipleGames;
        }

        public void OnSelected(IGame selectedGame)
        {
            var configWindow = new ConfigWindow();
            configWindow.Show();
        }

        public void OnSelected(IGame[] selectedGames)
        {

        }
    }
}
