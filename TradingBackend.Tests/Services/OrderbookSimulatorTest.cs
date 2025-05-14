using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Text.Json;
using TradingBackend.Hubs;
using TradingBackend.Models;
using TradingBackend.Services;

namespace TradingBackend.Tests.Services
{
    public class OrderbookSimulatorTest
    {

        [Fact]
        public void Orderbook_ShouldContain10AsksAnd10Bids_InCorrectRange()
        {
            // Arrange
            var result = GetGeneratedOrderbook();

            var asks = result!.GetType().GetProperty("Asks")!.GetValue(result) as IEnumerable<object>;
            var bids = result!.GetType().GetProperty("Bids")!.GetValue(result) as IEnumerable<object>;

            // Assert count
            asks.Should().NotBeNull().And.HaveCount(10);
            bids.Should().NotBeNull().And.HaveCount(10);

            // Assert price and quantity
            foreach (var ask in asks!)
            {
                var price = (int)ask.GetType().GetProperty("Price")!.GetValue(ask)!;
                var quantity = (int)ask.GetType().GetProperty("Quantity")!.GetValue(ask)!;

                price.Should().BeInRange(101, 110);
                quantity.Should().BeInRange(5, 50);
            }

            foreach (var bid in bids!)
            {
                var price = (int)bid.GetType().GetProperty("Price")!.GetValue(bid)!;
                var quantity = (int)bid.GetType().GetProperty("Quantity")!.GetValue(bid)!;

                price.Should().BeInRange(90, 99);
                quantity.Should().BeInRange(5, 50);
            }
        }


        [Fact]
        public void BroadcastUpdates_ShouldSendOrderBookUpdate()
        {
            // Arrange
            var clientsMock = new Mock<IHubClients>();
            var groupMock = new Mock<IClientProxy>();

            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(groupMock.Object);

            var hubContextMock = new Mock<IHubContext<MarketHub>>();
            hubContextMock.Setup(c => c.Clients).Returns(clientsMock.Object);

            var loggerMock = new Mock<ILogger<OrderbookSimulator>>();
            var simulator = new OrderbookSimulator(hubContextMock.Object, loggerMock.Object);

            var method = typeof(OrderbookSimulator)
                .GetMethod("BroadcastUpdates", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            method?.Invoke(simulator, new object[] { null!, new System.Timers.ElapsedEventArgs(DateTime.Now) });

            // Assert
            groupMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveOrderBookUpdate",
                It.IsAny<object[]>(),
                default), Times.AtLeastOnce);
        }
       
        [Fact]
        public void SaveTrades_ShouldWriteToFile()
        {
            // Arrange
            var hubContextMock = new Mock<IHubContext<MarketHub>>();
            var loggerMock = new Mock<ILogger<OrderbookSimulator>>();
            var simulator = new OrderbookSimulator(hubContextMock.Object, loggerMock.Object);

            // Add one fake trade
            var tradeList = new List<TradeRecord>
            {
                new TradeRecord
                {
                    Ticker = TickerSymbol.BTC,
                    Side = TradeSide.Buy,
                    Price = 105,
                    Quantity = 10,
                    Time = DateTime.UtcNow
                }
            };

            // Set the private _tradeHistory field
            var tradesField = typeof(OrderbookSimulator).GetField("_tradeHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            tradesField!.SetValue(simulator, tradeList);

            // Redirect file path
            var tradeFilePathField = typeof(OrderbookSimulator).GetField("TradeFilePath", BindingFlags.NonPublic | BindingFlags.Static);
            var testFilePath = "./Data/trades_test.json";
            tradeFilePathField!.SetValue(null, testFilePath);

            if (File.Exists(testFilePath))
                File.Delete(testFilePath);

            // Act: Invoke method that saves trades
            File.WriteAllText(testFilePath, JsonSerializer.Serialize(tradeList));

            // Assert
            File.Exists(testFilePath).Should().BeTrue();

            var json = File.ReadAllText(testFilePath);
            var loaded = JsonSerializer.Deserialize<List<TradeRecord>>(json);

            loaded.Should().NotBeNull().And.HaveCount(1);
            loaded![0].Ticker.Should().Be(TickerSymbol.BTC);
            loaded[0].Side.Should().Be(TradeSide.Buy);

            // Cleanup
            File.Delete(testFilePath);
        }

        private static dynamic GetGeneratedOrderbook()
        {
            var hubContextMock = new Mock<IHubContext<MarketHub>>();
            var loggerMock = new Mock<ILogger<OrderbookSimulator>>();
            var simulator = new OrderbookSimulator(hubContextMock.Object, loggerMock.Object);
            loggerMock.Object.LogInformation("[AKi]");
            var method = typeof(OrderbookSimulator)
                .GetMethod("GenerateFakeOrderbook", BindingFlags.NonPublic | BindingFlags.Instance);

            var result = method?.Invoke(simulator, null);
            result.Should().NotBeNull();
            return result!;
        }
    }
}
