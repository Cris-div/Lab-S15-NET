using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class MascotaServicio(IMascotaRepositorio mascotas,
    IPropietarioRepositorio propietarios, IMapper mapper) : IMascotaServicio
{
    public async Task<List<MascotaDto>> ListarAsync(int? propietarioId = null, string? buscar = null, string? especie = null) =>
        mapper.Map<List<MascotaDto>>(await mascotas.ListarAsync(propietarioId, buscar, especie));

    public async Task<MascotaDto?> ObtenerAsync(int id)
    {
        var mascota = await mascotas.ObtenerAsync(id);
        return mascota is null ? null : mapper.Map<MascotaDto>(mascota);
    }

    public async Task<MascotaDto> CrearAsync(MascotaCrearDto dto)
    {
        await ValidarPropietarioAsync(dto.PropietarioId);
        var mascota = mapper.Map<Mascotas>(dto);
        try { await mascotas.CrearAsync(mascota); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        { throw new ReglaNegocioException("El propietario seleccionado ya no existe."); }
        return mapper.Map<MascotaDto>((await mascotas.ObtenerAsync(mascota.Id))!);
    }

    public async Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto)
    {
        var mascota = await mascotas.ObtenerAsync(id);
        if (mascota is null) return false;
        await ValidarPropietarioAsync(dto.PropietarioId);
        mapper.Map(dto, mascota);
        try { await mascotas.ActualizarAsync(mascota); }
        catch (DbUpdateConcurrencyException) { return false; }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        { throw new ReglaNegocioException("El propietario seleccionado ya no existe."); }
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await mascotas.ObtenerAsync(id);
        if (mascota is null) return false;
        try { await mascotas.EliminarAsync(mascota); }
        catch (DbUpdateConcurrencyException) { return false; }
        return true;
    }

    private async Task ValidarPropietarioAsync(int id)
    {
        if (!await propietarios.ExisteAsync(id))
            throw new ReglaNegocioException("La mascota debe pertenecer a un propietario registrado.");
    }
}
