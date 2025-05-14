using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TradingBackend.Hubs;
using TradingBackend.Models;

namespace TradingBackend.Tests.Hubs
{
    public class MarketHubTests
    {

        [Fact]
        public async Task SubscribeToTicker_ShouldAddClientToGroup()
        {
            // Arrange
            var groupManagerMock = new Mock<IGroupManager>();
            var clientsMock = new Mock<IHubCallerClients>();
            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns("conn1");

            groupManagerMock.Setup(g => g.AddToGroupAsync("conn1", "BTC", default)).Returns(Task.CompletedTask);
            var hub = new MarketHub
            {
                Context = contextMock.Object,
                Groups = groupManagerMock.Object
            };

            // Act
            await hub.SubscribeToTicker("BTC");

            // Assert
            groupManagerMock.Verify(g => g.AddToGroupAsync("conn1", "BTC", default), Times.Once);
        }

        [Fact]
        public async Task UnsubscribeFromTicker_ShouldRemoveClientFromGroup()
        {
            // Arrange
            var groupManagerMock = new Mock<IGroupManager>();
            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns("conn2");

            var hub = new MarketHub
            {
                Context = contextMock.Object,
                Groups = groupManagerMock.Object
            };

            // Act
            await hub.UnsubscribeFromTicker("ETH");

            // Assert
            groupManagerMock.Verify(g => g.RemoveFromGroupAsync("conn2", "ETH", default), Times.Once);
        }

        [Fact]
        public async Task SubmitTrade_ShouldAppendToFile()
        {
            // Arrange
            var hub = new MarketHub();
            var trade = new TradeRecord
            {
                Ticker = TickerSymbol.BTC,
                Side = TradeSide.Buy,
                Price = 105,
                Quantity = 5,
                Time = DateTime.UtcNow
            };

            var testPath = "./Data/trade_test.json";
            var pathField = typeof(MarketHub)
                .GetField("Path", BindingFlags.NonPublic | BindingFlags.Static);
            pathField!.SetValue(null, testPath);

            if (File.Exists(testPath)) File.Delete(testPath);

            // Act
            await hub.SubmitTrade(trade);

            // Assert
            File.Exists(testPath).Should().BeTrue();
            var data = JsonSerializer.Deserialize<List<TradeRecord>>(File.ReadAllText(testPath));
            data.Should().ContainSingle(t => t.Ticker == TickerSymbol.BTC);

            // Cleanup
            File.Delete(testPath);
        }


    }
}
