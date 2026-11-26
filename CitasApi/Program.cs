using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MICROSERVICIO 2: CitasApi (datos en memoria, sin base de datos ni clases compartidas).
// Valida el token JWT con la misma clave, emisor y audiencia que Veterinaria.Api.
var jwtKey = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configura Jwt__Clave con al menos 32 bytes mediante variables de entorno o user-secrets.");

var jwtIssuer = builder.Configuration["Jwt:Emisor"] ?? throw new InvalidOperationException("Falta Jwt:Emisor.");
var jwtAudience = builder.Configuration["Jwt:Audiencia"] ?? throw new InvalidOperationException("Falta Jwt:Audiencia.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = "rol",
            NameClaimType = "email"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var citas = new List<Cita>
{
    new(1, "Luna", "Ana Torres", "Vacunacion anual antirrabica", "2026-11-28 09:30", "Dra. Sofia Mendoza", "Confirmada"),
    new(2, "Milo", "Ana Torres", "Control dermatologico", "2026-11-28 11:00", "Dr. Carlos Paredes", "Pendiente"),
    new(3, "Rocky", "Luis Ramos", "Desparasitacion y control de peso", "2026-11-29 15:00", "Dra. Sofia Mendoza", "Confirmada"),
    new(4, "Nala", "Carla Flores", "Revision postoperatoria", "2026-11-30 10:15", "Dr. Carlos Paredes", "Programada")
};

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "CitasApi Microservicio 2 v1"));

app.UseAuthentication();
app.UseAuthorization();

// GET /api/citas (requiere token JWT)
app.MapGet("/api/citas", () => Results.Ok(citas))
    .RequireAuthorization();

// GET /api/citas/{id} (requiere token JWT)
app.MapGet("/api/citas/{id:int}", (int id) =>
    citas.FirstOrDefault(c => c.Id == id) is { } cita
        ? Results.Ok(cita)
        : Results.NotFound())
    .RequireAuthorization();

app.Run();

record Cita(
    int Id,
    string Mascota,
    string Propietario,
    string Motivo,
    string FechaHora,
    string Veterinario,
    string Estado);
