FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["source/POI.DiscordDotNet/POI.DiscordDotNet.csproj", "source/POI.DiscordDotNet/"]
COPY ["source/POI.Persistence.EFCore.Npgsql/POI.Persistence.EFCore.Npgsql.csproj", "source/POI.Persistence.EFCore.Npgsql/"]
COPY ["source/POI.Persistence/POI.Persistence.csproj", "source/POI.Persistence/"]
COPY ["source/POI.Persistence.Domain/POI.Persistence.Domain.csproj", "source/POI.Persistence.Domain/"]
COPY ["source/POI.ThirdParty.BeatSaver/POI.ThirdParty.BeatSaver.csproj", "source/POI.ThirdParty.BeatSaver/"]
COPY ["source/POI.ThirdParty.Core/POI.ThirdParty.Core.csproj", "source/POI.ThirdParty.Core/"]
COPY ["source/POI.ThirdParty.BeatSavior/POI.ThirdParty.BeatSavior.csproj", "source/POI.ThirdParty.BeatSavior/"]
COPY ["source/POI.ThirdParty.ScoreSaber/POI.ThirdParty.ScoreSaber.csproj", "source/POI.ThirdParty.ScoreSaber/"]
RUN dotnet restore "source/POI.DiscordDotNet/POI.DiscordDotNet.csproj"
COPY . .
WORKDIR "/src/source/POI.DiscordDotNet"
RUN dotnet build "POI.DiscordDotNet.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "POI.DiscordDotNet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POI.DiscordDotNet.dll"]