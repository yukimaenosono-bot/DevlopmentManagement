using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Equipments.Commands;
using Synapse.Domain.Common;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;
using Synapse.Tests.TestHelpers;

namespace Synapse.Tests.Application.Equipments.Commands;

public class CreateEquipmentCommandHandlerTests
{
    private static readonly Guid ValidProcessId = Guid.NewGuid();

    // テストごとに独立したモックを作成するヘルパー
    private static (CreateEquipmentCommandHandler handler, Mock<IApplicationDbContext> context, Mock<DbSet<Equipment>> dbSet)
        CreateSut(IEnumerable<Equipment> existingEquipments, bool processExists = true)
    {
        var equipData = existingEquipments.AsQueryable();

        var mockEquipDbSet = new Mock<DbSet<Equipment>>();
        mockEquipDbSet.As<IQueryable<Equipment>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Equipment>(equipData.Provider));
        mockEquipDbSet.As<IQueryable<Equipment>>().Setup(m => m.Expression).Returns(equipData.Expression);
        mockEquipDbSet.As<IQueryable<Equipment>>().Setup(m => m.ElementType).Returns(equipData.ElementType);
        mockEquipDbSet.As<IQueryable<Equipment>>().Setup(m => m.GetEnumerator()).Returns(() => equipData.GetEnumerator());
        mockEquipDbSet.As<IAsyncEnumerable<Equipment>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Equipment>(equipData.GetEnumerator()));

        // 工程マスタのモック（存在チェック用）
        // Process.Create() は Id を内部生成するため、リフレクションで ValidProcessId に揃える
        Process[]? processes = null;
        if (processExists)
        {
            var process = Process.Create("PROC-001", "旋盤加工", ProcessType.Machining);
            typeof(Entity).GetProperty("Id")!.SetValue(process, ValidProcessId);
            processes = [process];
        }
        var processData = (processExists ? processes! : []).AsQueryable();

        var mockProcessDbSet = new Mock<DbSet<Process>>();
        mockProcessDbSet.As<IQueryable<Process>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Process>(processData.Provider));
        mockProcessDbSet.As<IQueryable<Process>>().Setup(m => m.Expression).Returns(processData.Expression);
        mockProcessDbSet.As<IQueryable<Process>>().Setup(m => m.ElementType).Returns(processData.ElementType);
        mockProcessDbSet.As<IQueryable<Process>>().Setup(m => m.GetEnumerator()).Returns(() => processData.GetEnumerator());
        mockProcessDbSet.As<IAsyncEnumerable<Process>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Process>(processData.GetEnumerator()));

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Equipments).Returns(mockEquipDbSet.Object);
        mockContext.Setup(c => c.Processes).Returns(mockProcessDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateEquipmentCommandHandler(mockContext.Object);
        return (handler, mockContext, mockEquipDbSet);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsNewGuid()
    {
        // Arrange
        var (handler, mockContext, mockDbSet) = CreateSut([]);

        var command = new CreateEquipmentCommand(
            Code: "EQ-001",
            Name: "旋盤A",
            ProcessId: ValidProcessId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        mockDbSet.Verify(d => d.Add(It.Is<Equipment>(e => e.Code == "EQ-001" && e.IsActive)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange: 同じコードの設備が既に存在する状態
        var existing = new[] { Equipment.Create("EQ-001", "旋盤A", ValidProcessId) };
        var (handler, _, _) = CreateSut(existing);

        var command = new CreateEquipmentCommand("EQ-001", "旋盤B", ValidProcessId);

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EQ-001*");
    }

    [Fact]
    public async Task Handle_NonExistentProcess_ThrowsNotFoundException()
    {
        // Arrange: 指定した工程が存在しない状態
        var (handler, _, _) = CreateSut([], processExists: false);

        var command = new CreateEquipmentCommand("EQ-002", "組立台1", Guid.NewGuid());

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
