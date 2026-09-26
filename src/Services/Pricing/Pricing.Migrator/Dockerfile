FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS subset
WORKDIR /src
RUN dotnet tool install --tool-path /tools dotnet-subset --version 0.3.2
ENV DOTNET_ROLL_FORWARD=Major
COPY . .
RUN /tools/dotnet-subset restore "src/Services/Pricing/Pricing.Migrator/Pricing.Migrator.csproj" --root-directory /src --output /subset

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --from=subset /subset/ ./
RUN dotnet restore "src/Services/Pricing/Pricing.Migrator/Pricing.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Services/Pricing/Pricing.Migrator"
RUN dotnet build "./Pricing.Migrator.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pricing.Migrator.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pricing.Migrator.dll"]
