using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FerreteriaAPI.Models;

namespace FerreteriaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly FerreteriaContext _context;

        public UsuariosController(FerreteriaContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // POST: api/Usuarios/Registro
        [HttpPost("Registro")]
        public async Task<ActionResult> PostUsuario(UsuarioRegistroDto dto)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (existe)
            {
                return BadRequest("Ya existe un usuario registrado con este correo.");
            }

            var nuevoUsuario = new Usuarios
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                ContrasenaHash = dto.ContrasenaHash,
                IdRol = dto.IdRol,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente", usuario = nuevoUsuario });
        }
    }

    // DTO para recibir los datos limpios desde Swagger sin errores de navegación
    public class UsuarioRegistroDto
    {
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ContrasenaHash { get; set; } = null!;
        public int IdRol { get; set; }
    }
}