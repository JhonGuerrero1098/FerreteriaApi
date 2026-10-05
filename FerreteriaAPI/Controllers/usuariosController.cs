using Microsoft.AspNetCore.Identity;
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
        private readonly IPasswordHasher<Usuarios> _passwordHasher;

        public UsuariosController(FerreteriaContext context, IPasswordHasher<Usuarios> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
                IdRol = dto.IdRol,
                Estado = true,
                FechaRegistro = DateTime.Now
            };
            nuevoUsuario.ContrasenaHash = _passwordHasher.HashPassword(nuevoUsuario, dto.Contrasena);

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente", usuario = nuevoUsuario });
        }

        // POST: api/Usuarios/Login
        [HttpPost("Login")]
        public async Task<ActionResult> Login(UsuarioLoginDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (usuario == null || usuario.Estado == false)
            {
                return Unauthorized("Correo o contraseña incorrectos.");
            }

            PasswordVerificationResult resultado;
            try
            {
                resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.ContrasenaHash, dto.Contrasena);
            }
            catch (FormatException)
            {
                // Usuarios antiguos con la contraseña guardada en texto plano (no es un hash válido)
                resultado = PasswordVerificationResult.Failed;
            }

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Correo o contraseña incorrectos.");
            }

            // El hash fue generado con parámetros antiguos: se actualiza al formato actual
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.ContrasenaHash = _passwordHasher.HashPassword(usuario, dto.Contrasena);
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Inicio de sesión exitoso", usuario });
        }
    }
}
