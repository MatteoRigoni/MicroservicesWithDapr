using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacesApi.Commands
{
    public class ProcessOrderCommand
    {
        public Guid OrderId { get; set; }
        public byte[] ImageData { get; set; }
        public string UserEmail { get; set; }
    }
}
