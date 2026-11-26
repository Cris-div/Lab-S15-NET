namespace Veterinaria.Entidades.DTOs;

public class MascotaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public int Edad { get; set; }
    public decimal Peso { get; set; }
    public int PropietarioId { get; set; }
    public string PropietarioNombre { get; set; } = string.Empty;
}
