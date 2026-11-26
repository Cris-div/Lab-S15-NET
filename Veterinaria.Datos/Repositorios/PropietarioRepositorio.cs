using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Models;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public class PropietarioRepositorio(VeterinariaContext context) : IPropietarioRepositorio
{
    public async Task<List<Propietarios>> ListarAsync() =>
        await context.Propietarios.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();

    public async Task<Propietarios?> ObtenerAsync(int id) =>
        await context.Propietarios.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> ExisteAsync(int id) =>
        await context.Propietarios.AnyAsync(p => p.Id == id);

    public async Task<bool> DocumentoExisteAsync(string documento, int excluirId = 0) =>
        await context.Propietarios.AnyAsync(p => p.Documento == documento && p.Id != excluirId);

    public async Task<bool> TieneMascotasAsync(int id) =>
        await context.Mascotas.AnyAsync(m => m.PropietarioId == id);

    public async Task CrearAsync(Propietarios propietario)
    {
        await context.Propietarios.AddAsync(propietario);
        await context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Propietarios propietario)
    {
        // ObtenerAsync devuelve la entidad seguida por este mismo contexto.
        await context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Propietarios propietario)
    {
        context.Propietarios.Remove(propietario);
        await context.SaveChangesAsync();
    }
}
