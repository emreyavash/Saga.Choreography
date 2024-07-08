using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        readonly MongoDBService _mongoDBService;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;
        public OrderCreatedEventConsumer(MongoDBService mongoDBService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _mongoDBService = mongoDBService;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            IMongoCollection<Models.Stock> collection= _mongoDBService.GetCollection<Models.Stock>();
            foreach (var orderItem in context.Message.OrderItems)
            {
             stockResult.Add( await  (await collection.FindAsync(s => s.ProductId == orderItem.ProductId.ToString() && s.Count > (long)orderItem.Count)).AnyAsync());
            }
            if (stockResult.TrueForAll(s => s.Equals(true)))
            {
                //Stock Güncellemesi
                foreach (var item in context.Message.OrderItems)
                {
                  Models.Stock stock=  await (await collection.FindAsync(s => s.ProductId == item.ProductId.ToString())).FirstOrDefaultAsync();
                    stock.Count -= item.Count;
                    await collection.FindOneAndReplaceAsync(x => x.ProductId == item.ProductId.ToString(), stock);
                }
                //paymentı uayaracak event fırlatılması
               var sendEnpoint= await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
                StockReservedEvent stockReservedEvent = new StockReservedEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice,
                    OrderItems = context.Message.OrderItems,
                };
                await sendEnpoint.Send(stockReservedEvent);
            }
            else
            {
                //stok işlemi başarısız.
                //orderı uyaracak event fırlatılır.
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Messagess = "Stok miktarı yetersiz"
                };
                await _publishEndpoint.Publish(stockNotReservedEvent);
            }
        }
    }
}
