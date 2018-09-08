using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
            DownloadSvn();
            SettingsModel.Init();
        }

        private static void OnSelectionChanged()
        {
            var selectedGame = PluginHelper.StateManager.GetAllSelectedGames().FirstOrDefault();

            if (Configurator.GetIsValidForGame(selectedGame))
            {
                if (Configurator.IsGameConfigured(selectedGame) &&
                    string.IsNullOrEmpty(selectedGame?.ConfigurationPath))
                {
                    Configurator.SetGameConfigParams(selectedGame);
                }

                if(!Configurator.IsGameConfigured(selectedGame) && !string.IsNullOrEmpty(selectedGame?.ConfigurationPath))
                {
                    Configurator.ClearGameConfigParams(selectedGame);
                }
            }
        }

        private static void DownloadSvn()
        {
            if (Directory.Exists(Configurator.LaunchBoxDirectory + "\\SVN")) return;
            try
            {
                new WebClient().DownloadFile("https://www.visualsvn.com/files/Apache-Subversion-1.10.2.zip", Configurator.LaunchBoxDirectory + "\\SVN.zip");
                ZipFile.ExtractToDirectory(Configurator.LaunchBoxDirectory + "\\SVN.zip", Configurator.LaunchBoxDirectory + "\\SVN");
                File.Delete(Configurator.LaunchBoxDirectory + "\\SVN.zip");
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
