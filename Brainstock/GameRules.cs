using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace Brainstock
{
    public class GameRules : BaseScript
    {
        public static RelationshipGroup relationMilitary, relationOutcast, zombieRelation;

        public static int MAX_ZOMBIES_PER_CLIENT = 4;
        public const int MAX_WORLD_EVENTS = 5;

        private static int TickDelay = 0;

        public enum Teams
        {
            TEAM_UNASSIGNED = 0,
            TEAM_MILITARY,
            TEAM_OUTCAST
        }

        public GameRules()
        {
            EventHandlers["playerSpawned"] += new Action(User.HandleSpawn);
            EventHandlers["onClientMapStart"] += new Action(User.OnClientStart);

            EventHandlers["brainstock:ServerSetup"] += new Action(WorldEventController.CleanupMap);
            EventHandlers["brainstock:SpawnedVehicle"] += new Action<int, int>(VehicleSpawner.AddServerVehicle);
            EventHandlers["brainstock:RemovedVehicle"] += new Action<int>(VehicleSpawner.RemoveServerVehicle);
            EventHandlers["brainstock:LoadSpawnedVehicles"] += new Action<int, int>(VehicleSpawner.LoadVehicleOnJoin);

            EventHandlers["brainstock:SetEventHost"] += new Action(WorldEventController.SetHost);
            EventHandlers["brainstock:AddEvent"] += new Action<int, int>(WorldEvents.AddWorldEvent);
            EventHandlers["brainstock:RemoveEvent"] += new Action<int>(WorldEvents.RemoveWorldEvent);
            EventHandlers["brainstock:AttachObject"] += new Action<int, int, int, float, float, float, float, float, float, int, bool>(WorldEvents.AttachObject);
            EventHandlers["brainstock:DetatchObject"] += new Action<int>(WorldEvents.DetatchObject);

            EventHandlers["brainstock:HardCleanup"] += new Action(WorldEventController.HardCleanup);

            relationMilitary = World.AddRelationshipGroup("military");
            relationOutcast = World.AddRelationshipGroup("outcast");
            zombieRelation = World.AddRelationshipGroup("zombie");

            Util.EntityDecor.RegisterDecorProperty("_BRAINSTOCK", Util.EntityDecor.DecorType.Bool);

            Tick += OnTick;

            //IPLS?
            RequestAllIPLs();
        }

        private async Task OnTick()
        {
            //Set Weather
            SetWeatherTypeNowPersist("FOGGY");
            NetworkOverrideClockTime(0, 0, 0);

            World.WeatherTransition = 0;
            World.Blackout = true;

            //Stop all vehicles from spawning
            SetVehicleDensityMultiplierThisFrame(0);
            SetRandomVehicleDensityMultiplierThisFrame(0);
            SetParkedVehicleDensityMultiplierThisFrame(0);

            //Stop all peds from spawning
            SetPedDensityMultiplierThisFrame(0);
            SetScenarioPedDensityMultiplierThisFrame(0, 0);

            VehicleCleanup();
            PedCleanup();

            if (!IsAudioSceneActive("CHARACTER_CHANGE_IN_SKY_SCENE"))
                StartAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");

            //Infinite sprint
            int playerID = PlayerId();
            if ( GetPlayerSprintStaminaRemaining( playerID ) < 10 )
                ResetPlayerStamina(playerID);

            Ped ped = Game.PlayerPed;
            if (ped.Health <= 0 && !ped.IsAlive)
                User.HandleDeath();

            if (GetGameTimer() > TickDelay)
                UpdateFriendlyBlips();

            HideHudAndRadarThisFrame();
            BlockWeaponWheelThisFrame();

            Input.SwitchWeapons();

            await Task.FromResult(0);
        }

        private void UpdateFriendlyBlips()
        {
            foreach (Ped ped in World.GetAllPeds())
            {
                if (ped.IsPlayer && ped != Game.PlayerPed)
                {
                    if (ped.RelationshipGroup != Game.PlayerPed.RelationshipGroup && ped.AttachedBlip != null)
                        ped.AttachedBlip.Delete();
                    else if (ped.RelationshipGroup == Game.PlayerPed.RelationshipGroup && ped.AttachedBlip == null)
                    {
                        Blip blip = ped.AttachBlip();
                        blip.Scale = 0.75f;
                        blip.Sprite = BlipSprite.Standard;
                        blip.Color = BlipColor.Blue;
                        blip.Name = "Friend";
                    }
                }
            }
            TickDelay = (GetGameTimer()+10000);
        }

        private void VehicleCleanup()
        {
            foreach (Vehicle veh in World.GetAllVehicles())
            {
                if (!Util.EntityDecor.HasDecor(veh, "_BRAINSTOCK") || !Util.EntityDecor.GetDecorBool(veh, "_BRAINSTOCK"))
                    veh.Delete();
            }
        }

        private void PedCleanup()
        {
            foreach (Ped ped in World.GetAllPeds())
            {
                if (!ped.IsPlayer && !ZombieSpawner.IsZombie(ped))
                    ped.Delete();
            }
        }

        private static void RequestAllIPLs()
        {
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);

            RequestIpl("rc12b_hospitalinterior");
            RequestIpl("rc12b_hospitalinterior_lod");

            //Tunnels
            RequestIpl("v_tunnel_hole");

            //Life Invader
            RequestIpl("linvader");
        }
    }
}
