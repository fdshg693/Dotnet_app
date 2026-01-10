namespace foodStoreSimulate.Components.Services.Entities
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public required Food Food { get; set; }
        public int CookingTime { get; set; }
        public int Price { get; set; }
    }
    public struct CookedFood
    {
        public int CookedItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public bool IsExpired()
        {
            return MenuItem.Food.IsExpired();
        }
    }
    public class Garbage
    {
        public required int Id { get; set; }
        public required string DishName { get; set; }
        public required DateTime ExpiredAt { get; set; }
        public static Garbage AutoGenereateGarbage(CookedFood cookedFood)
        {
            return new Garbage { Id = cookedFood.CookedItemId, DishName = cookedFood.MenuItem.Food.DishName, ExpiredAt = cookedFood.MenuItem.Food.ExpiredAt };
        }
    }
}
