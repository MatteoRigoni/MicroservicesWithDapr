using Microsoft.EntityFrameworkCore;
using OrdersApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Persistence
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly OrderContext context;

        public OrdersRepository(OrderContext context)
        {
            this.context = context;
        }
        public async Task<Order> GetOrderAsync(Guid id)
        {
            return await this.context.Orders
                .Include("OrderDetails")
                .FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task RegisterOrder(Order order)
        {
            this.context.Orders.Add(order);
            await this.context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            this.context.Entry(order).State = EntityState.Modified;
            this.context.Update(order);
            await this.context.SaveChangesAsync();
        }
    }
}
