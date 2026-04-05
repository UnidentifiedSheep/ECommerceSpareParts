FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Analytics/Analytics.Worker/Analytics.Worker.csproj", "src/Services/Analytics/Analytics.Worker/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Analytics/Analytics.Application/Analytics.Application.csproj", "src/Services/Analytics/Analytics.Application/"]
COPY ["src/Infrastructure/Localization/Localization.Abstractions/Localization.Abstractions.csproj", "src/Infrastructure/Localization/Localization.Abstractions/"]
COPY ["src/Infrastructure/Localization/Localization.Domain/Localization.Domain.csproj", "src/Infrastructure/Localization/Localization.Domain/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Services/Analytics/Analytics.Abstractions/Analytics.Abstractions.csproj", "src/Services/Analytics/Analytics.Abstractions/"]
COPY ["src/Services/Analytics/Analytics.Entities/Analytics.Entities.csproj", "src/Services/Analytics/Analytics.Entities/"]
COPY ["src/Services/Analytics/Analytics.Attributes/Analytics.Attributes.csproj", "src/Services/Analytics/Analytics.Attributes/"]
COPY ["src/Services/Analytics/Analytics.Enums/Analytics.Enums.csproj", "src/Services/Analytics/Analytics.Enums/"]
COPY ["src/Services/Analytics/Analytics.Utils/Analytics.Utils.csproj", "src/Services/Analytics/Analytics.Utils/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Core/Utils/Utils.csproj", "src/Core/Utils/"]
COPY ["src/Services/Analytics/Analytics.Persistence/Analytics.Persistence.csproj", "src/Services/Analytics/Analytics.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Infrastructure/Redis/Redis.csproj", "src/Infrastructure/Redis/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
RUN dotnet restore "src/Services/Analytics/Analytics.Worker/Analytics.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Analytics/Analytics.Worker"
RUN dotnet build "./Analytics.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Analytics.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Worker.dll"]
