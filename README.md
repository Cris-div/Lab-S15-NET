# Laboratorio S15 - Microservicios con API Gateway y Ocelot (Veterinaria)

Solución de microservicios en .NET 10 con **Ocelot 25.x** como único punto de entrada:

- **GatewayApi** (`http://localhost:5000`): API Gateway con Ocelot y validación JWT (`Bearer`).
- **Veterinaria.Api** (`http://localhost:5055`): Microservicio 1 (API en capas de la S14 con autenticación JWT, roles `Veterinario` y `Recepcionista`, y base de datos SQL Server / PostgreSQL).
- **CitasApi** (`http://localhost:5056`): Microservicio 2 independiente con datos en memoria (citas veterinarias) y validación JWT con la misma clave, emisor y audiencia.

## Ejecución local (3 terminales en VS Code)

Configura la misma clave JWT en las 3 terminales (sin subirla a Git):

```powershell
$env:Jwt__Clave = "ClaveSuperSecretaDeAlMenos32CaracteresParaLabS15!"
$env:ConnectionStrings__Veterinaria = "Server=.\SQLEXPRESS;Database=VeterinariaS14DB;Trusted_Connection=True;TrustServerCertificate=True;"
```

1. Terminal 1 (Microservicio 1): `dotnet run --project Veterinaria.Api`
2. Terminal 2 (Microservicio 2): `dotnet run --project CitasApi`
3. Terminal 3 (API Gateway): `dotnet run --project GatewayApi`

## Pruebas

Abre `Gateway.http` en VS Code (con la extensión REST Client) o ejecuta `scripts/Test-Gateway.ps1` para comprobar todos los códigos HTTP: `200`, `201`, `204`, `401`, `403`, `404`, `502` y la llamada directa sin token a `CitasApi` (`401`).

## Despliegue en la nube (plan gratuito)

- Base de datos: **Neon** (PostgreSQL gratuito), ejecutando `database/Veterinaria.postgres.sql`.
- Servicios web: **Render** (3 Web Services con Docker usando `Veterinaria.Api/Dockerfile`, `CitasApi/Dockerfile` y `GatewayApi/Dockerfile`).
- Variables de entorno en la nube:
  - `Veterinaria.Api`: `DatabaseProvider=Postgres`, `ConnectionStrings__Veterinaria=<cadena Neon>`, `Jwt__Clave=<clave compartida>`.
  - `CitasApi`: `Jwt__Clave=<clave compartida>`.
  - `GatewayApi`: `Jwt__Clave=<clave compartida>` y actualizar `GatewayApi/ocelot.json` (o `ocelot.Production.json`) con los hosts públicos `https` puerto `443` y `GlobalConfiguration.BaseUrl`.
