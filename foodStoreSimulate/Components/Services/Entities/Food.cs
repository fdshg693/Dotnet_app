using Microsoft.AspNetCore.Http.HttpResults;

namespace foodStoreSimulate.Components.Services.Entities
{
    public class Food
    {
        public int FoodId { get; set; }
        public string DishName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ExpirationSpan { get; set; }
        public DateTime ExpiredAt { get; set; }
        public Food(int foodId ,string dishName, DateTime createdAt, int expirationSpan)
        {
            FoodId = foodId;
            DishName = dishName;
            CreatedAt = createdAt;
            ExpirationSpan = expirationSpan;
            ExpiredAt = CreatedAt.AddMinutes(expirationSpan);
        }
        public bool IsExpired()
        {
            return DateTime.Now > this.ExpiredAt;
        }
    }
    
}
