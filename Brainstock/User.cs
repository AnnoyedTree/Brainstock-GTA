using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Brainstock.Util;

namespace Brainstock
{
    public enum WeaponSlots
    {
        Primary = 0,
        Secondary,
        Special,
        Melee,
        Unarmed
    }

    class User : BaseScript
    {
        private static int playerTeam = 0;
        private static int playerState; //0= DEAD, 1=ALIVE, 2=RESPAWNING
        public static WeaponHash[] playerWeapons { get; private set; }

        private static string[] militaryModels = { "csb_ramp_marine",
                                                   "s_m_y_blackops_01",
                                                   "s_m_y_blackops_02",
                                                   "s_m_y_armymech_01",
                                                   "s_m_y_marine_01",
                                                   "s_m_y_marine_03",
                                                   "s_m_m_pilot_02",
                                                   "s_m_y_pilot_01",
                                                   "s_m_m_marine_01",
                                                   "s_m_m_marine_02",
                                                   "s_m_m_prisguard_01",
                                                   "csb_mweather",
                                                   "s_m_m_armoured_01",
                                                   "s_m_m_armoured_02",
                                                   "s_m_y_swat_01",
                                                   "s_m_y_ammucity_01",
        };

        private static string[] outcastModels = { //"s_m_y_garbage",
                                                  "s_m_y_prisoner_01",
                                                  "u_m_y_prisoner_01",
                                                  "s_m_m_trucker_01",
                                                  "g_m_y_lost_01",
                                                  "g_m_y_lost_02",
                                                  "g_m_y_lost_03",
                                                  "cs_russiandrunk",
                                                  "csb_ramp_hic",
                                                  "a_m_o_acult_02",
                                                  "cs_terry",
                                                  "cs_clay",
                                                  "a_m_o_beach_01",
                                                  "a_m_y_juggalo_01",

        };

        public User()
        {
            //4 total weapons for player
            //INDEXS [0] = Primary, [1] = Secondary, [2] = Special,  [3] = Melee, [4] = Unarmed
            playerWeapons = new WeaponHash[5];
            playerWeapons[(int)WeaponSlots.Unarmed] = WeaponHash.Unarmed;
        }

        public static void GiveWeapon(int index, WeaponHash hash, int ammo, bool bHidden, bool equipNow)
        {
            //do stuff
            int ped = GetPlayerPed(PlayerId());

            if (index > playerWeapons.Length)
                return;

            if (playerWeapons[index] != 0)
                RemoveWeaponFromPed(ped, (uint)playerWeapons[index]);
                //SetPedDropsInventoryWeapon(ped, (uint)playerWeapons[index], 2, 2, 0, GetAmmoInPedWeapon(ped, (uint)playerWeapons[index]));

            playerWeapons[index] = hash;
            GiveWeaponToPed(ped, (uint)hash, ammo, bHidden, equipNow);
        }

        /*public static WeaponHash[] GetWeapons()
        {
            return playerWeapons;
        }*/

        public static void SetTeam(int team)
        {
            playerTeam = team;
        }

        public static void ChangeTeam(int team)
        {
            SetTeam(team);
            HandleSpawn();
        }

        public static int GetTeam()
        {
            return playerTeam;
        }

        public static void SetPlayerState(int state)
        {
            playerState = state;
        }

        public static int GetPlayerState()
        {
            return playerState;
        }

        public static async void OnClientStart()
        {
            ShutdownLoadingScreen();
            DoScreenFadeIn(500);

            Initialize(PlayerId());

            while (IsScreenFadingIn())
                await Delay(1);
        }

        public static async void HandleSpawn()
        {
            if (GetPlayerState() == 2)
                return;

            SetPlayerState(2);

            int playerID = PlayerId();
            int ped = GetPlayerPed(playerID);

            //Set DEBUG Spawn Position
            if (GetTeam() == 1)
            {
                int randomNum = GetRandomIntInRange(0, militaryModels.Length);
                string model = militaryModels[randomNum];
                await EntityCreate.SetSkin(model, playerID);

                //Debug.WriteLine($"Model={model}");
                ped = GetPlayerPed(playerID);
                Game.PlayerPed.RelationshipGroup = GameRules.relationMilitary;
                Game.PlayerPed.Heading = 60;
                SetEntityCoordsNoOffset(ped.GetHashCode(), -1820, 2971, 33, false, false, false); //Military Base Air Hangar
                //SetEntityCoordsNoOffset(ped.GetHashCode(), 1871.151f, 2601.618f, 44.4f, false, false, false); //DEBUG TEST SPAWN

                //GiveWeaponToPed(ped, (uint)WeaponHash.Knife, 0, false, false);
                GiveWeapon((int)WeaponSlots.Melee, WeaponHash.Knife, 0, false, false);
                GiveWeapon((int)WeaponSlots.Primary, WeaponHash.CarbineRifle, 300, false, false);
                GiveWeaponComponentToPed(ped, (uint)WeaponHash.CarbineRifleMk2, (uint)WeaponComponentHash.CarbineRifleClip01);

                SetPedRelationshipGroupHash(ped, (uint)GetHashKey("military"));
            }
            else if (GetTeam() == 2)
            {
                int randomNum = GetRandomIntInRange(0, outcastModels.Length);
                string model = outcastModels[randomNum];
                await EntityCreate.SetSkin(model, playerID);

                ped = GetPlayerPed(playerID);
                Game.PlayerPed.RelationshipGroup = GameRules.relationOutcast;
                Game.PlayerPed.Heading = 270;
                SetEntityCoordsNoOffset(ped.GetHashCode(), 1710, 2605, 46, false, false, false); //Prison
                //SetEntityCoordsNoOffset(ped.GetHashCode(), 1860.814f, 2602.814f, 45.7f, false, false, false); //DEBUG TEST SPAWN

                //GiveWeaponToPed(ped, (uint)WeaponHash.BattleAxe, 0, false, false);
                GiveWeapon((int)WeaponSlots.Melee, WeaponHash.BattleAxe, 0, false, false);
                GiveWeapon((int)WeaponSlots.Primary, WeaponHash.AssaultRifle, 300, false, false);
                GiveWeaponComponentToPed(ped, (uint)WeaponHash.AssaultRifle, (uint)WeaponComponentHash.AssaultRifleClip01);

                SetPedRelationshipGroupHash(ped, (uint)GetHashKey("outcast"));
            }

            RespawnPlayer(playerID, ped);
            NetworkSetEntityVisibleToNetwork(ped, true);

            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("military"), (uint)GetHashKey("outcast"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("outcast"), (uint)GetHashKey("military"));
        }

        public static void HandleDeath()
        {
            DoScreenFadeOut(7000);

            if (!IsScreenFadingOut()) //Was it really that fucking easy?
                HandleSpawn();
        }

        private static void Initialize( int playerID )
        {
            //DEBUG SET SPAWN POSITION
            int ped = GetPlayerPed(playerID);
            SetEntityCoordsNoOffset(ped, 0, 0, 1000, false, false, false);

            //Initalizers
            RemoveAllPedWeapons(ped, true);
            SetMaxWantedLevel(0);

            //Set Player Team
            int team = (int)GameRules.Teams.TEAM_UNASSIGNED;
            SetTeam(team);

            SetPlayerInvincible(playerID, true);
            SetPlayerInvisibleLocally(playerID, true);
            SetEntityVisible(ped, false, false);
            SetEntityCollision(ped, false, false);
            FreezeEntityPosition(ped, true);

            StartAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");

            TriggerServerEvent("brainstock:PlayerInit");

            int random = GetRandomIntInRange(1, 2);
            ChangeTeam(random);
        }

        private static void RespawnPlayer( int playerID, int ped )
        {
            Vector3 pos = GetEntityCoords( ped, true );

            SetPlayerInvincible(playerID, false);
            SetPlayerInvisibleLocally(playerID, false);
            SetEntityVisible(ped, true, true);
            SetEntityCollision(ped, true, true);

            SetPlayerInvisibleLocally(playerID, false);
            SetPedDefaultComponentVariation(ped);
            FreezeEntityPosition(ped, false);
            SetCanAttackFriendly(ped, false, false);

            SetEntityHealth(ped, 100);
            SetPlayerMaxArmour(playerID, 100);
            SetPedArmour(ped, 100);

            //GiveWeaponToPed(ped, (uint)WeaponHash.PistolMk2, 60, false, false);
            //GiveWeaponToPed(ped, (uint)WeaponHash.Flare, 1, false, false);
            //GiveWeaponComponentToPed(ped, (uint)WeaponHash.PistolMk2, (uint)WeaponComponentHash.PistolMk2Flash);
            GiveWeapon((int)WeaponSlots.Special, WeaponHash.Flare, 2, false, false);
            GiveWeapon((int)WeaponSlots.Secondary, WeaponHash.PistolMk2, 60, false, false);
            GiveWeaponComponentToPed(ped, (uint)WeaponHash.PistolMk2, (uint)WeaponComponentHash.PistolMk2Flash);

            NetworkResurrectLocalPlayer(pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading, true, true);
            //Debug.WriteLine($"RespawnPlayer: PlayerID={playerID} ServerID={GetPlayerServerId(playerID)} Ped={ped} Team={GetTeam()}");

            SetPlayerState(1);
            DoScreenFadeIn(500);

            //WorldEvents.SetCanPickup(true);
        }
    }

}
