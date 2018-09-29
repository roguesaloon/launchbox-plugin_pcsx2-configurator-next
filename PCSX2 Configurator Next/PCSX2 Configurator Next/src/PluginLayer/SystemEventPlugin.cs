using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using PCSX2_Configurator_Next.ConfiguratorLayer;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;


namespace PCSX2_Configurator_Next.PluginLayer
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
            DownloadSvn();
            SettingsModel.Init();
        }

        private static void OnSelectionChanged()
        {
            var selectedGame = PluginHelper.StateManager.GetAllSelectedGames().FirstOrDefault();
            Configurator.ApplyGameConfigParams(selectedGame);
        }

        private static void DownloadSvn()
        {
            var svnDir = $"{ConfiguratorModel.LaunchBoxDir}\\SVN";
            var svnZip = $"{ConfiguratorModel.LaunchBoxDir}\\SVN.zip";

            if (Directory.Exists(svnDir)) return;
            try
            {
                new WebClient().DownloadFile("https://www.visualsvn.com/files/Apache-Subversion-1.10.2.zip", svnZip);
                ZipFile.ExtractToDirectory(svnZip, svnDir);
                File.Delete(svnZip);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
