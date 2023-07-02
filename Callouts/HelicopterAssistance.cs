using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using CalloutInterfaceAPI;
using LSPD_First_Response.Engine;

namespace TornadoCallouts.Callouts
{
    [CalloutInterface("Helicopter Assistance", CalloutProbability.Medium, "Air 1 is requesting assistance.", "Code 2", "LSPD")]
    public class HelicopterAssistance : Callout
    {
        private Ped Suspect;
        private Vehicle SuspectVehicle;
        private Vector3 SpawnPoint;
        private Blip SuspectBlip;
        private Blip SearchAreaBlip;
        private DateTime lastBlipUpdateTime;
        private LHandle Pursuit;
        private bool PursuitCreated = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250f));
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            CalloutPosition = SpawnPoint;
            CalloutMessage = "Helicopter Assistance Required";
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_04 CRIME_GRAND_THEFT_AUTO_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_02", SpawnPoint);

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

            Random random = new Random();
            int index = random.Next(sportsCars.Count);
            string randomSportsCar = sportsCars[index];

            string location = World.GetStreetName(SpawnPoint);
            CalloutInterfaceAPI.Functions.SendMessage(this, $"Air 1 to ground units: We are tracking a vehicle traveling at high speed near {location}. The driver is driving erratically. Requesting a unit to perform a traffic stop. Proceed with caution.");

            SuspectVehicle = new Vehicle(randomSportsCar, SpawnPoint);
            SuspectVehicle.IsPersistent = true;

            Suspect = SuspectVehicle.CreateRandomDriver();
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = false;
            SuspectBlip.Delete(); // Hide it initially

            SearchAreaBlip = new Blip(SpawnPoint);
            SearchAreaBlip.Color = System.Drawing.Color.Red;
            SearchAreaBlip.Scale = 50f;
            SearchAreaBlip.IsRouteEnabled = true;

            lastBlipUpdateTime = DateTime.Now;

            Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(Suspect.Position) < 30f)
            {
                if (SearchAreaBlip.Exists()) { SearchAreaBlip.Delete(); }

                SuspectBlip = Suspect.AttachBlip();

                Random rand = new Random();
                if (rand.Next(100) < 50) // 50% chance to initiate pursuit
                {
                    Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                    LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Suspect);
                    LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                    PursuitCreated = true;
                }
            }

            if (!PursuitCreated)
            {
                // Check if it has been more than 10 seconds since last update
                if ((DateTime.Now - lastBlipUpdateTime).TotalSeconds >= 10)
                {
                    // Update the search area blip location to suspect vehicle position
                    if (SearchAreaBlip.Exists()) { SearchAreaBlip.Delete(); }
                    SearchAreaBlip = new Blip(SuspectVehicle.Position);
                    SearchAreaBlip.Color = System.Drawing.Color.Red;
                    SearchAreaBlip.Scale = 50f;

                    // Update the last blip update time
                    lastBlipUpdateTime = DateTime.Now;
                }
            }

            if (PursuitCreated && !LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(Pursuit)) End();
        }


        public override void End()
        {
            base.End();

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SearchAreaBlip.Exists()) { SearchAreaBlip.Delete(); }

            Game.LogTrivial("[TornadoCallouts LOG]: | Helicopter Assistance | Has Cleaned Up.");
        }
    }
}
