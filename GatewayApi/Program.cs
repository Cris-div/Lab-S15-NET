using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// GATEWAY: unico punto de entrada hacia Veterinaria.Api y CitasApi.
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

var jwtKey = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configura Jwt__Clave con al menos 32 bytes mediante variables de entorno o user-secrets.");

var jwtIssuer = builder.Configuration["Jwt:Emisor"] ?? throw new InvalidOperationException("Falta Jwt:Emisor.");
var jwtAudience = builder.Configuration["Jwt:Audiencia"] ?? throw new InvalidOperationException("Falta Jwt:Audiencia.");

// Valida el JWT antes de reenviar hacia las rutas protegidas definidas en ocelot.json.
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
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

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.Run();
