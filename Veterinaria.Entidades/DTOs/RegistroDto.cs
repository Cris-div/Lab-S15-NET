using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.DTOs;

public class RegistroDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(100)]
    public string Clave { get; set; } = string.Empty;
}
