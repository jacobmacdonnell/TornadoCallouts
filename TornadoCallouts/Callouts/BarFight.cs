using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;
using System.Drawing;


namespace TornadoCallouts.Callouts
{

    [CalloutInterface("Bar Fight", CalloutProbability.Medium, "Two males currently fighting", "Code 3", "LSPD")]
    public class BarFight : Callout

    {
        private Ped Suspect1, Suspect2;
        private Blip SuspectBlip1, SuspectBlip2;
        private Vector3 Spawnpoint;
        private bool FightCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            Spawnpoint = new Vector3(127.44f, -1306.12f, 29.23f); // Vanilla Unicorn Strip Club Location
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 50f);
            AddMinimumDistanceCheck(100f, Spawnpoint);
            CalloutPosition = Spawnpoint;

            CalloutInterfaceAPI.Functions.SendMessage(this, "Staff at the Vanilla Unicorn bar are reporting two males currently fighting outside. Approach with caution.");
            
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            // creates suspect 1 at the Vanilla Unicorn Strip Club
            Suspect1 = new Ped("a_m_y_mexthug_01", Spawnpoint, 180f); 
            Suspect1.IsPersistent = true;
            Suspect1.BlockPermanentEvents = true;
            Suspect1.Alertness += 50;
            Suspect1.CanOnlyBeDamagedByPlayer = true;
            Suspect1.Health += 50;

            SuspectBlip1 = Suspect1.AttachBlip(); // attach a blip to suspect 1
            SuspectBlip1.Color = System.Drawing.Color.Yellow;
            SuspectBlip1.IsRouteEnabled = true;

            // creates suspect 2 at the Vanilla Unicorn Strip Club
            Suspect2 = new Ped("g_m_importexport_01", Spawnpoint, 180f);
            Suspect2.IsPersistent = true;
            Suspect2.BlockPermanentEvents = true;
            Suspect2.Alertness += 50;
            Suspect2.CanOnlyBeDamagedByPlayer = true;
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
            // Check if both suspects are dead, they have stopped fighting, are cuffed, or player is dead.

            bool v = Suspect1.IsCuffed || Suspect2.IsCuffed; // Suspect 1 or 2 is cuffed.
            bool n = Suspect1.IsCuffed && Suspect2.IsCuffed; // Suspect 1 and 2 are cuffed.
            if (Suspect1.IsDead && Suspect2.IsDead || Game.LocalPlayer.Character.IsDead || !Suspect1.Exists() || !Suspect2.Exists() || v || n)
            {
                Game.DisplayNotification("Callout Ended. ~g~We Are Code 4. DUBUG CHECK");
                End();
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
           
            // Removes blips for suspects 1 & 2

            if (SuspectBlip1.Exists())
            {
                SuspectBlip1.Delete();
            }
            if (SuspectBlip2.Exists())
            {
                SuspectBlip2.Delete();
            }

            Game.LogTrivial("TornadoCallouts Bar Fight Has Cleaned Up.");
        }
    }
}
