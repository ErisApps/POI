FROM mcr.microsoft.com/dotnet/runtime:5.0.4

RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig

COPY bin/Release/net5.0/publish/ App/
WORKDIR /App

VOLUME /Data

ENTRYPOINT ["dotnet", "PoiDiscordDotNet.dll"]

