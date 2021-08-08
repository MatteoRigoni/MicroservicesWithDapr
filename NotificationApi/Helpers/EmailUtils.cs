using NotificationApi.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationApi.Helpers
{
    public class EmailUtils
    {
        internal static string CreateEmailBody(DispathOrderCommand command)
        {
            return $"Order id processed! ({command.OrderId})";
        }
    }
}
