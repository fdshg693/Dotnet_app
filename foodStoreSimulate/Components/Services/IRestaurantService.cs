using foodStoreSimulate.Components.Services.Entities;
using static foodStoreSimulate.Components.Pages.Queue;

namespace foodStoreSimulate.Components.Services
{
    public interface IRestaurantService
    {
        void EnqueueCustomer();
        void EnqueueFood(MenuItem orderedFood);
    }

}
