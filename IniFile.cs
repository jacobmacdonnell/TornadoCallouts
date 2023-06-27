using System;
using System.Reflection;
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
        internal static bool ActiveStabbing = true;
        internal static bool TrafficStopBackupRequired = true;
        internal static bool DrugOverdose = true;
        internal static bool StudentsFighting = true;
        internal static bool StoreAltercation = true;


        internal static bool HelpMessages = true;
        internal static Keys EndCall = Keys.End;
        internal static Keys Dialog = Keys.Y;


        private static string path = "plugins/LSPDFR/TornadoCallouts.ini";
        private static InitializationFile ini = new InitializationFile(path);

        internal static void LoadIniFile()
        {
            try
            {
                Game.LogTrivial("[TornadoCallouts LOG]: Attempting to load ini file from TornadoCallouts."); // Debugging line
                ini.Create();
                
                BarFight = ini.ReadBoolean("Callouts", "BarFight", true);
                StolenVehicle = ini.ReadBoolean("Callouts", "StolenVehicle", true);
                Mugging = ini.ReadBoolean("Callouts", "Mugging", true);
                ActiveStabbing = ini.ReadBoolean("Callouts", "ActiveStabbing", true);
                TrafficStopBackupRequired = ini.ReadBoolean("Callouts", "TrafficStopBackupRequired", true);
                DrugOverdose = ini.ReadBoolean("Callouts", "DrugOverdose", true);
                StudentsFighting = ini.ReadBoolean("Callouts", "StudentsFighting", true);
                StoreAltercation = ini.ReadBoolean("Callouts", "StoreAltercation", true);

                // Keys
                EndCall = ini.ReadEnum("Keys", "EndCall", Keys.End);
                Dialog = ini.ReadEnum("Keys", "Dialog", Keys.Y);

                Game.LogTrivial("[TornadoCallouts LOG]: Config file from TornadoCallouts has been loaded."); // Debugging line
            }
                catch (Exception e)
            {
                Game.LogTrivial("[TornadoCallouts LOG]: Exception while loading  Ini file - " + e.Message);
            }
        }
        public static readonly string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}