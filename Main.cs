using System;
using System.Reflection;
using Rage;
using LSPD_First_Response.Mod.API;
using TornadoCallouts.VersionChecker;
using System.Linq;
using TornadoCallouts.Callouts;

namespace TornadoCallouts
{
    public class Main : Plugin
    {
        internal static bool UsingUB { get; private set; }

        public override void Initialize()
        {
            Game.LogTrivial("[TornadoCallouts LOG]: Initialize() method called.");

            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("[TornadoCallouts LOG]: Attempting to load IniFile...");
            IniFile.LoadIniFile();
            Game.LogTrivial("[TornadoCallouts LOG]: IniFile.LoadIniFile() method has been called.");
            Game.LogTrivial($"[TornadoCallouts LOG]: Plugin TornadoCallouts {Assembly.GetExecutingAssembly().GetName().Version} by TornadoMac has been initialized!");
            Game.LogTrivial("[TornadoCallouts LOG]: Go on duty to fully load TornadoCallouts.");
            AppDomain.CurrentDomain.AssemblyResolve += LSPDFRResolveEventHandler;
        }


        public override void Finally()
        {
            Game.LogTrivial("[TornadoCallouts LOG]: TornadoCallouts has been cleaned up!");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                // Plugin check
                UsingUB = IsPluginLoaded("UltimateBackup");
                Game.LogTrivial($"[TornadoCallouts LOG]: UltimateBackup plugin loaded: {UsingUB}");

                // Register ini file settings for the callouts
                RegisterCallouts(IniFile.BarFight, IniFile.StolenVehicle, IniFile.Mugging, IniFile.ActiveStabbing, IniFile.DrugOverdose, IniFile.HelicopterAssistance);

                GameFiber.StartNew(delegate
                {
                    Game.LogTrivial("[TornadoCallouts LOG]: Checking for new plugin version...");
                    PluginCheck.IsUpdateAvailable();
                    Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "TornadoCallouts", "by ~b~TornadoMac~b~", "~s~|~s~ ~y~Version " + Assembly.GetExecutingAssembly().GetName().Version + "~s~ | Has ~g~Successfully Loaded!");
                    if (IniFile.HelpMessages)
                    {
                        Game.DisplayHelp("You can change all ~y~keys~w~ in the ~g~TornadoCallouts.ini~w~. Press ~b~" + IniFile.EndCall + "~w~ to end a callout.", 5000);
                    }
                });
            }
        }

        private static void RegisterCallouts(bool barFightEnabled, bool stolenVehicleEnabled, bool muggingEnabled, bool activeStabbingEnabled, bool drugOverdoseEnabled, bool helicopterAssistanceEnabled)
        {
            if (barFightEnabled) { Functions.RegisterCallout(typeof(Callouts.BarFight)); }
            if (stolenVehicleEnabled) { Functions.RegisterCallout(typeof(Callouts.StolenVehicle)); }
            if (muggingEnabled) { Functions.RegisterCallout(typeof(Callouts.Mugging)); }
            if (activeStabbingEnabled) { Functions.RegisterCallout(typeof(Callouts.ActiveStabbing)); }
            if (drugOverdoseEnabled) { Functions.RegisterCallout(typeof(Callouts.DrugOverdose)); }
            if (helicopterAssistanceEnabled) { Functions.RegisterCallout(typeof(Callouts.HelicopterAssistance)); }

           
            // CURRENTLY DISABLED CALLOUTS

            // if (trafficStopBackupRequiredEnabled) { Functions.RegisterCallout(typeof(Callouts.TrafficStopBackupRequired)); }
            // if (studentsFightingEnabled) { Functions.RegisterCallout(typeof(Callouts.StudentsFighting)); }
            // if (storeAltercationEnabled) { Functions.RegisterCallout(typeof(Callouts.StoreAltercation)); }

            Game.LogTrivial("[TornadoCallouts LOG]: All callouts were loaded successfully.");
        }

        private static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()))
                {
                    return assembly;
                }
            }
            return null;
        }

        public static bool IsPluginLoaded(string pluginName, Version minVersion = null)
        {
            var plugins = Functions.GetAllUserPlugins();
            if (plugins == null)
            {
                Game.LogTrivial("[TornadoCallouts LOG]: GetAllUserPlugins() returned null.");
                return false;
            }

            return plugins.Any(assembly =>
            {
                var assemblyName = assembly?.GetName();
                if (assemblyName == null) return false;

                return assemblyName.Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase) &&
                       (minVersion == null || assemblyName.Version.CompareTo(minVersion) >= 0);
            });
        }
    }
}
