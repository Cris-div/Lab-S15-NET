using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades.Models;

public partial class Propietarios
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Documento { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public virtual ICollection<Mascotas> Mascotas { get; set; } = new List<Mascotas>();
}
