FROM mcr.microsoft.com/dotnet/runtime:5.0

COPY bin/Release/net5.0/publish/ App/
WORKDIR /App

VOLUME /Data

ENTRYPOINT ["dotnet", "PoiDiscordDotNet.dll"]

