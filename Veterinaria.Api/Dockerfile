FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .
COPY Veterinaria.Entidades/Veterinaria.Entidades.csproj Veterinaria.Entidades/
COPY Veterinaria.Datos/Veterinaria.Datos.csproj Veterinaria.Datos/
COPY Veterinaria.Negocio/Veterinaria.Negocio.csproj Veterinaria.Negocio/
COPY Veterinaria.Api/Veterinaria.Api.csproj Veterinaria.Api/
RUN dotnet restore Veterinaria.Api/Veterinaria.Api.csproj

COPY Veterinaria.Entidades/ Veterinaria.Entidades/
COPY Veterinaria.Datos/ Veterinaria.Datos/
COPY Veterinaria.Negocio/ Veterinaria.Negocio/
COPY Veterinaria.Api/ Veterinaria.Api/
RUN dotnet publish Veterinaria.Api/Veterinaria.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "dotnet Veterinaria.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
