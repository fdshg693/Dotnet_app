using static foodStoreSimulate.Components.Pages.Queue;
using foodStoreSimulate.Components.Services;
using foodStoreSimulate.Components.Services.Entities;
using Microsoft.VisualBasic;

namespace foodStoreSimulate.Components.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly TextLogger logger;
        private readonly CustomerQueueService customerQueueService;
        private readonly FoodQueueService foodQueueService;
        private UIStateService uIStateService;

        public RestaurantService(TextLogger logger, CustomerQueueService customerQueueService, FoodQueueService foodQueueService, UIStateService uIStateService)
        {
            this.logger = logger;
            this.customerQueueService = customerQueueService;
            this.uIStateService = uIStateService;
            this.foodQueueService = foodQueueService;
        }

        public IReadOnlyCollection<Customer> Customers => customerQueueService.Customers;
        public IReadOnlyCollection<CookedFood> Foods => foodQueueService.cookedFoods;
        public IReadOnlyCollection<Garbage> Garbages => foodQueueService.Garbages;

        public void EnqueueCustomer()
        {
            customerQueueService.EnqueueCustomer();
            TryServe();
        }

        public void EnqueueFood(MenuItem orderedFood)
        {
            foodQueueService.EnqueueFood(orderedFood);
            TryServe();
        }

        public void DisposeExpiredFood()
        {
            foodQueueService.DisposeExpiredFood();
        }

        public async Task CookAsync(int SelectedValue)
        {
            MenuItem? orderedFood = MenuService.GetMenuItem(SelectedValue);
            if (orderedFood == null)
            {
                return;
            }
            uIStateService.SetCooking(true);
            await Task.Delay(orderedFood.CookingTime  * 1000);
            EnqueueFood(orderedFood);
            uIStateService.SetCooking(false);
        }

        private void TryServe()
        {
            DisposeExpiredFood();
            while (customerQueueService.Any() && foodQueueService.AnyFood())
            {
                var customer = customerQueueService.DequeueCustomer();
                var cookedFood = foodQueueService.DequeueFood();
                logger.AddLog($"{customer.Name} に {cookedFood.Food.DishName} を提供しました。");
            }
        }
    }

}
