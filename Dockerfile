# syntax=docker/dockerfile:1.7
# -------------------------------------------------------------
# Stage 1: BUILD
# Uses the .NET 10 SDK image (~700MB) to restore packages and compile.
# We name this stage "build" so the next stage can copy artifacts from it.
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the .sln and every .csproj first, then restore.
# This is a caching optimization: as long as your .csproj files don't change,
# Docker can reuse the cached "restore" layer and skip downloading all NuGet packages.
# This is why builds are slow the first time and fast every time after.
COPY ExpenseTracker.slnx ./
COPY ExpenseTracker.Domain/*.csproj         ExpenseTracker.Domain/
COPY ExpenseTracker.Application/*.csproj    ExpenseTracker.Application/
COPY ExpenseTracker.Infrastructure/*.csproj ExpenseTracker.Infrastructure/
COPY ExpenseTracker.Api/*.csproj            ExpenseTracker.Api/
RUN dotnet restore

# Now copy the rest of the source code and build.
# This layer only rebuilds when .cs files (or other source) change.
COPY . .
RUN dotnet publish ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# -------------------------------------------------------------
# Stage 2: FINAL
# Uses the smaller .NET 10 RUNTIME image (~200MB), not the SDK.
# Copies only the published output from Stage 1.
# The final image won't contain the SDK, source code, or NuGet packages.
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the compiled artifacts from the build stage.
COPY --from=build /app/publish .

# Container will listen on port 8080 internally.
# We'll map this to host port 8080 in docker-compose.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Run as a non-root user for security. The aspnet base image already has one.
USER $APP_UID

# Start the API when the container runs.
ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]
