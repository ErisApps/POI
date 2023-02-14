# Set up runtime environment
FROM mcr.microsoft.com/dotnet/runtime:7.0.3 AS runtime-env
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig

# Set up build environment
# NOP

# Combine runtime and build environments
FROM runtime-env as final
WORKDIR /App
VOLUME /Data
WORKDIR /App

ENTRYPOINT ["dotnet", "POI.DiscordDotNet.dll"]