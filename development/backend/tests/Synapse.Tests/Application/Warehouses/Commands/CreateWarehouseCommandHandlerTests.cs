using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Warehouses.Commands;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Tests.TestHelpers;
using Xunit;

namespace Synapse.Tests.Application.Warehouses.Commands;

public class CreateWarehouseCommandHandlerTests
{
    private static Mock<DbSet<Warehouse>> CreateDbSetMock(List<Warehouse> data)
    {
        var mock = new Mock<DbSet<Warehouse>>();
        var queryable = data.AsQueryable();
        mock.As<IAsyncEnumerable<Warehouse>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Warehouse>(data.GetEnumerator()));
        mock.As<IQueryable<Warehouse>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Warehouse>(queryable.Provider));
        mock.As<IQueryable<Warehouse>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<Warehouse>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<Warehouse>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return mock;
    }

    [Fact]
    public async Task Handle_新規コードの場合_倉庫を作成してIDを返す()
    {
        // Arrange
        var warehouses = new List<Warehouse>();
        var mockDbSet = CreateDbSetMock(warehouses);
        mockDbSet.Setup(m => m.Add(It.IsAny<Warehouse>()))
            .Callback<Warehouse>(w => warehouses.Add(w));

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Warehouses).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateWarehouseCommandHandler(mockContext.Object);
        var command = new CreateWarehouseCommand("WH-001", "原材料倉庫", WarehouseType.RawMaterial);

        // Act
        var id = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(warehouses);
        Assert.Equal("WH-001", warehouses[0].Code);
        Assert.Equal("原材料倉庫", warehouses[0].Name);
        Assert.Equal(WarehouseType.RawMaterial, warehouses[0].WarehouseType);
        Assert.True(warehouses[0].IsActive);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_コード重複の場合_InvalidOperationExceptionをスローする()
    {
        // Arrange
        var existing = Warehouse.Create("WH-001", "既存倉庫", WarehouseType.RawMaterial);
        var warehouses = new List<Warehouse> { existing };
        var mockDbSet = CreateDbSetMock(warehouses);

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Warehouses).Returns(mockDbSet.Object);

        var handler = new CreateWarehouseCommandHandler(mockContext.Object);
        var command = new CreateWarehouseCommand("WH-001", "別倉庫", WarehouseType.FinishedGoods);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
