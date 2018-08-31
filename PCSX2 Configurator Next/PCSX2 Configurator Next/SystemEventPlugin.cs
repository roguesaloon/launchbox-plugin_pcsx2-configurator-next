using System.Threading.Tasks;
using System.Windows.Forms;
using Unbroken.LaunchBox.Plugins;

namespace PCSX2_Configurator_Next
{
    internal class SystemEventsPlugin : ISystemEventsPlugin
    {
        public void OnEventRaised(string eventType)
        {
            Task.Run(() => MessageBox.Show(new Form { TopMost = true }, eventType));
        }
    }
}
