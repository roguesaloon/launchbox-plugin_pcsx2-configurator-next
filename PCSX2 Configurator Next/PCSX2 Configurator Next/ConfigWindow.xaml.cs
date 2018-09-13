using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using PCSX2_Configurator_Next.WpfExtensions;
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
        private readonly Task<string> _selectedGameRemoteConfigPathTask;

        public ConfigWindow(IGame selectedGame = null)
        {
            _selectedGame = selectedGame;
            _selectedGameRemoteConfigPathTask = Task.Run(() => GameHelper.GetRemoteConfigPath(_selectedGame));
            InitializeComponent();
            InitializeConfigWindow();
            SetupEvents();
        }

        private void InitializeConfigWindow()
        {
            ((OutlinedTextBlock) ConfiguredLbl.Content).Text = "[Game Name]: [Configured]";
            ((OutlinedTextBlock) ConfiguredLbl.Content).Text = ((OutlinedTextBlock) ConfiguredLbl.Content).Text.Replace("[Game Name]", _selectedGame.Title);
            ((OutlinedTextBlock) ConfiguredLbl.Content).Text = GameHelper.IsGameConfigured(_selectedGame)
                ? ((OutlinedTextBlock) ConfiguredLbl.Content).Text.Replace("[Configured]", "Configured")
                : ((OutlinedTextBlock) ConfiguredLbl.Content).Text.Replace("[Configured]", "Not Configured");

            ((OutlinedTextBlock) DownloadConfigBtn.Content).Text = "[Download] Config";
            ((OutlinedTextBlock) DownloadConfigBtn.Content).Text = GameHelper.IsGameUsingRemoteConfig(_selectedGame)
                ? ((OutlinedTextBlock) DownloadConfigBtn.Content).Text.Replace("[Download]", "Update")
                : ((OutlinedTextBlock) DownloadConfigBtn.Content).Text.Replace("[Download]", "Download");

            DisableControl(DownloadConfigBtn);
            _selectedGameRemoteConfigPathTask.ContinueWith(remoteConfigPath =>
            {
                if (!GameHelper.IsGameUsingRemoteConfig(_selectedGame))
                {
                    if (remoteConfigPath.Result != null)
                    {
                        Dispatcher.Invoke(() => EnableControl(DownloadConfigBtn));
                    }
                }
                else
                {
                    if (Configurator.CheckForConfigUpdates(remoteConfigPath.Result))
                    {
                        Dispatcher.Invoke(() => EnableControl(DownloadConfigBtn));
                    }
                }
            });

            if (!GameHelper.IsGameConfigured(_selectedGame))
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

        private static void DisableControl(ContentControl control)
        {
            control.Cursor = null;
            control.IsEnabled = false;
            control.Effect = null;
            ((OutlinedTextBlock) control.Content).Stroke = new SolidColorBrush(Color.FromRgb(0, 27, 115));
            
        }

        private static void EnableControl(ContentControl control)
        {
            control.Cursor = Cursors.Hand;
            control.IsEnabled = true;
            control.Effect = new DropShadowEffect() { Direction = 220, ShadowDepth = 3, Color =  Color.FromRgb(27, 39, 220) };
            ((OutlinedTextBlock)control.Content).Stroke = new SolidColorBrush(Color.FromRgb(3, 148, 255));
        }

        private void SetupEvents()
        {
            MouseDown += delegate { try { DragMove(); } catch { /*ignored*/ } };

            CreateConfigBtn.MouseLeftButtonDown += CreateConfigBtn_Click;
            DownloadConfigBtn.MouseLeftButtonDown += DownloadConfigBtn_Click;
            RemoveConfigBtn.MouseLeftButtonDown += RemoveConfigBtn_Click;
            Pcsx2Btn.MouseLeftButtonDown += Pcsx2Btn_Click;

            CloseBtn.MouseLeftButtonDown += delegate { Close(); };
        }

        private void CreateConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            var createConfig = true;
            if (GameHelper.IsGameConfigured(_selectedGame))
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
            if (!GameHelper.IsGameUsingRemoteConfig(_selectedGame))
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var result = Configurator.DownloadConfig(_selectedGame, _selectedGameRemoteConfigPathTask.Result);
                Mouse.OverrideCursor = null;
                MessageBox.Show(result ? "Game Config Downloaded Successfully" : "Could Not Download Game Config");
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Configurator.UpdateGameConfig(_selectedGame, _selectedGameRemoteConfigPathTask.Result);
                Mouse.OverrideCursor = null;
                MessageBox.Show("Game Config Updated Successfully.");
            }

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
            Close();
        }
    }
}
