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
    [CalloutInterface("Store Altercation", CalloutProbability.High, "There is an altercation at a store.", "Code 2", "LSPD")]

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
        private readonly Random rand = new Random();
        private const float MaxDistance = 6500f; // Approx. 6.5km (4mi) in-game distance


        private readonly List<Vector3> spawnLocations = new List<Vector3>()
        {
            new Vector3(372.84f, 328.12f, 103.56f), // Clinton Avenue, Downtown Vinewood – 24/7 Supermarket
            new Vector3(1160.47f, -316.43f, 69.18f), // E Mirror Drive, Mirror Park – Limited LTD Gasoline
            new Vector3(1135.75f, -981.49f, 46.41f), // El Rancho Boulevard – Robs Liquor
            new Vector3(24.43f, -1345.97f, 29.49f), // Innocence Boulevard, Strawberry – 24/7 Supermarket
            new Vector3(-47.84f, -1758.68f, 29.42f), // Grove Street – Limited LTD Gasoline
            new Vector3(-708.17f, -913.63f, 19.21f), // Lindsay Circus – Limited LTD Gasoline
            new Vector3(-1222.74f, -906.86f, 12.32f), // San Andreas Avenue – Robs Liquor
            new Vector3(1961.11f, 3740.67f, 32.34f), // Sandy Shores – 24/7 Supermarket
            new Vector3(1734.54f, 6419.35f, 35.03f), // Paleto Bay – 24/7 Supermarket
            new Vector3(-709.17f, -904.21f, 19.21f), // Davis – LTD Gasoline
            new Vector3(-705.96f, -915.42f, 19.21f), // Little Seoul – LTD Gasoline
            new Vector3(1697.99f, 4924.40f, 42.06f), // Grapeseed – LTD Gasoline
            new Vector3(999f, 999f, 999f), // Prosperity Street – Robs Liquor
            new Vector3(999f, 999f, 999f), // Tongva Drive – Limited LTD Gasoline
            new Vector3(999f, 999f, 999f), // Great Ocean Highway – Robs Liquor
            new Vector3(999f, 999f, 999f), // Ineseno Road – 24/7 Supermarket
            new Vector3(999f, 999f, 999f), // Barbareno Road – 24/7 Supermarket
            new Vector3(999f, 999f, 999f), // Route 68, Harmony – 24/7 Supermarket
            new Vector3(999f, 999f, 999f), // Route 68 – Scoops Liquor Barn
            new Vector3(999f, 999f, 999f), // Niland Avenue – 24/7 Supermarket
            new Vector3(999f, 999f, 999f), // Grand Senora Freeway – Convenience Store
            new Vector3(999f, 999f, 999f), // Grapeseeds Main Street
            new Vector3(999f, 999f, 999f), // Senora Freeway, Mount Chiliad – 24/7 Supermarket
            new Vector3(999f, 999f, 999f), // Tataviam Mountains – 24/7 Supermarket
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
            
            // List of ped models used for Store Clerk
            List<string> pedModels = new List<string>()
            {
                "mp_m_shopkeep_01",
                "a_m_m_indian_01",
                "s_m_m_autoshop_01",
                "s_m_m_linecook",
                "s_m_y_shop_mask",
                "s_f_m_sweatshop_01",
                "s_f_y_sweatshop_01",
                "a_m_m_skidrow_01",
                "a_m_m_socenlat_01",
                "a_m_y_indian_01",
            };

            // Select random model from list for Store Clerk
            string Clerkmodel1 = pedModels[rand.Next(pedModels.Count)];

            // Create suspect 1 at the selected location
            Suspect1 = new Ped(Spawnpoint.Around2D(2f), 180f);
            Suspect1.IsPersistent = true;
            Suspect1.BlockPermanentEvents = true;
            Suspect1.CanOnlyBeDamagedByPlayer = true;

            // Suspect 1 blip and gender for speech.
            SuspectBlip1 = Suspect1.AttachBlip();
            SuspectBlip1.Color = System.Drawing.Color.Green;
            SuspectBlip1.IsRouteEnabled = true;

            if (Suspect1.IsMale) malefemaleSuspect1 = "sir"; else malefemaleSuspect1 = "ma'am";

            // Create suspect 2 at the selected location
            Suspect2 = new Ped (Spawnpoint.Around2D(4f), 180f);
            Suspect2.IsPersistent = true;
            Suspect2.BlockPermanentEvents = true;
            Suspect2.CanOnlyBeDamagedByPlayer = true;

            // Suspect 2 blip and gender for speech.
            SuspectBlip2 = Suspect2.AttachBlip();
            SuspectBlip2.Color = System.Drawing.Color.Red;

            if (Suspect2.IsMale) malefemaleSuspect2 = "sir"; else malefemaleSuspect2 = "ma'am";

            // Create store clerk at the selected location
            Clerk = new Ped(Clerkmodel1, Spawnpoint.Around2D(6f), 180f);
            Clerk.IsPersistent = true;
            Clerk.BlockPermanentEvents = true;
            Clerk.CanOnlyBeDamagedByPlayer = true;

            // Store Clerk blip and gender for speech.
            ClerkBlip = Clerk.AttachBlip();
            ClerkBlip.Color = System.Drawing.Color.Yellow;

            if (Clerk.IsMale) malefemaleClerk = "sir"; else malefemaleClerk = "ma'am";

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
                Game.DisplayHelp("Press ~y~Y ~s~to talk to ~h~~g~Suspect 1 (green blip).", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    counter++;

                    if (counter == 1)
                    {
                        NativeFunction.Natives.x5AD23D40115353AC(Suspect1, Game.LocalPlayer.Character, -1);
                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemaleSuspect1 + "?");
                    }
                    else if (counter == 2)
                    {
                        Game.DisplaySubtitle("~p~Suspect 1~s~: I was minding my own business when that person suddenly started provoking me. They were being aggressive and trying to start a fight!");
                    }
                    else if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Did you engage in a physical altercation with them?");
                    }
                    else if (counter == 4)
                    {
                        Game.DisplaySubtitle("~p~Suspect 1~s~: Yes, I defended myself! They attacked me first, and I had no choice but to fight back!");
                    }
                    else if (counter == 5)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Is there anything else you want to add to your side of the story?");
                    }
                    else if (counter == 6)
                    {
                        Game.DisplaySubtitle("~p~Suspect 1~s~: That's all I have to say. I hope you understand the truth!");
                    }
                    else if (counter == 7)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended, ~s~now talk to ~h~~q~Suspect 2.");

                        // Set Suspect1 conversation as finished
                        Suspect1ConversationFinished = true;
                        
                        // Set counter back to 0 for next conversation
                        counter = 0;
                        
                        GameFiber.Wait(1000);
                    }
                }
            }

            // Suspect 2 conversation
            if (Game.LocalPlayer.Character.DistanceTo(Suspect2) <= 5f && Suspect1ConversationFinished && !Suspect2ConversationFinished && !ClerkConversationFinished)
            {
                Game.DisplayHelp("Press ~y~Y ~s~to talk to ~h~~r~Suspect 2 (red blip).", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    counter++;

                    if (counter == 1)
                    {
                        NativeFunction.Natives.x5AD23D40115353AC(Suspect2, Game.LocalPlayer.Character, -1);
                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemaleSuspect2 + "?");
                    }
                    else if (counter == 2)
                    {
                        Game.DisplaySubtitle("~p~Suspect 2~s~: That's not true! they are lying! They were the one who instigated the fight. I was just defending myself!");
                    }
                    else if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Did you engage in a physical altercation with them?");
                    }
                    else if (counter == 4)
                    {
                        Game.DisplaySubtitle("~p~Suspect 2~s~: Yes, but only because they attacked me first. I had no choice but to fight back for my own safety!");
                    }
                    else if (counter == 5)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Is there anything else you want to add to your side of the story?");
                    }
                    else if (counter == 6)
                    {
                        Game.DisplaySubtitle("~p~Suspect 2~s~: That's all I have to say. Please understand that I was only defending myself!");
                    }
                    else if (counter == 7)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended, ~s~now go talk to the ~h~~y~Store Clerk.");

                        Clerk.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 1f, AnimationFlags.Loop);
                        
                        // Set Suspect2 conversation as finished
                        Suspect2ConversationFinished = true;

                        // Set counter back to 0 for next conversation
                        counter = 0;

                        GameFiber.Wait(1000);
                    }
                }
            }

            // Store Clerk conversation
            if (Game.LocalPlayer.Character.DistanceTo(Clerk) <= 5f && Suspect1ConversationFinished && Suspect2ConversationFinished && !ClerkConversationFinished)
            {
                Game.DisplayHelp("Press ~y~Y ~s~to talk to ~h~~y~the Clerk (yellow blip).", false);

                if (Game.IsKeyDown(Keys.Y))
                {
                    Clerk.Tasks.ClearImmediately();
                    Game.LogTrivial("[TornadoCallouts LOG]: Clear animation task for clerk initiated");

                    NativeFunction.Natives.x5AD23D40115353AC(Clerk, Game.LocalPlayer.Character, -1);
                    counter++;

                    if (counter == 1)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Can you tell me what happened here " + malefemaleClerk + "?");
                    }
                    else if (counter == 2)
                    {
                        Game.DisplaySubtitle("~y~Clerk~s~: I witnessed the altercation between these two people. They were arguing and exchanging blows. It seemed like a mutual fight, both actively participating.");
                    }
                    else if (counter == 3)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Did you see who initiated the fight?");
                    }
                    else if (counter == 4)
                    {
                        Game.DisplaySubtitle("~y~Clerk~s~: It's hard to say for sure, but it appeared that Suspect1 threw the first punch. However, Suspect 2 quickly retaliated.");
                    }
                    else if (counter == 5)
                    {
                        Game.DisplaySubtitle("~b~You~s~: Is there anything else you want to add to what you saw?");
                    }
                    else if (counter == 6)
                    {
                        Game.DisplaySubtitle("~y~Clerk~s~: That's all I can provide. It was a chaotic situation, and both suspects seemed equally involved in the altercation.");
                    }
                    else if (counter == 7)
                    {
                        Game.DisplaySubtitle("~g~Conversation has ended. ~s~Finish your investigation then end the callout.");

                        // Set clerk conversation as finished
                        ClerkConversationFinished = true;

                        GameFiber.Wait(6000);

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "~s~Press your ~h~~g~'END'~s~ Callout Key when you are finished.");
                    }
                }
            }

            // Initiate a fight between Suspect1 and Suspect2 if conditions are met
          //  if (!FightCreated && Game.LocalPlayer.Character.DistanceTo(Suspect1) <= 60f)
           // {
              //  Suspect1.Tasks.FightAgainst(Suspect2);
               // Suspect2.Tasks.FightAgainst(Suspect1);
               // FightCreated = true;
           // }

            // Check if either Suspect1 or Suspect2 is cuffed
            bool oneOfSuspectsIsCuffed = Suspect1.IsCuffed || Suspect2.IsCuffed;

            // Check if either Suspect1 or Suspect2 is dead
            bool oneOfSuspectsIsDead = Suspect1.IsDead || Suspect2.IsDead;

            // Determine if the callout should end based on various conditions
            bool shouldEnd = oneOfSuspectsIsDead
                             || Game.LocalPlayer.Character.IsDead
                             || !Suspect1.Exists()
                             || !Suspect2.Exists()
                             || oneOfSuspectsIsCuffed
                             || Game.IsKeyDown(IniFile.EndCall);

            // End callout if one of the conditions is met
            if (shouldEnd)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Store Altercation", "~b~You: ~w~Dispatch we're code 4.");
                End();
            }
        }




        public override void End()
        {
            base.End();

            // Clean up Suspect Peds
            if (Suspect1.Exists()) { Suspect1.Dismiss(); }
            if (Suspect2.Exists()) { Suspect2.Dismiss(); }

            // Clean up Suspect Blips
            if (SuspectBlip1.Exists()) { SuspectBlip1.Delete(); }
            if (SuspectBlip2.Exists()) { SuspectBlip2.Delete(); }

            // Clean up Store Clerk Ped and Blip
            if (Clerk.Exists()) { Clerk.Dismiss(); }
            if (ClerkBlip.Exists()) { ClerkBlip.Delete(); }

            Game.LogTrivial("[TornadoCallouts LOG]: | Store Altercation | Has Cleaned Up.");
        }
    }
}