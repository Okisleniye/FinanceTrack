# =============================================================================
# Stage 1: Build & Publish
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY FinanceTrack.sln ./
COPY src/FinanceTrack.Domain/FinanceTrack.Domain.csproj src/FinanceTrack.Domain/
COPY src/FinanceTrack.Application/FinanceTrack.Application.csproj src/FinanceTrack.Application/
COPY src/FinanceTrack.Infrastructure/FinanceTrack.Infrastructure.csproj src/FinanceTrack.Infrastructure/
COPY src/FinanceTrack.Web/FinanceTrack.Web.csproj src/FinanceTrack.Web/
COPY tests/FinanceTrack.UnitTests/FinanceTrack.UnitTests.csproj tests/FinanceTrack.UnitTests/
COPY tests/FinanceTrack.IntegrationTests/FinanceTrack.IntegrationTests.csproj tests/FinanceTrack.IntegrationTests/

# Restore dependencies
RUN dotnet restore FinanceTrack.sln

# Copy all remaining source code
COPY . .

# Build solution in Release configuration
RUN dotnet build FinanceTrack.sln -c Release --no-restore

# Publish the Web project
RUN dotnet publish src/FinanceTrack.Web/FinanceTrack.Web.csproj -c Release -o /app/publish --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Ensure dedicated directory for SQLite database storage
RUN mkdir -p /app/data

# Copy published application from build stage
COPY --from=build /app/publish .

# Set environment variables for Render & container execution
ENV ASPNETCORE_HTTP_PORTS=10000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/FinanceTrack.db"

# Expose Render default HTTP port
EXPOSE 10000

# Entry point
ENTRYPOINT ["dotnet", "FinanceTrack.Web.dll"]
