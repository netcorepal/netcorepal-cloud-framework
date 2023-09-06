using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaEntity
    {
        public Guid Id { get; set; }

        public bool IsComplete { get; set; }

        public string Data { get; set; } = null!;
    }
}
