using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Brainstock
{
    class Commands : BaseScript
    {
        public Commands()
        {
            // /giveallweapons
            API.RegisterCommand("giveallweapons", new Action<int, List<object>, string>((src, args, raw) =>
            {
                GiveAllWeapons();
            }), false);

            // /getpos
            API.RegisterCommand("getpos", new Action<int, List<object>, string>((src, args, raw) =>
            {
                Debug.WriteLine($"{GetPlayerPosition()}");
            }), false);

            API.RegisterCommand("go", new Action<int, List<object>, string>((src, args, raw) =>
            {
                int index = 0;
                Int32.TryParse(args[0].ToString(), out index);

                WorldEvents.TeleportToEvent(index);
            }), false);

            // /setteam
            API.RegisterCommand("j1", new Action<int, List<object>, string>((src, args, raw) =>
            {
                SetTeamMilitary();
            }), false);

            API.RegisterCommand("j2", new Action<int, List<object>, string>((src, args, raw) =>
            {
                SetTeamOutcast();
            }), false);

            // Spawwn humvee
            API.RegisterCommand("veh", new Action<int, List<object>, string>((src, args, raw) =>
            {
                string str = (string)args[0];

                int ped = API.GetPlayerPed(API.PlayerId());
                SpawnVehicle(str, ped);
            }), false);

            API.RegisterCommand("ar", new Action<int, List<object>, string>((src, args, raw) =>
            {
                API.SetPedArmour(API.GetPlayerPed(API.PlayerId()), 100);
            }), false);

            API.RegisterCommand("cleanup", new Action<int, List<object>, string>((src, args, raw) =>
            {
                CleanupCommand();
            }), false);

            API.RegisterCommand("szo", new Action<int, List<object>, string>((src, args, raw) =>
            {
                ReloadSafezoneObjects();
            }), false);

            API.RegisterCommand("kill", new Action<int, List<object>, string>((src, args, raw) =>
            {
                Game.PlayerPed.Kill();
            }), false);

            /*API.RegisterCommand("ev", new Action<int, List<object>, string>((src, args, raw) =>
            {
                int netid = 0;

                Int32.TryParse(args[0].ToString(), out netid);

                int index = 0;

                Int32.TryParse(args[1].ToString(), out index);

                WorldEvents.AddWorldEvent(netid, index);
            }), false);*/

            API.RegisterCommand("home", new Action<int, List<object>, string>((src, args, raw) =>
            {
                SafeZone.GoToSafezone();
            }), false);
        }

        private static void SetTeamMilitary()
        {
            User.SetTeam(1);
            User.HandleSpawn();
        }

        private static void SetTeamOutcast()
        {
            User.SetTeam(2);
            User.HandleSpawn();
        }

        private static async void SpawnVehicle(string str, int ped)
        {
            Vector3 pos = API.GetEntityCoords(ped, true);

            uint hash = Util.EntityGet.GetVehicleHashFromName(str);

            if (hash == 0)
                return;

            Vehicle veh = await Util.EntityCreate.CreateVehicle(hash, pos, Game.PlayerPed.Heading);
            Util.EntityDecor.SetDecorBool(veh, "_BRAINSTOCK", true);

            if (API.IsPedInAnyVehicle(ped, true))
            {
                Game.PlayerPed.CurrentVehicle.MarkAsNoLongerNeeded();
                Game.PlayerPed.CurrentVehicle.Delete();
            }

            API.SetPedIntoVehicle(ped, veh.GetHashCode(), -1);
        }

        public static void GiveAllWeapons()
        {
            try
            {
                foreach (WeaponHash weapon in Enum.GetValues(typeof(WeaponHash)))
                {
                    if (!Game.PlayerPed.Weapons.HasWeapon(weapon))
                    {
                        int ped = API.GetPlayerPed(API.PlayerId());
                        API.GiveWeaponToPed(ped, (uint)weapon, 500, false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        private string GetPlayerPosition()
        {
            Vector3 id = Game.PlayerPed.Position;
            return $"{id.X}, {id.Y}, {id.Z}, Heading={Game.PlayerPed.Heading}";
        }

        public static void CleanupCommand()
        {
            if (!WorldEventController.IsPlayerHost()) //Only for event controller (usually me but this is really just for dev testing)
                return;

            foreach (Vehicle veh in World.GetAllVehicles())
                veh.Delete();

            foreach (Ped ped in World.GetAllPeds())
                if (!ped.IsPlayer)
                    ped.Delete();

            foreach (Prop prop in World.GetAllProps())
                prop.Delete();
        }

        private static void ReloadSafezoneObjects()
        {
            SafeZone.SpawnSafezoneObjects();
        }
    }
}
