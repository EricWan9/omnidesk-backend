namespace OmniDesk.Application.Conversations.Exceptions;

public sealed class WidgetUnavailableException : Exception
{
    public WidgetUnavailableException()
        : base("The widget is unavailable.")
    {
    }
}