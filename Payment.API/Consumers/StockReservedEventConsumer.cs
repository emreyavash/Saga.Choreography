using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        readonly IPublishEndpoint _endpoint;

        public StockReservedEventConsumer(IPublishEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if(true)
            {
                //ödeme başarılı
                PaymentCompletedEvent paymentCompleted = new()
                {
                    OrderId = context.Message.OrderId,
                };
                await _endpoint.Publish(paymentCompleted);
                await Console.Out.WriteAsync("Ödeme başarılı");
            }
            else
            {
                //ödeme başarısız
                PaymentFailedEvent paymentFailed = new()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Yetersiz bakiye",
                    OrderItems = context.Message.OrderItems
                };
                await _endpoint.Publish(paymentFailed);
                await Console.Out.WriteAsync("Ödeme başarısız");

            }
        }
    }
}
