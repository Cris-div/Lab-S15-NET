FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .
COPY CitasApi/CitasApi.csproj CitasApi/
RUN dotnet restore CitasApi/CitasApi.csproj

COPY CitasApi/ CitasApi/
RUN dotnet publish CitasApi/CitasApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "dotnet CitasApi.dll --urls http://0.0.0.0:${PORT:-8080}"]
