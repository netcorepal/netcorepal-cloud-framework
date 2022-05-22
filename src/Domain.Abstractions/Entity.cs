using System;
using System.Collections.Generic;

namespace NetCorePal.Extensions.Domain
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract class Entity
    {
        public abstract object[] GetKeys();


        public override string ToString() => $"[Entity: {GetType().Name}] Keys = {string.Join(",", GetKeys())}";

        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(IDomainEvent eventItem) => _domainEvents.Remove(eventItem);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    /// <summary>
    /// 实体泛型基类
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    public abstract class Entity<TKey> : Entity where TKey : struct
    {
        public virtual TKey Id { get; protected set; }
        public override object[] GetKeys() => new object[] { Id };
        public override bool Equals(object? obj)
        {
            if (!(obj is Entity<TKey>))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            var item = (Entity<TKey>)obj;

            if (item.IsTransient() || IsTransient())
            {
                return false;
            }
            else
            {
                return item.Id.Equals(Id);
            }
        }


        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                return Id.GetHashCode() ^ 31;
            }
            else
            {
                return base.GetHashCode();
            }
        }



        //表示对象是否为全新创建的，未持久化的
        public bool IsTransient() => EqualityComparer<TKey>.Default.Equals(Id, default);

        public override string ToString() => $"[Entity: {GetType().Name}] Id = {Id}";


        public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
        {
            if (Object.Equals(left, null))
            {
                return (Object.Equals(right, null));
            }
            else
            {
                return left.Equals(right);
            }
        }

        public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right) => !(left == right);
    }
}
