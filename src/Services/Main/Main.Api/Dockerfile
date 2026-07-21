FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Main/Main.Api/Main.Api.csproj", "src/Services/Main/Main.Api/"]
COPY ["src/Infrastructure/Cache/Cache.csproj", "src/Infrastructure/Cache/"]
COPY ["src/Infrastructure/Common/Common.csproj", "src/Infrastructure/Common/"]
COPY ["src/Infrastructure/Integrations/ExchangeRate/ExchangeRate.csproj", "src/Infrastructure/Integrations/ExchangeRate/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Infrastructure/Mailing/Mail/Mail.csproj", "src/Infrastructure/Mailing/Mail/"]
COPY ["src/Infrastructure/Mailing/EmailTemplates/EmailTemplates.csproj", "src/Infrastructure/Mailing/EmailTemplates/"]
COPY ["src/Infrastructure/Mailing/Mailing.Core/Mailing.Core.csproj", "src/Infrastructure/Mailing/Mailing.Core/"]
COPY ["src/Infrastructure/Localization/Localization.Abstractions/Localization.Abstractions.csproj", "src/Infrastructure/Localization/Localization.Abstractions/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
COPY ["src/Infrastructure/S3/S3.csproj", "src/Infrastructure/S3/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Infrastructure/Localization/Localization.Domain/Localization.Domain.csproj", "src/Infrastructure/Localization/Localization.Domain/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Services/Main/Main.Application/Main.Application.csproj", "src/Services/Main/Main.Application/"]
COPY ["src/Services/Main/Main.Entities/Main.Entities.csproj", "src/Services/Main/Main.Entities/"]
COPY ["src/Services/Main/Main.Enums/Main.Enums.csproj", "src/Services/Main/Main.Enums/"]
COPY ["src/Services/Main/Main.Cache/Main.Cache.csproj", "src/Services/Main/Main.Cache/"]
COPY ["src/Services/Main/Main.Persistence/Main.Persistence.csproj", "src/Services/Main/Main.Persistence/"]
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
