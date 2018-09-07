using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    internal class SystemEventsPlugin : ISystemEventsPlugin
    {
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public void OnEventRaised(string eventType)
        {
            //Task.Run(() => MessageBox.Show(new Form { TopMost = true }, eventType));

            switch (eventType)
            {
                case SystemEventTypes.PluginInitialized:
                    OnPluginInitialized();
                    break;
                case SystemEventTypes.SelectionChanged:
                    OnSelectionChanged();
                    break;
                case SystemEventTypes.GameStarting:
                    break;
            }
        }

        private static void OnPluginInitialized()
        {
            SettingsModel.Init();
        }

        private static void OnSelectionChanged()
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
