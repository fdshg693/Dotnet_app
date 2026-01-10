using GameEngine.Factory;
using GameEngine.Interfaces;

namespace GameEngine.Systems
{
    public static class ShopSystem
    {
        public static void Shop(IPlayer player)
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Welcome to the shop!");
            Console.WriteLine("1. Buy Item");
            Console.WriteLine("2. Buy Weapon");
            Console.WriteLine("3. Exit Shop");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine($"Your Potion: {player.ReturnTotalPotions()}");
            Console.WriteLine($"Your Gold: {player.ReturnTotalGold()}");
            Console.WriteLine("-------------------------------------------------------------------");
            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.D1)
                {
                    int? potionAmount = UserInteraction.ReadPositiveInteger("Enter the amount of Potuion you want to buy: ");
                    if (potionAmount != null)
                    {
                        player.BuyPotion(potionAmount.Value);
                    }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("-------------------------------------------------------------------");
                    Console.WriteLine("Choose Weapon");
                    Console.WriteLine("1. SWORD");
                    Console.WriteLine("2. AXE");
                    Console.WriteLine("3. BOW");
                    Console.WriteLine("-------------------------------------------------------------------");
                    keyInfo = Console.ReadKey(intercept: true);
                    if (keyInfo.Key == ConsoleKey.D1)
                    {
                        player.EquipWeapon(WeaponFactory.CreateWeapon("SWORD"));
                    }
                    else if (keyInfo.Key == ConsoleKey.D2)
                    {
                        player.EquipWeapon(WeaponFactory.CreateWeapon("AXE"));
                    }
                    else if (keyInfo.Key == ConsoleKey.D3)
                    {
                        player.EquipWeapon(WeaponFactory.CreateWeapon("BOW"));
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice.");
                    }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.D3)
                {
                    break;
                }
            }
        }
    }
}
