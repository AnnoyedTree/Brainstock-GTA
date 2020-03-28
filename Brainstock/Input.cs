using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Brainstock
{
    class Input : BaseScript
    {
        private static int WeaponDelay;
        private static int WeaponIndex;

        public static uint EquippedWeapon { get; private set; }

        public Input()
        {
            WeaponIndex = 3;
            //Tick += OnTick;
        }

        public static void SwitchWeapons()
        {
            if (SafeZone.IsInSafezone())
            {
                Game.DisableControlThisFrame(0, Control.Aim);
                Game.DisableControlThisFrame(0, Control.Attack);
                Game.DisableControlThisFrame(0, Control.Attack2);
                Game.DisableControlThisFrame(0, Control.VehicleAttack);
                Game.DisableControlThisFrame(0, Control.VehicleAttack2);
                Game.DisableControlThisFrame(0, Control.MeleeAttackAlternate);
                Game.DisableControlThisFrame(0, Control.MeleeAttack1);
                Game.DisableControlThisFrame(0, Control.MeleeAttack2);
                Game.DisableControlThisFrame(0, Control.MeleeAttackHeavy);
                Game.DisableControlThisFrame(0, Control.MeleeAttackLight);
            }

            if (API.GetGameTimer() < WeaponDelay)
                return;

            if (Game.IsControlPressed(0, Control.WeaponWheelNext)) // Next weapons
            {
                WeaponIndex++;
                if (WeaponIndex > (User.playerWeapons.Length-1))
                    WeaponIndex = 0;

                if (User.playerWeapons[WeaponIndex] != 0)
                    EquipWeapon(User.playerWeapons[WeaponIndex], WeaponIndex);

                GUI.WeaponWheel.IconAlpha = 800;
                WeaponDelay = API.GetGameTimer() + 200;
            }
            else if (Game.IsControlPressed(0, Control.WeaponWheelPrev)) //Prev weapon
            {
                WeaponIndex--;
                if (WeaponIndex < 0)
                    WeaponIndex = (User.playerWeapons.Length-1);

                if (User.playerWeapons[WeaponIndex] != 0)
                    EquipWeapon(User.playerWeapons[WeaponIndex], WeaponIndex);

                GUI.WeaponWheel.IconAlpha = 800;
                WeaponDelay = API.GetGameTimer() + 200;
            }

            if (Game.IsControlPressed(0, Control.SelectWeaponUnarmed))// Number 1 key - PRIMARY
            {
                EquipWeapon(User.playerWeapons[0], 0);
                WeaponIndex = 0;
                GUI.WeaponWheel.IconAlpha = 800;
            }
            else if (Game.IsControlPressed(0, Control.SelectWeaponMelee)) //Number 2 key - SECONDARY
            {
                EquipWeapon(User.playerWeapons[1], 1);
                GUI.WeaponWheel.IconAlpha = 800;
            }
            else if (Game.IsControlPressed(0, Control.SelectWeaponShotgun)) //Number 3 key - SPECIAL
            {
                EquipWeapon(User.playerWeapons[2], 2);
                GUI.WeaponWheel.IconAlpha = 800;
            }
            else if (Game.IsControlPressed(0, Control.SelectWeaponHeavy)) //Number 4 key - MELEE
            {
                EquipWeapon(User.playerWeapons[3], 3);
                GUI.WeaponWheel.IconAlpha = 800;
            }
            else if (Game.IsControlPressed(0, Control.SelectWeaponSpecial)) //Number 5 key - UNARMED
            {
                EquipWeapon(User.playerWeapons[4], 4);
                GUI.WeaponWheel.IconAlpha = 800;
            }
        }

        public static void EquipWeapon( WeaponHash hash, int index )
        {
            int ped = API.GetPlayerPed(API.PlayerId());
            API.SetCurrentPedWeapon(ped, (uint)hash, false);

            if (API.GetVehiclePedIsIn(ped, false) != 0)
                API.SetCurrentPedVehicleWeapon(ped, (uint)hash);

            EquippedWeapon = (uint)hash;
        }
    }
}
