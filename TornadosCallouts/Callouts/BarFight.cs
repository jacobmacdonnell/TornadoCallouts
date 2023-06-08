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
        private Ped Suspect1, Suspect2;
        private Blip SuspectBlip1, SuspectBlip2;
        private Vector3 Spawnpoint;
        private bool FightCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            Spawnpoint = new Vector3(929.79f, -948.78f, 7.14f); // YellowJack in the County
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);
            CalloutMessage = "Bar Fight in Progress";
            CalloutPosition = Spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ASSAULT_01 IN_OR_ON_POSITION", Spawnpoint);
            
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            // creates suspect 1 at the YellowJack Bar
            Suspect1 = new Ped("a_m_y_mexthug_01", Spawnpoint, 180f); 
            Suspect1.IsPersistent = true;
            Suspect1.BlockPermanentEvents = true;

            SuspectBlip1 = Suspect1.AttachBlip(); // attach a blip to the suspect
            SuspectBlip1.Color = System.Drawing.Color.Yellow;
            SuspectBlip1.IsRouteEnabled = true;

            // creates suspect 2 at the YellowJack Bar
            Suspect2 = new Ped("a_m_m_tramp_01", Spawnpoint, 180f);
            Suspect2.IsPersistent = true;
            Suspect2.BlockPermanentEvents = true;

            SuspectBlip2 = Suspect2.AttachBlip(); // attach a blip to the suspect
            SuspectBlip2.Color = System.Drawing.Color.Yellow;

            FightCreated = false;

            return base.OnCalloutAccepted();
        }


        public override void Process()
        {
            base.Process();

            if (!FightCreated && Game.LocalPlayer.Character.DistanceTo(Suspect1) <= 50f)
            {
                // Have Suspect1 fight against Suspect2
                Suspect1.Tasks.FightAgainst(Suspect2);

                // Have Suspect2 fight against Suspect1
                Suspect2.Tasks.FightAgainst(Suspect1);

                FightCreated = true;
            }
            Suspect1
            if (FightCreated && !Suspect1.Tasks.FightAgainst(Suspect2))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();

            if (Suspect1.Exists())
            {
                Suspect1.Dismiss();
            }
            if (Suspect2.Exists())
            {
                Suspect2.Dismiss();
            }

        }
    }
}
