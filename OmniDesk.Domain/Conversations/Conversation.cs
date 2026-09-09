using OmniDesk.Domain.Conversations;

namespace OmniDesk.Domain.Entities;
    
public class Conversation
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public ConversationStatus Status { get; set; }

    public Guid? AssignedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();
}