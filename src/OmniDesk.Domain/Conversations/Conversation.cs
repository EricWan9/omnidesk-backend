using OmniDesk.Domain.Customers;

namespace OmniDesk.Domain.Conversations;
    
public class Conversation
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public ConversationStatus Status { get; set; }

    public Guid? AssignedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();

    public void MarkUpdated(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}