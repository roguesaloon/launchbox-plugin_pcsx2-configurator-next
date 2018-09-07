using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    internal class SystemEventsPlugin : ISystemEventsPlugin
    {
        public void OnEventRaised(string eventType)
        {
            //Task.Run(() => MessageBox.Show(new Form { TopMost = true }, eventType));

            
            switch (eventType)
            {
                case SystemEventTypes.PluginInitialized:
                    OnPluginInitialized();
                    break;
                case SystemEventTypes.SelectionChanged:
                    OnSelectionChaged();
                    break;
            }
        }

        private static void OnPluginInitialized()
        {
            SettingsModel.Init();
        }

        private static void OnSelectionChaged()
        {
            var selectedGame = PluginHelper.StateManager.GetAllSelectedGames().FirstOrDefault();

            if (Configurator.GetIsValidForGame(selectedGame) && Configurator.IsGameConfigured(selectedGame) &&
                string.IsNullOrEmpty(selectedGame?.ConfigurationPath))
            {
                Configurator.SetConfigParamsForGame(selectedGame);
            }
        }
    }
}
