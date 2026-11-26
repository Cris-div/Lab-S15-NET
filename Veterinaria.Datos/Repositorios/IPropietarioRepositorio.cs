using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public interface IPropietarioRepositorio
{
    Task<List<Propietarios>> ListarAsync();
    Task<Propietarios?> ObtenerAsync(int id);
    Task<bool> ExisteAsync(int id);
    Task<bool> DocumentoExisteAsync(string documento, int excluirId = 0);
    Task<bool> TieneMascotasAsync(int id);
    Task CrearAsync(Propietarios propietario);
    Task ActualizarAsync(Propietarios propietario);
    Task EliminarAsync(Propietarios propietario);
}
