using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.DTOs;

public class MascotaCrearDto
{
    [Required, StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Especie { get; set; } = string.Empty;

    [Range(0, 50)]
    public int Edad { get; set; }

    [Range(typeof(decimal), "0.01", "1000")]
    public decimal Peso { get; set; }

    [Range(1, int.MaxValue)]
    public int PropietarioId { get; set; }
}
