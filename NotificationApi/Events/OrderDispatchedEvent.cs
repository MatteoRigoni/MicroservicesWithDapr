using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationApi.Events
{
    public class OrderDispatchedEvent
    {
        public Guid OrderId { get; set; }
        public DateTime DispatchedAt { get; set; }
    }
}
