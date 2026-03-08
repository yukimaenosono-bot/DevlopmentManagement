using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Inventory.Commands;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Tests.TestHelpers;
using Xunit;

namespace Synapse.Tests.Application.Inventory.Commands;

public class IssueInventoryCommandHandlerTests
{
    private static readonly Guid ValidItemId = Guid.NewGuid();
    private static readonly Guid ValidWarehouseId = Guid.NewGuid();

    private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> data) where T : class
    {
        var mock = new Mock<DbSet<T>>();
        var queryable = data.AsQueryable();
        mock.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mock.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return mock;
    }

    private static Stock BuildStock(decimal quantity)
    {
        var stock = Stock.Create(ValidItemId, ValidWarehouseId, null);
        stock.Receive(quantity);
        return stock;
    }

    private static Mock<IApplicationDbContext> BuildContext(List<Stock> stocks, List<InventoryTransaction> transactions)
    {
        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Stocks).Returns(CreateDbSetMock(stocks).Object);

        var txMock = CreateDbSetMock(transactions);
        txMock.Setup(m => m.Add(It.IsAny<InventoryTransaction>())).Callback<InventoryTransaction>(t => transactions.Add(t));
        mockContext.Setup(c => c.InventoryTransactions).Returns(txMock.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mockContext;
    }

    [Fact]
    public async Task Handle_在庫十分の場合_出庫して履歴を記録する()
    {
        // Arrange
        var stock = BuildStock(100m);
        var stocks = new List<Stock> { stock };
        var transactions = new List<InventoryTransaction>();
        var mockContext = BuildContext(stocks, transactions);

        var handler = new IssueInventoryCommandHandler(mockContext.Object);
        var command = new IssueInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            30m, "MO-20260308-0001", null, "user-001");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(70m, stocks[0].Quantity); // 100 - 30
        Assert.Single(transactions);
        Assert.Equal(InventoryTransactionType.Issue, transactions[0].TransactionType);
        Assert.Equal(30m, transactions[0].Quantity);
        Assert.Equal("MO-20260308-0001", transactions[0].ReferenceNumber);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_在庫不足の場合_InvalidOperationExceptionをスローする()
    {
        // Arrange
        var stock = BuildStock(10m); // 10枚しかない
        var stocks = new List<Stock> { stock };
        var transactions = new List<InventoryTransaction>();
        var mockContext = BuildContext(stocks, transactions);

        var handler = new IssueInventoryCommandHandler(mockContext.Object);
        var command = new IssueInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            50m, null, null, "user-001"); // 50枚出庫しようとする

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(10m, stocks[0].Quantity); // 在庫は変わっていない
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_在庫行なしの場合_InvalidOperationExceptionをスローする()
    {
        // Arrange
        var stocks = new List<Stock>(); // 在庫行なし
        var transactions = new List<InventoryTransaction>();
        var mockContext = BuildContext(stocks, transactions);

        var handler = new IssueInventoryCommandHandler(mockContext.Object);
        var command = new IssueInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            10m, null, null, "user-001");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
