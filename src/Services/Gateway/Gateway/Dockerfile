FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS subset
WORKDIR /src
RUN dotnet tool install --tool-path /tools dotnet-subset --version 0.3.2
ENV DOTNET_ROLL_FORWARD=Major
COPY . .
RUN /tools/dotnet-subset restore "src/Services/Gateway/Gateway/Gateway.csproj" --root-directory /src --output /subset

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --from=subset /subset/ ./
RUN dotnet restore "src/Services/Gateway/Gateway/Gateway.csproj"
COPY . .
RUN mkdir -p /app/fusion \
    && if [ -s /src/artifacts/graphql/Gateway/gateway.far ]; then \
         cp /src/artifacts/graphql/Gateway/gateway.far /app/fusion/gateway.far; \
       else \
         touch /app/fusion/gateway.far; \
       fi
WORKDIR "/src/src/Services/Gateway/Gateway"
RUN dotnet build "./Gateway.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gateway.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/fusion/gateway.far ./gateway.far
ENTRYPOINT ["dotnet", "Gateway.dll"]
