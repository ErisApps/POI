# Set up runtime environment
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime-env
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig

# Set up build environment
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

COPY *.sln .
COPY POI.Core/POI.Core.csproj ./POI.Core/
COPY POI.DiscordDotNet/POI.DiscordDotNet.csproj ./POI.DiscordDotNet/
RUN dotnet restore

COPY POI.Core/. ./POI.Core/
COPY POI.DiscordDotNet/. ./POI.DiscordDotNet/

WORKDIR ./POI.DiscordDotNet
RUN dotnet publish -c Release -o out

FROM runtime-env
WORKDIR /app
COPY --from=build-env /app/POI.DiscordDotNet/out .
VOLUME /Data

ENTRYPOINT ["dotnet", "POI.DiscordDotNet.dll"]