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
using Rage.Native;

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
        private bool ArrivalNotificationSent = false;
        private bool ConversationFinished = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(94.63f, -217.37f, 54.49f);
            heading = 53.08f;
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 100f);
            AddMinimumDistanceCheck(50f, SpawnPoint);
            CalloutMessage = "Potential Drug Overdose";
            CalloutPosition = SpawnPoint;
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 CITIZENS_REPORT_04 ASSISTANCE_REQUIRED_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_02", SpawnPoint);
            
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("[TornadoCallouts LOG]: Drug Overdose callout accepted.");

            CalloutInterfaceAPI.Functions.SendMessage(this, "Citizens are currently reporting an individual who has collapsed in a public area, exhibiting signs consistent with a drug overdose. The nature of the substance involved is unknown. Respond and assist the individual if EMS has not arrived yet.");

            // Spawn Victim ped
            Victim = new Ped(SpawnPoint, heading);

            Game.LogTrivial("[TornadoCallouts LOG]: Victim ped created");

            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Victim.CanRagdoll = true;
            Victim.Health = 0;
            VictimBlip = Victim.AttachBlip();
            VictimBlip.Color = System.Drawing.Color.CadetBlue;
            VictimBlip.IsRouteEnabled = true;

            Game.LogTrivial("[TornadoCallouts LOG]: Victim blip created");
            
            // Spawn Bystander ped
            Bystander = new Ped(SpawnPoint, heading);
            Bystander.IsPersistent = true;
            Bystander.BlockPermanentEvents = true;
            BystanderBlip = Bystander.AttachBlip();
            BystanderBlip.Color = System.Drawing.Color.Yellow;

            if (Bystander.IsMale)
                malefemale = "sir";
            else
                malefemale = "ma'am";

            // Make the bystander do the waving animation
            Bystander.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1f, AnimationFlags.Loop);

            counter = 0;


            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character.DistanceTo(Victim) <= 250f && !ArrivalNotificationSent)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "On arrival, call EMS and speak with the bystander to get more info.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "When you arrive on scene, call EMS and speak with the bystander to see what happened.");

                ArrivalNotificationSent = true;
            }

            if (Game.LocalPlayer.Character.DistanceTo(Bystander) <= 10f && !ConversationFinished)
            {
                Game.DisplayHelp("Press ~y~Y ~s~to talk to the bystander.", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    counter++;

                    if (counter == 1)
                    {
                        // Stop the waving animation
                        Bystander.Tasks.ClearImmediately();


                        //Turn_Ped_To_Face_Entity
                        NativeFunction.Natives.x5AD23D40115353AC(Bystander, Game.LocalPlayer.Character, -1); 


                        // Calculate the direction to face
                             //  Vector3 directionToFace = Game.LocalPlayer.Character.Position - Bystander.Position;
                             // float headingToFacePlayer = MathHelper.ConvertDirectionToHeading(directionToFace);

                        // Set the heading of the bystander
                             // Bystander.Heading = headingToFacePlayer;

                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemale + "?");
                    }
                    if (counter == 2)
                    {
                        Game.DisplaySubtitle("~y~Bystander~s~: I don't know, I was walking by when I saw this person collapse to the ground, and then I called 9-11.");
                    }
                    if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Okay, we believe it may be a drug overdose, thank you for calling us " + malefemale + ", you are free to go.");
                    }
                    if (counter == 4)
                    {
                        Game.DisplaySubtitle("~y~Bystander~s~: Of course. I hope they are okay, bye.");
                    }
                    if (counter == 5)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended. Complete your investigation then end the callout.");

                        Bystander.Tasks.Wander();

                        if (BystanderBlip.Exists())
                        {
                            BystanderBlip.Delete();
                        }

                        GameFiber.Wait(5000);

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "~s~Press your ~g~'END'~s~ Callout Key when you are finished.");
                        
                        
                        // Set the conversation as finished
                        ConversationFinished = true;
                    }
                }
            }

            if (Victim.IsCuffed || !Victim.Exists() || Victim.IsInAnyVehicle(false) || Game.IsKeyDown(IniFile.EndCall) || Game.LocalPlayer.Character.IsDead)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "~b~You: ~w~Dispatch we're code 4.");
                End();
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


            Game.LogTrivial("[TornadoCallouts LOG]: | Drug Overdose | Has Cleaned Up.");
        }
    }
}