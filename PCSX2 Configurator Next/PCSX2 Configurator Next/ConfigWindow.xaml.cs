using System;
using System.Windows.Media;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    /// <inheritdoc cref="System.Windows.Window" />
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow
    {
        private readonly IGame _selectedGame;

        public ConfigWindow(IGame selectedGame = null)
        {
            _selectedGame = selectedGame;
            InitializeComponent();
            InitializeConfigWindow();
        }

        private void InitializeConfigWindow()
        {
            ConfiguredLbl.Content = ConfiguredLbl.Content.ToString().Replace("[Game Name]", _selectedGame.Title);
            if (Configurator.IsGameConfigured(_selectedGame))
            {
                ConfiguredLbl.Content = ConfiguredLbl.Content.ToString().Replace("Not ", string.Empty);
            }

            if (Configurator.IsGameUsingRemoteConfig(_selectedGame))
            {
                DownloadConfigBtn.Content = DownloadConfigBtn.Content.ToString().Replace("Download", "Update");
            }

            if (!Configurator.IsGameConfigured(_selectedGame))
            {
                Pcsx2Btn.Cursor = null;
                Pcsx2Btn.Foreground = Brushes.DarkGray;
            }

            Console.WriteLine("PCSX2 Configurator Config Window Initialized");
        }
    }
}
