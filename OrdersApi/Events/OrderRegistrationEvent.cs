using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Events
{
    public class OrderStatusChangedToDispatched
    {
        public Guid OrderId { get; set; }
        public DateTime DispatchedAt { get; set; }
    }
}
