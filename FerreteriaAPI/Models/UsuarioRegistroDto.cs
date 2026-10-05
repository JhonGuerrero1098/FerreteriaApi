namespace FerreteriaAPI.Models
{
    // DTO para recibir los datos limpios desde Swagger sin errores de navegación
    public class UsuarioRegistroDto
    {
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        // Contraseña en texto plano; se guarda hasheada en Usuarios.ContrasenaHash
        public string Contrasena { get; set; } = null!;
        public int IdRol { get; set; }
    }

    public class UsuarioLoginDto
    {
        public string Email { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
    }
}
