using System.Windows;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow
    {
        private IGame _selectedGame;

        public ConfigWindow(IGame selectedGame = null)
        {
            _selectedGame = selectedGame;
            InitializeComponent();
        }
    }
}
