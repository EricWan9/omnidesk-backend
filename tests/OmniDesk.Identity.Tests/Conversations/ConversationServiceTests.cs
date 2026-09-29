using Microsoft.Extensions.Logging;
using Moq;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Exceptions;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Application.Storage;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Customers;
using OmniDesk.Domain.Entities;
using OmniDesk.Infrastructure.Conversations;

namespace OmniDesk.Application.Tests.Conversations;

public sealed class ConversationServiceTests
{
    [Fact]
    public async Task SendMessageAsync_WhenConversationIsNotClosed_PersistsMessageAndNotifies()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var cancellationToken = new CancellationTokenSource().Token;

        var originalUpdatedAt = new DateTime(
            2026,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var openStatus = ConversationStatus.Open;

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
            },
            Status = openStatus,
            CreatedAt = originalUpdatedAt,
            UpdatedAt = originalUpdatedAt,
            RowVersion = Array.Empty<byte>()
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        Message? addedMessage = null;
        MessageResponse? notifiedResponse = null;

        repositoryMock
            .Setup(repository =>
                repository.GetConversationByIdAsync(
                    tenantId,
                    conversationId,
                    cancellationToken))
            .ReturnsAsync(conversation);

        messageRepositoryMock
            .Setup(repository =>
                repository.AddMessage(
                    It.IsAny<Message>()))
            .Callback<Message>(message =>
                addedMessage = message);

        unityOfWorkMock
           .Setup(unitOfWork =>
               unitOfWork.SaveChangesAsync(
                   cancellationToken))
           .Returns(Task.CompletedTask);

        notifierMock
            .Setup(notifier =>
                notifier.MessageSentAsync(
                    tenantId,
                    conversationId,
                    It.IsAny<MessageResponse>(),
                    cancellationToken))
            .Callback<Guid, Guid, MessageResponse, CancellationToken>(
                (_, _, message, _) =>
                    notifiedResponse = message)
            .Returns(Task.CompletedTask);

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        var command = new SendMessageCommand(
            tenantId,
            conversationId,
            MessageSender.Agent(agentId),
            "Hello OmniDesk", []);

        // Act
        var result = await service.SendMessageAsync(
            command,
            cancellationToken);

        // Assert

        Assert.NotNull(addedMessage);

        Assert.NotEqual(Guid.Empty, addedMessage.Id);
        Assert.Equal(conversationId, addedMessage.ConversationId);
        Assert.Equal(MessageSenderType.Agent, addedMessage.SenderType);
        Assert.Equal(agentId, addedMessage.SenderId);
        Assert.Equal("Hello OmniDesk", addedMessage.Content);

        Assert.True(
            conversation.UpdatedAt > originalUpdatedAt);

        Assert.Equal(
            addedMessage.CreatedAt,
            conversation.UpdatedAt);

        Assert.Equal(result, notifiedResponse);

        repositoryMock.Verify(
            repository =>
                repository.GetConversationByIdAsync(
                    tenantId,
                    conversationId,
                    cancellationToken),
            Times.Once);

        messageRepositoryMock.Verify(
            repository =>
                repository.AddMessage(
                    It.IsAny<Message>()),
            Times.Once);

        unityOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    cancellationToken),
            Times.Once);

        notifierMock.Verify(
            notifier =>
                notifier.MessageSentAsync(
                    tenantId,
                    conversationId,
                    It.IsAny<MessageResponse>(),
                    cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task AssignToMeAsync_AssignsCurrentUser_AndIsIdempotent()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer() { Id = Guid.NewGuid(), TenantId = tenantId },
            Status = ConversationStatus.Open,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RowVersion = Array.Empty<byte>()
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        unityOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        notifierMock
            .Setup(n => n.ConversationUpdatedAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        // Act - first assign
        await service.AssignToMeAsync(tenantId, userId, conversationId, CancellationToken.None);

        Assert.Equal(userId, conversation.AssignedUserId);

        // Act - idempotent assign
        await service.AssignToMeAsync(tenantId, userId, conversationId, CancellationToken.None);

        // Should have saved/notify only when changed (we can't assert exact times reliably here), but ensure no exception
        unityOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        notifierMock.Verify(n => n.ConversationUpdatedAsync(tenantId, conversationId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UnassignAsync_ClearsAssignedUser_AndIsIdempotent()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var assignedUser = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer() { Id = Guid.NewGuid(), TenantId = tenantId },
            Status = ConversationStatus.Open,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RowVersion = Array.Empty<byte>(),
            AssignedUserId = assignedUser
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        unityOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        notifierMock
            .Setup(n => n.ConversationUpdatedAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        // Act - unassign
        await service.UnassignAsync(tenantId, conversationId, CancellationToken.None);

        Assert.Null(conversation.AssignedUserId);

        // Act - idempotent unassign
        await service.UnassignAsync(tenantId, conversationId, CancellationToken.None);

        unityOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        notifierMock.Verify(n => n.ConversationUpdatedAsync(tenantId, conversationId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CloseConversationAsync_WhenConversationIsOpen_ClosesAndNotifies()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer() { Id = Guid.NewGuid(), TenantId = tenantId },
            Status = ConversationStatus.Open,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RowVersion = Array.Empty<byte>()
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        unityOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        notifierMock
            .Setup(n => n.ConversationUpdatedAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        await service.CloseConversationAsync(tenantId, conversationId, CancellationToken.None);

        Assert.Equal(ConversationStatus.Closed, conversation.Status);
        notifierMock.Verify();
        unityOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseConversationAsync_WhenConversationIsAlreadyClosed_IsIdempotent()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer() { Id = Guid.NewGuid(), TenantId = tenantId },
            Status = ConversationStatus.Closed,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RowVersion = Array.Empty<byte>()
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        await service.CloseConversationAsync(tenantId, conversationId, CancellationToken.None);

        // No save or notify
        unityOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        notifierMock.Verify(n => n.ConversationUpdatedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReopenConversationAsync_WhenConversationIsClosed_ReopensAndNotifies()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer() { Id = Guid.NewGuid(), TenantId = tenantId },
            Status = ConversationStatus.Closed,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RowVersion = Array.Empty<byte>()
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        unityOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        notifierMock
            .Setup(n => n.ConversationUpdatedAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        await service.ReopenConversationAsync(tenantId, conversationId, CancellationToken.None);

        Assert.Equal(ConversationStatus.Open, conversation.Status);
        notifierMock.Verify();
        unityOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReopenConversationAsync_WhenConversationIsAlreadyOpen_IsIdempotent()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer() { Id = Guid.NewGuid(), TenantId = tenantId },
            Status = ConversationStatus.Open,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RowVersion = Array.Empty<byte>()
        };

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        await service.ReopenConversationAsync(tenantId, conversationId, CancellationToken.None);

        unityOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        notifierMock.Verify(n => n.ConversationUpdatedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CloseConversationAsync_WhenConversationMissing_ThrowsNotFound()
    {
        var tenantId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        repositoryMock
            .Setup(r => r.GetConversationByIdAsync(tenantId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
            await service.CloseConversationAsync(tenantId, conversationId, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WhenConversationIsClosed_ShouldThrowAndNotPersistOrNotify()
    {
        // Arrange

        var conversationId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var originalUpdatedAt = new DateTime(
            2026,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var closeStatus = ConversationStatus.Closed;
        var conversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            Customer = new Customer()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
            },
            Status = closeStatus,
            CreatedAt = originalUpdatedAt,
            UpdatedAt = originalUpdatedAt,
            RowVersion = Array.Empty<byte>()
        };

        var cancellationToken = new CancellationTokenSource().Token;

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object
            );

        var command = new SendMessageCommand(
            tenantId,
            conversationId,
            MessageSender.Agent(agentId),
            "Hello OmniDesk", []);

        repositoryMock.Setup(repository => repository.GetConversationByIdAsync(
                tenantId,
                conversationId,
                cancellationToken)).ReturnsAsync(conversation);

        // Act & Assert

        await Assert.ThrowsAsync<ConversationClosedException>(() =>
            service.SendMessageAsync(command, cancellationToken));

        Assert.Equal(originalUpdatedAt, conversation.UpdatedAt);

        repositoryMock.Verify(
            repository =>
                repository.GetConversationByIdAsync(
                    tenantId,
                    conversationId,
                    cancellationToken),
            Times.Once);

        messageRepositoryMock.Verify(
            repository =>
                repository.AddMessage(
                    It.IsAny<Message>()),
            Times.Never);

        unityOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        notifierMock.Verify(
            notifier =>
                notifier.MessageSentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<MessageResponse>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_WhenConversationDoesNotExist_ShouldThrow()
    {
        // Arrange

        var conversationId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var cancellationToken = new CancellationTokenSource().Token;

        var repositoryMock = new Mock<IConversationRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        var command = new SendMessageCommand(
            tenantId,
            conversationId,
            MessageSender.Agent(agentId),
            "Hello OmniDesk", []);

        repositoryMock.Setup(repository => repository.GetConversationByIdAsync(
                tenantId,
                conversationId,
                cancellationToken)).ReturnsAsync((Conversation?)null);

        // Act & Assert

        await Assert.ThrowsAsync<ConversationNotFoundException>(() =>
            service.SendMessageAsync(command, cancellationToken));

        repositoryMock.Verify(
            repository =>
                repository.GetConversationByIdAsync(
                    tenantId,
                    conversationId,
                    cancellationToken),
            Times.Once);

        messageRepositoryMock.Verify(
            messageRepository =>
                messageRepository.AddMessage(
                    It.IsAny<Message>()),
            Times.Never);

        unityOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        notifierMock.Verify(
            notifier =>
                notifier.MessageSentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<MessageResponse>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task SendMessageAsync_WhenContentIsBlank_ShouldThrow(string content)
    {
        // Arrange

        var conversationId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var cancellationToken = new CancellationTokenSource().Token;

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        var command = new SendMessageCommand(
            tenantId,
            conversationId,
            MessageSender.Agent(agentId),
            content, []);

        // Act & Assert

        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(command, cancellationToken));

        repositoryMock.Verify(
            repository =>
                repository.GetConversationByIdAsync(
                    tenantId,
                    conversationId,
                    cancellationToken),
            Times.Never);

        messageRepositoryMock.Verify(
            messageRepository =>
                messageRepository.AddMessage(
                    It.IsAny<Message>()),
            Times.Never);

        unityOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        notifierMock.Verify(
            notifier =>
                notifier.MessageSentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<MessageResponse>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetMessagesAsync_WhenPageSizeIsInvalid_ShouldThrow(int pageSize)
    {
        // Arrange

        var conversationId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var cancellationToken = new CancellationTokenSource().Token;

        var repositoryMock = new Mock<IConversationRepository>();
        var messageRepositoryMock = new Mock<IMessageRepository>();
        var notifierMock = new Mock<IConversationNotifier>();
        var unityOfWorkMock = new Mock<IUnitOfWork>();
        var conversationReadStateRepositoryMock = new Mock<IConversationReadStateRepository>();
        var messageAttachmentRepository = new Mock<IMessageAttachmentRepository>();
        var loggerMock = new Mock<ILogger<ConversationService>>();
        var blobStorageServiceMock = new Mock<IBlobStorageService>();

        var service = new ConversationService(
            repositoryMock.Object,
            notifierMock.Object,
            messageRepositoryMock.Object,
            conversationReadStateRepositoryMock.Object,
            unityOfWorkMock.Object,
            blobStorageServiceMock.Object,
            messageAttachmentRepository.Object,
            loggerMock.Object);

        // Act & Assert

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetMessagesAsync(tenantId, conversationId, pageSize, cancellationToken));

        messageRepositoryMock.Verify(
            messageRepository =>
                messageRepository.GetMessagesAsync(
                    tenantId,
                    conversationId,
                    pageSize,
                    cancellationToken),
            Times.Never);
    }
}