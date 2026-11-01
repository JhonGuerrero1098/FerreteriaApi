using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreteriaAPI.Models;

[Table("Productos")]
public partial class Producto
{
    [Key]
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public string? Descripcion { get; set; }

    public int? IdCategoria { get; set; }

    public int? IdProveedor { get; set; }

    public string? CodigoBarras { get; set; }

    public bool Activo { get; set; } = true;

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Categorias? IdCategoriaNavigation { get; set; }

    public virtual Proveedor? IdProveedorNavigation { get; set; }
}
