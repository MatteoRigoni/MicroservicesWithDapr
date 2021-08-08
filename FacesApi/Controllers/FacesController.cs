using Dapr;
using Dapr.Client;
using FacesApi.Commands;
using FacesApi.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacesApi.Controllers
{
    [ApiController]
    public class FacesController : Controller
    {
        private readonly ILogger<FacesController> logger;
        private readonly DaprClient daprClient;
        private readonly AzureFaceCredentials creds;

        public FacesController(ILogger<FacesController> logger, DaprClient daprClient, AzureFaceCredentials creds)
        {
            this.logger = logger;
            this.daprClient = daprClient;
            this.creds = creds;
        }

        [Route("processorder")]
        [HttpPost]
        [Topic("eventbus", "OrderRegisteredEvent")]
        public async Task<IActionResult> ProcessOrder(ProcessOrderCommand command)
        {
            this.logger.LogInformation("ProcessOrder method entered...");
            if (ModelState.IsValid)
            {
                this.logger.LogInformation($"Command params: {command.OrderId}");
                Image img = Image.Load(command.ImageData);
                img.Save("dummy.jpg");

                var orderState = await this.daprClient.GetStateEntryAsync<List<ProcessOrderCommand>>("redisstore", "orderList");
                List<ProcessOrderCommand> orderList = new();
                if (orderState.Value == null)
                {
                    this.logger.LogInformation("OrderState, case 1...");
                    orderList.Add(command);
                }
                else
                {
                    this.logger.LogInformation("OrderState, case 2...");
                    orderList = orderState.Value;
                    orderList.Add(command);
                }
                await this.daprClient.SaveStateAsync("redisstore", "orderList", orderList);

                return Ok();
            }
            else
                return BadRequest();
            
        }

        [HttpPost("cron")]
        public async Task<IActionResult> Cron()
        {
            this.logger.LogInformation("Cron method entered");
            var orderState = await this.daprClient.GetStateEntryAsync<List<ProcessOrderCommand>>("redisstore", "orderList");
            if (orderState?.Value?.Count > 0)
            {
                this.logger.LogInformation($"Number of items to process: {orderState?.Value?.Count }");
                var orderList = orderState.Value;
                var firstInTheList = orderList[0];
                if (firstInTheList != null)
                {
                    this.logger.LogInformation($"First's order id: {firstInTheList.OrderId}");
                    byte[] imageBytes = firstInTheList.ImageData.ToArray();
                    Image img = Image.Load(imageBytes);
                    img.Save("dummy1.jpg");

                    List<byte[]> facesCropped = UploadPhotoAndDetectFaces(img, new MemoryStream(imageBytes));
                    var ope = new OrderProcessedEvent()
                    {
                        OrderId = firstInTheList.OrderId,
                        UserEmail = firstInTheList.UserEmail,
                        FaceData = firstInTheList.ImageData,
                        Faces = facesCropped
                    };
                    await this.daprClient.PublishEventAsync<OrderProcessedEvent>("eventbus", "OrderProcessedEvent", ope);
                    this.logger.LogInformation($"Item processed: {ope.OrderId}");
                    orderList.Remove(firstInTheList);
                    await this.daprClient.SaveStateAsync("redisstore", "orderList", orderList);
                    this.logger.LogInformation($"Nuber of items after processing: {orderList.Count}");

                    return Ok();
                }
            }

            return NoContent();
        }

        private List<byte[]> UploadPhotoAndDetectFaces(Image img, MemoryStream memoryStream)
        {
            //todo, parsing with Azure faces api
            //IFaceClient client = Authenticate(this.creds.AzureEndpoint, this.creds.AzureSubscriptionKey);

            var faceList = new List<byte[]> { memoryStream.ToArray() };
            return faceList;
        }
    }
}
