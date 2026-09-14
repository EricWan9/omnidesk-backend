using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Exceptions;
using OmniDesk.Application.Customers;
using OmniDesk.Application.Identity;
using OmniDesk.Application.Widgets.Models;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Customers;

namespace OmniDesk.Application.Widgets;

public sealed class WidgetConversationService : IWidgetConversationService
{
    private readonly ITokenService _tokenService;
    private readonly IWidgetConfigurationRepository _widgetConfigurations;
    private readonly ICustomerRepository _customers;
    private readonly IConversationRepository _conversations;
    private readonly IUnitOfWork _unitOfWork;

    public WidgetConversationService(
        ITokenService tokenService,
        IWidgetConfigurationRepository widgetConfigurations, 
        ICustomerRepository customers, 
        IConversationRepository conversations, 
        IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _widgetConfigurations = widgetConfigurations;
        _customers = customers;
        _conversations = conversations;
        _unitOfWork = unitOfWork;
    }

    public async Task<StartWidgetConversationResult> StartConversationAsync(
        StartWidgetConversationCommand command, 
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(command.WidgetKey))
        {
            throw new ArgumentException("Widget key cannot be null or whitespace.", nameof(command.WidgetKey));
        }

        var activeWidgetTenantId = await _widgetConfigurations.GetActiveWidgetTenantIdAsync(
            command.WidgetKey,
            cancellationToken);

        if(activeWidgetTenantId is null)
        {
            throw new WidgetUnavailableException();
        }

        var tenantId = activeWidgetTenantId.Value;

        var now = DateTime.UtcNow;
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            CreatedAt = now
        };

        _customers.AddCustomer(customer);

        var conversationId = Guid.NewGuid();

        var newConversation = new Conversation
        {
            Id = conversationId,
            TenantId = tenantId,
            CustomerId = customerId,
            Status = ConversationStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        var accessToken = _tokenService.GenerateCustomerAccessToken(
            tenantId, customerId, conversationId);

        _conversations.AddConversation(newConversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StartWidgetConversationResult(
            newConversation.CustomerId, 
            newConversation.Id,
            accessToken);
    }
}
