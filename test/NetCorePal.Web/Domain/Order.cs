using System.ComponentModel.DataAnnotations.Schema;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;

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
            this.CreateTime = DateTime.UtcNow;
            this.OrderItems.Add(new OrderItem(name, count));
            this.RowVersion = new RowVersion(0);
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
        public DateTimeOffset CreateTime { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 订单货品列表
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

        /// <summary>
        /// 记录版本号，用以乐观锁
        /// </summary>
        [Column("row_version")]
        public RowVersion RowVersion { get; private set; } = new RowVersion(0);

        /// <summary>
        /// 更新时间
        /// </summary>
        public UpdateTime UpdateAt { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

        /// <summary>
        ///     是否已删除
        /// </summary>
        public Deleted Deleted { get; private set; } = new();

        /// <summary>
        ///     删除时间
        /// </summary>
        public DeletedTime DeletedTime { get; private set; } = new(DateTimeOffset.MinValue);

        /// <summary>
        /// 
        /// </summary>
        public void OrderPaid()
        {
            this.Paid = true;
            this.CreateTime = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            Deleted = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="KnownException"></exception>
        public void ChangeItemName(string name)
        {
            this.Name = name;
            var item = this.OrderItems.FirstOrDefault();
            if (item != null)
            {
                item.ChangeName(name);
            }
            else
            {
                throw new KnownException("OrderItem not found");
            }
        }
    }
}