using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.WorkOrders.Commands;
using Synapse.Domain.Common;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;
using Synapse.Tests.TestHelpers;

namespace Synapse.Tests.Application.WorkOrders.Commands;

public class StartCompleteWorkOrderCommandHandlerTests
{
    private static readonly Guid ItemId = Guid.NewGuid();

    // 指定したステータスの製造指示を含むモックを構築する
    private static (Mock<IApplicationDbContext> context, Mock<DbSet<WorkOrder>> dbSet, WorkOrder workOrder)
        CreateContextWithWorkOrder(WorkOrderStatus targetStatus)
    {
        var workOrder = WorkOrder.Create(
            "MO-20260308-0001", null, ItemId, null,
            10m, new DateOnly(2026, 3, 8), new DateOnly(2026, 3, 15), null, "user-1");

        // 指定ステータスになるよう遷移させる
        if (targetStatus == WorkOrderStatus.InProgress || targetStatus == WorkOrderStatus.Completed)
            workOrder.Start();
        if (targetStatus == WorkOrderStatus.Completed)
            workOrder.Complete();
        if (targetStatus == WorkOrderStatus.Cancelled)
            workOrder.Cancel();

        var data = new[] { workOrder }.AsQueryable();
        var mockDbSet = new Mock<DbSet<WorkOrder>>();
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<WorkOrder>(data.Provider));
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.Expression).Returns(data.Expression);
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<WorkOrder>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<WorkOrder>(data.GetEnumerator()));

        // ハンドラー内の FirstOrDefaultAsync が workOrder を返すよう Id を揃える
        var targetId = workOrder.Id;

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.WorkOrders).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (mockContext, mockDbSet, workOrder);
    }

    // ── Start ──

    [Fact]
    public async Task Start_IssuedWorkOrder_ChangesStatusToInProgress()
    {
        // Arrange
        var (mockContext, _, workOrder) = CreateContextWithWorkOrder(WorkOrderStatus.Issued);
        var handler = new StartWorkOrderCommandHandler(mockContext.Object);

        // Act
        await handler.Handle(new StartWorkOrderCommand(workOrder.Id), CancellationToken.None);

        // Assert
        workOrder.Status.Should().Be(WorkOrderStatus.InProgress);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Start_NonExistentWorkOrder_ThrowsNotFoundException()
    {
        // Arrange: 空のDbSet
        var emptyData = Enumerable.Empty<WorkOrder>().AsQueryable();
        var mockDbSet = new Mock<DbSet<WorkOrder>>();
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<WorkOrder>(emptyData.Provider));
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.Expression).Returns(emptyData.Expression);
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.ElementType).Returns(emptyData.ElementType);
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.GetEnumerator()).Returns(() => emptyData.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<WorkOrder>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<WorkOrder>(emptyData.GetEnumerator()));

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.WorkOrders).Returns(mockDbSet.Object);
        var handler = new StartWorkOrderCommandHandler(mockContext.Object);

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(new StartWorkOrderCommand(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ── Complete ──

    [Fact]
    public async Task Complete_InProgressWorkOrder_ChangesStatusToCompleted()
    {
        // Arrange
        var (mockContext, _, workOrder) = CreateContextWithWorkOrder(WorkOrderStatus.InProgress);
        var handler = new CompleteWorkOrderCommandHandler(mockContext.Object);

        // Act
        await handler.Handle(new CompleteWorkOrderCommand(workOrder.Id), CancellationToken.None);

        // Assert
        workOrder.Status.Should().Be(WorkOrderStatus.Completed);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Complete_IssuedWorkOrder_ThrowsInvalidOperationException()
    {
        // Arrange: 着手していない製造指示を完了しようとする
        var (mockContext, _, workOrder) = CreateContextWithWorkOrder(WorkOrderStatus.Issued);
        var handler = new CompleteWorkOrderCommandHandler(mockContext.Object);

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(new CompleteWorkOrderCommand(workOrder.Id), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*着手中*");
    }
}
