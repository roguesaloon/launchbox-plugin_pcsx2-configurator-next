using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
            GenericYesNo, Generic, Success, Error
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
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/MsgBox_Generic.png")));
                    CloseBtn.Visibility = Visibility.Hidden;
                    CloseBtn.IsEnabled = false;
                    break;
                case Type.Generic:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/MsgBox_Generic.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    break;
                case Type.Success:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/MsgBox_Success.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    break;
                case Type.Error:
                    Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/MsgBox_Error.png")));
                    YesBtn.Visibility = Visibility.Hidden;
                    YesBtn.IsEnabled = false;
                    NoBtn.Visibility = Visibility.Hidden;
                    NoBtn.IsEnabled = false;
                    Message.Stroke = new SolidColorBrush(Color.FromRgb(255, 9, 9));
                    CloseBtnImg.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Plugins/PCSX2 Configurator Next/Assets/Images/MsgBoxBtn_Red.png"));
                    CloseBtnTxt.Stroke = new SolidColorBrush(Color.FromRgb(255, 9, 9));
                    ((DropShadowEffect) CloseBtnTxt.Effect).Color = Color.FromRgb(130, 20, 20);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }

            Message.Text = message;
            Message.FontSize = fontSize;
        }

        public static bool? Show(Window owner, Type messageType, string message, double fontSize = 14)
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
