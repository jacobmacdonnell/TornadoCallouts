using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Reflection;
using System.Runtime.Remoting.Channels;

namespace TornadosCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("Plugin TornadosCallouts" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "by TornadoMac has been initialized!");
            Game.LogTrivial("Go on duty to fully load TornadosCallouts.");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);

        }
        public override void Finally()
        {
            Game.LogTrivial("TornadosCallouts has been cleaned up.");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts();
                Game.DisplayNotification("TornadosCallouts by TornadoMac | Version 1.0.0 - Alpha | Was Successfully Loaded!");
            }
        }

        private static void RegisterCallouts()
        {

        }

        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
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
