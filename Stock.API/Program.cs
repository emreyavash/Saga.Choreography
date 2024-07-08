using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});
builder.Services.AddSingleton<MongoDBService>();
var app = builder.Build();

using IServiceScope scope = app.Services.CreateScope();
MongoDBService mongoDBService=scope.ServiceProvider.GetService<MongoDBService>();
var stockCollection=mongoDBService.GetCollection<Stock.API.Models.Stock>();
if (!stockCollection.FindSync(x => true).Any())
{
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 100 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 101 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 102 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 103 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 104 });
}
app.Run();
