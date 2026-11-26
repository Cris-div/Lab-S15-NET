using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public interface IMascotaRepositorio
{
    Task<List<Mascotas>> ListarAsync(int? propietarioId = null, string? buscar = null, string? especie = null);
    Task<Mascotas?> ObtenerAsync(int id);
    Task CrearAsync(Mascotas mascota);
    Task ActualizarAsync(Mascotas mascota);
    Task EliminarAsync(Mascotas mascota);
}
