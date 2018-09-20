using System.Diagnostics.CodeAnalysis;
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

            ((OutlinedTextBlock) DownloadConfigBtnLbl.Content).Text = "[Download] Config";
            ((OutlinedTextBlock) DownloadConfigBtnLbl.Content).Text = GameHelper.IsGameUsingRemoteConfig(_selectedGame)
                ? ((OutlinedTextBlock) DownloadConfigBtnLbl.Content).Text.Replace("[Download]", "Update")
                : ((OutlinedTextBlock) DownloadConfigBtnLbl.Content).Text.Replace("[Download]", "Download");

            DisableControl(DownloadConfigBtnLbl, DownloadConfigBtnBtn);
            _selectedGameRemoteConfigPathTask.ContinueWith(remoteConfigPath =>
            {
                if (!GameHelper.IsGameUsingRemoteConfig(_selectedGame))
                {
                    if (remoteConfigPath.Result != null)
                    {
                        Dispatcher.Invoke(() => EnableControl(DownloadConfigBtnLbl, DownloadConfigBtnBtn));
                    }
                }
                else
                {
                    if (Configurator.CheckForConfigUpdates(remoteConfigPath.Result))
                    {
                        Dispatcher.Invoke(() => EnableControl(DownloadConfigBtnLbl, DownloadConfigBtnBtn));
                    }
                }
            });

            if (!GameHelper.IsGameConfigured(_selectedGame))
            {
                DisableControl(RemoveConfigBtnLbl, RemoveConfigBtnBtn);
                DisableControl(ConfigureWithPcsx2BtnLbl, ConfigureWithPcsx2BtnBtn);
            }
            else
            {
                EnableControl(RemoveConfigBtnLbl, RemoveConfigBtnBtn);
                EnableControl(ConfigureWithPcsx2BtnLbl, ConfigureWithPcsx2BtnBtn);
            }
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private static void DisableControl(Label label, Button button)
        {
            button.Cursor = null;
            button.IsEnabled = false;

            label.Effect = null;
            ((OutlinedTextBlock) label.Content).Stroke = new SolidColorBrush(Color.FromRgb(0, 27, 115));
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private static void EnableControl(Label label, Button button)
        {
            button.Cursor = Cursors.Hand;
            button.IsEnabled = true;

            label.Effect = new DropShadowEffect() { Direction = 220, ShadowDepth = 3, Color =  Color.FromRgb(27, 39, 220) };
            ((OutlinedTextBlock)label.Content).Stroke = new SolidColorBrush(Color.FromRgb(3, 148, 255));
        }

        private void SetupEvents()
        {
            MouseDown += delegate { try { DragMove(); } catch { /*ignored*/ } };

            CreateConfigBtnBtn.Click += CreateConfigBtn_Click;
            DownloadConfigBtnBtn.Click += DownloadConfigBtn_Click;
            RemoveConfigBtnBtn.Click += RemoveConfigBtn_Click;
            ConfigureWithPcsx2BtnBtn.Click += Pcsx2Btn_Click;

            CloseBtn.Click += delegate { Close(); };
        }

        private void CreateConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            var createConfig = true;
            if (GameHelper.IsGameConfigured(_selectedGame))
            {
                var msgResult = MessageDialog.Show(this, MessageDialog.Type.ConfigOverwriteConfirm);

                if (msgResult != true)
                {
                    createConfig = false;
                }
            }

            if (!createConfig) return;
            Configurator.CreateConfig(_selectedGame);
            MessageDialog.Show(this, MessageDialog.Type.ConfigConfiguredSuccess);
            InitializeConfigWindow();
        }

        private void DownloadConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!GameHelper.IsGameUsingRemoteConfig(_selectedGame))
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var result = Configurator.DownloadConfig(_selectedGame, _selectedGameRemoteConfigPathTask.Result);
                Mouse.OverrideCursor = null;

                MessageDialog.Show(this,
                    result ? MessageDialog.Type.ConfigDownloadSuccess : MessageDialog.Type.ConfigDownloadError);
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Configurator.UpdateGameConfig(_selectedGame, _selectedGameRemoteConfigPathTask.Result);
                Mouse.OverrideCursor = null;

                MessageDialog.Show(this, MessageDialog.Type.ConfigUpdateSuccess);
            }

            InitializeConfigWindow();
        }

        private void RemoveConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            var removeConfig = true;
            var msgResult = MessageDialog.Show(this, MessageDialog.Type.ConfigRemoveConfirm);

            if (msgResult != true)
            {
                removeConfig = false;
            }

            if (!removeConfig) return;
            Configurator.RemoveConfig(_selectedGame);
            InitializeConfigWindow();

            MessageDialog.Show(this, MessageDialog.Type.CongfigRemoveSuccess);
        }

        private void Pcsx2Btn_Click(object sender, RoutedEventArgs e)
        {
            _selectedGame.Configure();
            Close();
        }
    }
}
