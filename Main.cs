using System;
using System.Reflection;
using Rage;
using LSPD_First_Response.Mod.API;
using TornadoCallouts.VersionChecker;
using TornadoCallouts.Callouts;
using System.Runtime;

namespace TornadoCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Game.LogTrivial("TornadoCallouts Initialize() method called.");
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("Attempting to load IniFile...");
            IniFile.LoadIniFile();
            Game.LogTrivial("IniFile.LoadIniFile() method has been called.");
            Game.LogTrivial($"Plugin TornadoCallouts {Assembly.GetExecutingAssembly().GetName().Version} by TornadoMac has been initialized!");
            Game.LogTrivial("Go on duty to fully load TornadoCallouts.");
            AppDomain.CurrentDomain.AssemblyResolve += LSPDFRResolveEventHandler;
        }

        public override void Finally()
        {
            Game.LogTrivial("TornadoCallouts has been cleaned up!");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts(IniFile.BarFight, IniFile.StolenVehicle, IniFile.Mugging, IniFile.ActiveStabbing);
                GameFiber.StartNew(delegate
                {
                    Game.LogTrivial("Checking for a new TornadoCallouts version...");
                    PluginCheck.IsUpdateAvailable();
                    Game.DisplayNotification("TornadoCallouts by TornadoMac | ~y~Version " + Assembly.GetExecutingAssembly().GetName().Version + "~s~| Has ~g~Successfully Loaded!");

                    if (IniFile.HelpMessages)
                    {
                        Game.DisplayHelp("You can change all ~y~keys~w~ in the ~g~TornadoCallouts.ini~w~. Press ~b~" + IniFile.EndCall + "~w~ to end a callout.", 5000);
                    }
                    else { IniFile.HelpMessages = false; }


                });
            }
        }

        private static void RegisterCallouts(bool barFightEnabled, bool stolenVehicleEnabled, bool muggingEnabled, bool activeStabbingEnabled)
        {
           
            if (barFightEnabled) { Functions.RegisterCallout(typeof(Callouts.BarFight)); }
            if (stolenVehicleEnabled) { Functions.RegisterCallout(typeof(Callouts.StolenVehicle)); }
            if (muggingEnabled) { Functions.RegisterCallout(typeof(Callouts.Mugging)); }
            if (activeStabbingEnabled) { Functions.RegisterCallout(typeof(Callouts.ActiveStabbing)); }
           
            Game.LogTrivial("[TornadoCallouts] All callouts were loaded successfully.");
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

        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
