FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Main/Main.Migrator/Main.Migrator.csproj", "src/Services/Main/Main.Migrator/"]
COPY ["src/Services/Main/Main.Persistence/Main.Persistence.csproj", "src/Services/Main/Main.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Main/Main.Abstractions/Main.Abstractions.csproj", "src/Services/Main/Main.Abstractions/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Services/Main/Main.Entities/Main.Entities.csproj", "src/Services/Main/Main.Entities/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Services/Main/Main.Enums/Main.Enums.csproj", "src/Services/Main/Main.Enums/"]
RUN dotnet restore "src/Services/Main/Main.Migrator/Main.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Services/Main/Main.Migrator"
RUN dotnet build "./Main.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Main.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Main.Migrator.dll"]
