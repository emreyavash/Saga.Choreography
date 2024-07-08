using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        readonly MongoDBService _mondoDBService;

        public PaymentFailedEventConsumer(MongoDBService mondoDBService)
        {
            _mondoDBService = mondoDBService;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var stocks = _mondoDBService.GetCollection<Models.Stock>();
            foreach (var orderItem in context.Message.OrderItems)
            {
                var product = await (await stocks.FindAsync(x=>x.ProductId==orderItem.ProductId.ToString())).FirstOrDefaultAsync();
                if(product != null)
                {
                    product.Count += orderItem.Count;
                    await stocks.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId.ToString(), product);
                    
                }
            }
        }
    }
}
