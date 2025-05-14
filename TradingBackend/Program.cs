using TradingBackend.Hubs;
using TradingBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddSingleton<OrderbookSimulator>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<OrderbookSimulator>());
builder.Services.AddHostedService<OrderbookSimulator>();

var app = builder.Build();


app.MapHub<MarketHub>("/market");

app.Run();
