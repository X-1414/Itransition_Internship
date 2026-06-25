FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y ffmpeg

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Playlist.dll"]