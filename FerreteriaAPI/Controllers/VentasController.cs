using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FerreteriaAPI.Models;

[Route("api/[controller]")]
[ApiController]
public class VentasController : ControllerBase
{
    private readonly FerreteriaContext _context;

    public VentasController(FerreteriaContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ventas>>> GetVentas()
    {
        return await _context.Ventas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.DetalleVenta)
                .ThenInclude(d => d.IdProductoNavigation)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Ventas>> GetVenta(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.DetalleVenta)
                .ThenInclude(d => d.IdProductoNavigation)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
        {
            return NotFound();
        }

        return venta;
    }

    [HttpPost]
    public async Task<ActionResult<Ventas>> PostVenta(Ventas venta)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (venta.DetalleVenta == null || !venta.DetalleVenta.Any())
            {
                return BadRequest("La venta debe incluir al menos un producto.");
            }
            decimal valorTotalCalculado = 0;

            foreach (var detalle in venta.DetalleVenta)
            {
                var producto = await _context.Productos.FindAsync(detalle.IdProducto);

                if (producto == null)
                {
                    return NotFound($"El producto con ID {detalle.IdProducto} no existe.");
                }

                if (producto.Stock < detalle.Cantidad)
                {
                    return BadRequest($"Stock insuficiente para el producto {producto.Nombre}. Stock actual: {producto.Stock}, solicitado: {detalle.Cantidad}");
                }

                detalle.PrecioUnitario = producto.Precio;
                valorTotalCalculado += producto.Precio * detalle.Cantidad;

                producto.Stock -= detalle.Cantidad;
                _context.Productos.Update(producto);

                if (producto.Stock <= 10)
                {
                    EnviarSmsAlAdmin(producto.Nombre, producto.Stock);
                }
            }

            venta.Fecha = DateTime.Now;
            venta.ValorTotal = valorTotalCalculado;

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, venta);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Ocurrió un error al procesar la venta: {ex.Message}");
        }
    }

    private void EnviarSmsAlAdmin(string nombreProducto, int stockActual)
    {
        string mensaje = $"Stock insuficiente para el producto {nombreProducto}. Stock actual: {stockActual}";
    }

    // GET: api/Ventas/PorFecha?fecha=2026-09-30
    [HttpGet("PorFecha")]
    public async Task<ActionResult<IEnumerable<Ventas>>> GetVentasPorFecha([FromQuery] DateTime fecha)
    {
        var inicioDia = fecha.Date;
        var finDia = fecha.Date.AddDays(1).AddTicks(-1);

        var ventas = await _context.Ventas
            .Where(v => v.Fecha >= inicioDia && v.Fecha <= finDia)
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.DetalleVenta)
                .ThenInclude(d => d.IdProductoNavigation)
            .ToListAsync();

        return ventas;
    }

    // GET: api/Ventas/ProductoMasVendido (Histórico General)
    [HttpGet("ProductoMasVendido")]
    public async Task<ActionResult> GetProductoMasVendido()
    {
        var productoMasVendido = await _context.DetalleVenta
            .GroupBy(d => d.IdProducto)
            .Select(g => new
            {
                IdProducto = g.Key,
                TotalCantidadVendida = g.Sum(d => d.Cantidad)
            })
            .OrderByDescending(x => x.TotalCantidadVendida)
            .FirstOrDefaultAsync();

        if (productoMasVendido == null)
        {
            return NotFound("No hay registros de ventas suficientes para determinar el producto más vendido.");
        }

        var productoInfo = await _context.Productos.FindAsync(productoMasVendido.IdProducto);

        return Ok(new
        {
            mensaje = "Producto más vendido de la ferretería (General)",
            producto = productoInfo,
            totalUnidadesVendidas = productoMasVendido.TotalCantidadVendida
        });
    }

    // GET: api/Ventas/ProductoMasVendidoDiario?fecha=2026-09-30
    [HttpGet("ProductoMasVendidoDiario")]
    public async Task<ActionResult> GetProductoMasVendidoDiario([FromQuery] DateTime? fecha)
    {
        DateTime fechaBuscada = fecha ?? DateTime.Today;
        var inicioDia = fechaBuscada.Date;
        var finDia = fechaBuscada.Date.AddDays(1).AddTicks(-1);

        var productoMasVendido = await _context.DetalleVenta
            .Join(_context.Ventas,
                  d => d.IdVenta,
                  v => v.Id,
                  (d, v) => new { Detalle = d, Venta = v })
            .Where(x => x.Venta.Fecha >= inicioDia && x.Venta.Fecha <= finDia)
            .GroupBy(x => x.Detalle.IdProducto)
            .Select(g => new
            {
                IdProducto = g.Key,
                TotalCantidadVendida = g.Sum(x => x.Detalle.Cantidad)
            })
            .OrderByDescending(x => x.TotalCantidadVendida)
            .FirstOrDefaultAsync();

        if (productoMasVendido == null)
        {
            return NotFound("No hay registros de ventas suficientes para la fecha seleccionada.");
        }

        var productoInfo = await _context.Productos.FindAsync(productoMasVendido.IdProducto);

        return Ok(new
        {
            mensaje = "Producto más vendido del día",
            fechaConsulta = fechaBuscada.ToString("yyyy-MM-dd"),
            producto = productoInfo,
            totalUnidadesVendidas = productoMasVendido.TotalCantidadVendida
        });
    }
}