using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Models;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public class UsuarioRepositorio(VeterinariaContext context) : IUsuarioRepositorio
{
    public Task<Usuario?> BuscarPorCorreoAsync(string correo) =>
        context.Usuarios.SingleOrDefaultAsync(u => u.Correo == correo);

    public async Task AgregarAsync(Usuario usuario) => await context.Usuarios.AddAsync(usuario);

    public async Task GuardarAsync() => await context.SaveChangesAsync();
}
