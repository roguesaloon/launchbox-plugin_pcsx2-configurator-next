using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PCSX2_Configurator_Next.Ui
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

            
            CloseBtnBtn.Click += delegate { Close(); };
            YesBtnBtn.Click += delegate { DialogResult = true; };
            NoBtnBtn.Click += delegate { DialogResult = false; };

            switch (messageType)
            {
                case Type.GenericYesNo:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Generic.png")));
                    CloseBtnGrd.Visibility = Visibility.Hidden;
                    break;
                case Type.Generic:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Generic.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    break;
                case Type.GenericSpecial:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Success.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    break;
                case Type.GenericError:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Error.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    Message.Stroke = new SolidColorBrush(Color.FromRgb(255, 14, 12));
                    CloseBtnImg.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Close (Red).png"));
                    break;
                case Type.ConfigDownloadSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Config download complete.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.CongfigRemoveSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Config removed successfully.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigConfiguredSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Game successfully configured.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigUpdateSuccess:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Config update successful.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigRemoveConfirm:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Remove config query.png")));
                    CloseBtnGrd.Visibility = Visibility.Hidden;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigOverwriteConfirm:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/Overwrite config.png")));
                    CloseBtnGrd.Visibility = Visibility.Hidden;
                    Message.Visibility = Visibility.Hidden;
                    break;
                case Type.ConfigDownloadError:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/Messages/could not download game config.png")));
                    YesBtnGrd.Visibility = Visibility.Hidden;
                    NoBtnGrd.Visibility = Visibility.Hidden;
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
