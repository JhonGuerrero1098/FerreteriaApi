using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreteriaAPI.Models;


[Table("Proveedores")]
public partial class Proveedor
{
    [Key]
    public int Id { get; set; }

    public string? NombreProveedor { get; set; }

    public string? NombreContacto { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
