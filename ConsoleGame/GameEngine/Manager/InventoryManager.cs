using GameEngine.Constants;
using GameEngine.Interfaces;
using GameEngine.Models;

namespace GameEngine.Manager
{
    public class InventoryManager : IEquipmentStatsProvider
    {
        public int TotalGold { get; private set; } = GameConstants.InitialGold;
        public IWeapon Weapon { get; private set; }
        public event Action? EquipmentChanged;
        public int TotalPotions { get; private set; } = GameConstants.InitialPotions;
        public InventoryManager()
        {
            Weapon = new Weapon(0, 0, 0, "Default");
        }
        public void EquipWeapon(IWeapon newWeapon)
        {
            Weapon = newWeapon;
            EquipmentChanged?.Invoke();
            Console.WriteLine($"You equipped a {newWeapon.Name}");
        }
        public void GainGold(int amount)
        {
            Console.WriteLine($"You gain {amount} gold");
            TotalGold += amount;
        }
        public void BuyPotion(int amount)
        {
            if (TotalGold >= amount * GameConstants.PotionPrice)
            {
                TotalGold -= amount * GameConstants.PotionPrice;
                TotalPotions += amount;
                Console.WriteLine($"You bought {amount} potions");
            }
            else
            {
                Console.WriteLine("Not enough gold!");

            }
        }
        public void UsePotion(int amount)
        {
            if (TotalPotions >= amount)
            {
                TotalPotions -= amount;
                Console.WriteLine($"You used {amount} potions");
            }
            else
            {
                Console.WriteLine("Not enough potions!");
            }
        }
        public int ReturnTotalPotions()
        {
            return TotalPotions;
        }
        public int ReturnTotalGold()
        {
            return TotalGold;
        }
        public void ShowInfo()
        {
            Console.WriteLine($"Total Gold: {TotalGold}");
            Console.WriteLine($"Total Potions: {TotalPotions}");
            Console.WriteLine($"Equipped Weapon: {Weapon.Name}");
        }
    }
}
