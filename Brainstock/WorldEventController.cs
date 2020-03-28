using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Brainstock
{
    //This class will hanle what player controls the world event spawn
    //Base if off ServerIDs, player with the lowest ServerID will be controller
    class WorldEventController : BaseScript
    {
        private static bool IS_CONTROL;

        public WorldEventController()
        {
            IS_CONTROL = false;
            //Tick += OnTick;
        }

        public static bool IsPlayerHost()
        {
            return IS_CONTROL;
        }

        public static void SetHost()
        {
            if (IS_CONTROL)
                return;

            IS_CONTROL = true;
            Debug.WriteLine("You are now the WorldEvents Host");

            WorldEvents.SwitchHost();
            VehicleSpawner.SwitchHost();
        }

        public static async void CleanupMap()
        {
            while (User.GetTeam() == 0)
                await Delay(1);

            foreach (Vehicle veh in World.GetAllVehicles())
                if (!Util.EntityDecor.GetDecorBool(veh, "_BRAINSTOCK"))
                    veh.Delete();

            foreach (Ped ped in World.GetAllPeds())
                if (!ped.IsPlayer && !ZombieSpawner.IsZombie(ped))
                    ped.Delete();

            foreach (Prop prop in World.GetAllProps())
                if (!Util.EntityDecor.GetDecorBool(prop, "_BRAINSTOCK"))
                    prop.Delete();

            Debug.WriteLine("Cleanup Complete");
        }

        public static async void HardCleanup()
        {
            while (User.GetTeam() == 0)
                await Delay(1);

            Debug.WriteLine("Hard Cleanup");

            foreach (Vehicle veh in World.GetAllVehicles())
            {
                while (NetworkGetEntityIsNetworked(veh.Handle) && !NetworkHasControlOfEntity(veh.Handle))
                {
                    await Delay(1);
                }
                veh.Delete();
            }

            foreach (Ped ped in World.GetAllPeds())
            {
                while (NetworkGetEntityIsNetworked(ped.Handle) && !NetworkHasControlOfEntity(ped.Handle))
                {
                    await Delay(1);
                }
                ped.Delete();
            }

            foreach (Prop prop in World.GetAllProps())
            {
                while (NetworkGetEntityIsNetworked(prop.Handle) && !NetworkHasControlOfEntity(prop.Handle))
                {
                    await Delay(1);
                }
                prop.Delete();
            }

            SafeZone.SpawnSafezoneObjects();
        }
    }
}
