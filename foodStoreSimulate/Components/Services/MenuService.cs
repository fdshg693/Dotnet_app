using foodStoreSimulate.Components.Services.Entities;

namespace foodStoreSimulate.Components.Services
{
    public static class MenuService
    {
        public static MenuItem? GetMenuItem(int id)
        {
            switch (id)
            {
                case 1:
                    Food curry = new Food(1,"カレー",DateTime.Now, 1);
                    return new MenuItem { MenuItemId = 1, Food = curry, CookingTime = 1, Price = 500 };
                case 2:
                    Food ramen = new Food(2, "ラーメン", DateTime.Now, 2);
                    return new MenuItem { MenuItemId = 2, Food = ramen, CookingTime = 2, Price = 600 };
                case 3:
                    Food yakisoba = new Food (3, "焼きそば", DateTime.Now, 3);
                    return new MenuItem { MenuItemId = 3, Food = yakisoba, CookingTime = 3, Price = 550 };
                default:
                    return null;
            }
        }
    }


}
