﻿using System.Drawing;
using System.Reflection;
using System.Windows;
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
            var isValidForGame = selectedGame.Platform == "Sony Playstation 2";

            // This is a Hack
            // May be good idea to call this, but not here (Should be on a slectionChanged Event)
            // This is less than perfect, and does not always function as expected.
            if (isValidForGame && Configurator.IsGameConfigured(selectedGame) && string.IsNullOrEmpty(selectedGame.ConfigurationPath))
                Configurator.SetConfigParamsForGame(selectedGame);

            return isValidForGame;
        }

        public bool GetIsValidForGames(IGame[] selectedGames)
        {
            return SupportsMultipleGames;
        }

        public void OnSelected(IGame selectedGame)
        {
            var configWindow = new ConfigWindow(selectedGame)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            configWindow.Closing += (sender, args) =>
            {
                configWindow.Owner.IsEnabled = true;
                configWindow.Owner.Focus();
            };
            configWindow.Owner.IsEnabled = false;
            configWindow.Show();
        }

        public void OnSelected(IGame[] selectedGames)
        {
        }
    }
}
