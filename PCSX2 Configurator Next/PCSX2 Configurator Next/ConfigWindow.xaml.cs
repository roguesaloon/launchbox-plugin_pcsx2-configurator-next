using System;
using System.Windows;
using System.Windows.Media;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    /// <inheritdoc cref="Window" />
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
                Pcsx2Btn.IsEnabled = false;
                Pcsx2Btn.Foreground = Brushes.DarkGray;
            }

            CreateConfigBtn.MouseDown += CreateConfigBtn_Click;
            DownloadConfigBtn.MouseDown += DownloadConfigBtn_Click;
            Pcsx2Btn.MouseDown += Pcsx2Btn_Click;
        }

        private static void CreateConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void DownloadConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Pcsx2Btn_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
