using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class AuthServicio(IUsuarioRepositorio usuarios, IConfiguration configuration) : IAuthServicio
{
    public async Task RegistrarAsync(RegistroDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (await usuarios.BuscarPorCorreoAsync(correo) is not null)
            throw new ReglaNegocioException("Ya existe una cuenta con ese correo.");

        var usuario = new Usuario
        {
            Correo = correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
            Rol = "Recepcionista"
        };

        await usuarios.AgregarAsync(usuario);
        try { await usuarios.GuardarAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new ReglaNegocioException("Ya existe una cuenta con ese correo."); }
    }

    public async Task<TokenDto?> IniciarSesionAsync(LoginDto dto)
    {
        var usuario = await usuarios.BuscarPorCorreoAsync(dto.Correo.Trim().ToLowerInvariant());
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash)) return null;

        var ahora = DateTime.UtcNow;
        var expiracion = ahora.AddMinutes(int.Parse(configuration["Jwt:ExpiracionMinutos"]!));
        var claims = new[]
        {
            new Claim("sub", usuario.Id.ToString()),
            new Claim("email", usuario.Correo),
            new Claim("rol", usuario.Rol)
        };
        var clave = configuration["Jwt:Clave"]!;
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Emisor"],
            audience: configuration["Jwt:Audiencia"],
            claims: claims,
            notBefore: ahora,
            expires: expiracion,
            signingCredentials: credenciales);

        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}
