using FluentAssertions;
using Moq;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Users.Commands;

namespace Synapse.Tests.Application.Users.Commands;

public class CreateUserCommandHandlerTests
{
    private static (CreateUserCommandHandler handler, Mock<IUserManagementService> service)
        CreateSut()
    {
        var mockService = new Mock<IUserManagementService>();
        return (new CreateUserCommandHandler(mockService.Object), mockService);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUserId()
    {
        // Arrange
        var (handler, mockService) = CreateSut();
        var expectedId = "user-123";

        mockService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedId, (string?)null));

        var command = new CreateUserCommand(
            UserName: "yamada",
            DisplayName: "山田 太郎",
            Password: "Password1",
            Roles: ["floor-worker"]);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedId);
        mockService.Verify(s => s.CreateUserAsync(
            "yamada", "山田 太郎", "Password1",
            It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IdentityError_ThrowsInvalidOperationException()
    {
        // Arrange: Identity のバリデーションエラー（パスワード強度不足等）
        var (handler, mockService) = CreateSut();

        mockService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string.Empty, "ユーザー名 'yamada' は既に使用されています。"));

        var command = new CreateUserCommand("yamada", "山田 太郎", "Password1", []);

        // Act & Assert
        await handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*yamada*");
    }
}
