namespace foodStoreSimulate.Components.Services.Entities
{
    public class Customer
    {
        public int Id { get; }
        public string Name { get; }
        public DateTime CreatedAt { get; }

        public Customer(int id, string name, DateTime createdAt)
        {
            Id = id;
            Name = name;
            CreatedAt = createdAt;
        }

        public TimeSpan GetWaitingTime(DateTime now) =>
            now - CreatedAt;
    }
}
