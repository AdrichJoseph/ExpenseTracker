# syntax=docker/dockerfile:1.7

# -------------------------------------------------------------
# Stage 1: BUILD — uses .NET 10 SDK image to compile and publish
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + every .csproj first, then restore.
# This layer caches as long as project files don't change,
# so changing C# code doesn't trigger a full package re-download.
COPY ExpenseTracker.slnx ./
COPY ExpenseTracker.Domain/*.csproj         ExpenseTracker.Domain/
COPY ExpenseTracker.Application/*.csproj    ExpenseTracker.Application/
COPY ExpenseTracker.Infrastructure/*.csproj ExpenseTracker.Infrastructure/
COPY ExpenseTracker.Api/*.csproj            ExpenseTracker.Api/
RUN dotnet restore

# Copy the rest of the source and build.
COPY . .
RUN dotnet publish ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# -------------------------------------------------------------
# Stage 2: FINAL — slim runtime image, no SDK or source
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]
