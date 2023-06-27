using System;
using System.Collections.Generic;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;
using System.Reflection;
using System.Windows.Forms;

namespace TornadoCallouts.Callouts
{
    [CalloutInterface("Store Altercation", CalloutProbability.High, "There is an altercation at a store.", "Code 3", "LSPD")]

    public class StoreAltercation : Callout
    {
        private Ped Suspect1, Suspect2;
        private Blip SuspectBlip1, SuspectBlip2;
        private Ped Clerk;
        private Blip ClerkBlip;
        private Vector3 Spawnpoint;
        private int counter;
        private bool FightCreated;
        private bool ArrivalNotificationSent;
        private bool ClerkConversationFinished;
        private bool Suspect1ConversationFinished;
        private bool Suspect2ConversationFinished;
        private string malefemaleSuspect1;
        private string malefemaleSuspect2;
        private string malefemaleClerk;
        private const float MaxDistance = 6500f; // Approx. 6.5km (4mi) in-game distance
        private readonly Random rand = new Random();

        private List<Vector3> spawnLocations = new List<Vector3>()
        {
            new Vector3(127.44f, -1306.12f, 29.23f), // Vanilla Unicorn Strip Club (Strawberry)
            new Vector3(-1391.837f, -585.1603f, 30.23638f), // Bahama Mamas Bar (Del Perro)
            new Vector3(487.0231f, -1545.364f, 29.19354f), // Hi-Men Bar (Rancho)
            new Vector3(250.0017f, -1011.299f, 29.26846f), // Shenanigan's Bar (Legion Square)
            new Vector3(226.7715f, 301.7152f, 105.5336f), // Singleton's Nightclub (Downtown Vinewood)
            new Vector3(919.417f, 50.9967f, 80.89855f), // Diamond Casino (Los Santos)
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
                AddMinimumDistanceCheck(100f, Spawnpoint);
                CalloutMessage = "Store Altercation";
                CalloutPosition = Spawnpoint;
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_04 CRIME_ASSAULT_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_02", Spawnpoint);
                return base.OnBeforeCalloutDisplayed();
            }

            // If none of the spawn locations were within the maximum distance, do not display the callout
            return false;
        }

        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("[TornadoCallouts LOG]: Store Altercation callout accepted");

            CalloutInterfaceAPI.Functions.SendMessage(this, "A store clerk is currently reporting two people having an altercation. Call backup if needed. Approach with caution.");
            
            // List of ped model names
            List<string> pedModels = new List<string>()
            {
                // Male Gang Peds
                "a_m_y_mexthug_01",
                "g_m_importexport_01",
                "g_m_m_korboss_01",
                "g_m_y_ballaorig_01",
                "g_m_y_famdnf_01",
                "g_m_y_mexgoon_01",
                "g_m_y_salvaboss_01",
                "g_m_y_mexgoon_01",
                "g_m_m_chigoon_02",

                // Female Gang Peds
                "g_f_y_ballas_01",
                "g_f_y_lost_01",
                "g_f_y_families_01",
                "g_f_y_vagos_01",
            };

            // Select random models for the suspects
            string model1 = pedModels[rand.Next(pedModels.Count)];
            string model2 = pedModels[rand.Next(pedModels.Count)];
            string model3 = pedModels[rand.Next(pedModels.Count)];

            // Create suspect 1 at the selected location
            Suspect1 = new Ped(model1, Spawnpoint, 180f);
            Suspect1.IsPersistent = true;
            Suspect1.BlockPermanentEvents = true;
            Suspect1.Alertness += 50;
            Suspect1.CanOnlyBeDamagedByPlayer = true;

            // Suspect 1 blip and gender for speech.
            SuspectBlip1 = Suspect1.AttachBlip();
            SuspectBlip1.Color = System.Drawing.Color.Orange;
            SuspectBlip1.IsRouteEnabled = true;

            if (Suspect1.IsMale)
                malefemaleSuspect1 = "sir";
            else
                malefemaleSuspect1 = "ma'am";


            // Create suspect 2 at the selected location
            Suspect2 = new Ped(model2, Spawnpoint, 180f);
            Suspect2.IsPersistent = true;
            Suspect2.BlockPermanentEvents = true;
            Suspect2.Alertness += 50;
            Suspect2.CanOnlyBeDamagedByPlayer = true;


            // Suspect 2 blip and gender for speech.
            SuspectBlip2 = Suspect2.AttachBlip();
            SuspectBlip2.Color = System.Drawing.Color.Purple;

            if (Suspect2.IsMale)
                malefemaleSuspect2 = "sir";
            else
                malefemaleSuspect2 = "ma'am";


            // Create store clerk at the selected location
            Clerk = new Ped(model3, Spawnpoint, 180f);
            Clerk.IsPersistent = true;
            Clerk.BlockPermanentEvents = true;
            Clerk.CanOnlyBeDamagedByPlayer = true;

            // Store Clerk blip and gender for speech.
            ClerkBlip = Clerk.AttachBlip();
            ClerkBlip.Color = System.Drawing.Color.Cyan;

            if (Clerk.IsMale)
                malefemaleClerk = "sir";
            else
                malefemaleClerk = "ma'am";


            FightCreated = false;

            counter = 0;


            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character.DistanceTo(Clerk) <= 250f && !ArrivalNotificationSent)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "On arrival, handle the altercation, then speak with the clerk and suspects for more info.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "When you arrive on scene, handle the situation, then speak with the clerk and suspects to see what happened.");

                ArrivalNotificationSent = true;
            }

            // Suspect 1 conversation
            if (Game.LocalPlayer.Character.DistanceTo(Suspect1) <= 5f && !Suspect1ConversationFinished && !Suspect2ConversationFinished && !ClerkConversationFinished)
            {
                Game.DisplayHelp("Press ~y~Y ~s~to talk to ~o~Suspect 1.", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    counter++;

                    if (counter == 1)
                    {

                        //Turn_Ped_To_Face_Entity
                        NativeFunction.Natives.x5AD23D40115353AC(Suspect1, Game.LocalPlayer.Character, -1);


                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemaleSuspect1 + "?");
                    }
                    if (counter == 2)
                    {
                        Game.DisplaySubtitle("~o~Suspect 1~s~: I don't know, I was walking by when I saw this person collapse to the ground, and then I called 9-11.");
                    }
                    if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Okay, we believe it may be a drug overdose, thank you for calling us " + malefemaleSuspect1 + ", you are free to go.");
                    }
                    if (counter == 4)
                    {
                        Game.DisplaySubtitle("~o~Suspect 1~s~: Of course. I hope they are okay, bye.");
                    }
                    if (counter == 5)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended, ~s~now talk to ~p~Suspect 2.");

                        GameFiber.Wait(5000);

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "~s~Press your ~g~'END'~s~ Callout Key when you are finished.");

                        // Set Suspect1 conversation as finished
                        Suspect1ConversationFinished = true;

                        counter = 0;
                    }
                }
            }

            // Suspect 2 conversation
            if (Game.LocalPlayer.Character.DistanceTo(Suspect2) <= 5f && Suspect1ConversationFinished && !Suspect2ConversationFinished && !ClerkConversationFinished)
            {
                Game.DisplayHelp("Press ~y~Y ~s~to talk to ~p~Suspect 2.", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    counter++;

                    if (counter == 1)
                    {

                        //Turn_Ped_To_Face_Entity
                        NativeFunction.Natives.x5AD23D40115353AC(Suspect2, Game.LocalPlayer.Character, -1);

                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemaleSuspect2 + "?");
                    }
                    if (counter == 2)
                    {
                        Game.DisplaySubtitle("~p~Suspect 2~s~: I don't know, I was walking by when I saw this person collapse to the ground, and then I called 9-11.");
                    }
                    if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Okay, we believe it may be a drug overdose, thank you for calling us " + malefemaleSuspect2 + ", you are free to go.");
                    }
                    if (counter == 4)
                    {
                        Game.DisplaySubtitle("~p~Suspect 2~s~: Of course. I hope they are okay, bye.");
                    }
                    if (counter == 5)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended, ~s~now go talk to the ~c~Store Clerk.");

                        GameFiber.Wait(5000);

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "~s~Press your ~g~'END'~s~ Callout Key when you are finished.");

                        // Set Suspect2 conversation as finished
                        Suspect2ConversationFinished = true;

                        // Set counter back to 0 for next conversation
                        counter = 0;
                    }
                }
            }

            // Store Clerk conversation
            if (Game.LocalPlayer.Character.DistanceTo(Clerk) <= 5f && Suspect1ConversationFinished && Suspect2ConversationFinished && !ClerkConversationFinished)
            {
                // Make the store clerk do the waving animation
                Clerk.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1f, AnimationFlags.Loop);

                Game.DisplayHelp("Press ~y~Y ~s~to talk to the ~y~Store Clerk.", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    counter++;

                    if (counter == 1)
                    {
                        // Stop the waving animation
                        Clerk.Tasks.ClearImmediately();


                        //Turn_Ped_To_Face_Entity
                        NativeFunction.Natives.x5AD23D40115353AC(Clerk, Game.LocalPlayer.Character, -1);


                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemaleClerk + "?");
                    }
                    if (counter == 2)
                    {
                        Game.DisplaySubtitle("~y~Clerk~s~: I don't know, I was walking by when I saw this person collapse to the ground, and then I called 9-11.");
                    }
                    if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Okay, we believe it may be a drug overdose, thank you for calling us " + malefemaleClerk + ", you are free to go.");
                    }
                    if (counter == 4)
                    {
                        Game.DisplaySubtitle("~y~Clerk~s~: Of course. I hope they are okay, bye.");
                    }
                    if (counter == 5)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended!");

                        GameFiber.Wait(5000);

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "~s~Press your ~g~'END'~s~ Callout Key when you are finished.");

                        // Set clerk conversation as finished
                        ClerkConversationFinished = true;

                    }
                }
            }


            // Initiate a fight between Suspect1 and Suspect2 if conditions are met
            if (!FightCreated && Game.LocalPlayer.Character.DistanceTo(Suspect1) <= 60f)
            {
                Suspect1.Tasks.FightAgainst(Suspect2);
                Suspect2.Tasks.FightAgainst(Suspect1);
                FightCreated = true;
            }

            // Check if either Suspect1 or Suspect2 is cuffed
            bool oneOfSuspectsIsCuffed = Suspect1.IsCuffed || Suspect2.IsCuffed;

            // Check if either Suspect1 or Suspect2 is dead
            bool oneOfSuspectsIsDead = Suspect1.IsDead || Suspect2.IsDead;

            // Determine if the callout should end based on various conditions
            bool shouldEnd = oneOfSuspectsIsDead
                             || Game.LocalPlayer.Character.IsDead
                             || !Suspect1.Exists()
                             || !Suspect2.Exists()
                             || oneOfSuspectsIsCuffed;

            // End the fight if necessary
            if (shouldEnd)
            {
                End();
            }

            // Check for player input to end the fight
            if (Game.IsKeyDown(IniFile.EndCall))
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "~b~You: ~w~Dispatch we're code 4.");
                End();
            }



        }

        public override void End()
        {
            base.End();

            // Clean up Suspects
            if (Suspect1.Exists()) { Suspect1.Dismiss(); }
            if (Suspect2.Exists()) { Suspect2.Dismiss(); }

            // Clean up Suspect Blips
            if (SuspectBlip1.Exists()) { SuspectBlip1.Delete(); }
            if (SuspectBlip2.Exists()) { SuspectBlip2.Delete(); }

            // Clean up Clerk
            if (Clerk.Exists()) { Clerk.Dismiss(); }

            // Clean up Clerk Blip
            if (ClerkBlip.Exists()) { ClerkBlip.Delete(); }

            Game.LogTrivial("[TornadoCallouts LOG]: | Store Altercation | Has Cleaned Up.");
        }
    }
}
