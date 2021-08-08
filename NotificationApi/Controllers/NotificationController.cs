using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotificationApi.Commands;
using NotificationApi.Events;
using NotificationApi.Helpers;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationApi.Controllers
{
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> logger;
        private readonly DaprClient daprClient;

        public NotificationController(ILogger<NotificationController> logger, DaprClient daprClient)
        {
            this.logger = logger;
            this.daprClient = daprClient;
        }

        [Route("sendemail")]
        [HttpPost()]
        [Topic("eventbus", "OrderProcessedEvent")]
        public async Task<IActionResult> SendEmail(DispathOrderCommand command)
        {
            this.logger.LogInformation("SendEmail method entered");
            this.logger.LogInformation("Order received for dispatch " + command.OrderId);

            var metadata = new Dictionary<string, string>()
            {
                ["emailFrom"] = "face@abc.com",
                ["emailTo"] = "aaa@aaa.it",
                ["subject"] = $"your order {command.OrderId}"
            };

            var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            var facesData = command.Faces;
            if (facesData == null || facesData.Count < 1)
            {
                this.logger.LogInformation("No faces detected");
            }
            else
            {
                int j = 0;
                foreach (var face in facesData)
                {
                    Image img = Image.Load(face);
                    img.Save(rootFolder + "/Images/face" + j + ".jpg");
                    j++;
                }
            }

            var body = EmailUtils.CreateEmailBody(command);
            await this.daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);
            var eventMsg = new OrderDispatchedEvent
            {
                OrderId = command.OrderId,
                DispatchedAt = DateTime.UtcNow
            };
            await this.daprClient.PublishEventAsync<OrderDispatchedEvent>("eventbus", "OrderDispatchedEvent", eventMsg);
            this.logger.LogInformation($"Dispatched order {command.OrderId} at {eventMsg.DispatchedAt} ");

            return Ok();
        }
    }

    
}
