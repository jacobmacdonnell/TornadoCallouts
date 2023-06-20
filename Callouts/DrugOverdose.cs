using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using System.Drawing;
using CalloutInterfaceAPI;
using System.Windows.Forms;
using LSPD_First_Response.Engine;

namespace TornadoCallouts.Callouts
{
    [CalloutInterface("Drug Overdose", CalloutProbability.Medium, "An individual has had a potential drug overdose", "Code 3", "LSPD")]


    public class DrugOverdose : Callout
    {

        private Ped Bystander;
        private Blip BystanderBlip;
        private Ped Victim;
        private Blip VictimBlip;
        private Vector3 SpawnPoint;
        private float heading;
        private int counter;
        private string malefemale;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(94.63f, -217.37f, 54.49f);
            heading = 53.08f;
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 500f);
            AddMinimumDistanceCheck(500f, SpawnPoint);
            CalloutMessage = "Potential Drug Overdose";
            CalloutPosition = SpawnPoint;
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 CITIZENS_REPORT_04 ASSISTANCE_REQUIRED_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_02", SpawnPoint);
            
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {

            CalloutInterfaceAPI.Functions.SendMessage(this, "Citizens are currently reporting an individual who has collapsed in a public area, exhibiting signs consistent with a drug overdose. The nature of the substance involved is unknown. Respond and assist the individual if EMS has not arrived yet.");

            // Victim ped

            Victim = new Ped(SpawnPoint, heading);
            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;

            VictimBlip = Victim.AttachBlip();
            VictimBlip.Color = System.Drawing.Color.CadetBlue;
            VictimBlip.IsRouteEnabled = true;

            if (Victim.IsMale)
                malefemale = "sir";
            else
                malefemale = "ma'am";

            // Bystander ped

            Bystander = new Ped(SpawnPoint, heading);
            Bystander.IsPersistent = true;
            Bystander.BlockPermanentEvents = true;

            BystanderBlip = Bystander.AttachBlip();
            BystanderBlip.Color = System.Drawing.Color.Yellow;

            if (Bystander.IsMale)
                malefemale = "sir";
            else
                malefemale = "ma'am";

            counter = 0;


            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if(Game.LocalPlayer.Character.DistanceTo(Bystander) <= 10f)
            {

                Game.DisplayHelp("Press ~y~Y~~ to talk to the bystander.", false);

                if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                {
                    counter++;

                    if(counter == 1)
                    {
                        Game.DisplaySubtitle("Player: Can you tell me what happened here" + malefemale + "?");
                    }
                    if(counter == 2)
                    {
                        Game.DisplaySubtitle("~y~Bystander: I don't know, I was walking by when I saw this person collapse to the ground, and then I called 9-11.");
                    }
                    if(counter == 3)
                    {
                        Game.DisplaySubtitle("Player: Okay, we beleive it may be a drug overdose, thank you for calling us" + malefemale + ", you are fee to go.");
                    }
                    if(counter == 4)
                    {
                        Game.DisplaySubtitle("~y~Bystander: Of course. I hope they are okay, bye.");
                    }
                    if(counter == 5)
                    {
                        Game.DisplaySubtitle("Conversation has ended!");
                        
                        Bystander.Tasks.Wander();
                    }
                }
            }

            if (Victim.IsCuffed || Victim.IsDead || !Victim.Exists() || Game.IsKeyDown(IniFile.EndCall) || Game.LocalPlayer.Character.IsDead)
            {
                End();
            }

                {
                    Game.DisplayNotification("Callout Ended. ~g~We Are Code 4.");
                }
        }



        public override void End()
        {
            base.End();

            if (Victim.Exists())
            {
                Victim.Dismiss();
            }
            if (VictimBlip.Exists())
            {
                VictimBlip.Delete();
            }   
            
            if (Bystander.Exists())
            {
                Bystander.Dismiss();
            }
            if (BystanderBlip.Exists())
            {
                BystanderBlip.Delete();
            }


            Game.LogTrivial("TornadoCallouts | Drug Overdose | Has Cleaned Up.");
        }
    }
}
