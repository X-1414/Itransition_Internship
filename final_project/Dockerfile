FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CV_mng_sys.slnx .
COPY src/CV_mng_sys.Core/CV_mng_sys.Core.csproj src/CV_mng_sys.Core/
COPY src/CV_mng_sys.Web/CV_mng_sys.Web.csproj src/CV_mng_sys.Web/
COPY tests/CV_mng_sys.Tests/CV_mng_sys.Tests.csproj tests/CV_mng_sys.Tests/
RUN dotnet restore CV_mng_sys.slnx

COPY . .
RUN dotnet publish src/CV_mng_sys.Web/CV_mng_sys.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render sets $PORT dynamically - Kestrel must bind to it
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CV_mng_sys.Web.dll"]