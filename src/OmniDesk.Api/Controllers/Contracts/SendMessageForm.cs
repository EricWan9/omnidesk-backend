namespace OmniDesk.Api.Controllers.Contracts;

public sealed class SendMessageForm
{
    public string? Content { get; init; }

    public List<IFormFile> Files { get; init; } = [];
}