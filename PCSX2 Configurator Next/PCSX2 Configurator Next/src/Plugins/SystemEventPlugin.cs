using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
#if DEBUG
using System.Threading;
#endif
using PCSX2_Configurator_Next.Core;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;


namespace PCSX2_Configurator_Next.Plugins
{
    internal class SystemEventsPlugin : ISystemEventsPlugin
    {
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public void OnEventRaised(string eventType)
        {
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
#if DEBUG
            Thread.Sleep(3000);
#endif
            Configurator.Initialize();
            DownloadSvn();

            Console.WriteLine("PCSX2 Configurator: Initialized");
        }

        private static void OnSelectionChanged()
        {
            var selectedGame = PluginHelper.StateManager.GetAllSelectedGames().FirstOrDefault();
            Configurator.ApplyGameConfigParams(selectedGame);
        }

        private static void DownloadSvn()
        {
            var svnDir = Configurator.Model.SvnDir;
            var svnZip = $"{Configurator.Model.LaunchBoxDir}\\SVN.zip";

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
