using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Commands
{
    public class OrderReceivedCommand
    {
        public Guid OrderId { get; set; }
        public string PhotoUrl { get; set; }
        public byte[] ImageData { get; set; }
        public string UserEmail { get; set; }
    }
}
