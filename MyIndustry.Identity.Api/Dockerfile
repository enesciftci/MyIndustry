FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyIndustry.Identity.Api/MyIndustry.Identity.Api.csproj", "MyIndustry.Identity.Api/"]
COPY ["MyIndustry.Identity.Domain/MyIndustry.Identity.Domain.csproj", "MyIndustry.Identity.Domain/"]
COPY ["MyIndustry.Identity.Repository/MyIndustry.Identity.Repository.csproj", "MyIndustry.Identity.Repository/"]
COPY ["RabbitMqCommunicator/RabbitMqCommunicator.csproj", "RabbitMqCommunicator/"]
COPY ["RedisCommunicator/RedisCommunicator.csproj", "RedisCommunicator/"]
COPY ["MyIndustry.Queue.Message/MyIndustry.Queue.Message.csproj", "MyIndustry.Queue.Message/"]
COPY ["CoreApiCommunicator/CoreApiCommunicator.csproj", "CoreApiCommunicator/"]
RUN dotnet restore "MyIndustry.Identity.Api/MyIndustry.Identity.Api.csproj"
COPY . .
WORKDIR "/src/MyIndustry.Identity.Api"
RUN dotnet build "./MyIndustry.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyIndustry.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyIndustry.Identity.Api.dll"]
