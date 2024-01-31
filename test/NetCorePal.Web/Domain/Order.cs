using NetCorePal.Extensions.Domain;
using NetCorePal.Web.Domain.DomainEvents;

namespace NetCorePal.Web.Domain
{

    public partial record OrderId : IInt64StronglyTypedId;
    public class Order : Entity<OrderId>, IAggregateRoot
    {
        protected Order()
        {
        }
        public Order(string name, int count)
        {
            this.Name = name;
            this.Count = count;
            this.AddDomainEvent(new OrderCreatedDomainEvent(this));
        }

        public bool Paid { get; private set; } = false;

        public string Name { get; private set; } = string.Empty;

        public int Count { get; private set; }
        
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;

        public void OrderPaid()
        {
            this.Paid = true;
            this.CreateTime = DateTime.UtcNow;
        }
    }
}
