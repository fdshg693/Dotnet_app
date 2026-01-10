using GameEngine.Interfaces;

namespace GameEngine.Systems
{
    public static class RestSystem
    {
        public static void UsePotion(IPlayer player)
        {
            Console.WriteLine("You can Use Potion!");
            Console.WriteLine($"Your Potion: {player.ReturnTotalPotions()}");
            int? potionAmount = UserInteraction.ReadPositiveInteger("Enter the amount of Potuion you want to use: ");
            if (potionAmount != null)
            {
                player.UsePotion(potionAmount.Value);
            }
        }
    }
}
