FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Pricing/Pricing.Worker/Pricing.Worker.csproj", "src/Services/Pricing/Pricing.Worker/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Infrastructure/Integrations/Suppliers/Integrations.Supplier/Integrations.Supplier.csproj", "src/Infrastructure/Integrations/Suppliers/Integrations.Supplier/"]
COPY ["src/Infrastructure/Integrations/Integrations.Common/Integrations.Common.csproj", "src/Infrastructure/Integrations/Integrations.Common/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Infrastructure/Integrations/Integrations.Client.Core/Integrations.Client.Core.csproj", "src/Infrastructure/Integrations/Integrations.Client.Core/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Infrastructure/Cache/Cache.csproj", "src/Infrastructure/Cache/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/Localization/Localization.Domain/Localization.Domain.csproj", "src/Infrastructure/Localization/Localization.Domain/"]
COPY ["src/Infrastructure/Localization/Localization.Abstractions/Localization.Abstractions.csproj", "src/Infrastructure/Localization/Localization.Abstractions/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Core/Internal.Integration.Core.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Core/"]
COPY ["src/Infrastructure/Common/Common.csproj", "src/Infrastructure/Common/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
COPY ["src/Infrastructure/S3/S3.csproj", "src/Infrastructure/S3/"]
COPY ["src/Services/Pricing/Pricing.Application/Pricing.Application.csproj", "src/Services/Pricing/Pricing.Application/"]
COPY ["src/Services/Pricing/Pricing.Enums/Pricing.Enums.csproj", "src/Services/Pricing/Pricing.Enums/"]
COPY ["src/Services/Pricing/Pricing.Entities/Pricing.Entities.csproj", "src/Services/Pricing/Pricing.Entities/"]
COPY ["src/Services/Pricing/Pricing.Cache/Pricing.Cache.csproj", "src/Services/Pricing/Pricing.Cache/"]
COPY ["src/Services/Pricing/Pricing.Persistence/Pricing.Persistence.csproj", "src/Services/Pricing/Pricing.Persistence/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Di/Internal.Integration.Di.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Di/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Auth/Internal.Integration.Auth.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Auth/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Main/Internal.Integration.Main.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Main/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Common/Internal.Integration.Common.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Common/"]
COPY ["src/Infrastructure/Integrations/Suppliers/Integrations.Supplier.DI/Integrations.Supplier.DI.csproj", "src/Infrastructure/Integrations/Suppliers/Integrations.Supplier.DI/"]
COPY ["src/Infrastructure/Integrations/Suppliers/Favorit/Favorit.Integrations.Client/Favorit.Integrations.Client.csproj", "src/Infrastructure/Integrations/Suppliers/Favorit/Favorit.Integrations.Client/"]
COPY ["src/Infrastructure/Integrations/Suppliers/Favorit/Favorit.Integrations.Core/Favorit.Integrations.Core.csproj", "src/Infrastructure/Integrations/Suppliers/Favorit/Favorit.Integrations.Core/"]
COPY ["src/Services/Pricing/Pricing.Api/Pricing.Api.csproj", "src/Services/Pricing/Pricing.Api/"]
RUN dotnet restore "src/Services/Pricing/Pricing.Worker/Pricing.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Pricing/Pricing.Worker"
RUN dotnet build "./Pricing.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pricing.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pricing.Worker.dll"]
