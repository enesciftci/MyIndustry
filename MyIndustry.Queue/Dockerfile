FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyIndustry.Queue/MyIndustry.Queue.csproj", "MyIndustry.Queue/"]
COPY ["MyIndustry.Queue.Message/MyIndustry.Queue.Message.csproj", "MyIndustry.Queue.Message/"]
COPY ["RabbitMqCommunicator/RabbitMqCommunicator.csproj", "RabbitMqCommunicator/"]
COPY ["CoreApiCommunicator/CoreApiCommunicator.csproj", "CoreApiCommunicator/"]
COPY ["MyIndustry.Repository/MyIndustry.Repository.csproj", "MyIndustry.Repository/"]
COPY ["MyIndustry.Domain/MyIndustry.Domain.csproj", "MyIndustry.Domain/"]
RUN dotnet restore "MyIndustry.Queue/MyIndustry.Queue.csproj"
COPY . .
WORKDIR "/src/MyIndustry.Queue"
RUN dotnet build "./MyIndustry.Queue.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyIndustry.Queue.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyIndustry.Queue.dll"]
