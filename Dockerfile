# MyIndustry Monorepo Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files
COPY ["MyIndustry.Api/MyIndustry.Api.csproj", "MyIndustry.Api/"]
COPY ["MyIndustry.ApplicationService/MyIndustry.ApplicationService.csproj", "MyIndustry.ApplicationService/"]
COPY ["MyIndustry.Repository/MyIndustry.Repository.csproj", "MyIndustry.Repository/"]
COPY ["MyIndustry.Domain/MyIndustry.Domain.csproj", "MyIndustry.Domain/"]
COPY ["RabbitMqCommunicator/RabbitMqCommunicator.csproj", "RabbitMqCommunicator/"]
COPY ["MyIndustry.Queue.Message/MyIndustry.Queue.Message.csproj", "MyIndustry.Queue.Message/"]

# Restore dependencies
RUN dotnet restore "MyIndustry.Api/MyIndustry.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/MyIndustry.Api"
RUN dotnet build "./MyIndustry.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyIndustry.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MyIndustry.Api.dll"]
