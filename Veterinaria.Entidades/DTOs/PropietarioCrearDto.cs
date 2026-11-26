using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.DTOs;

public class PropietarioCrearDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(8, MinimumLength = 8)]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El documento debe contener 8 dígitos.")]
    public string Documento { get; set; } = string.Empty;

    [Required, StringLength(15, MinimumLength = 7)]
    [RegularExpression(@"^[0-9+ -]{7,15}$", ErrorMessage = "El teléfono no tiene un formato válido.")]
    public string Telefono { get; set; } = string.Empty;
}
