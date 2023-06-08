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
            Spawnpoint = new Vector3(127.44f, -1306.12f, 29.23f); // Vanilla Unicorn Strip Club Location
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);
            CalloutMessage = "Bar Fight in Progress";
            CalloutPosition = Spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ASSAULT_01 IN_OR_ON_POSITION", Spawnpoint);
            
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            // creates suspect 1 at the Vanilla Unicorn Strip Club
            Suspect1 = new Ped("a_m_y_mexthug_01", Spawnpoint, 180f); 
            Suspect1.IsPersistent = true;
            Suspect1.BlockPermanentEvents = true;
            Suspect1.CanRagdoll = false;
            Suspect1.Alertness += 50;
            Suspect1.CanOnlyBeDamagedByPlayer = true;
            Suspect1.MaxHealth = 200;
            Suspect1.Health += 50;

            SuspectBlip1 = Suspect1.AttachBlip(); // attach a blip to suspect 1
            SuspectBlip1.Color = System.Drawing.Color.Yellow;
            SuspectBlip1.IsRouteEnabled = true;

            // creates suspect 2 at the Vanilla Unicorn Strip Club
            Suspect2 = new Ped("g_m_importexport_01", Spawnpoint, 180f);
            Suspect2.IsPersistent = true;
            Suspect2.CanRagdoll = false;
            Suspect2.Alertness += 50;
            Suspect2.CanOnlyBeDamagedByPlayer = true;
            Suspect2.MaxHealth = 200;
            Suspect2.Health += 50;

            SuspectBlip2 = Suspect2.AttachBlip(); // attach a blip to suspect 2
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

            // Check if both suspects are dead, they have stopped fighting, or both are cuffed.
            bool v = (Suspect1.IsCuffed && Suspect2.IsCuffed);
            if (FightCreated && ((Suspect1.IsDead && Suspect2.IsDead) || (!Suspect1.IsInCombat && !Suspect2.IsInCombat) || v))
            {
                End();

                // Display code 4 notification with green "Code 4" text
                string notificationText = "~g~Code 4~s~: Situation resolved";
                Game.DisplayNotification(notificationText);
            }


        }

        public override void End()
        {
            base.End();

            // Dismisses suspects 1 & 2
            if (Suspect1.Exists())
            {
                Suspect1.Dismiss();
            }
            if (Suspect2.Exists())
            {
                Suspect2.Dismiss();
            }
           
            // Removes blips for Suspects 1 & 2
            if (SuspectBlip1.Exists())
            {
                SuspectBlip1.Delete();
            }
            if (SuspectBlip2.Exists())
            {
                SuspectBlip2.Delete();
            }

            Game.LogTrivial("TornadosCallouts Bar Fight Cleaned Up.");
        }
    }
}
