FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 7292

ENV ASPNETCORE_URLS=http://+:8080;https://+:7292

ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Analytics/Analytics.Api/Analytics.Api.csproj", "src/Services/Analytics/Analytics.Api/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Services/Analytics/Analytics.Core/Analytics.Core.csproj", "src/Services/Analytics/Analytics.Core/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Core/Utils/Utils.csproj", "src/Core/Utils/"]
COPY ["src/Services/Analytics/Analytics.Application/Analytics.Application.csproj", "src/Services/Analytics/Analytics.Application/"]
COPY ["src/Services/Analytics/Analytics.Persistence/Analytics.Persistence.csproj", "src/Services/Analytics/Analytics.Persistence/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
RUN dotnet restore "src/Services/Analytics/Analytics.Api/Analytics.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Analytics/Analytics.Api"
RUN dotnet build "./Analytics.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Analytics.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Api.dll"]
