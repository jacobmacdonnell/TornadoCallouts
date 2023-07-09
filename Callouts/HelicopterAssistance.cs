using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using CalloutInterfaceAPI;
using LSPD_First_Response.Engine;
using System.Drawing;

namespace TornadoCallouts.Callouts
{
    [CalloutInterface("Helicopter Assistance", CalloutProbability.Medium, "Air 1 is requesting assistance.", "Code 2", "LSPD")]
    public class HelicopterAssistance : Callout
    {
        private Ped Suspect;
        private Vehicle SuspectVehicle;
        private Vector3 SpawnPoint;
        private Blip SuspectBlip;
        private Vector3 SearchArea;
        private DateTime lastBlipUpdateTime;
        private readonly Random rand = new Random();
        private LHandle Pursuit;
        private bool PursuitCreated = false;




        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250f));
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            CalloutPosition = SpawnPoint;
            CalloutMessage = "Air 1 Requesting Officer Assistance";
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 ASSISTANCE_REQUIRED_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_02_02", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }



        public override bool OnCalloutAccepted()
        {
            // List of sports car models
            List<string> sportsCars = new List<string>
            {
                "ZENTORNO", "ENTITYXF", "ADDER", "T20", "OSIRIS",
                "JESTER", "MASSACRO", "FELTZER", "SURANO", "NINEF",
                "SCHAFTERV12", "COMET2", "BANSHEE", "ALPHA", "FUSILADE",
                "COQUETTE", "RAPIDGT", "SEVEN70", "SPECTER", "ELEGY",
                "PARIAH", "ITALIGTO", "NEO", "IMORGON", "VAGNER",
                "DEVESTE", "KRIEGER", "EMERUS", "S80", "THRAX",
                "XA21", "TEZERACT", "TURISMOR", "REAPER", "TYRANT"
            };

            // List of muscle car models
            List<string> muscleCars = new List<string>
            {
                "BLADE", "BUCCANEER", "CHINO", "CLIQUE", "COQUETTE3",
                "DEVIANTE", "DOMINATOR", "DUKES", "ELLIE", "GAUNTLET",
                "GAUNTLET5", "HERMES", "HOTKNIFE", "IMPERATOR", "IMPERATOR2",
                "IMPERATOR3", "LURCHER", "MOONBEAM2", "PEYOTE", "PHOENIX",
                "PICADOR", "RATLOADER2", "RUINER", "SABREGT", "SABREGT2",
                "SLAMVAN", "SLAMVAN2", "SLAMVAN3", "STALION", "STALION2",
                "TAMPA", "TAMPA2", "TULIP", "VAMOS", "VIGERO",
                "VIRGO", "VIRGO2", "VIRGO3", "VOODOO", "VOODOO2"
            };

            // List of sedan car models
            List<string> sedanCars = new List<string>
            {
                "ASTEROPE", "FUGITIVE", "INGOT", "INTRUDER", "PREMIER",
                "PRIMO", "REGINA", "ROMERO", "SCHAFTER2", "STANIER",
                "STRATUM", "STRETCH", "SUPERD", "SURGE", "TAILGATER",
                "WARRENER", "WASHINGTON"
            };

            int index = rand.Next(sportsCars.Count + muscleCars.Count + sedanCars.Count);

            string randomCarModel;
            if (index < sportsCars.Count)
            {
                randomCarModel = sportsCars[index];
            }
            else if (index < sportsCars.Count + muscleCars.Count)
            {
                randomCarModel = muscleCars[index - sportsCars.Count];
            }
            else
            {
                randomCarModel = sedanCars[index - sportsCars.Count - muscleCars.Count];
            }

            SuspectVehicle = new Vehicle(randomCarModel, SpawnPoint);
            SuspectVehicle.IsPersistent = true;

            Suspect = SuspectVehicle.CreateRandomDriver();
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = false;
            SuspectBlip.Delete(); // Hide it initially

            SearchArea = SpawnPoint.Around2D(1f, 2f);
           

                       lastBlipUpdateTime = DateTime.Now;

            Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);

            string carColor = SuspectVehicle.PrimaryColor.ToString();
            string location = World.GetStreetName(SpawnPoint);

            CalloutInterfaceAPI.Functions.SendMessage(this, $"Air 1 to ground units: We are tracking {carColor} {randomCarModel} traveling at high speed near {location}. The driver is driving erratically. Requesting a unit to perform a traffic stop. Proceed with caution.");
            Game.DisplayNotification($"Air 1 to ground units: Look for a {carColor} {randomCarModel} around {location} traveling at high speed.");

            return base.OnCalloutAccepted();
        }



        public override void Process()
        {
            base.Process();

            float distanceToSuspect = Game.LocalPlayer.Character.DistanceTo(Suspect.Position);

            if (!PursuitCreated)
            {
                if (distanceToSuspect < 30f)
                {
                    if (SuspectBlip == null || !SuspectBlip.Exists())
                    {
                        SuspectBlip = Suspect.AttachBlip();
                        SuspectBlip.Color = System.Drawing.Color.Red;
                    }
                }
                else
                {
                    if ((DateTime.Now - lastBlipUpdateTime).TotalSeconds >= 15)
                    {
                        SearchArea = Suspect.Position.Around2D(1f, 2f);
                        lastBlipUpdateTime = DateTime.Now;
                    }
                }

                if (distanceToSuspect < 5f)
                {
                    if (rand.Next(100) < 60)
                    {
                        Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                        LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Suspect);
                        LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                        PursuitCreated = true;
                    }
                }
            }
            else
            {
                if (!LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(Pursuit))
                {
                    End();
                }
            }
        }



        public override void End()
        {
            base.End();

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

            Game.LogTrivial("[TornadoCallouts LOG]: | Helicopter Assistance | Has Cleaned Up.");
        }
    }
}