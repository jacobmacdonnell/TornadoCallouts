using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using System.Drawing;


namespace TornadosCallouts.Callouts
{

    [CalloutInfo("IntoxicatedPerson", CalloutProbability.High)]
    public class IntoxicatedPerson : Callout
    {
        private Ped Suspect;
        private Blip SuspectBlip;
        private Vector3 Spawnpoint;

        public override bool OnBeforeCalloutDisplayed()
        {
            Spawnpoint = new Vector3(127.44f, -1306.12f, 29.23f); // ADD A NEW SPAWNPOINT HERE
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);
            CalloutMessage = "There Is a Publicly Intoxicted Person";
            CalloutPosition = Spawnpoint;
            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_04 CRIME_DISTURBING_THE_PEACE_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_02_02", Spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {















            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();















        }

        public override void End()
        {
            base.End();

            // Dismisses suspect

            if (Suspect.Exists())
            {
                Suspect.Dismiss();
            }

            // Removes suspect blip

            if (SuspectBlip.Exists())
            {
                SuspectBlip.Delete();
            }

        }  
    }
}

