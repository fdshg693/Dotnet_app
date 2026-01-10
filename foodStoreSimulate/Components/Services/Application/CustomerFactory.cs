using foodStoreSimulate.Components.Services.Entities;

namespace foodStoreSimulate.Components.Services.Application
{
    public interface IDateTimeProvider { DateTime Now { get; } }
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
    public class CustomerFactory
    {
        private readonly IDateTimeProvider _clock;
        public CustomerFactory(IDateTimeProvider clock) => _clock = clock;

        public Customer Create(int id) =>
            new Customer(id, $"customer {id}", _clock.Now);
    }
}
