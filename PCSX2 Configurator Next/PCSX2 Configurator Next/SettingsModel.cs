using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;

namespace PCSX2_Configurator_Next
{
    public static class SettingsModel
    {
        public static void Init()
        {
            var settingsFilePath = $"{ConfiguratorModel.PluginDir}\\Settings.ini";
            var settings = typeof(SettingsModel).GetProperties();
            var iniParser = new FileIniDataParser();

            if (!File.Exists(settingsFilePath))
            {
                GeneratSettingsFile();
            }
            else
            {
                ReadFromSettingsFile();
            }


            void GeneratSettingsFile()
            {
                var settingsFile = new IniData();

                foreach (var setting in settings)
                {
                    var value = setting.GetValue(null).ToString();
                    value = setting.PropertyType == typeof(bool) ? value.ToLower() : value;
                    settingsFile["PCSX2_Configurator"][setting.Name] = value;
                }

                iniParser.WriteFile(settingsFilePath, settingsFile, Encoding.UTF8);
            }

            void ReadFromSettingsFile()
            {
                var settingsFile = iniParser.ReadFile(settingsFilePath);

                foreach (var setting in settings)
                {
                    var settingString = settingsFile["PCSX2_Configurator"][setting.Name];

                    if (setting.PropertyType == typeof(bool))
                    {
                        var value = bool.Parse(settingString);
                        setting.SetValue(null, value);
                    }
                    else
                    {
                        setting.SetValue(null, settingString);
                    }
                }
            }
        }

        public static string GameConfigsDir { get; internal set; } = ConfiguratorModel.Pcsx2InisDir;
        public static bool CopyLogSettings { get; internal set; } = true;
        public static bool CopyFolderSettings { get; internal set; } = false;
        public static bool CopyFileSettings { get; internal set; } = true;
        public static bool CopyWindowSettings { get; internal set; } = true;
        public static bool UseIndependantMemCards { get; internal set; } = true;
        public static bool ExposeAllConfigSettings { get; internal set; } = false;
        public static bool CopyVmSettingsFile { get; internal set; } = true;
        public static bool CopyGsdxSettingsFile { get; internal set; } = true;
        public static bool CopySpu2XSettingsFile { get; internal set; } = false;
        public static bool CopyLilyPadSettingsFile { get; internal set; } = false;
    }
}
