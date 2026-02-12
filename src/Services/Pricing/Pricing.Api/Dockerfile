FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


ENV ASPNETCORE_URLS=http://+:8080;https://+:7292
ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Pricing/Pricing.Api/Pricing.Api.csproj", "src/Services/Pricing/Pricing.Api/"]
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
