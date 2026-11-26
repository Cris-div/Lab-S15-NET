using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades.Models;

public partial class Mascotas
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Especie { get; set; } = null!;

    public int Edad { get; set; }

    public decimal Peso { get; set; }

    public int PropietarioId { get; set; }

    public virtual Propietarios Propietario { get; set; } = null!;
}
