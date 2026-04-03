FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Pricing/Pricing.Migrator/Pricing.Migrator.csproj", "src/Services/Pricing/Pricing.Migrator/"]
COPY ["src/Services/Pricing/Pricing.Persistence/Pricing.Persistence.csproj", "src/Services/Pricing/Pricing.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Pricing/Pricing.Abstractions/Pricing.Abstractions.csproj", "src/Services/Pricing/Pricing.Abstractions/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Services/Pricing/Pricing.Entities/Pricing.Entities.csproj", "src/Services/Pricing/Pricing.Entities/"]
COPY ["src/Services/Pricing/Pricing.Enums/Pricing.Enums.csproj", "src/Services/Pricing/Pricing.Enums/"]
RUN dotnet restore "src/Services/Pricing/Pricing.Migrator/Pricing.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Services/Pricing/Pricing.Migrator"
RUN dotnet build "./Pricing.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pricing.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pricing.Migrator.dll"]
