using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.DTOs;

public class LoginDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Clave { get; set; } = string.Empty;
}
