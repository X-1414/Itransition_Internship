FROM mcr.microsoft.com/dotnet/sdk:10.0.301 AS build
WORKDIR /src

# copy csproj first (better caching + reliability)
COPY *.csproj ./
RUN dotnet restore

# copy everything else
COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.301
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AdminDashboard.dll"]