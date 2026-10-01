using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FerreteriaAPI.Models;

[Table("Ventas")]
public partial class Ventas
{
    [Key]
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public decimal ValorTotal { get; set; }

    public int IdCliente { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Clientes? IdClienteNavigation { get; set; } 
}
