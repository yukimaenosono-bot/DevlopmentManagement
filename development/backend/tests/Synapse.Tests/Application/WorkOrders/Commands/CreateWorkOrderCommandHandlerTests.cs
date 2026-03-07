using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.WorkOrders.Commands;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;
using Synapse.Tests.TestHelpers;

namespace Synapse.Tests.Application.WorkOrders.Commands;

public class CreateWorkOrderCommandHandlerTests
{
    private static readonly Guid ValidItemId = Guid.NewGuid();

    private static (CreateWorkOrderCommandHandler handler, Mock<IApplicationDbContext> context, Mock<DbSet<WorkOrder>> dbSet)
        CreateSut(bool itemExists = true, int existingWorkOrderCount = 0)
    {
        // 品目マスタのモック
        var items = itemExists
            ? new[] { CreateItem(ValidItemId) }.AsQueryable()
            : Enumerable.Empty<Item>().AsQueryable();

        var mockItemDbSet = BuildMockDbSet(items);

        // 製造指示のモック（採番用カウント）
        var workOrders = Enumerable.Range(0, existingWorkOrderCount)
            .Select(_ => WorkOrder.Create(
                $"MO-20260308-{_ + 1:0000}", null, ValidItemId, null,
                10m, new DateOnly(2026, 3, 8), new DateOnly(2026, 3, 15), null, "user-1"))
            .ToArray().AsQueryable();

        var mockWorkOrderDbSet = BuildMockDbSet(workOrders);

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Items).Returns(mockItemDbSet.Object);
        mockContext.Setup(c => c.WorkOrders).Returns(mockWorkOrderDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateWorkOrderCommandHandler(mockContext.Object);
        return (handler, mockContext, mockWorkOrderDbSet);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsNewGuid()
    {
        // Arrange
        var (handler, mockContext, mockDbSet) = CreateSut();
        var command = new CreateWorkOrderCommand(
            ProductionPlanNumber: null,
            ItemId: ValidItemId,
            LotNumber: "LOT-001",
            Quantity: 100m,
            PlannedStartDate: new DateOnly(2026, 3, 10),
            PlannedEndDate: new DateOnly(2026, 3, 17),
            WorkInstruction: "慎重に作業すること",
            CreatedByUserId: "user-1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        mockDbSet.Verify(d => d.Add(It.Is<WorkOrder>(w =>
            w.LotNumber == "LOT-001" &&
            w.Quantity == 100m &&
            w.Status == WorkOrderStatus.Issued)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentItem_ThrowsNotFoundException()
    {
        // Arrange: 存在しない品目を指定
        var (handler, _, _) = CreateSut(itemExists: false);
        var command = new CreateWorkOrderCommand(
            null, Guid.NewGuid(), null, 10m,
            new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 17), null, "user-1");

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_InvalidQuantity_ThrowsArgumentException()
    {
        // Arrange: 数量0は業務上不正
        var (handler, _, _) = CreateSut();
        var command = new CreateWorkOrderCommand(
            null, ValidItemId, null, 0m,
            new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 17), null, "user-1");

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();
    }

    // ドメインロジックのテスト（DB不要）
    [Fact]
    public void WorkOrder_Cancel_CompletedOrder_ThrowsInvalidOperationException()
    {
        // Arrange: 完了済み製造指示
        var workOrder = WorkOrder.Create(
            "MO-20260308-0001", null, ValidItemId, null,
            10m, new DateOnly(2026, 3, 8), new DateOnly(2026, 3, 15), null, "user-1");
        workOrder.Start();
        workOrder.Complete();

        // Act & Assert: 完了済みへのキャンセルは不可
        workOrder.Invoking(w => w.Cancel())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WorkOrder_Update_InProgressOrder_CannotChangeQuantity()
    {
        // Arrange: 着手中の製造指示
        var workOrder = WorkOrder.Create(
            "MO-20260308-0001", null, ValidItemId, null,
            10m, new DateOnly(2026, 3, 8), new DateOnly(2026, 3, 15), null, "user-1");
        workOrder.Start();

        // Act & Assert: 着手中は数量変更不可
        workOrder.Invoking(w => w.Update(
                20m,  // 数量を変更しようとする
                new DateOnly(2026, 3, 8), new DateOnly(2026, 3, 15), null))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*数量*");
    }

    // ── ヘルパーメソッド ──

    private static Item CreateItem(Guid id)
    {
        var item = Item.Create("ITEM-001", "テスト品目", null, ItemType.Product, "個", 1000m, 10m, false, false);
        typeof(Synapse.Domain.Common.Entity).GetProperty("Id")!.SetValue(item, id);
        return item;
    }

    private static Mock<DbSet<T>> BuildMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mock = new Mock<DbSet<T>>();
        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mock.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        return mock;
    }
}
