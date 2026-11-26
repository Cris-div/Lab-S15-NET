using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class PropietarioServicio(IPropietarioRepositorio propietarios,
    IMascotaRepositorio mascotas, IMapper mapper) : IPropietarioServicio
{
    public async Task<List<PropietarioDto>> ListarAsync() =>
        mapper.Map<List<PropietarioDto>>(await propietarios.ListarAsync());

    public async Task<PropietarioDto?> ObtenerAsync(int id)
    {
        var propietario = await propietarios.ObtenerAsync(id);
        return propietario is null ? null : mapper.Map<PropietarioDto>(propietario);
    }

    public async Task<List<MascotaDto>?> ListarMascotasAsync(int id)
    {
        if (!await propietarios.ExisteAsync(id)) return null;
        return mapper.Map<List<MascotaDto>>(await mascotas.ListarAsync(propietarioId: id));
    }

    public async Task<PropietarioDto> CrearAsync(PropietarioCrearDto dto)
    {
        await ValidarDocumentoAsync(dto.Documento);
        var propietario = mapper.Map<Propietarios>(dto);
        try { await propietarios.CrearAsync(propietario); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new ReglaNegocioException("Ya existe un propietario con ese documento."); }
        return mapper.Map<PropietarioDto>(propietario);
    }

    public async Task<bool> ActualizarAsync(int id, PropietarioActualizarDto dto)
    {
        var propietario = await propietarios.ObtenerAsync(id);
        if (propietario is null) return false;
        await ValidarDocumentoAsync(dto.Documento, id);
        mapper.Map(dto, propietario); // Conserva Id y navegación; modifica la entidad existente.
        try { await propietarios.ActualizarAsync(propietario); }
        catch (DbUpdateConcurrencyException) { return false; }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new ReglaNegocioException("Ya existe un propietario con ese documento."); }
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var propietario = await propietarios.ObtenerAsync(id);
        if (propietario is null) return false;
        if (await propietarios.TieneMascotasAsync(id))
            throw new ReglaNegocioException("No se puede eliminar un propietario con mascotas registradas.");
        try { await propietarios.EliminarAsync(propietario); }
        catch (DbUpdateConcurrencyException) { return false; }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        { throw new ReglaNegocioException("No se puede eliminar un propietario con mascotas registradas."); }
        return true;
    }

    private async Task ValidarDocumentoAsync(string documento, int excluirId = 0)
    {
        if (await propietarios.DocumentoExisteAsync(documento, excluirId))
            throw new ReglaNegocioException("Ya existe un propietario con ese documento.");
    }
}
