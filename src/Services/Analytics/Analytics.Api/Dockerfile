FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 7292

ENV TZ=Europe/Moscow


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Analytics.Api/Analytics.Api.csproj", "Analytics.Api/"]
RUN dotnet restore "Analytics.Api/Analytics.Api.csproj"
COPY . .
WORKDIR "/src/Analytics.Api"
RUN dotnet build "./Analytics.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Analytics.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /usr/local/share/ca-certificates/custom \
    && for cert in /app/certs/*.pfx; do \
        openssl pkcs12 -in "$cert" -nokeys -out "/usr/local/share/ca-certificates/custom/$(basename "$cert").crt" -clcerts -passin pass:PASSWORD; \
    done \
    && chmod 644 /usr/local/share/ca-certificates/custom/* \
    && update-ca-certificates

ENTRYPOINT ["dotnet", "Analytics.Api.dll"]
