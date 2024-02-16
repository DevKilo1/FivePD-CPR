using CitizenFX.Core;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core.NaturalMotion;
using ERCallouts;
using FivePD.API;
using Newtonsoft.Json.Linq;

namespace ERCallouts
{
    internal class Utils
    {
        public static Vector3 GetRandomLocationInRadius(int min, int max)
        {
            int distance = new Random().Next(min, max);
            float offsetX = new Random().Next(-1 * distance, distance);
            float offsetY = new Random().Next(-1 * distance, distance);


            return World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0)));
        }
        public static async Task PedTaskLaptopHackAnimation(Ped ped, Vector3 runToPos, Vector3 explosionPos, Entity door)
        {
            Ped hacker = ped;
            Debug.WriteLine("gotocheck1");
            hacker.Task.RunTo(runToPos);
            Debug.WriteLine("gotocheck2");
            KeepTaskGoToForPed(hacker, runToPos, 1f);
            Debug.WriteLine("gotocheck3");
            await WaitUntilEntityIsAtPos(hacker, runToPos, 1f);
            Debug.WriteLine("gotocheck4");
            hacker.Task.AchieveHeading(249.7f);
            await BaseScript.Delay(750);
Debug.WriteLine("After goto");
            int bagscene = Function.Call<int>(Hash.NETWORK_CREATE_SYNCHRONISED_SCENE, runToPos.X - 0.5f, runToPos.Y, runToPos.Z + 0.4f, hacker.Rotation.X, hacker.Rotation.Y, hacker.Rotation.Z, 2, false, false, 1065353216, 0, 1.3);
            int bag = Function.Call<int>(Hash.CREATE_OBJECT, Function.Call<uint>(Hash.GET_HASH_KEY, "hei_p_m_bag_var22_arm_s"), hacker.Position.X, hacker.Position.Y, hacker.Position.Z + 0.2, true, true, false);
            Debug.WriteLine("After bag");
            Function.Call(Hash.SET_ENTITY_COLLISION, bag, false, true);
            Debug.WriteLine("After bag collision");
            Entity bagentity = Entity.FromHandle(bag);
            
            int laptop = Function.Call<int>(Hash.CREATE_OBJECT, Function.Call<uint>(Hash.GET_HASH_KEY, "hei_prop_hst_laptop"), hacker.Position.X, hacker.Position.Y, hacker.Position.Z + 0.2, true, true, true);
            Debug.WriteLine("After thermite");
            Entity thermiteEntity = Entity.FromHandle(laptop);
            Function.Call(Hash.SET_ENTITY_COLLISION, laptop, false, true);

            Function.Call(Hash.NETWORK_ADD_PED_TO_SYNCHRONISED_SCENE, hacker, bagscene, "anim@heists@ornate_bank@hack", "hack_enter", 1.5, -4.0, 1, 16, 1148846080, 0);
            Function.Call(Hash.NETWORK_ADD_ENTITY_TO_SYNCHRONISED_SCENE, bag, bagscene, "anim@heists@ornate_bank@hack", "hack_enter_bag", 4.0, -8.0, 1);
            Function.Call(Hash.NETWORK_ADD_ENTITY_TO_SYNCHRONISED_SCENE, laptop, bagscene, "anim@heists@ornate_bank@hack", "hack_enter_laptop", 4.0, -8.0, 1);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, hacker, 5, 0, 0, 0);
            Function.Call(Hash.NETWORK_START_SYNCHRONISED_SCENE, bagscene);
            await BaseScript.Delay(6000);
            Function.Call(Hash.NETWORK_STOP_SYNCHRONISED_SCENE, bagscene);

            int hackscene = Function.Call<int>(Hash.NETWORK_CREATE_SYNCHRONISED_SCENE, runToPos.X - 0.5f,runToPos.Y,runToPos.Z + 0.4f,hacker.Rotation.X,hacker.Rotation.Y,hacker.Rotation.Z,2,false,true, 1065353216,0,1);
            Function.Call(Hash.NETWORK_ADD_PED_TO_SYNCHRONISED_SCENE, hacker, hackscene, "anim@heists@ornate_bank@hack", "hack_loop", 0, 0, 1, 16, 1148846080, 0);
            Function.Call(Hash.NETWORK_ADD_ENTITY_TO_SYNCHRONISED_SCENE, bag, hackscene, "anim@heists@ornate_bank@hack", "hack_loop_bag", 4.0, -8.0, 1);
            Function.Call(Hash.NETWORK_ADD_ENTITY_TO_SYNCHRONISED_SCENE, laptop, hackscene, "anim@heists@ornate_bank@hack", "hack_loop_laptop", 1.0, -0.0, 1);
            Function.Call(Hash.NETWORK_START_SYNCHRONISED_SCENE, hackscene);
            await BaseScript.Delay(10000);
            Function.Call(Hash.NETWORK_STOP_SYNCHRONISED_SCENE, hackscene);

            int hackexit = Function.Call<int>(Hash.NETWORK_CREATE_SYNCHRONISED_SCENE, runToPos.X - 0.5f, runToPos.Y, runToPos.Z + 0.4f, hacker.Rotation.X, hacker.Rotation.Y, hacker.Rotation.Z, 2, false, false, 1065353216, -1, 1.3);
            Function.Call(Hash.NETWORK_ADD_PED_TO_SYNCHRONISED_SCENE, hacker, hackexit, "anim@heists@ornate_bank@hack", "hack_exit", 0, 0, -1, 16, 1148846080, 0);
            Function.Call(Hash.NETWORK_ADD_ENTITY_TO_SYNCHRONISED_SCENE, bag, hackexit, "anim@heists@ornate_bank@hack", "hack_exit_bag", 4.0, -8.0, 1);
            Function.Call(Hash.NETWORK_ADD_ENTITY_TO_SYNCHRONISED_SCENE, laptop, hackexit, "anim@heists@ornate_bank@hack", "hack_exit_laptop", 4.0, -8.0, 1);
            Function.Call(Hash.NETWORK_START_SYNCHRONISED_SCENE, hackexit);
            await BaseScript.Delay(6000);
            Function.Call(Hash.NETWORK_STOP_SYNCHRONISED_SCENE, hackexit);
            bagentity.Delete();
            Entity laptope = Entity.FromHandle(laptop);
            laptope.Delete();
            Entity vaultdoor = door;
            vaultdoor.IsPositionFrozen = false;
            Function.Call(Hash.ADD_EXPLOSION, explosionPos.X,explosionPos.Y,explosionPos.Z, 2, 0.5f, false, false, 0f, true);
            await BaseScript.Delay(1000);
            
            vaultdoor.IsPositionFrozen = true;
        }
        public static async Task PedTaskSassyChatAnimation(Ped ped)
        {
            if (!API.HasAnimDictLoaded("oddjobs@assassinate@vice@hooker"))
                API.RequestAnimDict("oddjobs@assassinate@vice@hooker");
            Debug.WriteLine("Requesting anim dict");
            if (!API.HasAnimSetLoaded("argue_b"))
                API.RequestAnimSet("argue_b");
            Debug.WriteLine("Requesting anim set");
            while (!API.HasAnimDictLoaded("oddjobs@assassinate@vice@hooker"))
                await BaseScript.Delay(200);
            Debug.WriteLine("Loaded anim dict");
            
            Debug.WriteLine("Loaded anim set");
            Debug.WriteLine("after waiting load");
            ped.Task.ClearAllImmediately();
            ped.Task.PlayAnimation("oddjobs@assassinate@vice@hooker", "argue_b", 8f, 8f, 10000, AnimationFlags.Loop,
                1f);
        }

        public static async Task KeepTaskEnterVehicle(Ped ped, Vehicle veh, VehicleSeat targetSeat)
        {
            SetIntoVehicleAfterTimer(ped, veh, VehicleSeat.Any, 30000);
            while (true)
            {
                Vector3 startPos = ped.Position;
                await BaseScript.Delay(2500);
                if (!ped.IsInVehicle(veh) && ped.Position.DistanceTo(startPos) < 1f)
                    ped.Task.EnterVehicle(veh, targetSeat);
                await BaseScript.Delay(2500);
            }
        }

        public static async Task WaitUntilPedIsInVehicle(Ped ped, Vehicle veh, VehicleSeat targetSeat)
        {
            while (true)
            {
                if (ped.IsInVehicle(veh) || ped.IsInVehicle() && ped.SeatIndex == targetSeat)
                    return;
                await BaseScript.Delay(500);
            }
            //
        }

        public static async Task RequestAnimDict(string animDict)
        {
            API.RequestAnimDict(animDict);
            while (!API.HasAnimDictLoaded(animDict))
                await BaseScript.Delay(100);
        }

        public static async Task<Ped> SpawnPedOneSync(PedHash pedHash, Vector4 pos)
        {
            Ped ped = await World.CreatePed(pedHash, (Vector3)pos, pos.W);
            ped.IsPersistent = true;
            return ped;
        }

        public static async Task<Vehicle> SpawnVehicleOneSync(VehicleHash vehicleHash, Vector4 pos)
        {
            Vehicle veh = await World.CreateVehicle(new(vehicleHash), (Vector3)pos, pos.W);
            veh.IsPersistent = true;
            return veh;
        }

        public static async Task SetIntoVehicleAfterTimer(Ped ped, Vehicle veh, VehicleSeat targetSeat, int ms)
        {
            await BaseScript.Delay(ms);
            if (!ped.IsInVehicle(veh))
            {
                ped.SetIntoVehicle(veh, targetSeat);
            }
        }
        public static async Task KeepTaskGoToForPed(Ped ped, Vector3 pos, float buffer)
        {
            while (true)
            {
                Vector3 startPos = ped.Position;
                await BaseScript.Delay(1000);
                if (ped.Position == startPos)
                {
                    ped.Task.GoTo(pos);
                }

                if (ped.Position.DistanceTo(pos) < buffer)
                    return;
                await BaseScript.Delay(1000);
            }
        }
        public static async Task WaitUntilEntityIsAtPos(Entity ent, Vector3 pos, float buffer)
        {
            if (ent == null || !ent.Exists())
                return;
            while (true)
            {
                if (ent == null || !ent.Exists())
                    return;
                if (ent.Position.DistanceTo(pos) < buffer)
                    break;
                await BaseScript.Delay(200);
            }
        }
        
        public static PedHash GetRandomPed()
        {
            return RandomUtils.GetRandomPed(exclusions);
        }

        public static Vector3 JSONCoordsToVector3(JObject coordsObj)
        {
            return new Vector3((float)coordsObj["x"], (float)coordsObj["y"], (float)coordsObj["z"]);
        }
        public static Vector4 JSONCoordsToVector4(JObject coordsObj)
        {
            return new Vector4((float)coordsObj["x"], (float)coordsObj["y"], (float)coordsObj["z"],
                (float)coordsObj["w"]);
        }
        
        public static void KeepTask(Ped ped)
        {
            if (ped == null || !ped.Exists()) return;
            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;
        }

        public static void UnKeepTask(Ped ped)
        {
            ped.IsPersistent = false;
            ped.AlwaysKeepTask = false;
            ped.BlockPermanentEvents = false;
        }

        public static void SendWebhookErrMessage(string message,string innerException)
        {
            BaseScript.TriggerServerEvent("DevKiloCalloutHandler::CreateErrorReport", message, innerException);
        }

        public static JArray FleecaLocations = new JArray()
        {
            new JObject()
            {
                ["name"] = "ReplaceThis",
                ["coords"] = new JObject()
                { 
                    ["x"] = 151.22694396973,
                    ["y"] = -1036.7858886719,
                    ["z"] = 29.339130401611
                },
                ["suspect1Pos"] = new JObject()
                { 
                    ["x"] = 152.4542388916,
                    ["y"] = -1040.8637695313,
                    ["z"] = 29.374210357666,
                    ["w"] = 32.407669067383 
                },
                ["suspect2Pos"] = new JObject()
                { 
                    ["x"] = 146.56207275391,
                    ["y"] = -1038.3604736328,
                    ["z"] = 29.367946624756,
                    ["w"] = 268.79797363281 
                },
                ["suspect3Pos"] = new JObject()
                { 
                    ["x"] = 143.92475891113,
                    ["y"] = -1042.2863769531,
                    ["z"] = 29.367971420288,
                    ["w"] = 340.05996704102 
                },
                ["suspect4Pos"] = new JObject()
                { 
                    ["x"] = 145.72930908203,
                    ["y"] = -1044.5009765625,
                    ["z"] = 29.377872467041,
                    ["w"] = 236.51379394531 
                },
                ["hostage1Pos"] = new JObject()
                { 
                    ["x"] = 151.2699432373,
                    ["y"] = -1039.2730712891,
                    ["z"] = 29.377555847168,
                    ["w"] = 341.34036254883 
                },
                ["hostage2Pos"] = new JObject()
                { 
                    ["x"] = 149.86596679688,
                    ["y"] = -1038.5411376953,
                    ["z"] = 29.377880096436,
                    ["w"] = 337.91201782227 
                },
                ["vaultDoorPos"] = new JObject()
                { 
                    ["x"] = 147.62257385254,
                    ["y"] = -1044.4913330078,
                    ["z"] = 29.368091583252 
                },
                ["vaultDoorExplosionPos"] = new JObject()
                { 
                    ["x"] = 148.37721252441,
                    ["y"] = -1045.5529785156,
                    ["z"] = 29.346343994141 
                },
                ["moneyCart1Pos"] = new JObject()
                { 
                    ["x"] = 148.41299438477,
                    ["y"] = -1050.3021240234,
                    ["z"] = 29.340667724609,
                    ["w"] = 248.48597717285 
                },
                ["moneyCart2Pos"] = new JObject()
                { 
                    ["x"] = 150.23678588867,
                    ["y"] = -1048.8026123047,
                    ["z"] = 29.346458435059,
                    ["w"] = 236.41622924805 
                },
                ["suspectVehiclePos"] = new JObject()
                { 
                    ["x"] = 157.36538696289,
                    ["y"] = -1037.5871582031,
                    ["z"] = 29.219705581665,
                    ["w"] = 344.15447998047 
                }
            }
        };
        public static Vector3[] ConvenienceLocations = new Vector3[]
        {
            new(-712.12f, -913.06f, 19.22f),
            new(29.49f, -1346.94f, 29.5f),
            new(-50.78f, -1753.61f, 29.42f),
            new(376.4f, 325.75f, 103.57f),
            new(-1223.94f, -906.52f, 12.33f)
        };
        public static Vector3[] HomeLocations = new Vector3[]
        {
            new(-120.15f, -1574.39f, 34.18f),
            new(-148.07f, -1596.64f, 38.21f),
            new(-32.44f, -1446.5f, 31.89f),
            new(-14.11f, -1441.93f, 31.1f),
            new(72.21f, -1938.59f, 21.37f),
            new(126.68f, -1930.01f, 21.38f),
            new(270.2f, -1917.19f, 26.18f),
            new(325.68f, -2050.86f, 20.93f),
            new(1099.52f, -438.65f, 67.79f),
            new(1046.24f, -498.14f, 64.28f),
            new(980.1f, -627.29f, 59.24f),
            new(943.45f, -653.49f, 58.43f),
            new(1223.08f, -696.85f, 60.8f),
            new(1201.06f, -575.68f, 69.14f),
            new(1265.9f, -648.33f, 67.92f),
            new(1241.5f, -566.4f, 69.66f),
            new(1204.73f, -557.74f, 69.62f),
            new(1223.06f, -696.74f, 60.81f),
            new(930.88f, -244.82f, 69.0f),
            new(880.01f, -205.01f, 71.98f),
            new(798.39f, -158.83f, 74.89f),
            new(820.86f, -155.84f, 80.75f), // Second floor
            new(208.65f, 74.53f, 87.9f),
            new(119.34f, 494.13f, 147.34f),
            new(79.74f, 486.13f, 148.2f),
            new(151.2f, 556.09f, 183.74f),
            new(232.1f, 672.06f, 189.98f),
            new(-66.76f, 490.13f, 144.88f),
            new(-175.94f, 502.73f, 137.42f),
            new(-230.26f, 488.29f, 128.77f),
            new(-355.91f, 469.56f, 112.61f),
            new(-353.17f, 423.13f, 110.98f),
            new(-312.53f, 474.91f, 111.83f),
            new(-348.99f, 514.99f, 120.65f),
            new(-376.59f, 547.66f, 123.85f),
            new(-406.6f, 566.28f, 124.61f),
            new(-520.28f, 594.07f, 120.84f),
            new(-581.37f, 494.04f, 108.26f),
            new(-678.67f, 511.67f, 113.53f),
            new(-784.46f, 459.47f, 100.25f),
            new(-824.67f, 422.6f, 92.13f),
            new(-881.97f, 364.1f, 85.36f),
            new(-967.59f, 436.88f, 80.57f),
            new(-1570.71f, 23.0f, 59.55f),
            new(-1629.9f, 36.25f, 62.94f),
            new(-1750.22f, -695.19f, 11.75f),
            new(-1270.03f, -1296.53f, 4.0f),
            new(-1148.96f, -1523.2f, 10.63f),
            new(-1105.61f, -1596.67f, 4.61f)
        };

        public static bool IsPedNonLethalOrMelee(Ped ped)
        {
            WeaponHash weapon = ped.Weapons.Current;
            return nonlethals.Contains(weapon) || melee.Contains(weapon);
        }

        public static WeaponHash[] nonlethals =
        {
            WeaponHash.Ball,
            WeaponHash.Parachute,
            WeaponHash.Flare,
            WeaponHash.Snowball,
            WeaponHash.Unarmed,
            WeaponHash.StunGun,
            WeaponHash.FireExtinguisher
        };

        public static WeaponHash[] melee =
        {
            WeaponHash.Crowbar,
            WeaponHash.Bat,
            WeaponHash.Bottle,
            WeaponHash.Flashlight,
            WeaponHash.Hatchet,
            WeaponHash.Knife,
            WeaponHash.Machete,
            WeaponHash.Nightstick,
            WeaponHash.Unarmed,
            WeaponHash.PoolCue,
            WeaponHash.StoneHatchet
        };

        public static VehicleHash GetRandomVehicleForRobberies()
        {
            return RandomUtils.GetRandomVehicle(FourPersonVehicleClasses);
        }

        public static IEnumerable<VehicleClass> FourPersonVehicleClasses = new List<VehicleClass>()
        {
            VehicleClass.Compacts,
            VehicleClass.Sedans,
            VehicleClass.Vans,
            VehicleClass.SUVs
        };
        
        public static PedHash GetRandomSuspect()
        {
            return suspects[new Random().Next(suspects.Length - 1)];
        }
        public static WeaponHash GetRandomWeapon()
        {
            int index = new Random().Next(weapons.Length);
            return weapons[index];
        }

        public static WeaponHash[] weapons =
        {
            WeaponHash.AssaultRifle,
            WeaponHash.PumpShotgun,
            WeaponHash.CombatPistol
        };

        public static PedHash[] suspects =
        {
            PedHash.MerryWeatherCutscene,
            PedHash.Armymech01SMY,
            PedHash.MerryWeatherCutscene,
            PedHash.ChemSec01SMM,
            PedHash.Blackops01SMY,
            PedHash.CiaSec01SMM,
            PedHash.PestContDriver,
            PedHash.PestContGunman,
            PedHash.TaoCheng,
            PedHash.Hunter,
            PedHash.EdToh,
            PedHash.PrologueMournMale01,
            PedHash.PoloGoon01GMY
        };

        public static IEnumerable<WeaponHash> weapExclusions = new List<WeaponHash>
        {
            WeaponHash.Ball,
            WeaponHash.Bat,
            WeaponHash.Snowball,
            WeaponHash.RayMinigun,
            WeaponHash.RayCarbine,
            WeaponHash.BattleAxe,
            WeaponHash.Bottle,
            WeaponHash.BZGas,
            WeaponHash.Crowbar,
            WeaponHash.Dagger,
            WeaponHash.FireExtinguisher,
            WeaponHash.Firework,
            WeaponHash.Flare,
            WeaponHash.FlareGun,
            WeaponHash.Flashlight,
            WeaponHash.GolfClub,
            WeaponHash.Grenade,
            WeaponHash.GrenadeLauncher,
            WeaponHash.Gusenberg,
            WeaponHash.Hammer,
            WeaponHash.Hatchet,
            WeaponHash.StoneHatchet,
            WeaponHash.StunGun,
            WeaponHash.Musket,
            WeaponHash.HeavySniper,
            WeaponHash.HeavySniperMk2,
            WeaponHash.HomingLauncher,
            WeaponHash.Knife,
            WeaponHash.KnuckleDuster,
            WeaponHash.Machete,
            WeaponHash.Molotov,
            WeaponHash.Nightstick,
            WeaponHash.NightVision,
            WeaponHash.Parachute,
            WeaponHash.PetrolCan,
            WeaponHash.PipeBomb,
            WeaponHash.PoolCue,
            WeaponHash.ProximityMine,
            WeaponHash.Railgun,
            WeaponHash.RayPistol,
            WeaponHash.RPG,
            WeaponHash.SmokeGrenade,
            WeaponHash.SniperRifle,
            WeaponHash.StickyBomb,
            WeaponHash.SwitchBlade,
            WeaponHash.Unarmed,
            WeaponHash.Wrench
        };

        public static IEnumerable<PedHash> exclusions = new List<PedHash>()
        {
            PedHash.Acult01AMM,
            PedHash.Motox01AMY,
            PedHash.Boar,
            PedHash.Cat,
            PedHash.ChickenHawk,
            PedHash.Chimp,
            PedHash.Chop,
            PedHash.Cormorant,
            PedHash.Cow,
            PedHash.Coyote,
            PedHash.Crow,
            PedHash.Deer,
            PedHash.Dolphin,
            PedHash.Fish,
            PedHash.Hen,
            PedHash.Humpback,
            PedHash.Husky,
            PedHash.KillerWhale,
            PedHash.MountainLion,
            PedHash.Pig,
            PedHash.Pigeon,
            PedHash.Poodle,
            PedHash.Rabbit,
            PedHash.Rat,
            PedHash.Retriever,
            PedHash.Rhesus,
            PedHash.Rottweiler,
            PedHash.Seagull,
            PedHash.HammerShark,
            PedHash.TigerShark,
            PedHash.Shepherd,
            PedHash.Stingray,
            PedHash.Westy,
            PedHash.BradCadaverCutscene,
            PedHash.Orleans,
            PedHash.OrleansCutscene,
            PedHash.ChiCold01GMM,
            PedHash.DeadHooker,
            PedHash.Marston01,
            PedHash.Niko01,
            PedHash.PestContGunman,
            PedHash.Pogo01,
            PedHash.Ranger01SFY,
            PedHash.Ranger01SMY,
            PedHash.RsRanger01AMO,
            PedHash.Zombie01
        };
        
    }
}
