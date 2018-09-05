using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PCSX2_Configurator_Next
{
    internal class SystemEventsPlugin : ISystemEventsPlugin
    {
        public void OnEventRaised(string eventType)
        {
            //Task.Run(() => MessageBox.Show(new Form { TopMost = true }, eventType));
        }
    }
}
