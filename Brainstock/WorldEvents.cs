using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

/// <summary>
/// /////////////////////////////////
/// Network ID may be if use here <prop.NetworkId()> 4/12/19
/// /////////////////////////////////
/// </summary>
namespace Brainstock
{
    public enum Events
    {
        Prop = 0,
        Vehicle,
        Ped
    }

    public struct WorldEvent
    {
        public string DisplayName { get; private set; }
        public int EntityType { get; private set; }
        public string ModelName { get; private set; }
        public Vector3 EntityPos { get; private set; }
        public Action<Entity, int> Callback { get; private set; }
        public Entity EventEntity { get; set; }
        public float Heading { get; private set; }
        public BlipSprite Blip { get; private set; }

        public WorldEvent(string displayName, string modelName, int type, float x, float y, float z, Action<Entity, int> callback, BlipSprite sprite, float heading = 0, Entity ent = null) 
        {
            DisplayName = displayName;
            ModelName = modelName;
            EntityPos = new Vector3(x, y, z);
            EntityType = type;
            Callback = callback;
            EventEntity = ent;
            Heading = heading;
            Blip = sprite;
        }
    }

    public class WorldEvents : BaseScript
    {
        private static WorldEvent[] eventList =
        {
            new WorldEvent("RPG Pickup", "prop_box_guncase_03a", (int)Events.Prop, -37.00494f, 1946.552f, 190.1861f, Event_RPGPickup, BlipSprite.RPG), //prop_box_guncase_03a
            new WorldEvent("Steal Truck", "boxville", (int)Events.Vehicle, 67.100f, -391.524f, 39.92f, Event_StealTruck, BlipSprite.VinewoodTours),
            new WorldEvent("Fuel Supply", "tanker", (int)Events.Vehicle, -232.4302f, 6259.027f, 31.49f, Event_StealFuel, BlipSprite.JerryCan, 161), //-1854.767f, 2987.573f, 32.9f
            new WorldEvent("Fuel Supply", "tanker", (int)Events.Vehicle, -1027.266f, -2216.088f, 9.0f, Event_StealFuel, BlipSprite.JerryCan, 225), //Airport Spawn
            new WorldEvent("Medicine Bag", "prop_big_bag_01", (int)Events.Prop, 450.9678f, -975.1078f, 30.67f, Event_MedsPickup, BlipSprite.Health), //-1842.805f, 2982.699f, 32.81f, //450.9678f, -975.1078f, 30.67f
            new WorldEvent("Medicine Bag", "prop_big_bag_01", (int)Events.Prop, 363.2757f, -592.8103f, 43.31f, Event_MedsPickup, BlipSprite.Health), //RCB Hospital
            new WorldEvent("Sniper Pickup", "prop_box_guncase_03a", (int)Events.Prop, 1544.888f, -2138.895f, 77.81f, Event_SniperPickup, BlipSprite.Sniper), //prop_box_guncase_03a
        };

        private static int[] eventObjects;
        private static bool bCanPickup;

        public WorldEvents()
        {
            eventObjects = new int[eventList.Length];
            Tick += OnTick;

            bCanPickup = true;
        }

        private static async Task OnTick()
        {
            if (User.GetTeam() == 0 )
                return;

            if (WorldEventController.IsPlayerHost())
            {
                if (NumberOfActiveEvents() < GameRules.MAX_WORLD_EVENTS)
                    await SpawnNewWorldEvent();

                await DeleteInvalidWorldEvents();
            }

            await TickForAllActiveEvents();
            await Delay(100);
        }

        private static async Task SpawnNewWorldEvent()
        {
            int randomNum = GetRandomIntInRange(0, eventList.Length);
            WorldEvent randomEvent = eventList[randomNum];

            if (eventObjects[randomNum] != 0 || (eventList[randomNum].EventEntity != null && DoesEntityExist(eventList[randomNum].EventEntity.Handle)))
                return;

            Entity ent = null;
            int netID = 0;

            if (randomEvent.EntityType == 0)
            {
                ent = await Util.EntityCreate.CreateProp(randomEvent.ModelName, randomEvent.EntityPos, true, false);
                netID = ObjToNet(ent.Handle);
            }
            else if (randomEvent.EntityType == 1)
            {
                uint hash = Util.EntityGet.GetVehicleHashFromName(randomEvent.ModelName);
                if (hash <= 0)
                    return;

                ent = await Util.EntityCreate.CreateVehicle(hash, randomEvent.EntityPos, 0);
                netID = VehToNet(ent.Handle);
            }

            if (ent == null || !NetworkGetEntityIsNetworked(ent.Handle)|| netID <= 0 )
            {
                ent.Delete();
                return;
            }

            // Brian, new problem. Vehicles spawn but props do not. ITs a network issue, idunno what to do

            //ent.Heading = randomEvent.Heading;
            SetEntityHeading(ent.Handle, randomEvent.Heading);

            //NetworkRegisterEntityAsNetworked(ent.Handle);
            //NetworkSetNetworkIdDynamic(netID, false);
            //SetNetworkIdExistsOnAllMachines(netID, true);
            //SetNetworkIdCanMigrate(netID, true);
            //NetworkSetEntityInvisibleToNetwork(ent.Handle, false);
            //NetworkSetEntityVisibleToNetwork(ent.Handle, true);
            //NetworkRequestControlOfEntity(ent.Handle);
            //SetEntityRegister(ent.Handle, true);
            //SetEntitySomething(ent.Handle, true);
            //NetworkSetEntityCanBlend(ent.Handle, true);

            SetEntityAsMissionEntity(ent.Handle, true, true);
            //FreezeEntityPosition(ent.Handle, false);
            //SetEntityCollision(ent.Handle, true, true);
            //SetEntityDynamic(ent.Handle, true);
            //SetEntityHasGravity(ent.Handle, true);
            //ActivatePhysics(ent.Handle);
            //SetEntityVelocity(ent.Handle, 1, 1, 1); //Nudge the entity to wake?
            //SetEntityVisible(ent.Handle, true, true);
            //SetNetworkIdSyncToPlayer(netID, Game.Player.Handle, true);

            AddEventObject(ent, randomNum, netID);
        }

        private static async Task DeleteInvalidWorldEvents()
        {
            for (int i = 0; i < eventObjects.Length; i++)
            {
                if (eventObjects[i] != 0) //Check only active events
                {
                    Entity ent = eventList[i].EventEntity;
                    if (ent == null || !DoesEntityExist(ent.Handle))
                        RemoveEventObject(ent, i);
                }
                else
                {
                    Entity ent = eventList[i].EventEntity;
                    if (ent != null && DoesEntityExist(ent.Handle))
                        RemoveEventObject(ent, i);
                }
            }

            await Delay(1);
        }

        private static async Task TickForAllActiveEvents()
        {
            for (int i = 0; i < eventObjects.Length; i++)
            {
                if (eventObjects[i] > 0)
                {
                    Entity ent = eventList[i].EventEntity;
                    if (ent == null || !DoesEntityExist(ent.Handle))
                        return;

                    eventList[i].Callback(ent, i);
                }
            }
            await Delay(500);
        }

        /// <summary>
        /// ///////////////////
        /// WORLD EVENTS THINK PER ENTITY
        /// ///////////////////
        /// </summary>

        private static void Event_StealTruck(Entity ent, int index)
        {
            if (ent.Health < 0)
                RemoveEventObject(ent, index);

            Ped ped = Game.PlayerPed;
            if (IsPedInVehicle(ped.Handle, ent.Handle, true) && SafeZone.IsInSafezone())
                RemoveEventObject(ent, index);
        }

        private static void Event_RPGPickup(Entity ent, int index)
        {
            int ped = Game.PlayerPed.Handle;

            Vector3 pos = ent.Position;
            Vector3 pedPos = GetEntityCoords(ped, true);

            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, pedPos.X, pedPos.Y, pedPos.Z, false);
            if (dist <= 2f && !IsPedInAnyVehicle(ped, true))
            {
                /*if (!HasPedGotWeapon(ped, (uint)WeaponHash.RPG, false))
                    GiveWeaponToPed(ped, (uint)WeaponHash.RPG, 1, false, true);
                else
                    AddAmmoToPed(ped, (uint)WeaponHash.RPG, 1);*/
                User.GiveWeapon((int)WeaponSlots.Special, WeaponHash.RPG, 1, false, false);

                RemoveEventObject(ent, index);
            }
        }

        private static void Event_SniperPickup(Entity ent, int index)
        {
            int ped = Game.PlayerPed.Handle;

            Vector3 pos = ent.Position;
            Vector3 pedPos = GetEntityCoords(ped, true);

            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, pedPos.X, pedPos.Y, pedPos.Z, false);
            if (dist <= 2f && !IsPedInAnyVehicle(ped, true))
            {
                User.GiveWeapon((int)WeaponSlots.Special, WeaponHash.HeavySniper, 12, false, false);

                RemoveEventObject(ent, index);
            }
        }

        private static void Event_StealFuel(Entity ent, int index)
        {
            if (ent.Health <= 0)
                RemoveEventObject(ent, index);

            int attach = GetEntityAttachedTo(ent.Handle);
            if (attach <= 0)
            {
                if (Game.PlayerPed == null || Game.PlayerPed.CurrentVehicle == null)
                    return;

                int ped = Game.PlayerPed.Handle;

                Vector3 pos = ent.Position;
                Vector3 pedPos = GetEntityCoords(ped, true);

                float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, pedPos.X, pedPos.Y, pedPos.Z, false);
                if (dist <= 20f)
                {
                    if (!NetworkHasControlOfEntity(ent.Handle) || !NetworkHasControlOfNetworkId(ent.NetworkId))
                    {
                        NetworkRequestControlOfEntity(ent.Handle);
                        NetworkRequestControlOfNetworkId(ent.NetworkId);
                    }
                }
                return;
            }

            Vehicle hauler = (Vehicle)ent.GetEntityAttachedTo();
            Ped driver = hauler.Driver;

            if (driver == null || driver != Game.PlayerPed)
                return;

            if (SafeZone.IsInSafezone())
                RemoveEventObject(ent, index);
        }

        private static void Event_MedsPickup(Entity ent, int index)
        {
            int attach = GetEntityAttachedTo(ent.Handle);
            if (attach > 0 && attach != Game.PlayerPed.Handle)
                return;

            if (attach <= 0 && bCanPickup)
            {
                int ped = Game.PlayerPed.Handle;

                Vector3 pos = ent.Position;
                Vector3 pedPos = GetEntityCoords(ped, true);

                float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, pedPos.X, pedPos.Y, pedPos.Z, false);
                if (dist <= 2f && Game.PlayerPed.IsAlive)
                {
                    int serverID = GetPlayerServerId(PlayerId());
                    TriggerServerEvent("brainstock:RequestAttachment", index, serverID, (int)Bone.SKEL_Spine1, 0.16f, -0.25f, 0.05f, 25, 15, 0, 0, 0, true);

                    SetCanPickup(false);
                }
            }
            else if (attach > 0 && attach == Game.PlayerPed.Handle)
            {
                if (Game.PlayerPed.Health <= 0 || !Game.PlayerPed.IsAlive)
                {
                    TriggerServerEvent("brainstock:RequestDetatchment", index);
                    SetCanPickup(true);
                }

                if (SafeZone.IsInSafezone() && !bCanPickup)
                {
                    RemoveEventObject(ent, index);
                    SetCanPickup(true);
                }
            }
        }

        /// <summary>
        /// ///////////////////
        /// WORLD EVENTS THINK PER ENTITY END
        /// ///////////////////
        /// </summary>

        //GETS & SETS
        private static void AddEventObject(Entity ent, int index, int netID)
        {
            eventObjects[index] = netID;
            eventList[index].EventEntity = ent;

            AttachBlip(ent, index);
            TriggerServerEvent("brainstock:AddServerEvent", netID, index);

            Debug.WriteLine($"AddedEVENT: {netID}, {index}");
        }

        private static void RemoveEventObject(Entity ent, int index)
        {
            TriggerServerEvent("brainstock:RemoveServerEvent", index);

            if (!WorldEventController.IsPlayerHost())
                return;

            ent.Detach();
            ent.Delete();

            Debug.WriteLine($"RemoveEVENT: {eventObjects[index]}, {index}");

            eventList[index].EventEntity = null;
            eventObjects[index] = 0;
        }

        private static void AttachBlip(Entity ent, int index)
        {
            Blip blip = ent.AttachBlip();
            blip.Sprite = eventList[index].Blip;
            blip.Scale = 0.75f;
            blip.Name = eventList[index].DisplayName;
            blip.Color = BlipColor.TrevorOrange;
        }

        private static int NumberOfActiveEvents()
        {
            int count = 0;
            for (int i = 0; i < eventObjects.Length; i++)
            {
                if (eventObjects[i] != 0)
                    count++;
            }
            return count;
        }

        public static async void AddWorldEvent(int netID, int index) // 4/19/2019; I changed this to a task if it breaks shit
        {
            if (WorldEventController.IsPlayerHost())
                return;

            while (User.GetTeam() == 0)
                await Delay(1);

            Debug.WriteLine("Searching Event...");

            int time = GetGameTimer();
            Debug.WriteLine($"time = {time}");

            Entity ent = SearchEventByNetID(netID);
            while (ent == null && GetGameTimer() < (time+20000))
            {
                ent = SearchEventByNetID(netID);
                await Delay(10);
            }

            if (ent == null)
            {
                Debug.WriteLine($"Event search timed out {GetGameTimer()}");
                return;
            }

             Debug.WriteLine("Added");

            eventObjects[index] = netID;
            eventList[index].EventEntity = ent;

            NetworkRequestControlOfEntity(ent.Handle);
            NetworkRequestControlOfNetworkId(netID);
            SetNetworkIdSyncToPlayer(netID, Game.Player.Handle, true);

            if (ent.AttachedBlip == null && DoesEntityExist(ent.Handle))
                AttachBlip(ent, index);
        }

        public static void RemoveWorldEvent(int index)
        {
            if (WorldEventController.IsPlayerHost())
                return;

            Entity ent = eventList[index].EventEntity;
            if (ent != null && DoesEntityExist(ent.Handle) && NetworkHasControlOfEntity(ent.Handle))
                ent.Delete();

            eventObjects[index] = 0;
            eventList[index].EventEntity = null;
        }

        public static Entity SearchEventByNetID(int netID)
        {
            foreach (Vehicle veh in World.GetAllVehicles())
                if (NetworkGetEntityIsNetworked(veh.Handle) && VehToNet(veh.Handle) == netID)
                    return veh;

            foreach (Prop prop in World.GetAllProps())
                if (NetworkGetEntityIsNetworked(prop.Handle) && ObjToNet(prop.Handle) == netID)
                    return prop;

            return null;
        }

        public static void AttachObject( int index, int serverID, int boneID, float x, float y, float z, float p, float yaw, float r, int vert, bool fixedRot = true)
        {
            Entity ent = eventList[index].EventEntity;
            if (ent == null || !NetworkHasControlOfEntity(ent.Handle))
                return;

            int ped = GetPlayerPed(GetPlayerFromServerId(serverID));
            int bone = GetPedBoneIndex(ped, boneID);

            AttachEntityToEntity(ent.Handle, ped, bone, x, y, z, p, yaw, r, false, false, false, false, vert, true);
        }

        public static void DetatchObject(int index)
        {
            Entity ent = eventList[index].EventEntity;
            if (ent == null)
                return;

            ent.Detach();
        }

        public static void SetCanPickup(bool pickup)
        {
            bCanPickup = pickup;
        }

        public static void TeleportToEvent(int index)
        {
            if (index > eventList.Length)
                return;

            Entity ent = eventList[index].EventEntity;
            if (ent == null)
                return;

            Game.PlayerPed.Position = ent.Position;
        }

        public static void SwitchHost()
        {
            for (int i = 0; i < eventObjects.Length; i++)
            {
                Entity ent = eventList[i].EventEntity;
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
