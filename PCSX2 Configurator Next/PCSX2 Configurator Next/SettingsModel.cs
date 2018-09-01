namespace PCSX2_Configurator_Next
{
    public static class SettingsModel
    {
        static SettingsModel()
        {
            GameConfigsDir = "D:\\Emulators\\PCSX2\\inis";
        }

        public static string GameConfigsDir { get; }
    }
}
