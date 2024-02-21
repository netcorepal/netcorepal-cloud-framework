using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Snowflake
{
#pragma warning disable S3925
    public sealed class WorkerIdConflictException : Exception
#pragma warning restore S3925
    {
        public WorkerIdConflictException(string message) : base(message)
        {
        }

    }
}
