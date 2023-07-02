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
            new Vector3(1994.2f, 3800.8f, 32.2f), // Abandoned Motel in Sandy Shores
            new Vector3(1972.5f, 3804.6f, 32.1f), // Sandy Shores Trailer Park
            new Vector3(1961.5f, 3743.7f, 32.4f), // Sandy Shores Gas Station
            new Vector3(1845.7f, 3642.1f, 34.2f), // Alamo Sea Shoreline
            new Vector3(1848.4f, 3681.1f, 34.2f), // Sandy Shores Medical Clinic
            new Vector3(-193.4f, -1772.1f, 29.3f), // Underpass near Strawberry Ave (Strawberry)
            new Vector3(-538.2f, -243.7f, 35.5f), // Parking Garage near Del Perro Freeway (Del Perro)
            new Vector3(-1095.1f, -1469.4f, 4.9f), // Construction Site near Elysian Fields Freeway (La Puerta)
            new Vector3(-1016.2f, -2694.4f, 13.6f), // Abandoned Factory near Palmer-Taylor Power Station (Davis)
            new Vector3(-722.8f, -857.9f, 22.3f), // Docks near Terminal (La Mesa)
            new Vector3(-1469.3f, -647.5f, 29.5f), // Construction Site near Sustancia Rd (Cypress Flats)
            new Vector3(267.9f, -1964.7f, 23.5f), // Motel near Davis Quartz Quarry (Davis)
            new Vector3(1404.6f, 1119.3f, 113.8f), // Vinewood Hills Overlook (Vinewood Hills)
            new Vector3(-94.7f, -1140.6f, 26.2f), // Convenience Store near Vespucci Blvd (Vespucci Beach)
            new Vector3(726.5f, -1045.9f, 22.6f), // Warehouse near Innocence Blvd (Strawberry)
            new Vector3(1449.7f, 3742.9f, 33.9f), // Beachfront near Pacific Bluffs (Pacific Bluffs)
            new Vector3(121.8f, -732.9f, 45.8f), // Observatory Parking Lot (Vinewood Hills)
            new Vector3(-48.7f, -577.8f, 37.0f), // Apartment Complex near Alta St (Downtown Los Santos)
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
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Drug Overdose", "If you arrive before EMS give CPR, later speak with the bystander.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "If you arrive before EMS give CPR, later speak with the bystander to see what happened.");

                ArrivalNotificationSent = true;
            }

            if (Game.LocalPlayer.Character.DistanceTo(Victim) <= 18f && !MedicalSent)
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