using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace Brainstock.GUI
{
    class HUD : BaseScript
    {
        private static int ScreenX, ScreenY;

        public HUD()
        {
            Tick += OnTick;
        }

        private static async Task OnTick()
        {
            GetScreenResolution(ref ScreenX, ref ScreenY);

            ScreenX /= 2;
            ScreenY /= 2;

            if (Game.PlayerPed == null || User.GetTeam() == 0)
                return;

            Ped ped = Game.PlayerPed;
            int playerHealth = ped.Health;

            if (playerHealth < 0 || !ped.IsAlive)
                return;

            int x = (int)(ScreenX * 0.04);
            int y = (int)(ScreenY * 1.96);

            Text healthText = new Text("HP: " + playerHealth, new PointF(x,y), 0.2f);
            healthText.Color = Color.FromArgb(255, 0, 185, 0);
            healthText.Draw();

            int ammoMagazine = 0;
            GetAmmoInClip(ped.Handle, Input.EquippedWeapon, ref ammoMagazine);
            //int ammoMagazine = GetAmmoInPedWeapon(ped.Handle, Input.EquippedWeapon);
            int type = GetPedAmmoType(ped.Handle, (uint)Input.EquippedWeapon);
            int ammoReserve = (GetPedAmmoByType(ped.Handle, type) - ammoMagazine);

            x = (int)(ScreenX * 1.90);
            //y = (int)(ScreenY * 0.1);

            if (ammoMagazine > 0 || ammoReserve > 0)
            {
                Text ammoText = new Text(ammoMagazine + "/" + ammoReserve, new PointF(x, y), 0.2f);
                ammoText.Color = Color.FromArgb(255, 120, 120, 120);
                ammoText.Draw();
            }

            if (Game.IsControlPressed(0, Control.Aim) && Game.IsControlEnabled(0, Control.Aim))
            {
                Text crosshair = new Text("+", new PointF(ScreenX-3, ScreenY-7), 0.3f);
                crosshair.Color = Color.FromArgb(255, 120, 120, 120);
                crosshair.Draw();
            }

            if (SafeZone.IsInSafezone())
            {
                x = (int)(ScreenX * 1);
                y = (int)(ScreenY * 1.96);

                Text safeText = new Text("[SAFEZONE]", new PointF(x,y), 0.2f);
                safeText.Color = Color.FromArgb(255, 120, 180, 120);
                safeText.Draw();
            }

            WeaponWheel.Draw(ScreenX, ScreenY);

            await Task.FromResult(0);
        }
    }
}
