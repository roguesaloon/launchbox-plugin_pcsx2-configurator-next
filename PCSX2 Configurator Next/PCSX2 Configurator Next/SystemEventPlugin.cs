using System.Diagnostics.CodeAnalysis;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    internal class SystemEventsPlugin : ISystemEventsPlugin
    {
        [SuppressMessage("ReSharper", "InvertIf")]
        public void OnEventRaised(string eventType)
        {
            //Task.Run(() => MessageBox.Show(new Form { TopMost = true }, eventType));

            if (eventType == SystemEventTypes.PluginInitialized)
            {
                SettingsModel.Init();
                Configurator.Init();
            }
        }
    }
}
