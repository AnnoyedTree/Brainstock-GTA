using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Brainstock.Util;

namespace Brainstock
{
    public struct SafezoneObjects
    {
        public Vector3 SpawnPos { get; private set; }
        public float EntHeading { get; private set; }
        public string modelName { get; private set; }
        public bool SpawnDynamic { get; private set; }

        public SafezoneObjects(string model, float x, float y, float z, float yaw, bool dynamic = false)
        {
            SpawnPos = new Vector3(x,y,z);
            modelName = model;
            EntHeading = yaw;
            SpawnDynamic = dynamic;
        }
    }

    class SafeZone : BaseScript
    {

        private static bool initalizedBlips;
        private static bool inSafezone;

        //Safezone list array setup: (posX, posY, posZ, Radius)

        public static int[] militarySafezone = { -1827, 2974, 33, 65 }; //Military Safezone
        public static int[] outcastSafezone = { 1660, 2606, 46, 150 }; //Outcast safezone

        //Objects = { "hei_prop_hei_ammo_pile_02, "hei_prop_hei_ammo_pile", "prop_mb_hesco_06", "prop_mb_sandblock_01", "prop_mb_sandblock_02", "prop_mb_sandblock_03_cr", prop_mb_sandblock_04, prop_mb_sandblock_05_cr, prop_mil_crate_01, prop_mil_crate_02, prop_conc_sacks_02a

        private static SafezoneObjects[] militaryObjects = { new SafezoneObjects("prop_mb_sandblock_03_cr", -1847.48f, 3015.06f, 31.81f, 330),
                                                             new SafezoneObjects("prop_mb_hesco_06", -1852.14f, 3013.45f, 31.81f, 330),
                                                             new SafezoneObjects("prop_mb_sandblock_03_cr", -1855.8f, 3009.75f, 31.81f, 330),
                                                             new SafezoneObjects("prop_mb_sandblock_03_cr", -1857.93f, 3006.01f, 31.81f, 330),
                                                             new SafezoneObjects("prop_mb_sandblock_04", -1859.49f, 3001.98f, 31.81f, 240),
                                                             new SafezoneObjects("prop_mb_sandblock_04", -1859.49f, 3001.98f, 33.2f, 240),
                                                             //Other side
                                                             new SafezoneObjects("prop_mb_sandblock_04", -1868.59f, 2986.26f, 31.81f, 240),
                                                             new SafezoneObjects("prop_mb_sandblock_04", -1868.59f, 2986.26f, 33.2f, 240),
                                                             new SafezoneObjects("prop_mb_sandblock_03_cr", -1871.34f, 2982.96f, 31.81f, 330),
                                                             new SafezoneObjects("prop_mb_sandblock_03_cr", -1873.47f, 2979.22f, 31.81f, 330),
                                                             new SafezoneObjects("prop_mb_hesco_06", -1874.72f, 2974.17f, 31.81f, 330),
        };

        private static SafezoneObjects[] outcastObjects = {
        };

        public SafeZone()
        {
            EntityDecor.RegisterDecorProperty("_SAFEZONE", EntityDecor.DecorType.Bool);

            Tick += OnTick;
        }

        private static void HandleMilitaryTick(Ped ped)
        {
            Vector3 pos = ped.Position;
            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, militarySafezone[0], militarySafezone[1], militarySafezone[2], false);

            if (User.GetTeam() == 2)
            {
                if (dist < (militarySafezone[3]*1.25))
                    ped.Kill();

                return;
            }


            //In safe zone
            if ( dist < militarySafezone[3] )
            {
                EnterSafezone(ped);
            }
            else if (dist > militarySafezone[3] && IsInSafezone())
            {
                LeaveSafezone(ped);
            }
        }

        private static void HandleOutcastTick(Ped ped)
        {
            Vector3 pos = ped.Position;
            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, outcastSafezone[0], outcastSafezone[1], outcastSafezone[2], false);

            if (User.GetTeam() == 1)
            {
                if (dist < (outcastSafezone[3]*1.25))
                    ped.Kill();

                return;
            }

            //In safe zone
            if (dist < outcastSafezone[3])
            {
                EnterSafezone(ped);
            }
            else if (dist > outcastSafezone[3] && IsInSafezone())
            {
                LeaveSafezone(ped);
            }
        }

        private async Task OnTick()
        {
            while (!NetworkIsSessionStarted())
                await Delay(0);

            //TODO: Add player blips and clean this the fuck up
            if (!initalizedBlips)
                InitBlips();

            //if (!spawnObjectsLoaded && WorldEventController.IsPlayerHost())
                //SpawnSafezoneObjects();

            Ped ped = Game.PlayerPed;

            HandleMilitaryTick(ped);
            HandleOutcastTick(ped);

            //VehicleSpawner.OnTick();

            await Delay(1);
        }

        private static void EnterSafezone(Ped ped)
        {
            if (!IsInSafezone())
            {
                SetPlayerInvincible(PlayerId(), true);
                //Screen.ShowNotification("You entered the Safe Zone");

                SetInSafezone(true);
                ZombieSpawner.RemoveAllZombies();
            }

            /*uint wephash = (uint)WeaponHash.Unarmed;

            if (GetCurrentPedWeaponEntityIndex(ped.GetHashCode()) != wephash)
                SetCurrentPedWeapon(ped.GetHashCode(), wephash, true);*/
        }

        private static void LeaveSafezone(Ped ped)
        {
            SetPlayerInvincible(PlayerId(), false);

            //Screen.ShowNotification("You left the Safe Zone");
            SetInSafezone(false);
        }

        //Sets
        private static void SetInSafezone(bool safe)
        {
            inSafezone = safe;
        }

        public static bool IsInSafezone()
        {
            return inSafezone;
        }

        private static void InitBlips()
        {
            Vector3 pos = new Vector3(militarySafezone[0], militarySafezone[1], militarySafezone[2]);

            Blip blip = World.CreateBlip(pos);
            blip.Sprite = BlipSprite.PoliceStation;
            blip.Scale = 1.5f;
            blip.Name = "Security Forces";

            pos = new Vector3(outcastSafezone[0], outcastSafezone[1], outcastSafezone[2]);

            blip = World.CreateBlip(pos);
            blip.Sprite = BlipSprite.GTAOSurvival;
            blip.Scale = 1.5f;
            blip.Color = BlipColor.Red;
            blip.Name = "Outcasts";

            initalizedBlips = true;
        }

        public static async void SpawnSafezoneObjects()
        {
            foreach (SafezoneObjects objects in militaryObjects)
            {
                Prop prop = await Util.EntityCreate.CreateProp(objects.modelName, objects.SpawnPos, objects.SpawnDynamic, false);
                prop.Heading = objects.EntHeading;
            }

            foreach (SafezoneObjects objects in outcastObjects)
            {
                Prop prop = await Util.EntityCreate.CreateProp(objects.modelName, objects.SpawnPos, objects.SpawnDynamic, false);
                prop.Heading = objects.EntHeading;
            }
        }

        public static bool PositionInSafeZone(Vector3 pos)
        {
            //Check military safezone
            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, militarySafezone[0], militarySafezone[1], militarySafezone[2], false);
            if (dist <= militarySafezone[3])
                return true;

            //Check outcast safezone
            dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, outcastSafezone[0], outcastSafezone[1], outcastSafezone[2], false);
            if (dist <= outcastSafezone[3])
                return true;

            return false;
        }

        public static void GoToSafezone()
        {
            if (User.GetTeam() == 1)
            {
                Game.PlayerPed.Position = new Vector3(militarySafezone[0], militarySafezone[1], militarySafezone[2]);
            }
            else if (User.GetTeam() == 2)
            {
                Game.PlayerPed.Position = new Vector3(outcastSafezone[0], outcastSafezone[1], outcastSafezone[2]);
            }
        }
    }
}
