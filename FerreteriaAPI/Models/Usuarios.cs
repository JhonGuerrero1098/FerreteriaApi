using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FerreteriaAPI.Models;

[Table("Usuarios")]         
public partial class Usuarios
{
    [Key]
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public int IdRol { get; set; }

    public bool? Estado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Roles IdRolNavigation { get; set; } = null!;
}
