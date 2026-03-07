using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Items.Commands;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;

namespace Synapse.Tests.Application.Items.Commands;

public class CreateItemCommandHandlerTests
{
    // テストごとに独立したモックを作成するヘルパー
    private static (CreateItemCommandHandler handler, Mock<IApplicationDbContext> context, Mock<DbSet<Item>> dbSet)
        CreateSut(IEnumerable<Item> existingItems)
    {
        var data = existingItems.AsQueryable();

        // As<T>() は .Object より前に呼ぶ必要があるため、先に全インターフェースを登録する
        var mockDbSet = new Mock<DbSet<Item>>();
        mockDbSet.As<IQueryable<Item>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Item>(data.Provider));
        mockDbSet.As<IQueryable<Item>>().Setup(m => m.Expression).Returns(data.Expression);
        mockDbSet.As<IQueryable<Item>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockDbSet.As<IQueryable<Item>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Item>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Item>(data.GetEnumerator()));

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Items).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateItemCommandHandler(mockContext.Object);
        return (handler, mockContext, mockDbSet);
    }

    [Fact]
    public async Task Handle_NewCode_ReturnsNewGuid()
    {
        // Arrange
        var (handler, mockContext, mockDbSet) = CreateSut([]);

        var command = new CreateItemCommand(
            Code: "ITEM-001",
            Name: "テスト品目",
            ShortName: null,
            ItemType: ItemType.Product,
            Unit: "個",
            StandardUnitPrice: 1000m,
            SafetyStockQuantity: 10m,
            HasExpirationDate: false,
            IsLotManaged: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        mockDbSet.Verify(d => d.Add(It.Is<Item>(i => i.Code == "ITEM-001" && i.IsActive)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange: 同じコードの品目が既に存在する状態
        var existing = new[] { Item.Create("ITEM-001", "既存品目", null, ItemType.Product, "個", 500m, 5m, false, false) };
        var (handler, _, _) = CreateSut(existing);

        var command = new CreateItemCommand("ITEM-001", "重複品目", null, ItemType.Product, "個", 0m, 0m, false, false);

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ITEM-001*");
    }
}

// ── EF Core の非同期クエリをテストで使うためのヘルパー ──

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => _inner.CreateQuery(expression);
    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TElement>(expression);
    public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var result = Execute(expression);
        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(typeof(TResult).GetGenericArguments()[0])
            .Invoke(null, [result])!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
}
