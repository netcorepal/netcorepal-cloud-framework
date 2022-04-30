using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Extensions.EventBus
{
    public class EventData
    {
        public string? Body { get; set; }

        public Dictionary<string, string>? Headers { get; set; }
    }
}
