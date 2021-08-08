using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcFront.Models
{
    public class UploadDataCommand
    {
        public Guid OrderId { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public IFormFile File { get; set; }
    }
}
