using GameEngine.Interfaces;
using GameEngine.Models;

namespace GameEngine.Systems
{
    /// <summary>
    /// Console向けの入力実装
    /// </summary>
    public class ConsoleGameInput : IGameInput
    {
        public AttackAction SelectAttackAction(BattleState battleState, PlayerState playerState, EnemyState enemyState)
        {
            var strategyName = UserInteraction.SelectAttackStrategy(battleState.AvailableStrategies);
            return new AttackAction(strategyName);
        }

        public ShopAction SelectShopAction(ShopState shopState, PlayerState playerState)
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Welcome to the shop!");
            Console.WriteLine("1. Buy Item");
            Console.WriteLine("2. Buy Weapon");
            Console.WriteLine("3. Exit Shop");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine($"Your Potion: {playerState.Potions}");
            Console.WriteLine($"Your Gold: {playerState.Gold}");
            Console.WriteLine("-------------------------------------------------------------------");

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.D1)
                {
                    int? potionAmount = UserInteraction.ReadPositiveInteger("Enter the amount of Potion you want to buy: ");
                    if (potionAmount != null)
                    {
                        return new ShopAction(ShopActionType.BuyPotion, quantity: potionAmount.Value);
                    }
                    return new ShopAction(ShopActionType.Exit);
                }
                if (keyInfo.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("-------------------------------------------------------------------");
                    Console.WriteLine("Choose Weapon");
                    for (int i = 0; i < shopState.AvailableWeapons.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {shopState.AvailableWeapons[i].Name}");
                    }
                    Console.WriteLine("-------------------------------------------------------------------");
                    keyInfo = Console.ReadKey(intercept: true);
                    int choiceIndex = keyInfo.Key switch
                    {
                        ConsoleKey.D1 => 0,
                        ConsoleKey.D2 => 1,
                        ConsoleKey.D3 => 2,
                        _ => -1
                    };

                    if (choiceIndex >= 0 && choiceIndex < shopState.AvailableWeapons.Count)
                    {
                        string weaponName = shopState.AvailableWeapons[choiceIndex].Name;
                        return new ShopAction(ShopActionType.BuyWeapon, weaponName, 1);
                    }

                    Console.WriteLine("Invalid choice.");
                    return new ShopAction(ShopActionType.Exit);
                }
                if (keyInfo.Key == ConsoleKey.D3)
                {
                    return new ShopAction(ShopActionType.Exit);
                }
            }
        }

        public UseItemAction? SelectRestAction(PlayerState playerState)
        {
            Console.WriteLine("You can Use Potion!");
            Console.WriteLine($"Your Potion: {playerState.Potions}");
            int? potionAmount = UserInteraction.ReadPositiveInteger("Enter the amount of Potion you want to use: ");
            if (potionAmount != null)
            {
                return new UseItemAction("Potion", potionAmount.Value);
            }

            return null;
        }
    }
}
