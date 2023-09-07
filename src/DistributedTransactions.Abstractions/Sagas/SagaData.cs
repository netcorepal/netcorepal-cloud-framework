using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public abstract class SagaData
    {
        public Guid SagaId { get; set; } = Guid.Empty;
    }

    public abstract class SagaData<TResult> : SagaData
    {
        public TResult? Result { get; set; }
    }
}