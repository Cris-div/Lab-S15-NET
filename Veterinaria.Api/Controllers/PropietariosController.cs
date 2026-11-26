using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Recepcionista")]
public class PropietariosController(IPropietarioServicio servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PropietarioDto>>> Listar() => Ok(await servicio.ListarAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType<PropietarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropietarioDto>> Obtener(int id)
    {
        var dto = await servicio.ObtenerAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("{id:int}/mascotas")]
    [ProducesResponseType<List<MascotaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MascotaDto>>> ListarMascotas(int id)
    {
        var datos = await servicio.ListarMascotasAsync(id);
        return datos is null ? NotFound() : Ok(datos);
    }

    [HttpPost]
    [ProducesResponseType<PropietarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropietarioDto>> Crear(PropietarioCrearDto dto)
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, PropietarioActualizarDto dto)
    {
        try { return await servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException ex)
        { return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        try { return await servicio.EliminarAsync(id) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException ex)
        { return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = ex.Message }); }
    }
}
