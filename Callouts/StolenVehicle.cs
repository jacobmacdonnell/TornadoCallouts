using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using CalloutInterfaceAPI;
using LSPD_First_Response.Engine;


namespace TornadoCallouts.Callouts
{

    [CalloutInterface("Stolen Vehicle", CalloutProbability.Medium, "Possible stolen vehicle", "Code 3", "LSPD")]
    public class StolenVehicle : Callout

    {
        private Ped Suspect;
        private Vehicle SuspectVehicle;
        private Vector3 SpawnPoint;
        private Blip SuspectBlip;
        private LHandle Pursuit;
        private bool PursuitCreated = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250f));

            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutPosition = SpawnPoint;
            CalloutMessage = "Possible Stolen Vehicle";
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_04 CRIME_GRAND_THEFT_AUTO_01 IN_OR_ON_POSITION SUSPECT_LAST_SEEN_01 DRIVING_A_01 VEHICLE_CATEGORY_SPORTS_CAR_01 UNITS_RESPOND_CODE_03_02", SpawnPoint);


            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()

        {
            CalloutInterfaceAPI.Functions.SendMessage(this, "Citiznes are reporting someone just stole a vehicle and are driving eratically. Approach with caution.");


            SuspectVehicle = new Vehicle("ZENTORNO", SpawnPoint);
            SuspectVehicle.IsPersistent = true;

            Suspect = SuspectVehicle.CreateRandomDriver();
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = false;

            Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            
            base.Process();
            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(Suspect.Position) < 30f)
            {
                Pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(Pursuit, Suspect);
                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                PursuitCreated = true;
            }

            if (PursuitCreated && !LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(Pursuit))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

            Game.LogTrivial("TornadoCallouts Stolen Vehicle Has Cleaned Up.");
        }
    }
}
