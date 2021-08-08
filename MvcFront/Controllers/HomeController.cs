using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MvcFront.Events;
using MvcFront.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvcFront.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DaprClient _daprClient;

        public HomeController(ILogger<HomeController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpGet]
        public IActionResult UploadData()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadData(UploadDataCommand model)
        {
            MemoryStream ms = new MemoryStream();
            using (var uploadedFile = model.File.OpenReadStream())
            {
                await uploadedFile.CopyToAsync(ms);
            }
            var imageData = ms.ToArray();
            model.PhotoUrl = model.File.FileName;
            model.OrderId = Guid.NewGuid();

            var eventData = new OrderReceivedEvent(model.OrderId, model.PhotoUrl, model.Email, imageData);

            try
            {
                await _daprClient.PublishEventAsync("eventbus", "OrderReceivedEvent", eventData);
                _logger.LogInformation("Published event: OrderReceivedEvent {orderId}", model.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event: OrderReceivedEvent {orderId}", model.OrderId);
                throw;
            }

            ViewData["OrderId"] = model.OrderId;
            return View("Thanks");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
