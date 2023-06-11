using System;
using System.Windows.Forms;
using Rage;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TornadoCallouts
{
    internal static class IniFile
    {
        internal static bool BarFight = true;
        internal static bool StolenVehicle = true;
        internal static bool Mugging = true;

        private static string path = "plugins/LSPDFR/TornadoCallouts.ini";
        private static InitializationFile ini = new InitializationFile(path);

        internal static void LoadIniFile()
        {
            try
            {
                Game.LogTrivial("[LOG]: Attempting to load config file from TornadoCallouts."); // Debugging line
                ini.Create();
                BarFight = ini.ReadBoolean("Callouts", "BarFight", true);
                StolenVehicle = ini.ReadBoolean("Callouts", "StolenVehicle", true);
                Mugging = ini.ReadBoolean("Callouts", "Mugging", true);
                Game.LogTrivial("[LOG]: Config file from TornadoCallouts has been loaded."); // Debugging line
            }
            catch (Exception e)
            {
                Game.LogTrivial("[LOG]: Exception while loading config file - " + e.Message);
            }
        }


        public static readonly string PluginVersion = "1.0.4";
    }

}