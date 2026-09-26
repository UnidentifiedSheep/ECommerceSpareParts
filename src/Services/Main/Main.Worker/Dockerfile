FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS subset
WORKDIR /src
RUN dotnet tool install --tool-path /tools dotnet-subset --version 0.3.2
ENV DOTNET_ROLL_FORWARD=Major
COPY . .
RUN /tools/dotnet-subset restore "src/Services/Main/Main.Worker/Main.Worker.csproj" --root-directory /src --output /subset

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --from=subset /subset/ ./
RUN dotnet restore "src/Services/Main/Main.Worker/Main.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Main/Main.Worker"
RUN dotnet build "./Main.Worker.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Main.Worker.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Main.Worker.dll"]
