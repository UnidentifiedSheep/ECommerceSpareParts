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
COPY ["src/Services/Pricing/Pricing.Api/Pricing.Api.csproj", "src/Services/Pricing/Pricing.Api/"]
COPY ["src/Services/Pricing/Pricing.Abstractions/Pricing.Abstractions.csproj", "src/Services/Pricing/Pricing.Abstractions/"]
COPY ["src/Services/Pricing/Pricing.Entities/Pricing.Entities.csproj", "src/Services/Pricing/Pricing.Entities/"]
COPY ["src/Services/Pricing/Pricing.Enums/Pricing.Enums.csproj", "src/Services/Pricing/Pricing.Enums/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Pricing/Pricing.Application/Pricing.Application.csproj", "src/Services/Pricing/Pricing.Application/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Core/Utils/Utils.csproj", "src/Core/Utils/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Services/Pricing/Pricing.Cache/Pricing.Cache.csproj", "src/Services/Pricing/Pricing.Cache/"]
COPY ["src/Services/Pricing/Pricing.Persistence/Pricing.Persistence.csproj", "src/Services/Pricing/Pricing.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Infrastructure/Redis/Redis.csproj", "src/Infrastructure/Redis/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
RUN dotnet restore "src/Services/Pricing/Pricing.Api/Pricing.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Pricing/Pricing.Api"
RUN dotnet build "./Pricing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pricing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pricing.Api.dll"]
