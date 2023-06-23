using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using CalloutInterfaceAPI;
using System;
using System.Drawing;
using System.Runtime;


namespace TornadoCallouts.Callouts
{
    [CalloutInfo("Active Stabbing", CalloutProbability.Medium)]
    public class ActiveStabbing: Callout
    {
        private string[] pedList = new string[] { "A_F_M_SouCent_01", "A_F_M_SouCent_02", "A_M_Y_Skater_01", "A_M_M_FatLatin_01", "A_M_M_EastSA_01", "A_M_Y_Latino_01", "G_M_Y_FamDNF_01",
                                                  "G_M_Y_FamCA_01", "G_M_Y_BallaSout_01", "G_M_Y_BallaOrig_01", "G_M_Y_BallaEast_01", "G_M_Y_StrPunk_02", "S_M_Y_Dealer_01", "A_M_M_RurMeth_01",
                                                  "A_M_M_Skidrow_01", "A_M_Y_MexThug_01", "G_M_Y_MexGoon_03", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_01", "G_M_Y_SalvaGoon_01", "G_M_Y_SalvaGoon_02",
                                                  "G_M_Y_SalvaGoon_03", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_StrPunk_01" };
        private Ped _subject;
        private Ped _victim;
        private Ped _bystander;
        private Vector3 _SpawnPoint;
        private Vector3 _searcharea;
        private Blip _subjectBlip;
        private LHandle _pursuit;
        private int _scenario = 0;
        private bool _hasBegunAttacking = false;
        private bool _isArmed = false;
        private bool _hasPursuitBegun = false;
        private bool _hasSpoke = false;
        private bool _pursuitCreated = false;
        private bool _hasBegunFleeing = false;
        private bool _fightAgainstVictim = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            _scenario = new Random().Next(0, 100);
            _SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(1000f));
            ShowCalloutAreaBlipBeforeAccepting(_SpawnPoint, 100f);
            CalloutMessage = "~w~Reports of an Active Stabbing.";
            CalloutPosition = _SpawnPoint;
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS ASSAULT_WITH_AN_DEADLY_WEAPON CIV_ASSISTANCE IN_OR_ON_POSITION", _SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            CalloutInterfaceAPI.Functions.SendMessage(this, "Citizens are reporting a person is actively stabbing and attacking people. EMERGENCY RESPONSE. Approach with caution. ");

            _subject = new Ped(pedList[new Random().Next((int)pedList.Length)], _SpawnPoint, 0f);
            _subject.BlockPermanentEvents = true;
            _subject.IsPersistent = true;

            _victim = new Ped(pedList[new Random().Next((int)pedList.Length)], _SpawnPoint, 0f);
            _victim.BlockPermanentEvents = true;
            _victim.IsPersistent = true;

            _bystander = new Ped(pedList[new Random().Next((int)pedList.Length)], _SpawnPoint, 0f);
            _bystander.BlockPermanentEvents = true;
            _bystander.IsPersistent = true;
          
            _searcharea = _SpawnPoint.Around2D(1f, 2f);
            _subjectBlip = _subject.AttachBlip();
            _subjectBlip.Color = Color.Red;
            _subjectBlip.EnableRoute(Color.Yellow);
            _subjectBlip.Alpha = 0.5f;


            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (_subjectBlip) _subjectBlip.Delete();
            if (_subject) _subject.Delete();
            if (_victim) _victim.Delete();
            if (_bystander) _bystander.Delete();

            base.OnCalloutNotAccepted();
        }
        public override void Process()
        {
            GameFiber.StartNew(delegate
            {
                if (_subject.DistanceTo(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront)) <= 200f && !_isArmed && !_fightAgainstVictim)
                {
                    _subject.Inventory.GiveNewWeapon("WEAPON_KNIFE", 500, true);
                    _isArmed = true;
                    _subject.Tasks.FightAgainst(_victim);
                    _fightAgainstVictim = true;
                }

                if (_bystander && _bystander.DistanceTo(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront)) <= 70f && _fightAgainstVictim && !_hasBegunFleeing)
                {
                    _bystander.KeepTasks = true;
                    _bystander.Tasks.ReactAndFlee(_subject);
                    _hasBegunFleeing = true;
                }

                if (_subject && _subject.DistanceTo(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront)) < 18f && !_hasBegunAttacking)
                {
                    if (_scenario > 40)
                    {
                        _subject.KeepTasks = true;
                        _subject.Tasks.FightAgainst(Game.LocalPlayer.Character);
                        _hasBegunAttacking = true;
                        switch (new Random().Next(1, 3))
                        {
                            case 1:
                                Game.DisplaySubtitle("~r~Suspect: ~w~ I will kill them all!", 4000);
                                _hasSpoke = true;
                                break;
                            case 2:
                                Game.DisplaySubtitle("~r~Suspect: ~w~Go away! - I have business to finish!", 4000);
                                _hasSpoke = true;
                                break;
                            case 3:
                                Game.DisplaySubtitle("~r~Suspect: ~w~If you take one step closer I'll do it!", 4000);
                                _hasSpoke = true;
                                break;
                            default: break;
                        }
                        GameFiber.Wait(2000);
                    }
                    else
                    {
                        if (!_hasPursuitBegun)
                        {
                            _pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                            LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(_pursuit, _subject);
                            LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(_pursuit, true);
                            _hasPursuitBegun = true;

                            _subjectBlip.IsFriendly = false;

                            if (!_subject.IsInAnyVehicle(false))
                            {
                                Vehicle nearestVehicle = GetClosestVehicle(_subject.Position, 30f);
                                if (nearestVehicle != null)
                                {
                                    _subject.Tasks.EnterVehicle(nearestVehicle, -1);
                                    GameFiber.Wait(2000);
                                    _subject.Tasks.CruiseWithVehicle(nearestVehicle, 20f, VehicleDrivingFlags.Emergency);
                                }
                            }
                            else
                            {
                                _subject.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);
                            }
                        }
                    }
                }

                if (Game.LocalPlayer.Character.IsDead) End();
                if (Game.IsKeyDown(IniFile.EndCall)) End();
                if (_subject && _subject.IsDead) End();
                if (_subject && LSPD_First_Response.Mod.API.Functions.IsPedArrested(_subject)) End();
                if (_hasPursuitBegun && !LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(_pursuit)) End();

            }, "[TornadoCallouts] Active Stabbing");
            base.Process();
        }

        private Vehicle GetClosestVehicle(Vector3 position, float maxDistance)
        {
            Vehicle closestVehicle = null;
            float closestDistance = maxDistance;

            foreach (Vehicle vehicle in World.GetAllVehicles())
            {
                float distance = Vector3.Distance(position, vehicle.Position);

                if (distance < closestDistance)
                {
                    closestVehicle = vehicle;
                    closestDistance = distance;
                }
            }

            return closestVehicle;
        }

        public override void End()
        {
            if (_subject) _subject.Dismiss();
            if (_victim) _victim.Dismiss();
            if (_bystander) _bystander.Dismiss();
            if (_subjectBlip) _subjectBlip.Delete();

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Active Stabbing", "~b~You: ~w~Dispatch we're code 4.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");

            base.End();
        }

    }
}