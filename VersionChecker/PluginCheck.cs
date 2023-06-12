using System;
using Rage;
using System.Net;

namespace TornadoCallouts.VersionChecker
{
    public class PluginCheck
    {
        public static bool IsUpdateAvailable()
        {
            string curVersion = IniFile.PluginVersion;
            Uri latestVersionUri = new Uri("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44231&textOnly=1");
            WebClient webClient = new WebClient();
            string receivedData = string.Empty;
            try
            {
                receivedData = webClient.DownloadString(latestVersionUri).Trim();
            }
            catch (WebException)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~TornadoCallouts Warning", "~r~Failed to check for an update", "Please make sure you are ~y~connected~w~ to the internet or try to ~y~reload~w~ the plugin.");
                Game.Console.Print();
                Game.Console.Print("================================================== TornadoCallouts ===================================================");
                Game.Console.Print();
                Game.Console.Print("[WARNING]: Failed to check for an update.");
                Game.Console.Print("[LOG]: Please make sure you are connected to the internet or try to reload the plugin.");
                Game.Console.Print();
                Game.Console.Print("================================================== TornadoCallouts ===================================================");
                Game.Console.Print();
                return false;
            }
            if (receivedData != IniFile.PluginVersion)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~TornadoCallouts Warning", "~y~A new Update is available!", "Current Version: ~r~" + curVersion + "~w~<br>New Version: ~o~" + receivedData + "<br>~r~Please update to the latest build!");
                Game.Console.Print();
                Game.Console.Print("================================================== TornadoCallouts ===================================================");
                Game.Console.Print();
                Game.Console.Print("[WARNING]: A new version of TornadoCallouts is available! Update to the latest build or play on your own risk.");
                Game.Console.Print("[LOG]: Current Version:  " + curVersion);
                Game.Console.Print("[LOG]: New Version:  " + receivedData);
                Game.Console.Print();
                Game.Console.Print("================================================== TornadoCallouts ===================================================");
                Game.Console.Print();
                return true;
            }
            else
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "", "Detected the ~g~latest~w~ build of ~y~TornadoCallouts~w~!");
                return false;
            }
        }
    }
}