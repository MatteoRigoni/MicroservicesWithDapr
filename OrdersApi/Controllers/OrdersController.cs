using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrdersApi.Commands;
using OrdersApi.Events;
using OrdersApi.Models;
using OrdersApi.Persistence;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Controllers
{
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly ILogger<OrdersController> logger;
        private readonly IOrdersRepository orderRepository;
        private readonly DaprClient daprClient;

        public OrdersController(ILogger<OrdersController> logger, IOrdersRepository orderRepository, DaprClient daprClient)
        {
            this.logger = logger;
            this.orderRepository = orderRepository;
            this.daprClient = daprClient;
        }

        [Route("OrderReceived")]
        [HttpPost]
        [Topic("eventbus", "OrderReceivedEvent")]
        public async Task<IActionResult> OrderReceived(OrderReceivedCommand command)
        {
            if (command?.OrderId != null && command?.PhotoUrl != null 
                && command?.UserEmail != null && command?.ImageData != null)
            {
                this.logger.LogInformation($"Cloud event {command.OrderId} {command.UserEmail}");
                Image img = Image.Load(command.ImageData);
                img.Save("dummy.jpg");

                var order = new Order()
                {
                    OrderId = command.OrderId,
                    ImageData = command.ImageData,
                    UserEmail = command.UserEmail,
                    PhotoUrl = command.PhotoUrl,
                    Status = Status.Registered,
                    OrderDetails = new List<OrderDetail>()
                };

                var orderExists = await this.orderRepository.GetOrderAsync(order.OrderId);
                if (orderExists == null)
                {
                    await this.orderRepository.RegisterOrder(order);
                    var ore = new OrderRegistrationEvent()
                    {
                        OrderId = order.OrderId,
                        UserEmail = order.UserEmail,
                        ImageData = order.ImageData
                    };

                    await this.daprClient.PublishEventAsync("eventbus", "OrderRegisteredEvent", ore);
                    this.logger.LogInformation($"For {order.OrderId}", "OrderRegisteredEvent published");
                }

                return Ok();
            }

            return BadRequest();
        }

        [Route("orderprocessed")]
        [HttpPost()]
        [Topic("eventbus", "OrderProcessedEvent")]
        public async Task<IActionResult> OrderProcessed(OrderStatusChangedToProcessCommand command)
        {
            this.logger.LogInformation("OrderProcessed method entered");
            if (ModelState.IsValid)
            {
                Order order = await this.orderRepository.GetOrderAsync(command.OrderId);
                if (order != null)
                {
                    order.Status = Status.Processed;
                    int j = 0;
                    foreach (var face in command.Faces)
                    {
                        Image img = Image.Load(face);
                        img.Save("face" + j + ".jpg");
                        j++;

                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            FaceData = face
                        };
                        order.OrderDetails.Add(orderDetail);
                    }

                    await this.orderRepository.UpdateOrder(order);                    
                }

                return Ok();
            }
            else
                return BadRequest();
        }

        [Route("orderdispatched")]
        [HttpPost()]
        [Topic("eventbus", "OrderDispatchedEvent")]
        public async Task<IActionResult> OrderDispatched(OrderStatusChangedToDispatched model)
        {
            if (ModelState.IsValid)
            {
                this.logger.LogInformation("Order dispatched message received: " + model.OrderId);
                Order order = await this.orderRepository.GetOrderAsync(model.OrderId);
                if (order != null)
                {
                    order.Status = Status.Dispatched;
                    await this.orderRepository.UpdateOrder(order);
                }

                return Ok();
            }
            else
                return BadRequest();
        }
    }
}
