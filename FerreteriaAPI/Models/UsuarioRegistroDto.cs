namespace FerreteriaAPI.Models
{
    public class UsuarioRegistroDto
    {
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ContrasenaHash { get; set; } = null!;
        public int IdRol { get; set; }
    }
}