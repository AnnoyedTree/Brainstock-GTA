using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Brainstock
{
    //TODO: ADD SERVER NETWORKING FOR ZOMBIE CAP
    class ZombieSpawner : BaseScript
    {
        private const int SPAWN_TICK_DELAY = 100;

        //Save zombos to a list for cleanup
        public static List<Ped> zombieList { get; set; }

        public ZombieSpawner()
        {
            zombieList = new List<Ped>();

            Tick += OnTick;
        }

        private async Task OnTick()
        {
            int ped = API.GetPlayerPed(API.PlayerId());

            if (User.GetTeam() == 0 || !API.NetworkIsSessionActive() || SafeZone.IsInSafezone())
                return;

            if (zombieList.Count < GameRules.MAX_ZOMBIES_PER_CLIENT)
                SpawnZombie();
            else
                foreach ( Ped zombie in zombieList.ToArray() )
                {
                    if (zombie.IsDead || zombie.Health <= 0)
                        DeleteZombie(zombie, false);
                }

            ZombieThink();

            await Delay(SPAWN_TICK_DELAY);
        }

        private async void SpawnZombie()
        {
            int ped = API.GetPlayerPed(API.PlayerId());

            Vector3 playerPos = API.GetEntityCoords(ped, true);
            Vector3 pos = GetRandPosFromPlayer( ped, -80, 80 ); //min, max values

            //Bad Spawn Position
            if (pos.Z > (playerPos.Z + 2.5f) || pos.Z < (playerPos.Z - 2.5f) || API.GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, playerPos.X, playerPos.Y, playerPos.Z, false) < 45 || SafeZone.PositionInSafeZone(pos) )
                return;

            Ped zombie = await Util.EntityCreate.CreatePed(PedHash.Zombie01, 27, pos, 0);
            int handle = zombie.Handle;

            API.SetPedCombatRange(handle, 1);
            API.SetPedHearingRange(handle, 100);

            API.SetPedCombatAttributes(handle, 46, true);
            API.SetPedCombatAttributes(handle, 5, true);
            API.SetPedCombatAttributes(handle, 1, false);
            API.SetPedCombatAttributes(handle, 0, false);
            API.SetPedCombatAbility(handle, 0);

            API.SetAiMeleeWeaponDamageModifier(0.3f);
            API.SetPedRagdollBlockingFlags(handle, 4);
            API.SetPedCanPlayAmbientAnims(handle, false);

            zombie.MaxHealth = 250;
            zombie.Health = 250;
            zombie.Armor = API.GetRandomIntInRange(0, 25);
            zombie.DropsWeaponsOnDeath = false;

            zombie.RelationshipGroup = GameRules.zombieRelation;
            zombie.RelationshipGroup.SetRelationshipBetweenGroups("military", Relationship.Hate, true);
            zombie.RelationshipGroup.SetRelationshipBetweenGroups("outcast", Relationship.Hate, true);

            zombie.Task.WanderAround();
            zombie.Voice = "ALIENS";
            zombie.IsPainAudioEnabled = false;

            API.GiveWeaponToPed(zombie.GetHashCode(), (uint)WeaponHash.KnuckleDuster, 0, false, true);
            zombieList.Add(zombie);

            API.RequestAnimSet("move_m@drunk@verydrunk");
            API.SetPedMovementClipset(zombie.GetHashCode(), "move_m@drunk@verydrunk", 1f);

            //Debug.WriteLine($"Spawed Zombie #{zombieList.Count}");
            await Delay(1);
        }

        private async void ZombieThink()
        {
            foreach (Ped zombie in zombieList.ToArray())
            {
                /*if (zombie == null || !API.DoesEntityExist(zombie.Handle) || zombie.Health <= 0 || !zombie.IsAlive )
                {
                    DeleteZombie(zombie);
                    return;
                }*/

                API.SetPedMovementClipset(zombie.Handle, "move_m@drunk@verydrunk", 1f);

                Vector3 pos = API.GetEntityCoords(zombie.Handle, true);

                int ped = API.GetPlayerPed(API.GetNearestPlayerToEntity(zombie.Handle));
                Vector3 pedPos = API.GetEntityCoords(ped, true);
                float dist = API.GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, pedPos.X, pedPos.Y, pedPos.Z, false);

                if (dist >= 200.0f)
                    DeleteZombie(zombie, true);
            }
            await Delay(10000);
        }

        private static Vector3 GetRandPosFromPlayer( int id, float min, float max )
        {
            float rand_x = API.GetRandomFloatInRange(min, max);
            float rand_y = API.GetRandomFloatInRange(min, max);

            Vector3 pos = (API.GetEntityCoords(id, true) + new Vector3(rand_x, rand_y, 0));
            pos.Z = World.GetGroundHeight(pos);

            return pos;
        }

        private static void DeleteZombie(Ped ped, bool delete)
        {
            zombieList.Remove(ped);
            //ped.MarkAsNoLongerNeeded();

            if (delete)
                ped.Delete();
            else
                ped.MarkAsNoLongerNeeded();
        }

        public static bool IsZombie(Ped ped)
        {
            if (ped.RelationshipGroup.Equals("zombie") || Util.EntityDecor.HasDecor(ped, "_BRAINSTOCK") || Util.EntityDecor.GetDecorBool(ped, "_BRAINSTOCK"))
                return true;

            return false;
        }

        public static void RemoveAllZombies()
        {
            foreach (Ped zombie in zombieList.ToArray())
                 DeleteZombie(zombie, true);
        }
    }
}
