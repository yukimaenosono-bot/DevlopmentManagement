using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Processes.Commands;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Tests.TestHelpers;

namespace Synapse.Tests.Application.Processes.Commands;

public class CreateProcessCommandHandlerTests
{
    // テストごとに独立したモックを作成するヘルパー
    private static (CreateProcessCommandHandler handler, Mock<IApplicationDbContext> context, Mock<DbSet<Process>> dbSet)
        CreateSut(IEnumerable<Process> existingProcesses)
    {
        var data = existingProcesses.AsQueryable();

        // As<T>() は .Object より前に呼ぶ必要があるため、先に全インターフェースを登録する
        var mockDbSet = new Mock<DbSet<Process>>();
        mockDbSet.As<IQueryable<Process>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Process>(data.Provider));
        mockDbSet.As<IQueryable<Process>>().Setup(m => m.Expression).Returns(data.Expression);
        mockDbSet.As<IQueryable<Process>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockDbSet.As<IQueryable<Process>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Process>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Process>(data.GetEnumerator()));

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Processes).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateProcessCommandHandler(mockContext.Object);
        return (handler, mockContext, mockDbSet);
    }

    [Fact]
    public async Task Handle_NewCode_ReturnsNewGuid()
    {
        // Arrange
        var (handler, mockContext, mockDbSet) = CreateSut([]);

        var command = new CreateProcessCommand(
            Code: "PROC-001",
            Name: "旋盤加工",
            ProcessType: ProcessType.Machining);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        mockDbSet.Verify(d => d.Add(It.Is<Process>(p => p.Code == "PROC-001" && p.IsActive)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange: 同じコードの工程が既に存在する状態
        var existing = new[] { Process.Create("PROC-001", "既存工程", ProcessType.Machining) };
        var (handler, _, _) = CreateSut(existing);

        var command = new CreateProcessCommand("PROC-001", "重複工程", ProcessType.Assembly);

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PROC-001*");
    }
}
