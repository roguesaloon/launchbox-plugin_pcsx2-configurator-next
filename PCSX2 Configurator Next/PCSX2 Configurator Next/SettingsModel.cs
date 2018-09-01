using IniParser;

namespace PCSX2_Configurator_Next
{
    public static class SettingsModel
    {
        static SettingsModel()
        {
            var iniParser = new FileIniDataParser();
            var settingsFile = iniParser.ReadFile(Configurator.PluginDirectory + "\\Settings.ini");

            GameConfigsDir = settingsFile["PCSX2_Configurator"]["GameConfigsDir"];
        }

        public static string GameConfigsDir { get; }
    }
}
