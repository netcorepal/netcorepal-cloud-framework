using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public abstract class SagaDataBase
    {
        

    }


    public abstract class SagaData 
    {
        public string Id { get; set; } = null!;

        public bool IsComplate { get; set; }
    }

    public abstract class SagaData<TResult> : SagaData
    {
        public TResult? Result { get; set; }
    }
}
