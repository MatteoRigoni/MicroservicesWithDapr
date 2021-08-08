using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacesApi.Events
{
    public class OrderProcessedEvent
    {
        public Guid OrderId { get; set; }
        public byte[] FaceData { get; set; }
        public List<byte[]> Faces { get; set; }
        public string UserEmail { get; set; }
    }
}
