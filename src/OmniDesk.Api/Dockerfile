# Runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER $APP_UID
WORKDIR /app

EXPOSE 8080


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /workspace

# Copy project files first so Docker can reuse the restore cache
COPY ["src/OmniDesk.Api/OmniDesk.Api.csproj", "src/OmniDesk.Api/"]
COPY ["src/OmniDesk.Application/OmniDesk.Application.csproj", "src/OmniDesk.Application/"]
COPY ["src/OmniDesk.Domain/OmniDesk.Domain.csproj", "src/OmniDesk.Domain/"]
COPY ["src/OmniDesk.Infrastructure/OmniDesk.Infrastructure.csproj", "src/OmniDesk.Infrastructure/"]

RUN dotnet restore "src/OmniDesk.Api/OmniDesk.Api.csproj"

# Copy the remaining repository content
COPY . .

WORKDIR /workspace/src/OmniDesk.Api

RUN dotnet publish \
    "OmniDesk.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore


# Final production image
FROM base AS final

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "OmniDesk.Api.dll"]