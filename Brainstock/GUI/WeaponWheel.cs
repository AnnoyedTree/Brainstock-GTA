using System.Drawing;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace Brainstock.GUI
{

    public class WeaponWheel : BaseScript
    {
        private const float IconSpacing = 16;
        public static int IconAlpha = 1;

        private static List<string> DisplayNames;

        public WeaponWheel()
        {
            DisplayNames = new List<string>();
        }

        public static void Draw(int ScreenX, int ScreenY)
        {
            for (int i = 0; i < User.playerWeapons.Length; i++)
            {
                bool current = IsCurrentWeapon(i);

                int x = (int)(ScreenX * 1.8);
                int y = (int)((ScreenY * 1) + (i*IconSpacing));

                //string weaponName = "EMPTY";
                //string weaponName = Weapon.GetDisplayNameFromHash(User.playerWeapons[i]);
                string weaponName = DisplayName(User.playerWeapons[i]);
                int alpha = IconAlpha;

                if (alpha > 255)
                    alpha = 255;

                Color col = Color.FromArgb(alpha, 120, 120, 120);

                if (current)
                {
                    col = Color.FromArgb(alpha, 200, 200, 200);
                    x -= 3;
                }

                Text weaponText = new Text(weaponName, new PointF(x, y), 0.2f);
                weaponText.Color = col;
                weaponText.Draw();

                
            }

            if (IconAlpha > 0)
                FadeOut();
        }

        private static async void FadeOut()
        {
            while (IconAlpha > 0)
            {
                IconAlpha--;

                if (IconAlpha >= 800)
                    break;

                await Delay(250);
            }
        }

        private static bool IsCurrentWeapon(int index)
        {
            return (Input.EquippedWeapon == (uint)User.playerWeapons[index]);
        }

        private static string DisplayName(WeaponHash hash)
        {

            if (hash == WeaponHash.AssaultRifle)
                return "KLASHNIKOV";
            else if (hash == WeaponHash.CarbineRifle)
                return "M4A1 CARBINE";
            else if (hash == WeaponHash.PistolMk2)
                return "PISTOL MKII";
            else if (hash == WeaponHash.Flare)
                return "TACTICAL FLARE";
            else if (hash == WeaponHash.Knife)
                return "COMBAT KNIFE";
            else if (hash == WeaponHash.BattleAxe)
                return "BATTLE-AXE";
            else if (hash == WeaponHash.RPG)
                return "ROCKET LAUNCHER";
            else if (hash == WeaponHash.HeavySniper)
                return "SNIPER RIFLE";
            else if (hash == WeaponHash.Unarmed)
                return "UNARMED";

            return "INVALID";
        }
    }
}
