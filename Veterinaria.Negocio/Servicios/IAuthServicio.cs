using Veterinaria.Entidades.DTOs;

namespace Veterinaria.Negocio.Servicios;

public interface IAuthServicio
{
    Task RegistrarAsync(RegistroDto dto);
    Task<TokenDto?> IniciarSesionAsync(LoginDto dto);
}
