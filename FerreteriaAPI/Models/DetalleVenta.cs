using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreteriaAPI.Models;

[Table("DetalleVenta")]
public partial class DetalleVenta
{
    [Key]
    public int IdDetalle { get; set; }

    public int IdVenta { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Producto? IdProductoNavigation { get; set; } 

    public virtual Ventas? IdVentaNavigation { get; set; }
}
