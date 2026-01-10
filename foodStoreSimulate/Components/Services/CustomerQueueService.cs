using foodStoreSimulate.Components.Services.Entities;
using foodStoreSimulate.Components.Services.Application;

namespace foodStoreSimulate.Components.Services
{
    public class CustomerQueueService
    {
        private readonly TextLogger logger;
        private readonly Queue<Customer> customerQueue = new();
        private int maxCustomerId = 0;
        private CustomerFactory customerFactory;
        public IReadOnlyCollection<Customer> Customers => customerQueue;
        public CustomerQueueService(TextLogger logger, CustomerFactory customerFactory)
        {
            this.logger = logger;
            this.customerFactory = customerFactory;
        }
        public void EnqueueCustomer()
        {
            maxCustomerId++;
            var customer = customerFactory.Create(maxCustomerId);
            customerQueue.Enqueue(customer);
            logger.AddLog($"お客さんが並びました: {customer.Name}");
        }
        public Customer DequeueCustomer()
        {
            if (customerQueue.Count > 0)
            {
                var customer = customerQueue.Dequeue();
                logger.AddLog($"お客さんが料理を受け取りました: {customer.Name}");
                return customer;
            }
            else
            {
                throw new InvalidOperationException("キューが空です。");
            }
        }
        public bool Any()
        {
            return customerQueue.Any();
        }
    }
}
