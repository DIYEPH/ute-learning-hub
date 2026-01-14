FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY UteLearningHub.sln ./
COPY UteLearningHub.Api/UteLearningHub.Api.csproj UteLearningHub.Api/
COPY UteLearningHub.Application/UteLearningHub.Application.csproj UteLearningHub.Application/
COPY UteLearningHub.Infrastructure/UteLearningHub.Infrastructure.csproj UteLearningHub.Infrastructure/
COPY UteLearningHub.Persistence/UteLearningHub.Persistence.csproj UteLearningHub.Persistence/
COPY UteLearningHub.Domain/UteLearningHub.Domain.csproj UteLearningHub.Domain/
COPY UteLearningHub.CrossCuttingConcerns/UteLearningHub.CrossCuttingConcerns.csproj UteLearningHub.CrossCuttingConcerns/

RUN dotnet restore UteLearningHub.Api/UteLearningHub.Api.csproj

COPY . .

WORKDIR /src/UteLearningHub.Api
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 7080
ENV ASPNETCORE_URLS=http://+:7080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "UteLearningHub.Api.dll"]
