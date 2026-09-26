FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Main/Main.Api/Main.Api.csproj", "src/Services/Main/Main.Api/"]
COPY ["src/Infrastructure/Notifications/Notification.Core/Notification.Core.csproj", "src/Infrastructure/Notifications/Notification.Core/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Infrastructure/NamedObjects/NamedObject.Core/NamedObject.Core.csproj", "src/Infrastructure/NamedObjects/NamedObject.Core/"]
COPY ["src/Infrastructure/Notifications/Notification.Persistence/Notification.Persistence.csproj", "src/Infrastructure/Notifications/Notification.Persistence/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Infrastructure/NamedObjects/NamedObject/NamedObject.csproj", "src/Infrastructure/NamedObjects/NamedObject/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Infrastructure/Integrations/Internal/Internal.Integration.Core/Internal.Integration.Core.csproj", "src/Infrastructure/Integrations/Internal/Internal.Integration.Core/"]
COPY ["src/Infrastructure/Integrations/Integrations.Client.Core/Integrations.Client.Core.csproj", "src/Infrastructure/Integrations/Integrations.Client.Core/"]
COPY ["src/Infrastructure/Integrations/Integrations.Common/Integrations.Common.csproj", "src/Infrastructure/Integrations/Integrations.Common/"]
COPY ["src/Infrastructure/SchemaGeneration/SchemaGeneration.Abstractions/SchemaGeneration.Abstractions.csproj", "src/Infrastructure/SchemaGeneration/SchemaGeneration.Abstractions/"]
COPY ["src/Infrastructure/Integrations/Suppliers/Integrations.Supplier/Integrations.Supplier.csproj", "src/Infrastructure/Integrations/Suppliers/Integrations.Supplier/"]
COPY ["src/Infrastructure/SchemaGeneration/SchemaGeneration/SchemaGeneration.csproj", "src/Infrastructure/SchemaGeneration/SchemaGeneration/"]
COPY ["src/Infrastructure/Notifications/Notification/Notification.csproj", "src/Infrastructure/Notifications/Notification/"]
COPY ["src/Infrastructure/Cache/Cache.csproj", "src/Infrastructure/Cache/"]
COPY ["src/Infrastructure/Common/Common.csproj", "src/Infrastructure/Common/"]
COPY ["src/Infrastructure/Integrations/ExchangeRate/ExchangeRate.csproj", "src/Infrastructure/Integrations/ExchangeRate/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
COPY ["src/Infrastructure/S3/S3.csproj", "src/Infrastructure/S3/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Services/GraphQL.Common/GraphQL.Common.csproj", "src/Services/GraphQL.Common/"]
COPY ["src/Services/Main/Main.Application/Main.Application.csproj", "src/Services/Main/Main.Application/"]
COPY ["src/Services/Main/Main.Entities/Main.Entities.csproj", "src/Services/Main/Main.Entities/"]
COPY ["src/Services/Main/Main.Enums/Main.Enums.csproj", "src/Services/Main/Main.Enums/"]
COPY ["src/Services/Main/Main.Cache/Main.Cache.csproj", "src/Services/Main/Main.Cache/"]
COPY ["src/Services/Main/Main.Persistence/Main.Persistence.csproj", "src/Services/Main/Main.Persistence/"]
COPY ["src/Services/Persistence.Common/Persistence.Common.csproj", "src/Services/Persistence.Common/"]
RUN dotnet restore "src/Services/Main/Main.Api/Main.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Main/Main.Api"
RUN dotnet build "./Main.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Main.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Main.Api.dll"]
