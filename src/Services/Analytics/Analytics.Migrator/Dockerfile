FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Analytics/Analytics.Migrator/Analytics.Migrator.csproj", "src/Services/Analytics/Analytics.Migrator/"]
COPY ["src/Services/Analytics/Analytics.Persistence/Analytics.Persistence.csproj", "src/Services/Analytics/Analytics.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Analytics/Analytics.Abstractions/Analytics.Abstractions.csproj", "src/Services/Analytics/Analytics.Abstractions/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Services/Analytics/Analytics.Entities/Analytics.Entities.csproj", "src/Services/Analytics/Analytics.Entities/"]
COPY ["src/Services/Analytics/Analytics.Attributes/Analytics.Attributes.csproj", "src/Services/Analytics/Analytics.Attributes/"]
COPY ["src/Services/Analytics/Analytics.Enums/Analytics.Enums.csproj", "src/Services/Analytics/Analytics.Enums/"]
RUN dotnet restore "src/Services/Analytics/Analytics.Migrator/Analytics.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Services/Analytics/Analytics.Migrator"
RUN dotnet build "./Analytics.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Analytics.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Migrator.dll"]
