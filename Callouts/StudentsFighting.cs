using System;
using System.Collections.Generic;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;
using LSPD_First_Response.Engine;
using System.Reflection;
using System.Windows.Forms;


namespace TornadoCallouts.Callouts
{
    [CalloutInterface("Students Fighting", CalloutProbability.High, "Two students are currently fighting.", "Code 2", "LSPD")]
    public class StudentsFighting : Callout
    {
        private Ped Student1, Student2;
        private Blip Student1Blip, Student2Blip;
        private Vector3 Spawnpoint;
        private Vector3 Searcharea;
        private Random rand = new Random();
        private List<Ped> bystanders = new List<Ped>(); // To keep track of the bystander Peds
        private List<Blip> blips = new List<Blip>(); // To keep track of the blips
        private List<string> pedModels; // Declare pedModels at class level
        private bool FightCreated;
        private const float MaxDistance = 6500f; // Approx. 6.5km (4mi) in-game distance



        // List potential spawn locations
        private List<Vector3> spawnLocations = new List<Vector3>()
        {
               new Vector3(-1473f, 240f, 55f), // University of San Andreas, Los Santos (Richman District)
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

                ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 50f);
                AddMinimumDistanceCheck(100f, Spawnpoint);
                CalloutMessage = "Students Fighting";
                CalloutPosition = Spawnpoint;
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_04 CRIME_ASSAULT_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_02_01", Spawnpoint);
                return base.OnBeforeCalloutDisplayed();
            }

            // If none of the spawn locations were within the maximum distance, do not display the callout
            return false;
        }

        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("[TornadoCallouts LOG]: Students Fighting callout accepted");

            CalloutInterfaceAPI.Functions.SendMessage(this, "Students at the university are reporting two students are currently fighting. Call backup if needed. Approach with caution.");

            // List of ped model names
            pedModels = new List<string>()
    {
            // Male Student Peds
            "a_m_y_hipster_01", "a_m_y_hipster_02", "a_m_y_hipster_03",
            "a_m_y_indian_01", "a_m_y_epsilon_02", "a_m_y_epsilon_01",
            "a_m_y_vinewood_02", "a_m_y_vinewood_01", "a_m_y_vinewood_04", "a_m_y_vinewood_03",
    };

            Game.LogTrivial("[TornadoCallouts LOG]: About to create students and bystanders");

            CreateStudent(ref Student1, ref Student1Blip); // Spawning the first student involved in the fight
            CreateStudent(ref Student2, ref Student2Blip); // Spawning the second student involved in the fight
            CreateBystanders(); // Spawning bystanders around the fighting students

            Game.LogTrivial("[TornadoCallouts LOG]: Students and bystanders created");

            Searcharea = Spawnpoint.Around2D(1f, 2f);

            Game.LogTrivial("[TornadoCallouts LOG]: Searcharea created");

            FightCreated = false;

            return base.OnCalloutAccepted();
        }

        private void CreateStudent(ref Ped student, ref Blip studentBlip)
        {
            string model = pedModels[rand.Next(pedModels.Count)];

            // Create Student at the selected location
            student = new Ped(model, Spawnpoint, 180f);
            student.IsPersistent = true;
            student.BlockPermanentEvents = true;
            student.Alertness += 50;
            student.CanOnlyBeDamagedByPlayer = true;
            studentBlip = student.AttachBlip();
            studentBlip.Color = System.Drawing.Color.Yellow;

            Game.LogTrivial("[TornadoCallouts LOG]: 2 Student peds created");

            if (studentBlip == Student1Blip)
            {
                studentBlip.IsRouteEnabled = true;
            }
        }

        private void CreateBystanders()
        {
            Vector3 center = Spawnpoint; // Center of the circle where the students are fighting
            float radius = 8f; // Radius of the circle

            // Generate 6 bystanders around the fighting students
            for (int i = 0; i < 6; i++)
            {
                string model = pedModels[rand.Next(pedModels.Count)];
                float angle = (float)(i * (2 * Math.PI) / 6);
                float x = center.X + radius * (float)Math.Cos(angle);
                float y = center.Y + radius * (float)Math.Sin(angle);

                // Calculate heading towards the center of the circle
                float deltaX = center.X - x;
                float deltaY = center.Y - y;
                float headingTowardsCenter = (float)(Math.Atan2(deltaY, deltaX) * (180 / Math.PI)) + 90;

                // Create the bystanders
                Ped bystander = new Ped(model, new Vector3(x, y, center.Z), headingTowardsCenter);
                bystander.IsPersistent = true;
                bystander.BlockPermanentEvents = true;

                // Log the spawn position of the bystander
                Game.LogTrivial($"[TornadoCallouts LOG]: Bystander spawned at ({x}, {y}, {center.Z})");

                Blip blip = bystander.AttachBlip();
                blip.Color = System.Drawing.Color.Blue;

                bystanders.Add(bystander); // Add the bystander to the list
                blips.Add(blip); // Add the blip to the list
            }
        }

        public bool ShouldEndCallout()
        {
            return Student1.IsDead || Student2.IsDead || Game.LocalPlayer.Character.IsDead || !Student1.Exists() || !Student2.Exists() || Student1.IsCuffed || Student2.IsCuffed || Game.IsKeyDown(IniFile.EndCall);
        }

        public override void Process()
        {
            base.Process();

            // Start the fight between the two students if the player is close enough
            if (!FightCreated && Game.LocalPlayer.Character.DistanceTo(Student1) <= 350f)
            {
                Student1.Tasks.FightAgainst(Student2);
                Student2.Tasks.FightAgainst(Student1);
                FightCreated = true;

                Vector3 center = (Student1.Position + Student2.Position) / 2; // center point between the fighting students

                // Make sure bystanders are turned facing the fighting students
                foreach (Ped bystander in bystanders)
                {
                    NativeFunction.Natives.x5AD23D40115353AC(bystander, center, -1); //Turn_Ped_To_Face_Entity
                }

                Game.LogTrivial("[TornadoCallouts LOG]: Fight between students started");
            }

            // Check if the callout should be ended
            if (ShouldEndCallout())
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Students Fighting", "~b~You: ~w~Dispatch we're code 4.");
                End();
            }
        }


        public override void End()
        {
            base.End();

            // Clean up the students and their blips
            if (Student1.Exists())
            {
                Student1.Dismiss();
            }
            if (Student2.Exists())
            {
                Student2.Dismiss();
            }

            if (Student1Blip.Exists())
            {
                Student1Blip.Delete();
            }
            if (Student2Blip.Exists())
            {
                Student2Blip.Delete();
            }

            // Clean up the bystanders and their blips
            foreach (Ped bystander in bystanders)
            {
                if (bystander.Exists())
                {
                    bystander.Dismiss();
                }
            }

            foreach (Blip blip in blips)
            {
                if (blip.Exists())
                {
                    blip.Delete();
                }
            }

            Game.LogTrivial("[TornadoCallouts LOG]: | Students Fighting | Has Cleaned Up.");
        }

    }
}
