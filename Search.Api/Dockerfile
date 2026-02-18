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

RUN apt-get update && \
    apt-get install -y --no-install-recommends clang libc6-dev build-essential && \
    rm -rf /var/lib/apt/lists/*

COPY ["Search.Api/Search.Api.csproj", "Search.Api/"]
COPY ["src/Services/Search/Search.Entities/Search.Entities.csproj", "src/Services/Search/Search.Entities/"]
COPY ["src/Services/Search/Search.Persistence/Search.Persistence.csproj", "src/Services/Search/Search.Persistence/"]
COPY ["src/Services/Search/Search.Abstractions/Search.Abstractions.csproj", "src/Services/Search/Search.Abstractions/"]
COPY ["src/Services/Search/Search.Enums/Search.Enums.csproj", "src/Services/Search/Search.Enums/"]
COPY ["src/Core/Exceptions/Exceptions.csproj", "src/Core/Exceptions/"]
COPY ["src/Core/Abstractions/Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/Core/Attributes/Attributes.csproj", "src/Core/Attributes/"]
COPY ["src/Core/Enums/Enums.csproj", "src/Core/Enums/"]
COPY ["src/Services/Search/Search.Application/Search.Application.csproj", "src/Services/Search/Search.Application/"]
COPY ["src/Services/Application.Common.Aot/Application.Common.Aot.csproj", "src/Services/Application.Common.Aot/"]
COPY ["src/Core/Contracts/Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Services/Application.Common/Application.Common.csproj", "src/Services/Application.Common/"]
COPY ["src/Infrastructure/Security/Security.csproj", "src/Infrastructure/Security/"]
COPY ["src/Core/Utils/Utils.csproj", "src/Core/Utils/"]
COPY ["src/Services/Api.Common/Api.Common.csproj", "src/Services/Api.Common/"]
COPY ["src/Core/Extensions/Extensions.csproj", "src/Core/Extensions/"]
COPY ["src/Infrastructure/RabbitMq/RabbitMq.csproj", "src/Infrastructure/RabbitMq/"]
RUN dotnet restore "Search.Api/Search.Api.csproj"
COPY . .

WORKDIR "/src/Search.Api"

RUN dotnet build "./Search.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Search.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -r linux-x64 \
    -p:PublishAot=true \
    -p:SelfContained=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=Link \
    -p:StripSymbols=true \
    -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["./Search.Api"]
