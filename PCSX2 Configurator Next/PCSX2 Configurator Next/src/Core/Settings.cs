using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using IniParser;

namespace PCSX2_Configurator_Next.Core
{
    public static class Settings
    {
        public static SettingsModel Model { get; } = new SettingsModel();

        private static readonly string SettingsFilePath =
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Settings.ini";

        private static readonly FileIniDataParser IniParser = new FileIniDataParser();

        public static void Initialize()
        {
            CreateEmptySettingsFile();
            WriteModelToSettingsFile();
            ReadModelFromSettingsFile();
        }

        public static void ReplaceValues(params KeyValuePair<string,string>[] valuePairs)
        {
            var settingsFileString = File.ReadAllText(SettingsFilePath);

            foreach (var valuePair in valuePairs)
            {
                settingsFileString = settingsFileString.Replace(valuePair.Key, valuePair.Value);
            }

            File.WriteAllText(SettingsFilePath, settingsFileString, Encoding.UTF8);
            ReadModelFromSettingsFile();
        }

        private static void CreateEmptySettingsFile()
        {
            if (!File.Exists(SettingsFilePath))
            {
                File.CreateText(SettingsFilePath).Dispose();
            }
        }

        private static void WriteModelToSettingsFile()
        {
            var settingsFile = IniParser.ReadFile(SettingsFilePath);
            var settings = settingsFile["PCSX2_Configurator"];

            var properties = typeof(SettingsModel).GetProperties();
            foreach (var property in properties)
            {
                if (settings.ContainsKey(property.Name)) continue;

                var value = GetModelProperty(property);
                settings[property.Name] = value;
            }

            IniParser.WriteFile(SettingsFilePath, settingsFile, Encoding.UTF8);
        }

        private static void ReadModelFromSettingsFile()
        {
            var settingsFile = IniParser.ReadFile(SettingsFilePath);
            var settings = settingsFile["PCSX2_Configurator"];

            var properties = typeof(SettingsModel).GetProperties();
            foreach (var property in properties)
            {
                var data = settings[property.Name];
                SetModelProperty(property, data);
            }
        }

        private static string GetModelProperty(PropertyInfo property)
        {
            var value = property.GetValue(Model).ToString();
            value = property.PropertyType == typeof(bool) ? value.ToLower() : value;

            return value;
        }

        private static void SetModelProperty(PropertyInfo property, string data)
        {
            object value;
            if (property.PropertyType == typeof(bool))
            {
                value = bool.Parse(data);
            }
            else
            {
                value = data;
            }

            property.SetValue(Model, value, BindingFlags.NonPublic | BindingFlags.Instance, 
                null, null, CultureInfo.CurrentCulture);
        }
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class SettingsModel
    {
        public string GameConfigsDir { get; private set; } = "inis";
        public string Pcsx2BuildTitle { get; private set; } = "PCSX2 1.5.0";
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
