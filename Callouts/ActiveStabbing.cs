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
        private Vector3 _SpawnPoint;
        private Vector3 _searcharea;
        private Blip _Blip;
        private LHandle _pursuit;
        private int _scenario = 0;
        private bool _hasBegunAttacking = false;
        private bool _isArmed = false;
        private bool _hasPursuitBegun = false;
        private bool _hasSpoke = false;
        private bool _pursuitCreated = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            _scenario = new Random().Next(0, 100);
            _SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(1000f));
            ShowCalloutAreaBlipBeforeAccepting(_SpawnPoint, 100f);
            CalloutMessage = "~w~ Reports of an Active Stabbing.";
            CalloutPosition = _SpawnPoint;
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS ASSAULT_WITH_AN_DEADLY_WEAPON CIV_ASSISTANCE IN_OR_ON_POSITION", _SpawnPoint);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Active Stabbing", "~b~Dispatch: ~w~Stop the susepect. Respond with ~r~Code 3");

            CalloutInterfaceAPI.Functions.SendMessage(this, "Citizens are reporting a person stabbing and attacking people. EMERGENCY RESPONSE. Approach with caution. ");

            _subject = new Ped(pedList[new Random().Next((int)pedList.Length)], _SpawnPoint, 0f);
            _subject.BlockPermanentEvents = true;
            _subject.IsPersistent = true;
            _subject.Tasks.Wander();

            _searcharea = _SpawnPoint.Around2D(1f, 2f);
            _Blip = new Blip(_searcharea, 80f);
            _Blip.Color = Color.Yellow;
            _Blip.EnableRoute(Color.Yellow);
            _Blip.Alpha = 0.5f;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (_Blip) _Blip.Delete();
            if (_subject) _subject.Delete();
            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            GameFiber.StartNew(delegate
            {
                if (_subject.DistanceTo(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront)) < 18f && !_isArmed)
                {
                    _subject.Inventory.GiveNewWeapon("WEAPON_KNIFE", 500, true);
                    _isArmed = true;
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
                                Game.DisplaySubtitle("~r~Suspect: ~w~I do not want to live anymore!", 4000);
                                _hasSpoke = true;
                                break;
                            case 2:
                                Game.DisplaySubtitle("~r~Suspect: ~w~Go away! - I'm not going back to the psychiatric hospital!", 4000);
                                _hasSpoke = true;
                                break;
                            case 3:
                                Game.DisplaySubtitle("~r~Suspect: ~w~I'm not going back to the psychiatric hospital!", 4000);
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
                        }
                    }
                }
                if (Game.LocalPlayer.Character.IsDead) End();
                if (Game.IsKeyDown(IniFile.EndCall)) End();
                if (_subject && _subject.IsDead) End();
                if (_subject && LSPD_First_Response.Mod.API.Functions.IsPedArrested(_subject)) End();
            }, "Active Stabbing [TornadoCallouts]");
            base.Process();
        }

        public override void End()
        {
            if (_subject) _subject.Dismiss();
            if (_Blip) _Blip.Delete();
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~TornadoCallouts", "~y~Active Stabbing", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            base.End();
        }
    }
}