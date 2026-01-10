using foodStoreSimulate.Components.Services.Entities;

namespace foodStoreSimulate.Components.Services
{
    public class FoodQueueService
    {
        private readonly TextLogger logger;
        private readonly Queue<CookedFood> cookedFoodQueue = new();
        private readonly Queue<Garbage> garbageQueue = new();
        private int maxFoodId = 0;
        public IReadOnlyCollection<CookedFood> cookedFoods => cookedFoodQueue;
        public IReadOnlyCollection<Garbage> Garbages => garbageQueue;
        public FoodQueueService(TextLogger logger)
        {
            this.logger = logger;
        }
        public void EnqueueFood(MenuItem orderedFood)
        {
            maxFoodId++;
            var cookedFood = new CookedFood { CookedItemId = maxFoodId, MenuItem = orderedFood };
            cookedFoodQueue.Enqueue(cookedFood);
            logger.AddLog($"食品が準備されました: {cookedFood.MenuItem.Food.DishName}");
        }
        public MenuItem DequeueFood()
        {
            if (cookedFoodQueue.Count == 0)
            {
                throw new InvalidOperationException("食品がありません。");
            }
            var cookedFood = cookedFoodQueue.Dequeue();
            logger.AddLog($"食品が提供されました: {cookedFood.MenuItem.Food.DishName}");
            return cookedFood.MenuItem;
        }
        public void DisposeExpiredFood()
        {
            while (cookedFoodQueue.Any() && cookedFoodQueue.Peek().IsExpired())
            {
                var cookedFood = cookedFoodQueue.Dequeue();
                var garbage = Garbage.AutoGenereateGarbage(cookedFood);
                garbageQueue.Enqueue(garbage);
                logger.AddLog($"{garbage.DishName} は賞味期限切れのため廃棄しました。");
            }
        }
        public bool AnyFood()
        {
            return cookedFoodQueue.Any();
        }

    }
}
