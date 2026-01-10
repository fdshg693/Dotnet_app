namespace foodStoreSimulate.Components.Services
{
    public class UIStateService
    {
        public bool IsCooking { get; private set; } = false;

        public void SetCooking(bool cooking)
        {
            IsCooking = cooking;
        }
    }

}
