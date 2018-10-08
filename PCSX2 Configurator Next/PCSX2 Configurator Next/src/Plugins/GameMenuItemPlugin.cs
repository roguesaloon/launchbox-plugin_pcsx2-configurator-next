using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PCSX2_Configurator_Next.Core;
using PCSX2_Configurator_Next.Ui;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next.Plugins
{
    internal class GameMenuItemPlugin : IGameMenuItemPlugin
    {
        public bool SupportsMultipleGames => false;

        public string Caption => "PCSX2 Configurator";

        public Image IconImage
        {
            get
            {
                var icon = Icon.ExtractAssociatedIcon(Configurator.Model.Pcsx2AbsoluteAppPath);
                return icon?.ToBitmap();
            }
        }

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => false;

        public bool GetIsValidForGame(IGame selectedGame)
        {
            Configurator.ApplyGameConfigParams(selectedGame);
            return GameHelper.IsValidForGame(selectedGame);
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
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Icon = IconImage != null ? 
                    Imaging.CreateBitmapSourceFromHBitmap(((Bitmap) IconImage).GetHbitmap(), IntPtr.Zero,
                    Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(IconImage.Width, IconImage.Height)) : null
            };
            configWindow.Closing += (sender, args) =>
            {
                configWindow.Owner.IsEnabled = true;
                configWindow.Owner.Focus();
            };
            configWindow.Owner.IsEnabled = false;
            configWindow.Show();

            Console.WriteLine("PCSX2 Configurator: Config Window Opened");
        }

        public void OnSelected(IGame[] selectedGames)
        {
        }
    }
}
