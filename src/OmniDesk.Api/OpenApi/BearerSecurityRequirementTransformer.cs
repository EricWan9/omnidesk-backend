using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OmniDesk.Api.OpenApi;

internal sealed class BearerSecurityRequirementTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata =
            context.Description
                .ActionDescriptor
                .EndpointMetadata;

        var allowAnonymous =
            endpointMetadata
                .OfType<IAllowAnonymous>()
                .Any();

        if (allowAnonymous)
        {
            return Task.CompletedTask;
        }

        if (context.Document is null)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        JwtBearerDefaults.AuthenticationScheme,
                        context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}