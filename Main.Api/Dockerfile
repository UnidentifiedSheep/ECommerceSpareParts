FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_URLS=http://+:8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Monolite/MonoliteUnicorn/MonoliteUnicorn.csproj", "Monolite/MonoliteUnicorn/"]
RUN dotnet restore "Monolite/MonoliteUnicorn/MonoliteUnicorn.csproj"
COPY . .
WORKDIR "/src/Monolite/MonoliteUnicorn"
RUN dotnet build "MonoliteUnicorn.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MonoliteUnicorn.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MonoliteUnicorn.dll"]
