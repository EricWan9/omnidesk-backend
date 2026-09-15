using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OmniDesk.Api.OpenApi;
using OmniDesk.Api.Realtime.Conversations;
using OmniDesk.Application;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Customers;
using OmniDesk.Application.Identity;
using OmniDesk.Application.Storage;
using OmniDesk.Application.Widgets;
using OmniDesk.Infrastructure;
using OmniDesk.Infrastructure.Authentication;
using OmniDesk.Infrastructure.Conversations;
using OmniDesk.Infrastructure.Customers;
using OmniDesk.Infrastructure.Identity;
using OmniDesk.Infrastructure.Persistence;
using OmniDesk.Infrastructure.Storage;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<OmniDeskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDatabase")));

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IConversationNotifier, SignalRConversationNotifier>();
builder.Services.AddScoped<IConversationAccessService, ConversationAccessService>();
builder.Services.AddScoped<IWidgetConversationService, WidgetConversationService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IWidgetConfigurationRepository, WidgetConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationReadStateRepository, ConversationReadStateRepository>();
builder.Services.AddScoped<IMessageAttachmentRepository, MessageAttachmentRepository>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSignalR();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<BlobStorageOptions>(
    builder.Configuration.GetSection(BlobStorageOptions.SectionName));

var jwtSection =
    builder.Configuration.GetSection(JwtOptions.SectionName);

builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions =
    jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.Key)),

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =
                    context.Request.Query["access_token"];

                var path =
                    context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments(
                        "/hubs/conversations"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build();
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "OmniDesk Identity API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversations");

app.Run();