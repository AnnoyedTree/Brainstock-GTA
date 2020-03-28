using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace BrainstockServer
{
    public class Users : BaseScript
    {
        private static int[] vehicleSpawner;
        private static int[] eventObjects;

        private static Player[] playerList;
        private static Player EventHost;

        private static int ReInitialize = 0;

        public Users()
        {
            //EventHandlers["OnPlayerDropped"] += new Action<string>(OnPlayerDropped);
            EventHandlers["brainstock:AddSpawnVehicle"] += new Action<int, int>(OnVehicleSpawned);
            EventHandlers["brainstock:RemoveSpawnerVehicle"] += new Action<int>(OnVehicleRemove);
            EventHandlers["brainstock:PlayerInit"] += new Action<Player>(OnPlayerInit);

            EventHandlers["brainstock:AddServerEvent"] += new Action<int, int>(AddWorldEvent);
            EventHandlers["brainstock:RemoveServerEvent"] += new Action<int>(RemoveWorldEvent);
            EventHandlers["brainstock:RequestAttachment"] += new Action<Player, int, int, int, float, float, float, float, float, float, int, bool>(AttachmentRequest);
            EventHandlers["brainstock:RequestDetatchment"] += new Action<Player, int>(DetachmentRequest);

            EventHandlers["playerConnecting"] += new Action<Player, string, CallbackDelegate, IDictionary<string, object>>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            vehicleSpawner = new int[128];
            eventObjects = new int[128];
            playerList = new Player[255];
        }

        protected static void OnPlayerConnecting([FromSource]Player player, string playerName, CallbackDelegate callback, IDictionary<string, object> deferrals)
        {
            string license = GetPlayerLicense(player);
            Debug.WriteLine($"[BRAINSTOCK] PLAYER CONNECTING -> {player.Name} [{GetPlayerServerId(player)}], {license}");

            /*DataTable data = MySQL.ExecuteQueryWithResults("SELECT * FROM users WHERE id = '" + license + "'");

            if (data.Rows.Count <= 0)
            {
                CreateAccount(license);
                Debug.WriteLine("[MySQL] Creating Account...");
            }
            else
                Debug.WriteLine("[MySQL] Account Found");*/

            playerList[GetPlayerServerId(player)] = player;

            if (GetNumPlayerIndices() <= 0)
            {
                vehicleSpawner = new int[46];
                eventObjects = new int[46];

                ReInitialize = GetPlayerServerId(player);
            }
        }

        protected static void OnPlayerDropped([FromSource]Player player, string reason)
        {
            string license = GetPlayerLicense(player);
            Debug.WriteLine($"[BRAINSTOCK] PLAYER DROPPED -> {player.Name} [{GetPlayerServerId(player)}], {license} '{reason}'");

            playerList[GetPlayerServerId(player)] = null;

            CheckHost();
        }

        private static void CreateAccount(string id)
        {
            MySQL.ExecuteQuery("INSERT INTO users (id) VALUES('" + id + "')");
        }

        public static void LoadAccount(DataTable data)
        {
            /*foreach (DataRow row in data.Rows)
            {

            }*/
        }

        private static void OnPlayerInit([FromSource]Player player)
        {
            if (ReInitialize != 0 && ReInitialize == GetPlayerServerId(player))
            {
                Debug.WriteLine("Hard Cleanup");
                TriggerClientEvent(player, "brainstock:HardCleanup");

                CheckHost();
                ReInitialize = 0;
                return;
            }

            for (int i = 0; i < vehicleSpawner.Length; i++)
            {
                if (vehicleSpawner[i] > 0)
                    TriggerClientEvent(player, "brainstock:LoadSpawnedVehicles", vehicleSpawner[i], i);
            }

            for (int i = 0; i < eventObjects.Length; i++)
            {
                if (eventObjects[i] > 0)
                    TriggerClientEvent(player, "brainstock:AddEvent", eventObjects[i], i);
            }

            CheckHost();

            //if (GetPlayerServerId(player) == 1)// first player to connect
            TriggerClientEvent(player, "brainstock:ServerSetup");
        }

        protected static void CheckHost()
        {
            int lowestID = int.MaxValue;
            Player host = null;

            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] != null)
                {
                    if (i < lowestID)
                    {
                        lowestID = i;
                        host = playerList[i];
                    }
                }
            }

            if (host != null && EventHost != host)
            {
                Debug.WriteLine($"[EventController] New Host: {host.Name} [{GetPlayerServerId(host)}]");
                TriggerClientEvent(host, "brainstock:SetEventHost");

                EventHost = host;
            }
        }

        private static string GetPlayerLicense(Player player)
        {
            return player.Identifiers["license"];
        }

        //Thank you AppiChudilko
        public static int GetPlayerServerId(Player player)
        {
            return Convert.ToInt32(player.Handle) <= 65535 ? Convert.ToInt32(player.Handle) : Convert.ToInt32(player.Handle) - 65535;
        }

        private static void OnVehicleSpawned(int netID, int index)
        {
            //Debug.WriteLine($"[VEHICLE] SPAWNED [{index}] -> {netID}");

            TriggerClientEvent("brainstock:SpawnedVehicle", netID, index);
            vehicleSpawner[index] = netID;
        }

        private static void OnVehicleRemove(int index)
        {
            //Debug.WriteLine($"[VEHICLE] REMOVED [{index}] -> {netID}");

            TriggerClientEvent("brainstock:RemovedVehicle", index);
            vehicleSpawner[index] = 0;
        }

        private static void AddWorldEvent(int netID, int index)
        {
            //Debug.WriteLine($"[EVENT] SPAWNED [{index}] -> {netID}");

            TriggerClientEvent("brainstock:AddEvent", netID, index);
            eventObjects[index] = netID;
        }

        private static void RemoveWorldEvent(int index)
        {
            //Debug.WriteLine($"[EVENT] REMOVED [{index}] -> {netID}");

            TriggerClientEvent("brainstock:RemoveEvent", index);
            eventObjects[index] = 0;
        }

        private static void AttachmentRequest([FromSource]Player player, int index, int serverID, int boneID, float x, float y, float z, float p, float yaw, float r, int vert, bool fixRot)
        {
            TriggerClientEvent("brainstock:AttachObject", index, serverID, boneID, x, y, z, p, yaw, r, vert, fixRot); //May not need serverID
        }

        private static void DetachmentRequest([FromSource]Player player, int index)
        {
            TriggerClientEvent("brainstock:DetatchObject", index);
        }
    }
}
