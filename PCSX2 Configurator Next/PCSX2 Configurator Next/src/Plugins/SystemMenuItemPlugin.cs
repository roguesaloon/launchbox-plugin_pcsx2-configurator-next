using System;
using System.Drawing;
using Unbroken.LaunchBox.Plugins;

namespace PCSX2_Configurator_Next.Plugins
{
    internal class SystemMenuItemPlugin : ISystemMenuItemPlugin
    {
        public string Caption => "PCSX2 Configurator";

        public Image IconImage => null;

        public bool ShowInLaunchBox => false;

        public bool ShowInBigBox => false;

        public bool AllowInBigBoxWhenLocked => false;

        public void OnSelected()
        {
            throw new NotImplementedException();
        }
    }
}
