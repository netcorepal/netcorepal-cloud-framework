using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaEntity : Entity<Guid>, IAggregateRoot
    {
        protected SagaEntity()
        {
        }

        public SagaEntity(Guid sagaId, string sagaData, DateTime whenTimeout)
        {
            Id = sagaId;
            WhenTimeout = whenTimeout;
            this.SagaData = sagaData;
            IsComplete = false;
            IsError = false;
            EventStatus = EventStatus.Pending;
        }

        public bool IsComplete { get; set; }

        public DateTime WhenTimeout { get; set; }

        public bool IsError { get; set; }

        public EventStatus EventStatus { get; set; }

        public string SagaData { get; set; } = string.Empty;
    }

    public enum EventStatus
    {
        Pending,
        Completed,
        Error
    }
}