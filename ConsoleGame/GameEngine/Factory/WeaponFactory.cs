using GameEngine.Constants;
using GameEngine.Interfaces;
using GameEngine.Models;

namespace GameEngine.Factory
{
    public static class WeaponFactory
    {
        public static IWeapon CreateWeapon(string weaponType)
        {
            switch (weaponType.ToLower())
            {
                case "sword":
                    return new Weapon(GameConstants.Weapons.SwordHP, GameConstants.Weapons.SwordAP, GameConstants.Weapons.SwordDP, "sword");
                case "axe":
                    return new Weapon(GameConstants.Weapons.AxeHP, GameConstants.Weapons.AxeAP, GameConstants.Weapons.AxeDP, "axe");
                case "bow":
                    return new Weapon(GameConstants.Weapons.BowHP, GameConstants.Weapons.BowAP, GameConstants.Weapons.BowDP, "bow");
                case "default":
                default:
                    return new Weapon(0, 0, 0, "Default");
            }
        }
    }
}
