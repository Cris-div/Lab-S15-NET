using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MascotasController(IMascotaServicio servicio) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<List<MascotaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MascotaDto>>> Listar(
        [FromQuery, Range(1, int.MaxValue)] int? propietarioId,
        [FromQuery] string? buscar, [FromQuery] string? especie) =>
        Ok(await servicio.ListarAsync(propietarioId, buscar, especie));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType<MascotaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MascotaDto>> Obtener(int id)
    {
        var dto = await servicio.ObtenerAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Veterinario")]
    [ProducesResponseType<MascotaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MascotaDto>> Crear(MascotaCrearDto dto)
    {
        try
        {
            var creado = await servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
        }
        catch (ReglaNegocioException ex)
        { return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Veterinario")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, MascotaActualizarDto dto)
    {
        try { return await servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException ex)
        { return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Veterinario")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id) =>
        await servicio.EliminarAsync(id) ? NoContent() : NotFound();
}
