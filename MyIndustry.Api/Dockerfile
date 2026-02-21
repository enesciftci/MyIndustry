FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create uploads directory with proper permissions before switching user
RUN mkdir -p /app/wwwroot/uploads && chmod -R 777 /app/wwwroot

USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyIndustry.Api/MyIndustry.Api.csproj", "MyIndustry.Api/"]
COPY ["MyIndustry.ApplicationService/MyIndustry.ApplicationService.csproj", "MyIndustry.ApplicationService/"]
COPY ["MyIndustry.Repository/MyIndustry.Repository.csproj", "MyIndustry.Repository/"]
COPY ["MyIndustry.Domain/MyIndustry.Domain.csproj", "MyIndustry.Domain/"]
COPY ["RabbitMqCommunicator/RabbitMqCommunicator.csproj", "RabbitMqCommunicator/"]
COPY ["MyIndustry.Queue.Message/MyIndustry.Queue.Message.csproj", "MyIndustry.Queue.Message/"]
RUN dotnet restore "MyIndustry.Api/MyIndustry.Api.csproj"
COPY . .
WORKDIR "/src/MyIndustry.Api"
RUN dotnet build "./MyIndustry.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyIndustry.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyIndustry.Api.dll"]
