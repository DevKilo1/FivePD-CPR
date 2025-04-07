using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD_CPR;
using FivePD_HostageScenarioCallout;
using Newtonsoft.Json.Linq;

namespace CPR;

public class MedUtils
{
    public static List<Ped> CPRPeds = new List<Ped>();
    public class CPR
    {
        public Ped ped;
        public Ped plr = Game.PlayerPed;
        private bool early = true;
        public bool active = true;
        private bool isAmbientCPR = false;
        int survivalChance = 80;
        private JObject config;
        private EventHandlerDictionary eventHandlers = plugin.eventHandlers;
        private int netId;
        public CPR(Ped p, int netId, bool ambient = false)
        {
            this.netId = netId;
            if (Game.PlayerPed.NetworkId != netId) return;
            ped = p;
            isAmbientCPR = ambient;
            if (CPRPeds.Contains(p)) throw new Exception("The target ped is busy");
            if (CPRPeds.Contains(plr)) throw new Exception("The player ped is busy");
            CPRPeds.Add(p);
            CPRPeds.Add(plr);
            config = new Utils.Config();
        }
        
        public async Task Start(Ped p = null)
        {
            if (Game.PlayerPed.NetworkId != netId) return;
            while (ped == null || config == null)
                await BaseScript.Delay(100);
            if (p != null)
            {
                plr = p;
                if (CPRPeds.Contains(Game.PlayerPed))
                    CPRPeds.Remove(Game.PlayerPed);
            }

            try
            {
                await PrepareCPR();
                await CPR_Intro();
                await CPR_Pumping();
                await CPR_Finish();
            }
            catch (Exception ex)
            {
                active = false;
                Debug.WriteLine("Error in CPRHandler! " + ex.Message);
                Debug.WriteLine(ex.ToString());
                if (!isAmbientCPR)
                    Utils.ShowNotification("Error with CPR! " + ex.Message);
                return;
            }
        }

        private async Task CPR_Intro()
        {
            if (Game.PlayerPed.NetworkId != netId) return;
            
            if (!isAmbientCPR)
                Utils.ShowNotification("Starting CPR");
            // Intro
            
            /*int sceneId = API.NetworkCreateSynchronisedScene(ped.Position.X, ped.Position.Y, ped.Position.Z - 1f,
                ped.Rotation.X, ped.Rotation.Y, ped.Rotation.Z, 2, false, true, -1f, 0f, 1f);
            API.NetworkAddPedToSynchronisedScene(plr.Handle, sceneId, "mini@cpr@char_a@cpr_def", "cpr_intro", 2f, 2f,
                15000, 2, 0f, 0);
            API.NetworkAddEntityToSynchronisedScene(ped.Handle, sceneId, "mini@cpr@char_b@cpr_def", "cpr_intro", 1f, 1f,
                0);*/
            /*API.NetworkAddPedToSynchronisedScene(ped.Handle, sceneId, "mini@cpr@char_b@cpr_def", "cpr_intro", 2f, 2f,
                15000, 2, 0f, 0);*/
            await ped.Task.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_intro", 2f, 2f, 15000, AnimationFlags.Loop, 1f);
            Utils.KeepTaskPlayAnimation(ped, "mini@cpr@char_b@cpr_def", "cpr_intro", AnimationFlags.Loop);
            /*API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_def", "cpr_intro", 2.0f, 2.0f, 15000, 2, 0, true, true,
                true);*/
            await plr.Task.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", 2f, 2f, 15000, AnimationFlags.Loop, 1f);
            Utils.KeepTaskPlayAnimation(plr, "mini@cpr@char_a@cpr_def", "cpr_intro", AnimationFlags.Loop);
            /*API.TaskPlayAnim(plr.Handle, "mini@cpr@char_a@cpr_def", "cpr_intro", 2.0f, 2.0f, 15000, 2, 0,
                true,
                true, true);*/
            //API.NetworkStartSynchronisedScene(sceneId);
            
            if (!active)
            {
                OnCancel();
                return;
            }

            await BaseScript.Delay(12000);
            Utils.StopKeepTaskPlayAnimation(ped);
            Utils.StopKeepTaskPlayAnimation(plr);
            await BaseScript.Delay(3000);
            //API.NetworkStopSynchronisedScene(sceneId);
            if (!active)
            {
                OnCancel();
                return;
            }
        }

        private async Task CPR_Finish()
        {
            if (Game.PlayerPed.NetworkId != netId) return;
            int random = new Random().Next(1, 100);
            //Debug.WriteLine(random.ToString());
            //Debug.WriteLine(survivalChance.ToString());
            //Debug.WriteLine((survivalChance - random).ToString());
            //int random = 0; //new Random().Next(2); // 0 or 1
            if ((survivalChance - random) >= 0)
            {
                // Success
                //Debug.WriteLine("Success");
                if (!isAmbientCPR)
                    Utils.ShowNotification("CPR Success!");
                ped.Task.ClearAll();
                plr.Task.ClearAll();
                Utils.StopKeepTaskPlayAnimation(ped);
                Utils.StopKeepTaskPlayAnimation(plr);
                
                ped.Task.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_success", 2f, 2f, 26000, AnimationFlags.Loop,
                    1f);
                Utils.KeepTaskPlayAnimation(ped, "mini@cpr@char_b@cpr_str", "cpr_success", AnimationFlags.Loop);
                /*API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_str", "cpr_success", 2.0f, 2.0f, 26000, 0, 0, true,
                    true,
                    true);
                API.TaskPlayAnim(plr.Handle, "mini@cpr@char_a@cpr_str", "cpr_success", 2.0f, 2.0f, 26000, 0,
                    0,
                    true,
                    true, true);*/
                plr.Task.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_success", 2f, 2f, 26000, AnimationFlags.Loop,
                    1f);
                Utils.KeepTaskPlayAnimation(plr, "mini@cpr@char_a@cpr_str", "cpr_success", AnimationFlags.Loop);
                await BaseScript.Delay(26000);
                Utils.StopKeepTaskPlayAnimation(ped);
                Utils.StopKeepTaskPlayAnimation(plr);
                early = false;
            }
            else
            {
                // Failed
                //Debug.WriteLine("Failed");
                if (!isAmbientCPR)
                    Utils.ShowNotification("CPR Failed.");
                ped.Task.ClearAll();
                plr.Task.ClearAll();
                Utils.StopKeepTaskPlayAnimation(ped);
                Utils.StopKeepTaskPlayAnimation(plr);
                //Debug.WriteLine("Failed start");
                ped.Task.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_fail", 2.0f, 2.0f, 18000, AnimationFlags.Loop,
                    1f);
                Utils.KeepTaskPlayAnimation(ped, "mini@cpr@char_b@cpr_str", "cpr_fail", AnimationFlags.Loop);
                /*API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_str", "cpr_fail", 2.0f, 2.0f, 18000, 0, 0, true, true,
                    true);
                API.TaskPlayAnim(plr.Handle, "mini@cpr@char_a@cpr_str", "cpr_fail", 2.0f, 2.0f, 18000, 0, 0,
                    true,
                    true, true);*/
                plr.Task.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_fail", 2.0f, 2.0f, 18000, AnimationFlags.Loop,
                    1f);
                Utils.KeepTaskPlayAnimation(plr, "mini@cpr@char_a@cpr_str", "cpr_fail", AnimationFlags.Loop);
                await BaseScript.Delay(18000);
                Utils.StopKeepTaskPlayAnimation(ped);
                Utils.StopKeepTaskPlayAnimation(plr);
                ped.Kill();
            }

            OnCancel();
        }

        private async Task CPR_Pumping()
        {
            if (Game.PlayerPed.NetworkId != netId) return;
            ped.Task.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest", 2.0f, 2.0f, -1, AnimationFlags.Loop, 1f);
            Utils.KeepTaskPlayAnimation(ped, "mini@cpr@char_b@cpr_str", "cpr_pumpchest", AnimationFlags.Loop);
            /*API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_str", "cpr_pumpchest", 2.0f, 2.0f, -1, 1, 1, true, true,
                true);
            API.TaskPlayAnim(plr.Handle, "mini@cpr@char_a@cpr_str", "cpr_pumpchest", 2.0f, 2.0f, -1, 1, 1,
                true,
                true, true);*/
            plr.Task.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 2.0f, 2.0f, -1, AnimationFlags.Loop, 1f);
            Utils.KeepTaskPlayAnimation(plr, "mini@cpr@char_a@cpr_str", "cpr_pumpchest", AnimationFlags.Loop);
            int code = new Random().Next(99, 999999);
            bool stillWaiting = true;
            bool Result = false;
            if (!active)
            {
                OnCancel();
                return;
            }

            bool minigameEnabled = (bool)config["MinigameEnabled"] != null ? (bool)config["MinigameEnabled"] : false;
            if (plr.Handle != Game.PlayerPed.Handle)
                minigameEnabled = false;
            if (minigameEnabled)
            {
                int circles = (int)config["MinigameCirclesAmount"] != -1 ? (int)config["MinigameCirclesAmount"] : 3;
                int time = (int)config["MinigameTimelimitInSeconds"] != -1
                    ? (int)config["MinigameTimelimitInSeconds"]
                    : 15;

                BaseScript.TriggerEvent("CircleMinigame:client:openLockpick", circles, time, code);
                
                eventHandlers["CircleMinigame:client:openLockpick:Callback=" + code] += new Action<bool>((result) =>
                {
                    Result = result;
                    stillWaiting = false;
                    eventHandlers.Remove("CircleMinigame:client:openLockpick:Callback=" + code);
                });
                while (stillWaiting)
                {
                    if (!active)
                    {
                        OnCancel();
                        return;
                    }

                    await BaseScript.Delay(100);
                }
                if (!active)
                {
                    OnCancel();
                    return;
                }

                int MGSuccessSurvivalChance = (int)config["MinigameSuccessSurvivalChance"] > -1
                    ? (int)config["MinigameSuccessSurvivalChance"]
                    : 80;

                int MGFailSurvivalChance = (int)config["MinigameFailSurvivalChance"] > -1
                    ? (int)config["MinigameFailSurvivalChance"]
                    : 0;

                if (Result)
                {
                    survivalChance = MGSuccessSurvivalChance;
                    //Debug.WriteLine($"SurvivalChance is now {MGSuccessSurvivalChance}: Success");
                }
                else
                {
                    survivalChance = MGFailSurvivalChance;
                    //Debug.WriteLine($"SurvivalChance is now {MGFailSurvivalChance}: Fail");
                }
            }

            Utils.StopKeepTaskPlayAnimation(ped);
            Utils.StopKeepTaskPlayAnimation(plr);
            await BaseScript.Delay(5000);
        }
        
        private async Task PrepareCPR()
        {
            if (Game.PlayerPed.NetworkId != netId) return;
            API.NetworkRequestControlOfEntity(ped.Handle);
            if (plr.Handle != Game.PlayerPed.Handle)
                API.NetworkRequestControlOfEntity(plr.Handle);
            
            plr.Task.ClearAll();
            if (!active)
            {
                OnCancel();
                return;
            }

            ped.IsPersistent = true;
            ped.BlockPermanentEvents = true;
            ped.AlwaysKeepTask = true;
            
            var offset = API.GetOffsetFromEntityInWorldCoords(plr.Handle, 0f, 0.9f, -1f);
            var heading = plr.Heading;
            var newHeading = heading + 90f;
            if (newHeading < 0f)
                newHeading += 360f;
            else if (newHeading > 360f)
                newHeading -= 360f;
            if (!active) return;
            
            // Position ped
            if (!active)
            {
                OnCancel();
                return;
            }

            ped.Task.ClearAll();
            ped.Position = new Vector3(ped.Position.X, ped.Position.Y, ped.Position.Z + 2f);
            ped.Resurrect();
            ped.Health = ped.MaxHealth;
            ped.IsPositionFrozen = true;
            ped.Task.ClearAll();

            await BaseScript.Delay(50);
            if (!active)
            {
                OnCancel();
                return;
            }

            ped.Position = offset;
            ped.Heading = newHeading;
            //await BaseScript.Delay(500);
        }

        public async Task OnCancel()
        {
            if (Game.PlayerPed.NetworkId != netId) return;
            if (ped == null) return;
            if (ped.IsAlive)
                if (CPRPeds.Contains(ped)) CPRPeds.Remove(ped);
            if (CPRPeds.Contains(plr)) CPRPeds.Remove(plr);
            Utils.StopKeepTaskPlayAnimation(ped);
            Utils.StopKeepTaskPlayAnimation(plr);
            active = false;
            if (early)
                ped.Kill();
            ped.Task.ClearAll();
            plr.Task.ClearAll();
            ped.IsPositionFrozen = false;
            ped.Task.ClearAll();
            ped.Task.WanderAround();
            Utils.ReleaseEntity(ped);
            ped.MarkAsNoLongerNeeded();
        }
    }
}

