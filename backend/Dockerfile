# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY UteLearningHub.sln ./

# Copy project files
COPY UteLearningHub.Api/UteLearningHub.Api.csproj UteLearningHub.Api/
COPY UteLearningHub.Application/UteLearningHub.Application.csproj UteLearningHub.Application/
COPY UteLearningHub.Infrastructure/UteLearningHub.Infrastructure.csproj UteLearningHub.Infrastructure/
COPY UteLearningHub.Persistence/UteLearningHub.Persistence.csproj UteLearningHub.Persistence/
COPY UteLearningHub.Domain/UteLearningHub.Domain.csproj UteLearningHub.Domain/
COPY UteLearningHub.CrossCuttingConcerns/UteLearningHub.CrossCuttingConcerns.csproj UteLearningHub.CrossCuttingConcerns/
COPY UteLearningHub.UnitTest/UteLearningHub.UnitTest.csproj UteLearningHub.UnitTest/

# Restore dependencies
RUN dotnet restore UteLearningHub.sln

# Copy all source code
COPY . .

# Build the application
WORKDIR /src/UteLearningHub.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Expose port
EXPOSE 7080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:7080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "UteLearningHub.Api.dll"]

