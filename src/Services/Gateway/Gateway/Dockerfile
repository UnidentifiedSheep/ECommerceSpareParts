FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080;https://+:8081

ENV TZ=Europe/Moscow


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Gateway/Gateway/Gateway.csproj", "src/Services/Gateway/Gateway/"]
RUN dotnet restore "src/Services/Gateway/Gateway/Gateway.csproj"
COPY . .
WORKDIR "/src/src/Services/Gateway/Gateway"
RUN dotnet build "./Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Gateway.dll"]
