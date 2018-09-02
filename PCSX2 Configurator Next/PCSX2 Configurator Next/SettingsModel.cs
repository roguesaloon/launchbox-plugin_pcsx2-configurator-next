using IniParser;

namespace PCSX2_Configurator_Next
{
    public static class SettingsModel
    {
        static SettingsModel()
        {
            // TODO: Auto Generate Settings File if Not Exist
            var iniParser = new FileIniDataParser();
            var settingsFile = iniParser.ReadFile(Configurator.PluginDirectory + "\\Settings.ini");

            // TODO?: Use reflection to get all settings
            GameConfigsDir = settingsFile["PCSX2_Configurator"]["GameConfigsDir"];
            CopyLogSettings = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyLogSettings"]);
            CopyFolderSettings = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyFolderSettings"]);
            CopyFileSettings = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyFileSettings"]);
            CopyWindowSettings = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyWindowSettings"]);
            UseIndependantMemCards = bool.Parse(settingsFile["PCSX2_Configurator"]["UseIndependantMemCards"]);
            ExposeAllConfigSettings = bool.Parse(settingsFile["PCSX2_Configurator"]["ExposeAllConfigSettings"]);
            CopyVmSettingsFile = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyVmSettingsFile"]);
            CopyGsdxSettingsFile = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyGsdxSettingsFile"]);
            CopySpu2XSettingsFile = bool.Parse(settingsFile["PCSX2_Configurator"]["CopySpu2XSettingsFile"]);
            CopyLilyPadSettingsFile = bool.Parse(settingsFile["PCSX2_Configurator"]["CopyLilyPadSettingsFile"]);
        }

        public static string GameConfigsDir { get; }
        public static bool CopyLogSettings { get; }
        public static bool CopyFolderSettings { get; }
        public static bool CopyFileSettings { get; }
        public static bool CopyWindowSettings { get; }
        public static bool UseIndependantMemCards { get; }
        public static bool ExposeAllConfigSettings { get; }
        public static bool CopyVmSettingsFile { get; }
        public static bool CopyGsdxSettingsFile { get; }
        public static bool CopySpu2XSettingsFile { get; }
        public static bool CopyLilyPadSettingsFile { get; }
    }
}
