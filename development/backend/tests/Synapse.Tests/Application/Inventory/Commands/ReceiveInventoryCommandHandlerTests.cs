using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Inventory.Commands;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Tests.TestHelpers;
using Xunit;

namespace Synapse.Tests.Application.Inventory.Commands;

public class ReceiveInventoryCommandHandlerTests
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

    private static Mock<IApplicationDbContext> BuildContext(
        List<Item> items,
        List<Warehouse> warehouses,
        List<Stock> stocks,
        List<InventoryTransaction> transactions)
    {
        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Items).Returns(CreateDbSetMock(items).Object);
        mockContext.Setup(c => c.Warehouses).Returns(CreateDbSetMock(warehouses).Object);

        var stockMock = CreateDbSetMock(stocks);
        stockMock.Setup(m => m.Add(It.IsAny<Stock>())).Callback<Stock>(s => stocks.Add(s));
        mockContext.Setup(c => c.Stocks).Returns(stockMock.Object);

        var txMock = CreateDbSetMock(transactions);
        txMock.Setup(m => m.Add(It.IsAny<InventoryTransaction>())).Callback<InventoryTransaction>(t => transactions.Add(t));
        mockContext.Setup(c => c.InventoryTransactions).Returns(txMock.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mockContext;
    }

    [Fact]
    public async Task Handle_在庫行なしの場合_新規作成して入庫を記録する()
    {
        // Arrange
        var item = Item.Create("MAT-001", "鉄板", null, ItemType.RawMaterial, "枚", 100m, 10m, false, false);
        typeof(Synapse.Domain.Common.Entity).GetProperty("Id")!.SetValue(item, ValidItemId);
        var items = new List<Item> { item };

        var warehouse = Warehouse.Create("WH-001", "原材料倉庫", WarehouseType.RawMaterial);
        typeof(Synapse.Domain.Common.Entity).GetProperty("Id")!.SetValue(warehouse, ValidWarehouseId);
        var warehouses = new List<Warehouse> { warehouse };

        var stocks = new List<Stock>();
        var transactions = new List<InventoryTransaction>();
        var mockContext = BuildContext(items, warehouses, stocks, transactions);

        var handler = new ReceiveInventoryCommandHandler(mockContext.Object);
        var command = new ReceiveInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            InventoryTransactionType.PurchaseReceipt, 100m,
            "PO-2026-001", null, "user-001");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(stocks);
        Assert.Equal(100m, stocks[0].Quantity);
        Assert.Single(transactions);
        Assert.Equal(InventoryTransactionType.PurchaseReceipt, transactions[0].TransactionType);
        Assert.Equal(100m, transactions[0].Quantity);
        Assert.Equal("PO-2026-001", transactions[0].ReferenceNumber);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_在庫行ありの場合_既存在庫に加算する()
    {
        // Arrange
        var item = Item.Create("MAT-001", "鉄板", null, ItemType.RawMaterial, "枚", 100m, 10m, false, false);
        typeof(Synapse.Domain.Common.Entity).GetProperty("Id")!.SetValue(item, ValidItemId);
        var items = new List<Item> { item };

        var warehouse = Warehouse.Create("WH-001", "原材料倉庫", WarehouseType.RawMaterial);
        typeof(Synapse.Domain.Common.Entity).GetProperty("Id")!.SetValue(warehouse, ValidWarehouseId);
        var warehouses = new List<Warehouse> { warehouse };

        // 既存在庫行（50枚）
        var existingStock = Stock.Create(ValidItemId, ValidWarehouseId, null);
        existingStock.Receive(50m);
        var stocks = new List<Stock> { existingStock };
        var transactions = new List<InventoryTransaction>();
        var mockContext = BuildContext(items, warehouses, stocks, transactions);

        var handler = new ReceiveInventoryCommandHandler(mockContext.Object);
        var command = new ReceiveInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            InventoryTransactionType.PurchaseReceipt, 30m,
            null, null, "user-001");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(stocks); // 新規作成されていない
        Assert.Equal(80m, stocks[0].Quantity); // 50 + 30
        Assert.Single(transactions);
    }

    [Fact]
    public async Task Handle_存在しない品目の場合_NotFoundExceptionをスローする()
    {
        // Arrange
        var items = new List<Item>(); // 品目なし
        var warehouse = Warehouse.Create("WH-001", "原材料倉庫", WarehouseType.RawMaterial);
        typeof(Synapse.Domain.Common.Entity).GetProperty("Id")!.SetValue(warehouse, ValidWarehouseId);
        var warehouses = new List<Warehouse> { warehouse };
        var mockContext = BuildContext(items, warehouses, new List<Stock>(), new List<InventoryTransaction>());

        var handler = new ReceiveInventoryCommandHandler(mockContext.Object);
        var command = new ReceiveInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            InventoryTransactionType.PurchaseReceipt, 100m,
            null, null, "user-001");

        // Act & Assert
        await Assert.ThrowsAsync<Synapse.Domain.Exceptions.NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_出庫区分を指定した場合_ArgumentExceptionをスローする()
    {
        // Arrange
        var items = new List<Item>();
        var warehouses = new List<Warehouse>();
        var mockContext = BuildContext(items, warehouses, new List<Stock>(), new List<InventoryTransaction>());

        var handler = new ReceiveInventoryCommandHandler(mockContext.Object);
        var command = new ReceiveInventoryCommand(
            ValidItemId, ValidWarehouseId, null,
            InventoryTransactionType.Issue, 100m, // 入庫コマンドに Issue は不可
            null, null, "user-001");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
