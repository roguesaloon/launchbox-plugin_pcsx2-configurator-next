using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PCSX2_Configurator_Next
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog
    {
        public enum Type
        {
            GenericYesNo, Generic, GenericSpecial, GenericError,
            ConfigDownloadSuccess, CongfigRemoveSuccess, ConfigConfiguredSuccess, ConfigUpdateSuccess,
            ConfigRemoveConfirm, ConfigOverwriteConfirm,
            ConfigDownloadError
        }

        public MessageDialog(Type messageType, string message, double fontSize)
        {
            InitializeComponent();

            MouseDown += delegate { try { DragMove(); } catch { /*ignored*/ } };

            CloseBtn.MouseLeftButtonDown += delegate { Close(); };
            YesBtn.MouseLeftButtonDown += delegate { DialogResult = true; };
            NoBtn.MouseLeftButtonDown += delegate { DialogResult = false; };

            switch (messageType)
            {
                case Type.GenericYesNo:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Generic.png")));
                    CloseBtn.Visibility = Visibility.Hidden;
                    CloseBtn.IsEnabled = false;
                    break;
                case Type.Generic:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Generic.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    break;
                case Type.GenericSpecial:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Success.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    break;
                case Type.GenericError:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Error.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Stroke = new SolidColorBrush(Color.FromRgb(255, 14, 12));
                    CloseBtn.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Close (Red).png"));
                    break;
                case Type.ConfigDownloadSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Config download complete.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.CongfigRemoveSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Config removed successfully.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigConfiguredSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Game successfully configured.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigUpdateSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Config update successful.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigRemoveConfirm:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Remove config query.png")));
                    CloseBtn.Visibility = Visibility.Hidden;
                    CloseBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigOverwriteConfirm:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Overwrite config.png")));
                    CloseBtn.Visibility = Visibility.Hidden;
                    CloseBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigDownloadError:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/could not download game config.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Visibility = Visibility.Hidden;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }

            Message.Text = message;
            Message.FontSize = fontSize;
        }

        public static bool? Show(Window owner, Type messageType, string message = "", double fontSize = 14)
        {
            var dialog = new MessageDialog(messageType, message, fontSize)
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            return dialog.ShowDialog();
        }
    }
}
