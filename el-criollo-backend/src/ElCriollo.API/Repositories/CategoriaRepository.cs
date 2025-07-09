using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories;

/// <summary>
/// Repositorio para la gestión de categorías
/// </summary>
public class CategoriaRepository : BaseRepository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(ElCriolloDbContext context, ILogger<CategoriaRepository> logger) : base(context, logger)
    {
    }

    /// <summary>
    /// Obtiene todas las categorías activas
    /// </summary>
    public async Task<IEnumerable<Categoria>> GetCategoriasActivasAsync()
    {
        return await _context.Categorias
            .Where(c => c.Estado)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una categoría por nombre
    /// </summary>
    public async Task<Categoria?> GetByNombreAsync(string nombre)
    {
        return await _context.Categorias
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());
    }

    /// <summary>
    /// Verifica si existe una categoría con el nombre especificado
    /// </summary>
    public async Task<bool> ExistePorNombreAsync(string nombre, int? excludeId = null)
    {
        var nombreLower = nombre.ToLower();
        var query = _context.Categorias.Where(c => c.Nombre.ToLower() == nombreLower);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CategoriaID != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Obtiene categorías con información de productos
    /// </summary>
    public async Task<IEnumerable<Categoria>> GetCategoriasConProductosAsync()
    {
        return await _context.Categorias
            .Include(c => c.Productos.Where(p => p.Estado)) // Solo incluir productos activos
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si una categoría tiene productos
    /// </summary>
    public async Task<bool> TieneProductosAsync(int categoriaId)
    {
        return await _context.Categorias
            .Where(c => c.CategoriaID == categoriaId)
            .SelectMany(c => c.Productos)
            .AnyAsync(p => p.Estado);
    }

    /// <summary>
    /// Obtiene una categoría con sus productos
    /// </summary>
    public async Task<Categoria?> GetByIdWithProductosAsync(int id)
    {
        return await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.CategoriaID == id);
    }
} 