using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Synapse.Application.Bom.Commands;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Common;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;
using Synapse.Tests.TestHelpers;

namespace Synapse.Tests.Application.Bom.Commands;

public class AddBomLineCommandHandlerTests
{
    private static readonly Guid ParentItemId = Guid.NewGuid();
    private static readonly Guid ChildItemId = Guid.NewGuid();

    private static (AddBomLineCommandHandler handler, Mock<IApplicationDbContext> context, Mock<DbSet<BomLine>> dbSet)
        CreateSut(bool parentExists = true, bool childExists = true, bool lineAlreadyExists = false)
    {
        // 品目モック
        var items = new List<Item>();
        if (parentExists) items.Add(CreateItem(ParentItemId, "PAR-001"));
        if (childExists) items.Add(CreateItem(ChildItemId, "CHD-001"));
        var itemData = items.AsQueryable();
        var mockItemDbSet = BuildMockDbSet(itemData);

        // BOM ラインモック
        var bomLines = lineAlreadyExists
            ? new[] { BomLine.Create(ParentItemId, ChildItemId, 1m, "個",
                new DateOnly(2026, 1, 1), null) }.AsQueryable()
            : Enumerable.Empty<BomLine>().AsQueryable();
        var mockBomDbSet = BuildMockDbSet(bomLines);

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Items).Returns(mockItemDbSet.Object);
        mockContext.Setup(c => c.BomLines).Returns(mockBomDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new AddBomLineCommandHandler(mockContext.Object);
        return (handler, mockContext, mockBomDbSet);
    }

    [Fact]
    public async Task Handle_ValidLine_AddsAndSaves()
    {
        // Arrange
        var (handler, mockContext, mockBomDbSet) = CreateSut();
        var command = new AddBomLineCommand(
            ParentItemId, ChildItemId, 2m, "個",
            new DateOnly(2026, 1, 1), null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        mockBomDbSet.Verify(d => d.Add(It.Is<BomLine>(b =>
            b.ParentItemId == ParentItemId &&
            b.ChildItemId == ChildItemId &&
            b.Quantity == 2m)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ParentNotFound_ThrowsNotFoundException()
    {
        var (handler, _, _) = CreateSut(parentExists: false);
        var command = new AddBomLineCommand(ParentItemId, ChildItemId, 1m, "個",
            new DateOnly(2026, 1, 1), null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DuplicateLine_ThrowsInvalidOperationException()
    {
        var (handler, _, _) = CreateSut(lineAlreadyExists: true);
        var command = new AddBomLineCommand(ParentItemId, ChildItemId, 1m, "個",
            new DateOnly(2026, 1, 1), null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ── ドメインロジックテスト（DB 不要）──

    [Fact]
    public void BomLine_SameParentAndChild_ThrowsArgumentException()
    {
        // 自己参照は循環参照の第一歩なので禁止
        var sameId = Guid.NewGuid();
        FluentActions.Invoking(() =>
                BomLine.Create(sameId, sameId, 1m, "個", new DateOnly(2026, 1, 1), null))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BomLine_InvalidDateRange_ThrowsArgumentException()
    {
        FluentActions.Invoking(() =>
                BomLine.Create(ParentItemId, ChildItemId, 1m, "個",
                    new DateOnly(2026, 3, 1), new DateOnly(2026, 1, 1)))  // 終了 < 開始
            .Should().Throw<ArgumentException>();
    }

    // ── ヘルパー ──

    private static Item CreateItem(Guid id, string code)
    {
        var item = Item.Create(code, "テスト品目", null, ItemType.Product, "個", 0m, 0m, false, false);
        typeof(Entity).GetProperty("Id")!.SetValue(item, id);
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
