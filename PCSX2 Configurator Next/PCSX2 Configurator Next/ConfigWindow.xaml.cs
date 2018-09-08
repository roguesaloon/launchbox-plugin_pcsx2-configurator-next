using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            SetupEvents();
        }

        private void InitializeConfigWindow()
        {
            ConfiguredLbl.Content = "[Game Name]: [Configured]";
            ConfiguredLbl.Content = ConfiguredLbl.Content.ToString().Replace("[Game Name]", _selectedGame.Title);
            ConfiguredLbl.Content = Configurator.IsGameConfigured(_selectedGame)
                ? ConfiguredLbl.Content.ToString().Replace("[Configured]", "Configured")
                : ConfiguredLbl.Content.ToString().Replace("[Configured]", "Not Configured");

            DownloadConfigBtn.Content = "[Download] Config";
            DownloadConfigBtn.Content = Configurator.IsGameUsingRemoteConfig(_selectedGame)
                ? DownloadConfigBtn.Content.ToString().Replace("[Download]", "Update")
                : DownloadConfigBtn.Content.ToString().Replace("[Download]", "Download");

            if (!Configurator.IsGameConfigured(_selectedGame))
            {
                DisableControl(RemoveConfigBtn);
                DisableControl(Pcsx2Btn);
            }
            else
            {
                EnableControl(RemoveConfigBtn);
                EnableControl(Pcsx2Btn);
            }
        }

        private static void DisableControl(Control control)
        {
            control.Cursor = null;
            control.IsEnabled = false;
            control.Foreground = Brushes.DarkGray;
        }

        private static void EnableControl(Control control)
        {
            control.Cursor = Cursors.Hand;
            control.IsEnabled = true;
            control.Foreground = Brushes.Black;
        }

        private void SetupEvents()
        {
            CreateConfigBtn.MouseDown += CreateConfigBtn_Click;
            DownloadConfigBtn.MouseDown += DownloadConfigBtn_Click;
            RemoveConfigBtn.MouseDown += RemoveConfigBtn_Click;
            Pcsx2Btn.MouseDown += Pcsx2Btn_Click;
        }

        private void CreateConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            var createConfig = true;
            if (Configurator.IsGameConfigured(_selectedGame))
            {
                var message =
                    "This game is already configured\nThis will overwrite your previous configuration\nDo you still wish to continue?";
                var msgBox = MessageBox.Show(message, Title, MessageBoxButton.YesNo);

                if (msgBox == MessageBoxResult.No)
                {
                    createConfig = false;
                }
            }

            if (!createConfig) return;
            Configurator.CreateConfig(_selectedGame);
            MessageBox.Show("Game Successfully Configured", Title);
            InitializeConfigWindow();

        }

        private void DownloadConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: This is a Placeholder, To have multiple states
            Mouse.OverrideCursor = Cursors.Wait;
            var result = Configurator.DownloadConfig(_selectedGame);
            Mouse.OverrideCursor = Cursors.Arrow;
            MessageBox.Show(result ? "Game Config Downloaded Successfully" : "Could Not Download Game Config");
            InitializeConfigWindow();
        }

        private void RemoveConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            var removeConfig = true;

            var message = "This will remove the current config for this game\nDo you still wish to to continue?";
            var msgBox = MessageBox.Show(message, Title, MessageBoxButton.YesNo);

            if (msgBox == MessageBoxResult.No)
            {
                removeConfig = false;
            }

            if (!removeConfig) return;
            Configurator.RemoveConfig(_selectedGame);
            InitializeConfigWindow();

            MessageBox.Show("Config Successfully Removed", Title);
        }

        private void Pcsx2Btn_Click(object sender, RoutedEventArgs e)
        {
            _selectedGame.Configure();
        }
    }
}
