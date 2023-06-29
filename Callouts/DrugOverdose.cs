using System;
using System.Collections.Generic;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.Callouts;
using System.Drawing;
using CalloutInterfaceAPI;
using TornadoCallouts.Stuff;
using System.Windows.Forms;
using LSPD_First_Response;

namespace TornadoCallouts.Callouts
{
    [CalloutInterface("Drug Overdose", CalloutProbability.High, "An individual has had a potential drug overdose", "Code 3", "LSPD")]
    public class DrugOverdose : Callout
    {

        private Ped Bystander;
        private Blip BystanderBlip;
        private Ped Victim;
        private Blip VictimBlip;
        private Vector3 Spawnpoint;
        private readonly float heading;
        private int counter;
        private string malefemale;
        private bool ArrivalNotificationSent = false;
        private bool ConversationFinished = false;
        private bool MedicalSent = false;
        private const float MaxDistance = 6500f; // Approx. 6.5km (4mi) in-game distance
        private readonly Random rand = new Random();


        // List potential spawn locations
        private readonly List<Vector3> spawnLocations = new List<Vector3>()
        {
               new Vector3(94.63f, -217.37f, 54.49f),
               new Vector3(127.44f, -1306.12f, 29.23f), // Vanilla Unicorn Strip Club (Strawberry)
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            List<Vector3> validSpawnLocations = new List<Vector3>();

            // Check the distance to each spawn location
            foreach (var location in spawnLocations)
            {
                float distance = Game.LocalPlayer.Character.Position.DistanceTo(location);
                if (distance < MaxDistance)
                {
                    validSpawnLocations.Add(location);
                }
            }

            if (validSpawnLocations.Count > 0)
            {
                // Select a random spawn location from the valid locations
                Spawnpoint = validSpawnLocations[rand.Next(validSpawnLocations.Count)];

                ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 100f);
                AddMinimumDistanceCheck(50f, Spawnpoint);
                CalloutMessage = "Potential Drug Overdose";
                CalloutPosition = Spawnpoint;
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 CITIZENS_REPORT_04 ASSISTANCE_REQUIRED_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_02", Spawnpoint);
                return base.OnBeforeCalloutDisplayed();
            }

            // If none of the spawn locations were within the maximum distance, do not display the callout
            return false;
        }

        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("[TornadoCallouts LOG]: Drug Overdose callout accepted.");

            CalloutInterfaceAPI.Functions.SendMessage(this, "Citizens are currently reporting an individual who has collapsed in a public area, exhibiting signs consistent with a drug overdose. The nature of the substance involved is unknown. Respond and assist the individual if EMS has not arrived yet.");

            // Spawn Victim ped
            Victim = new Ped(Spawnpoint, heading);

            Game.LogTrivial("[TornadoCallouts LOG]: Victim ped created");

            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Victim.CanRagdoll = true;
            Victim.Health = 0;
            VictimBlip = Victim.AttachBlip();
            VictimBlip.Color = Color.CadetBlue;
            VictimBlip.IsRouteEnabled = true;

            Game.LogTrivial("[TornadoCallouts LOG]: Victim blip created");
            
            // Spawn Bystander ped
            Bystander = new Ped(Spawnpoint, heading);
            Bystander.IsPersistent = true;
            Bystander.BlockPermanentEvents = true;
            BystanderBlip = Bystander.AttachBlip();
            BystanderBlip.Color = Color.Yellow;

            if (Bystander.IsMale) malefemale = "sir"; else malefemale = "ma'am";

            // Make the bystander do the waving animation
            Bystander.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1f, AnimationFlags.Loop);

            counter = 0;

            return base.OnCalloutAccepted();
        }
        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character.DistanceTo(Victim) <= 350f && !ArrivalNotificationSent)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "On arrival speak with the bystander to get more info.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "When you arrive on scene speak with the bystander to see what happened.");

                ArrivalNotificationSent = true;
            }

            if (Game.LocalPlayer.Character.DistanceTo(Victim) <= 600f && !MedicalSent)
            {
                if (TornadoCallouts.Main.UsingUB)
                {
                    UltimateBackupWrapper.CallEms();
                }
                else
                {
                    LSPD_First_Response.Mod.API.Functions.RequestBackup(Game.LocalPlayer.Character.Position, EBackupResponseType.Code3,
                        EBackupUnitType.Ambulance);
                }

                GameFiber.Wait(1500);

                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "EMS has also been dispatched, if you arrive before them administer CPR.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "EMS has also been dispatched, if you arrive before them administer CPR.");

                MedicalSent = true;
            }

            if (Game.LocalPlayer.Character.DistanceTo(Bystander) <= 8f && !ConversationFinished)
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

                        ConversationFinished = true;

                        Bystander.Tasks.Wander();

                        if (BystanderBlip.Exists()) { BystanderBlip.Delete(); }

                        GameFiber.Wait(5000);

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "~s~Press your ~g~'END'~s~ Callout Key when you are finished.");
                    }
                }
            }
            
            // Determine if the callout should end based on various conditions
            bool shouldEnd = Victim.IsCuffed
                            || !Victim.Exists()
                            || Victim.IsInAnyVehicle(false)
                            || Game.IsKeyDown(IniFile.EndCall)
                            || Game.LocalPlayer.Character.IsDead;

            // End callout if one of the conditions is met
            if (shouldEnd)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "~b~You: ~w~Dispatch we're code 4.");
                End();
            }
        }
        public override void End()
        {
            base.End();

            // Clean up Victim ped and blip
            if (Victim.Exists()) { Victim.Dismiss(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }

            // Clean up Bystander ped and blip
            if (Bystander.Exists()) { Bystander.Dismiss(); }
            if (BystanderBlip.Exists()) { BystanderBlip.Delete(); }

            Game.LogTrivial("[TornadoCallouts LOG]: | Drug Overdose | Has Cleaned Up.");
        }
    }
}