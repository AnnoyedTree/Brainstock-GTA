using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Brainstock
{
    public struct VehicleSpawn
    {
        public Vector3 Position { get; private set; }
        public int Heading { get; private set; }
        public uint Model { get; private set; }

        public VehicleSpawn(uint model, float x, float y, float z, int yaw)
        {
            Model = model;
            Position = new Vector3(x, y, z);
            Heading = yaw;
        }
    }

    class VehicleSpawner : BaseScript
    {
        public static VehicleSpawn[] vehicleSpawns = {
                //MILITARY VEHICLE SPAWNS
                new VehicleSpawn((uint)VehicleHash.Crusader, -1806.845f, 2974.876f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Crusader, -1808.494f, 2972.084f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Crusader, -1810.195f, 2969.131f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Crusader, -1811.852f, 2966.17f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Crusader, -1813.525f, 2963.29f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Barracks2, -1803.399f, 2979.754f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Barracks2, -1801.726f, 2982.625f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Dune3, -1817.009f, 2958.928f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Dune3, -1818.222f, 2956.631f, 32.65f, 60),
                new VehicleSpawn((uint)VehicleHash.Dune3, -1819.662f, 2954.211f, 32.65f, 60),

                //OUTCAST VEHICLE SPAWNS
                new VehicleSpawn((uint)VehicleHash.Trash, 1670.609f, 2598.366f, 45.1f, 270),
                new VehicleSpawn((uint)VehicleHash.Trash, 1670.699f, 2601.656f, 45.1f, 270),
                new VehicleSpawn((uint)VehicleHash.Hauler2, 1668.152f, 2610.998f, 45.1f, 270),
                new VehicleSpawn((uint)VehicleHash.Hauler2, 1668.097f, 2607.558f, 45.1f, 270),
                new VehicleSpawn((uint)VehicleHash.Dukes2, 1720.858f, 2598.863f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Dukes2, 1717.551f, 2598.946f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Gargoyle, 1714.179f, 2598.249f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Gargoyle, 1712.674f, 2598.249f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Gargoyle, 1711.197f, 2598.249f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Sanctus, 1709.524f, 2598.249f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Technical, 1702.242f, 2594.914f, 45.16f, 0),
                new VehicleSpawn((uint)VehicleHash.Technical3, 1699.699f, 2594.914f, 45.16f, 0),
        };

        private static Vehicle[] vehicleList;

        public VehicleSpawner()
        {
            vehicleList = new Vehicle[vehicleSpawns.Length];

            Tick += OnTick;
        }

        public static async Task OnTick()
        {
            if (User.GetTeam() == 0)
                return;

            if (WorldEventController.IsPlayerHost())
                await CheckVehicleSpawns();

            await Delay(15000);
        }

        private static async Task CheckVehicleSpawns()
        {
            for (int i = 0; i < vehicleList.Length; i++)
            {
                Vehicle ent = vehicleList[i];
                if (ent == null || !DoesEntityExist(ent.Handle))
                    await SpawnNewVehicle(i);
                else if (ent.Health <= 0 || IsEmpty(ent))
                {
                    DeleteVehicle(ent, i);
                }
            }
        }

        private static bool IsEmpty(Vehicle veh)
        {
            Ped driver = veh.Driver;
            if (driver != null && DoesEntityExist(driver.Handle))
                return false;

            if (SafeZone.PositionInSafeZone(veh.Position))
                return false;

            foreach (Ped ped in World.GetAllPeds()) //TODO: Find a better way
                if (ped.IsPlayer)
                {
                    float dist = GetDistanceBetweenCoords(veh.Position.X, veh.Position.Y, veh.Position.Z, ped.Position.X, ped.Position.Y, ped.Position.Z, false);
                    if (dist <= 200)
                        return false;
                }

            return true;
        }

        private static async Task SpawnNewVehicle(int index)
        {
            VehicleSpawn data = vehicleSpawns[index];
            Vehicle veh = await Util.EntityCreate.CreateVehicle(data.Model, data.Position, data.Heading);

            await Delay(500);

            int netID = VehToNet(veh.Handle);
            if (veh == null || !NetworkGetEntityIsNetworked(veh.Handle) || netID <= 0)
            {
                veh.Delete();
                return;
            }

            SetEntityAsMissionEntity(veh.Handle, true, true);
            TriggerServerEvent("brainstock:AddSpawnVehicle", netID, index);

            vehicleList[index] = veh;
        }

        public static void AddServerVehicle(int netID, int index)
        {
            //Debug.WriteLine($"ServerEvent {netID}, {index}, {team}");
            if (WorldEventController.IsPlayerHost())
                return;

            foreach (Vehicle veh in World.GetAllVehicles())
            {
                if (veh.NetworkId == netID)
                {
                     vehicleList[index] = veh;
                }
            }
        }

        public static void RemoveServerVehicle(int index)
        {
            //if (WorldEventController.IsPlayerHost())
                //return;

            Vehicle ent = vehicleList[index];
            if (ent != null && DoesEntityExist(ent.Handle))
                ent.Delete();

            vehicleList[index] = null;
        }

        public static void DeleteVehicle(Vehicle veh, int index)
        {
            int netID = VehToNet(veh.Handle);
            TriggerServerEvent("brainstock:RemoveSpawnerVehicle", index);
        }

        public static async void LoadVehicleOnJoin(int netID, int index)
        {
            //await Delay(10000);
            //AddServerVehicle(netID, index);

            while (User.GetTeam() == 0)
                await Delay(1);

            int time = GetGameTimer();

            Vehicle ent = SearchVehicleByNetID(netID);
            while (ent == null && GetGameTimer() < (time + 20000))
            {
                ent = SearchVehicleByNetID(netID);
                await Delay(10);
            }

            if (ent == null)
            {
                Debug.WriteLine($"Vehicle search timed out {GetGameTimer()}");
                return;
            }

            vehicleList[index] = ent;
        }

        private static Vehicle SearchVehicleByNetID(int netID)
        {
            foreach (Vehicle veh in World.GetAllVehicles())
                if (NetworkGetEntityIsNetworked(veh.Handle) && VehToNet(veh.Handle) == netID)
                    return veh;

            return null;
        }
        public static void SwitchHost()
        {
            for (int i = 0; i < vehicleList.Length; i++)
            {
                Entity ent = vehicleList[i];
                if (ent != null && DoesEntityExist(ent.Handle))
                {
                    NetworkRequestControlOfEntity(ent.Handle);
                    NetworkRequestControlOfNetworkId(ent.NetworkId);
                    NetworkGetEntityOwner(ent.Handle);
                    SetNetworkIdSyncToPlayer(ent.NetworkId, Game.Player.Handle, true);
                }
            }
        }
    }
}
