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
COPY ["src/Services/Main/Main.Api/Main.Api.csproj", "src/Services/Main/Main.Api/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Main/Main.Enums/Main.Enums.csproj", "src/Services/Main/Main.Enums/"]
COPY ["src/Services/Main/Main.Entities/Main.Entities.csproj", "src/Services/Main/Main.Entities/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Services/Main/Main.Abstractions/Main.Abstractions.csproj", "src/Services/Main/Main.Abstractions/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Constants/Constants.csproj", "src/Core/Constants/"]
COPY ["src/Core/Utils/Utils.csproj", "src/Core/Utils/"]
COPY ["src/Infrastructure/Mail/Mail.csproj", "src/Infrastructure/Mail/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
COPY ["src/Infrastructure/Redis/Redis.csproj", "src/Infrastructure/Redis/"]
COPY ["src/Infrastructure/S3/S3.csproj", "src/Infrastructure/S3/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Services/Main/Main.Application/Main.Application.csproj", "src/Services/Main/Main.Application/"]
COPY ["src/Services/Main/Main.Persistence/Main.Persistence.csproj", "src/Services/Main/Main.Persistence/"]
COPY ["src/Services/Main/Main.Cache/Main.Cache.csproj", "src/Services/Main/Main.Cache/"]
COPY ["src/Infrastructure/Integrations/ExchangeRate/ExchangeRate.csproj", "src/Infrastructure/Integrations/ExchangeRate/"]
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
