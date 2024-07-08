﻿using MassTransit;
using Order.API.Models.Context;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        readonly OrderAPIDbContext _context;

        public PaymentCompletedEventConsumer(OrderAPIDbContext context1)
        {
            _context = context1;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order =await _context.Orders.FindAsync(context.Message.OrderId);
            if(order == null)
                throw new NullReferenceException();
            order.OrderStatus = Enums.OrderStatus.Completed;
            await _context.SaveChangesAsync();
        }
    }
}
