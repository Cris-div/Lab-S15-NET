using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Models;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public class MascotaRepositorio(VeterinariaContext context) : IMascotaRepositorio
{
    public async Task<List<Mascotas>> ListarAsync(int? propietarioId = null, string? buscar = null, string? especie = null)
    {
        var consulta = context.Mascotas.AsNoTracking().Include(m => m.Propietario).AsQueryable();
        if (propietarioId.HasValue) consulta = consulta.Where(m => m.PropietarioId == propietarioId.Value);
        if (!string.IsNullOrWhiteSpace(buscar)) consulta = consulta.Where(m => m.Nombre.Contains(buscar.Trim()));
        if (!string.IsNullOrWhiteSpace(especie)) consulta = consulta.Where(m => m.Especie == especie.Trim());
        return await consulta.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<Mascotas?> ObtenerAsync(int id) =>
        await context.Mascotas.Include(m => m.Propietario).FirstOrDefaultAsync(m => m.Id == id);

    public async Task CrearAsync(Mascotas mascota)
    {
        await context.Mascotas.AddAsync(mascota);
        await context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Mascotas mascota)
    {
        // DetectChanges persiste los escalares sin marcar al propietario como modificado.
        await context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Mascotas mascota)
    {
        context.Mascotas.Remove(mascota);
        await context.SaveChangesAsync();
    }
}
