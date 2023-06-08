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

    [CalloutInfo("BarFight", CalloutProbability.High )]
    public class BarFight : Callout
    {
        private Ped Suspect;
        private Blip SuspectBlip;
        private Vector3 Spawnpoint;
        private bool FightCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            Spawnpoint = World.
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);
            CalloutMessage = "Bar Fight in Progress";
            CalloutPosition = Spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ASSAULT_01 IN_OR_ON_POSITION", Spawnpoint);
            
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Suspect = Ped(

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();



        }

        public override void End()
        {
            base.End();



        }
    }
}
