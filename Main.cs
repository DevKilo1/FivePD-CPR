﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CPR;
using FivePD_HostageScenarioCallout;
using FivePD.API;
using FivePD.API.Utils;
using Newtonsoft.Json.Linq;

namespace FivePD_CPR;

public class plugin : Plugin
{
    public static Utils.Config config = new Utils.Config();
    public static bool busy = false;
    public static ExportDictionary exports;
    public static EventHandlerDictionary eventHandlers;
    public static Ped targetPed;
    public static bool cancel = false;
    public static bool early = false;
    public static MedUtils.CPR activeCPR;

    private Dictionary<int, Vector3>
        coveredAreas = new Dictionary<int, Vector3>(); // netId, location (radius of coveredAreaRadius)

    private float coveredAreaRadius = 50f;

    public static async Task CPRHandler(Ped ped)
    {
        if (busy)
        {
            Utils.ShowNotification("You are already performing CPR!");
            return;
        }

        early = true;

        //Debug.WriteLine("Running CPRHandler");
        try
        {
            busy = true;

            API.NetworkRequestControlOfEntity(ped.Handle);
            Game.PlayerPed.Task.ClearAll();
            if (cancel)
            {
                Cleanup(ped);
                return;
            }

            ped.IsPersistent = true;
            ped.BlockPermanentEvents = true;
            ped.AlwaysKeepTask = true;
            var offset = API.GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 0.9f, -1f);
            var heading = Game.PlayerPed.Heading;
            var newHeading = heading + 90f;
            if (newHeading < 0f)
                newHeading += 360f;
            else if (newHeading > 360f)
                newHeading -= 360f;
            if (cancel) return;
            // Position ped
            if (cancel)
            {
                Cleanup(ped);
                return;
            }

            ped.Task.ClearAllImmediately();
            ped.Position = new Vector3(ped.Position.X, ped.Position.Y, ped.Position.Z + 2f);
            ped.Resurrect();
            ped.Health = ped.MaxHealth;
            ped.IsPositionFrozen = true;
            ped.Task.ClearAllImmediately();

            await Delay(50);
            if (cancel)
            {
                Cleanup(ped);
                return;
            }

            ped.Position = offset;
            ped.Heading = newHeading;
            //await Delay(1000);
            Utils.ShowNotification("Starting CPR");
            // Intro
            //Debug.WriteLine("Playing intro");
            
            API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_def", "cpr_intro", 2.0f, 2.0f, 15000, 2, 0, true, true,
                true);
            API.TaskPlayAnim(Game.PlayerPed.Handle, "mini@cpr@char_a@cpr_def", "cpr_intro", 2.0f, 2.0f, 15000, 2, 0,
                true,
                true, true);
            if (cancel)
            {
                Cleanup(ped);
                return;
            }

            await Delay(15000);
            if (cancel)
            {
                Cleanup(ped);
                return;
            }

            // Pumping
            API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_str", "cpr_pumpchest", 2.0f, 2.0f, -1, 1, 1, true, true,
                true);
            API.TaskPlayAnim(Game.PlayerPed.Handle, "mini@cpr@char_a@cpr_str", "cpr_pumpchest", 2.0f, 2.0f, -1, 1, 1,
                true,
                true, true);
            int survivalChance = 80;
            int code = new Random().Next(99, 999999);
            bool stillWaiting = true;
            bool Result = false;
            if (cancel)
            {
                Cleanup(ped);
                return;
            }

            bool minigameEnabled = (bool)config["MinigameEnabled"] != null ? (bool)config["MinigameEnabled"] : false;
            if (minigameEnabled)
            {
                int circles = (int)config["MinigameCirclesAmount"] != -1 ? (int)config["MinigameCirclesAmount"] : 3;
                int time = (int)config["MinigameTimelimitInSeconds"] != -1
                    ? (int)config["MinigameTimelimitInSeconds"]
                    : 15;

                TriggerEvent("CircleMinigame:client:openLockpick", circles, time, code);
                eventHandlers["CircleMinigame:client:openLockpick:Callback=" + code] += new Action<bool>((result) =>
                {
                    Result = result;
                    stillWaiting = false;
                    eventHandlers.Remove("CircleMinigame:client:openLockpick:Callback=" + code);
                });
                while (stillWaiting)
                {
                    if (cancel)
                    {
                        Cleanup(ped);
                        return;
                    }

                    await Delay(100);
                }

                if (cancel)
                {
                    Cleanup(ped);
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

            await Delay(5000);

            int random = new Random().Next(1, 100);
            //Debug.WriteLine(random.ToString());
            //Debug.WriteLine(survivalChance.ToString());
            //Debug.WriteLine((survivalChance - random).ToString());
            //int random = 0; //new Random().Next(2); // 0 or 1
            if ((survivalChance - random) >= 0)
            {
                // Success
                //Debug.WriteLine("Success");
                Utils.ShowNotification("CPR Success!");
                ped.Task.ClearAll();
                Game.PlayerPed.Task.ClearAll();
                API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_str", "cpr_success", 2.0f, 2.0f, 26000, 0, 0, true,
                    true,
                    true);
                API.TaskPlayAnim(Game.PlayerPed.Handle, "mini@cpr@char_a@cpr_str", "cpr_success", 2.0f, 2.0f, 26000, 0,
                    0,
                    true,
                    true, true);
                await Delay(26000);
                early = false;
            }
            else
            {
                // Failed
                //Debug.WriteLine("Failed");
                Utils.ShowNotification("CPR Failed.");
                ped.Task.ClearAll();
                Game.PlayerPed.Task.ClearAll();
                //Debug.WriteLine("Failed start");
                API.TaskPlayAnim(ped.Handle, "mini@cpr@char_b@cpr_str", "cpr_fail", 2.0f, 2.0f, 18000, 0, 0, true, true,
                    true);
                API.TaskPlayAnim(Game.PlayerPed.Handle, "mini@cpr@char_a@cpr_str", "cpr_fail", 2.0f, 2.0f, 18000, 0, 0,
                    true,
                    true, true);
                await Delay(18000);
                ped.Kill();
            }

            Cleanup(ped);
        }
        catch (Exception ex)
        {
            busy = false;
            Debug.WriteLine("Error in CPRHandler! " + ex.Message);
            Debug.WriteLine(ex.ToString());
            Utils.ShowNotification("Error with CPR! " + ex.Message);
            return;
        }


        //await Delay(30000);
        busy = false;
    }

    public static void Cleanup(Ped ped)
    {
        if (ped == null) return;
        if (early)
            ped.Kill();
        ped.Task.ClearAll();
        Game.PlayerPed.Task.ClearAll();
        ped.IsPositionFrozen = false;
        ped.Task.ClearAll();
        Utils.ReleaseEntity(ped);
        cancel = false;
        busy = false;
    }

    internal plugin()
    {
        Utils.RequestAnimDict("mini@cpr@char_b@cpr_def");
        Utils.RequestAnimDict("mini@cpr@char_a@cpr_def");
        Utils.RequestAnimDict("mini@cpr@char_b@cpr_str");
        Utils.RequestAnimDict("mini@cpr@char_a@cpr_str");
        Debug.WriteLine("^2Loaded FivePD CPR Script 1.0 by DevKilo!");

        EventHandlers["KiloCPR:NewAreaControl"] += NewAreaControlHandler;
        EventHandlers["KiloCPR:SyncAnim"] += SyncAnim;
    }

    private void SyncAnim(int netId, string animDict, string animSet, int duration)
    {
        if (netId == Game.PlayerPed.NetworkId) return;
        Ped plr = (Ped)Entity.FromNetworkId(netId);
        if (plr == null)
        {
            Debug.WriteLine("^1Null thing"); // Remove after testing
            return;
        }
        API.TaskPlayAnim(plr.Handle, animDict, animSet, 2.0f, 2.0f, duration, 2, 0,
            true,
            true, true);
    }

    private void NewAreaControlHandler(int ownerNetId, float x, float y, float z)
    {
        Vector3 pos = new Vector3(x, y, z);
        if (coveredAreas.ContainsKey(ownerNetId))
        {
            coveredAreas.Remove(ownerNetId);
        }

        coveredAreas.Add(ownerNetId, pos);
    }

    private bool ambientAIEnabled = true;

    private bool RequestArea(Vector3 pos)
    {
        // Check if it is free
        bool isFree = true;
        foreach (var keyValuePair in coveredAreas)
        {
            var netId = keyValuePair.Key;
            var coords = keyValuePair.Value;
            if (pos.DistanceTo(coords) <= coveredAreaRadius)
            {
                if (netId != Game.PlayerPed.NetworkId)
                    isFree = false;
            }
        }

        return isFree;
    }

    private async Task WaitForEitherKeypress(Control ctrl1, Control ctrl2)
    {
    }

    private async Task SaveAI(Ped passerBy, Ped findDeadPed)
    {
        if (passerBy.IsDead)
        {
            ambientAIEnabled = true;
            return;
        }
        passerBy.Task.RunTo(findDeadPed.Position);
        Utils.KeepTaskGoToForPed(passerBy, findDeadPed.Position, 1f, Utils.GoToType.Walk);
        await Utils.WaitUntilEntityIsAtPos(passerBy, findDeadPed.Position, 1f);

        await BaseScript.Delay(2000);
        if (passerBy.IsDead)
        {
            ambientAIEnabled = true;
            return;
        }
        await Utils.SubtitleChat(passerBy, "I will save you!", 87, 175, 247);
        MedUtils.CPR cpr = new MedUtils.CPR(findDeadPed,Game.PlayerPed.NetworkId);
        cpr.Start(passerBy);
        while (cpr.active)
        {
            if (passerBy == null || !passerBy.Exists() || passerBy.IsDead)
            {
                ambientAIEnabled = true;
                cpr.OnCancel();
                return;
            }
            await BaseScript.Delay(100);
        }
    }
}

public class Main : BaseScript
{
    public Main()
    {
        plugin.eventHandlers = EventHandlers;
        plugin.exports = Exports;
        API.RegisterCommand("performCPR", new Action<int, List<object>, string>((source, args, rawCommand) =>
        {
            //Debug.WriteLine("Running performCPR");
            plugin.targetPed = Utils.GetClosestPed(Game.PlayerPed.Position, 2f, true, false);
            if (plugin.targetPed == null)
            {
                Debug.WriteLine("Failed to fetch closest ped!");
                return;
            }

            Ped closestPed = Utils.GetClosestPed(Game.PlayerPed.Position, 20f, true, true);
            plugin.activeCPR = new MedUtils.CPR(plugin.targetPed, Game.PlayerPed.NetworkId);
            plugin.activeCPR.Start();

            //plugin.CPRHandler(plugin.targetPed);
        }), false);

        string keybind = (string)plugin.config["Keybind"] != ";" ? (string)plugin.config["Keybind"] : ";";
        //Debug.WriteLine("Keybind is "+keybind);
        try
        {
            API.RegisterKeyMapping("performCPR", "[FivePD] Perform CPR", "keyboard", keybind);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("This keybind is unsupported! The default is 'O'");
        }

        try
        {
            API.RegisterCommand("cancelCPR", new Action<int, List<object>, string>((source, args, rawCommand) =>
            {
                //Debug.WriteLine("Running performCPR");
                if (plugin.activeCPR == null) return;
                if (plugin.activeCPR.plr.Handle == Game.PlayerPed.Handle)
                {
                    Utils.ShowNotification("Cancelling CPR...");
                    plugin.activeCPR.OnCancel();
                    //plugin.Cleanup(plugin.targetPed);
                    plugin.cancel = true;
                }
            }), false);
            string keybind2 = (string)plugin.config["CancelKeybind"] != "X"
                ? (string)plugin.config["CancelKeybind"]
                : "X";
            API.RegisterKeyMapping("cancelCPR", "[FivePD] Cancel CPR", "keyboard", "X");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("This keybind is unsupported! The default is 'O'");
        }
    }
}