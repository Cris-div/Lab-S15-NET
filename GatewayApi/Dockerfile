FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .
COPY GatewayApi/GatewayApi.csproj GatewayApi/
RUN dotnet restore GatewayApi/GatewayApi.csproj

COPY GatewayApi/ GatewayApi/
RUN dotnet publish GatewayApi/GatewayApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "dotnet GatewayApi.dll --urls http://0.0.0.0:${PORT:-8080}"]
