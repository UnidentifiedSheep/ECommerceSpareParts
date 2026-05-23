FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Pricing/Pricing.Api/Pricing.Api.csproj", "src/Services/Pricing/Pricing.Api/"]
COPY ["src/Infrastructure/Cache/Cache.csproj", "src/Infrastructure/Cache/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Utils/Utils.csproj", "src/Core/Utils/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Infrastructure/Localization/Localization.Abstractions/Localization.Abstractions.csproj", "src/Infrastructure/Localization/Localization.Abstractions/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/Localization/Localization.Domain/Localization.Domain.csproj", "src/Infrastructure/Localization/Localization.Domain/"]
COPY ["src/Services/Pricing/Pricing.Abstractions/Pricing.Abstractions.csproj", "src/Services/Pricing/Pricing.Abstractions/"]
COPY ["src/Services/Pricing/Pricing.Entities/Pricing.Entities.csproj", "src/Services/Pricing/Pricing.Entities/"]
COPY ["src/Services/Pricing/Pricing.Enums/Pricing.Enums.csproj", "src/Services/Pricing/Pricing.Enums/"]
COPY ["src/Services/Pricing/Pricing.Application/Pricing.Application.csproj", "src/Services/Pricing/Pricing.Application/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Core/Internal.Integration.Core.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Core/"]
COPY ["src/Services/Pricing/Pricing.Persistence/Pricing.Persistence.csproj", "src/Services/Pricing/Pricing.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
RUN dotnet restore "src/Services/Pricing/Pricing.Api/Pricing.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Pricing/Pricing.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pricing.Api.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pricing.Api.dll"]
