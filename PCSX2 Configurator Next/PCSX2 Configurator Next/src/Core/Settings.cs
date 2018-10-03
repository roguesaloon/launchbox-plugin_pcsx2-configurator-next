using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;

namespace PCSX2_Configurator_Next.Core
{
    public class Settings
    {
        public static SettingsModel Model { get; } = new SettingsModel();
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class SettingsModel
    {
        public void Init()
        {
            var settingsFilePath = $"{Configurator.Model.PluginDir}\\Settings.ini";
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
                    var value = setting.GetValue(this).ToString();
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
                        setting.SetValue(this, value);
                    }
                    else
                    {
                        setting.SetValue(this, settingString);
                    }
                }
            }
        }

        public string GameConfigsDir { get; private set; } = Configurator.Model.Pcsx2InisDir;
        public string Pcsx2BuildName { get; private set; } = "PCSX2";
        public bool CopyLogSettings { get; private set; } = true;
        public bool CopyFolderSettings { get; private set; } = false;
        public bool CopyFileSettings { get; private set; } = true;
        public bool CopyWindowSettings { get; private set; } = true;
        public bool UseIndependantMemCards { get; private set; } = true;
        public bool ExposeAllConfigSettings { get; private set; } = false;
        public bool CopyVmSettingsFile { get; private set; } = true;
        public bool CopyGsdxSettingsFile { get; private set; } = true;
        public bool CopySpu2XSettingsFile { get; private set; } = false;
        public bool CopyLilyPadSettingsFile { get; private set; } = false;
    }
}
