using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Entities;

namespace OmniDesk.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        // If conversation test data already exists,
        // do not insert it again every time the application starts.
        if (await dbContext.Conversations.AnyAsync())
        {
            return;
        }

        var tenant = await dbContext.Tenants.FirstOrDefaultAsync();

        if (tenant is null)
        {
            throw new InvalidOperationException(
                "No tenant exists. Please register a tenant before seeding conversation data.");
        }

        var users = await dbContext.Users
            .Where(x => x.TenantId == tenant.Id)
            .Take(2)
            .ToListAsync();

        if (users.Count == 0)
        {
            throw new InvalidOperationException(
                "No user exists for the current tenant. Please create a user before seeding conversation data.");
        }

        var agent1 = users[0];
        var agent2 = users.Count > 1
            ? users[1]
            : null;

        var now = DateTime.UtcNow;

        var conversation1 = new Conversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            CustomerName = "Alice Chen",
            CustomerEmail = "alice.chen@example.com",
            Status = ConversationStatus.Open,
            AssignedUserId = agent1.Id,
            CreatedAt = now.AddMinutes(-45),
            UpdatedAt = now.AddMinutes(-3)
        };

        var conversation2 = new Conversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            CustomerName = "Michael Wong",
            CustomerEmail = "michael.wong@example.com",
            Status = ConversationStatus.Pending,
            AssignedUserId = null,
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddMinutes(-20)
        };

        var conversation3 = new Conversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            CustomerName = "Sophia Lee",
            CustomerEmail = "sophia.lee@example.com",
            Status = ConversationStatus.Closed,
            AssignedUserId = agent2?.Id ?? agent1.Id,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddHours(-4)
        };

        var conversation4 = new Conversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            CustomerName = "Daniel Ho",
            CustomerEmail = "daniel.ho@example.com",
            Status = ConversationStatus.Open,
            AssignedUserId = agent1.Id,
            CreatedAt = now.AddMinutes(-25),
            UpdatedAt = now.AddMinutes(-1)
        };

        var conversation5 = new Conversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            CustomerName = "Emily Lam",
            CustomerEmail = "emily.lam@example.com",
            Status = ConversationStatus.Pending,
            AssignedUserId = agent2?.Id,
            CreatedAt = now.AddHours(-5),
            UpdatedAt = now.AddHours(-1)
        };

        dbContext.Conversations.AddRange(
            conversation1,
            conversation2,
            conversation3,
            conversation4,
            conversation5);

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation1.Id,
                SenderType = MessageSenderType.Customer,
                SenderUserId = null,
                Content = "Hi, I was charged twice for my subscription.",
                CreatedAt = now.AddMinutes(-45)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation1.Id,
                SenderType = MessageSenderType.Agent,
                SenderUserId = agent1.Id,
                Content = "Thanks for letting us know. Let me check the billing history for you.",
                CreatedAt = now.AddMinutes(-40)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation1.Id,
                SenderType = MessageSenderType.Customer,
                SenderUserId = null,
                Content = "The two charges both appeared this morning.",
                CreatedAt = now.AddMinutes(-8)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation1.Id,
                SenderType = MessageSenderType.Agent,
                SenderUserId = agent1.Id,
                Content = "I found the duplicate transaction. I'll submit a refund request for the second charge.",
                CreatedAt = now.AddMinutes(-3)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation2.Id,
                SenderType = MessageSenderType.Customer,
                SenderUserId = null,
                Content = "Hello, I can't log in to my account after resetting my password.",
                CreatedAt = now.AddHours(-2)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation2.Id,
                SenderType = MessageSenderType.System,
                SenderUserId = null,
                Content = "Conversation is waiting for an available agent.",
                CreatedAt = now.AddMinutes(-20)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation3.Id,
                SenderType = MessageSenderType.Customer,
                SenderUserId = null,
                Content = "Can I change the email address linked to my account?",
                CreatedAt = now.AddDays(-1)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation3.Id,
                SenderType = MessageSenderType.Agent,
                SenderUserId = agent2?.Id ?? agent1.Id,
                Content = "Yes. I've updated the email address after verifying your account.",
                CreatedAt = now.AddHours(-5)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation3.Id,
                SenderType = MessageSenderType.System,
                SenderUserId = null,
                Content = "Conversation was closed.",
                CreatedAt = now.AddHours(-4)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation4.Id,
                SenderType = MessageSenderType.Customer,
                SenderUserId = null,
                Content = "Is there any way to download all invoices for this year?",
                CreatedAt = now.AddMinutes(-25)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation4.Id,
                SenderType = MessageSenderType.Agent,
                SenderUserId = agent1.Id,
                Content = "Yes. You can export them from Billing > Invoice History.",
                CreatedAt = now.AddMinutes(-1)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation5.Id,
                SenderType = MessageSenderType.Customer,
                SenderUserId = null,
                Content = "Our team is seeing a delay when new conversations appear in the dashboard.",
                CreatedAt = now.AddHours(-5)
            },

            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation5.Id,
                SenderType = MessageSenderType.Agent,
                SenderUserId = agent2?.Id,
                Content = "Thanks. We're checking the delivery delay and will update you shortly.",
                CreatedAt = now.AddHours(-1)
            }
        };

        dbContext.Messages.AddRange(messages);

        await dbContext.SaveChangesAsync();
    }
}