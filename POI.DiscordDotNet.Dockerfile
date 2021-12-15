# Set up runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:6.0.1 AS runtime-env
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig

# Set up build environment
FROM mcr.microsoft.com/dotnet/sdk:6.0.100 AS build-env
WORKDIR /src

COPY ["nuget.config", ""]
COPY ["POI.Core/POI.Core.csproj", "POI.Core/"]
RUN dotnet restore "POI.Core/POI.Core.csproj" --configfile "nuget.config"
COPY ["POI.DiscordDotNet/POI.DiscordDotNet.csproj", "POI.DiscordDotNet/"]
RUN dotnet restore "POI.DiscordDotNet/POI.DiscordDotNet.csproj" --configfile "nuget.config"

COPY ["POI.Core/.", "POI.Core/"]
COPY ["POI.DiscordDotNet/.", "POI.DiscordDotNet/"]

WORKDIR ./POI.DiscordDotNet
RUN dotnet build "POI.DiscordDotNet.csproj" --configfile "../nuget.config" -c Release -o /app/build
RUN dotnet publish "POI.DiscordDotNet.csproj" --configfile "../nuget.config" -c Release -o /app/publish

FROM runtime-env as final
WORKDIR /app
COPY --from=build-env /app/publish .
VOLUME /Data
EXPOSE 80

ENTRYPOINT ["dotnet", "POI.DiscordDotNet.dll"]