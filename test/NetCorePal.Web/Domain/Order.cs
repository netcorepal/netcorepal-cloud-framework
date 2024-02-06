using NetCorePal.Extensions.Domain;
using NetCorePal.Web.Domain.DomainEvents;

namespace NetCorePal.Web.Domain
{

    /// <summary>
    /// 
    /// </summary>
    public partial record OrderId : IInt64StronglyTypedId;
    /// <summary>
    /// 
    /// </summary>
    public class Order : Entity<OrderId>, IAggregateRoot
    {
        /// <summary>
        /// 
        /// </summary>
        protected Order()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="count"></param>
        public Order(string name, int count)
        {
            this.Name = name;
            this.Count = count;
            this.AddDomainEvent(new OrderCreatedDomainEvent(this));
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Paid { get; private set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// 
        /// </summary>
        public void OrderPaid()
        {
            this.Paid = true;
            this.CreateTime = DateTime.UtcNow;
        }
    }
}
