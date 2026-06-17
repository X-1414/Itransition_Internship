FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj WITH correct path
COPY task4/AdminDashboard/*.csproj ./AdminDashboard/

WORKDIR /src/AdminDashboard

RUN dotnet restore

# copy everything
COPY task4/AdminDashboard/ ./

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AdminDashboard.dll"]