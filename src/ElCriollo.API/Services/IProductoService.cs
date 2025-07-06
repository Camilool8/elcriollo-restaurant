using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz simplificada para el servicio de gestión de productos de El Criollo
    /// </summary>
    public interface IProductoService
    {
        // ============================================================================
        // GESTIÓN DE PRODUCTOS
        // ============================================================================

        /// <summary>
        /// Obtiene el menú digital completo con productos dominicanos
        /// </summary>
        /// <param name="incluirNoDisponibles">Incluir productos no disponibles</param>
        /// <returns>Menú digital completo</returns>
        Task<MenuDigitalViewModel> GetMenuDigitalAsync(bool incluirNoDisponibles = false);

        /// <summary>
        /// Obtiene un producto específico por ID
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Datos del producto</returns>
        Task<ProductoResponse?> GetProductoByIdAsync(int productoId);

        /// <summary>
        /// Obtiene todos los productos disponibles
        /// </summary>
        /// <returns>Lista de productos disponibles</returns>
        Task<IEnumerable<ProductoResponse>> GetProductosDisponiblesAsync();

        /// <summary>
        /// Crea un nuevo producto básico
        /// </summary>
        /// <param name="crearProductoRequest">Datos del producto</param>
        /// <param name="usuarioId">ID del usuario que crea</param>
        /// <returns>Producto creado</returns>
        Task<ProductoResponse> CrearProductoAsync(CrearProductoRequest crearProductoRequest, int usuarioId);

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="actualizarRequest">Datos a actualizar</param>
        /// <param name="usuarioId">ID del usuario que actualiza</param>
        /// <returns>Producto actualizado</returns>
        Task<ProductoResponse?> ActualizarProductoAsync(int productoId, ActualizarProductoRequest actualizarRequest, int usuarioId);

        /// <summary>
        /// Activa/desactiva la disponibilidad de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="disponible">True para activar, false para desactivar</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se cambió exitosamente</returns>
        Task<bool> CambiarDisponibilidadProductoAsync(int productoId, bool disponible, int usuarioId);

        // ============================================================================
        // CONSULTAS
        // ============================================================================

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<IEnumerable<ProductoResponse>> GetProductosPorCategoriaAsync(int categoriaId);

        /// <summary>
        /// Obtiene todas las categorías disponibles
        /// </summary>
        /// <returns>Lista de categorías con conteo de productos</returns>
        Task<IEnumerable<CategoriaBasicaResponse>> GetCategoriasAsync();

        /// <summary>
        /// Busca productos por nombre (búsqueda simple)
        /// </summary>
        /// <param name="nombre">Nombre o parte del nombre</param>
        /// <returns>Lista de productos que coinciden</returns>
        Task<IEnumerable<ProductoResponse>> BuscarProductosPorNombreAsync(string nombre);

        // ============================================================================
        // GESTIÓN DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Verifica disponibilidad de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad requerida</param>
        /// <returns>True si está disponible en la cantidad solicitada</returns>
        Task<bool> VerificarDisponibilidadAsync(int productoId, int cantidad = 1);

        /// <summary>
        /// Obtiene el stock actual de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Información de stock</returns>
        Task<StockProductoResult> GetStockProductoAsync(int productoId);

        /// <summary>
        /// Actualiza el stock de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="nuevaCantidad">Nueva cantidad en stock</param>
        /// <param name="usuarioId">ID del usuario que actualiza</param>
        /// <returns>True si se actualizó exitosamente</returns>
        Task<bool> ActualizarStockAsync(int productoId, int nuevaCantidad, int usuarioId);

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        /// <returns>Lista de productos que necesitan restock</returns>
        Task<IEnumerable<ProductoStockBajoResponse>> GetProductosStockBajoAsync();

        // ============================================================================
        // CÁLCULOS
        // ============================================================================

        /// <summary>
        /// Calcula el precio total de una lista de productos
        /// </summary>
        /// <param name="items">Lista de productos con cantidades</param>
        /// <returns>Precio total calculado</returns>
        Task<CalculoPrecioResult> CalcularPrecioTotalAsync(List<ItemCalculoRequest> items);

        /// <summary>
        /// Calcula el precio con descuento si aplica
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad</param>
        /// <returns>Precio con descuento aplicado</returns>
        Task<decimal> CalcularPrecioConDescuentoAsync(int productoId, int cantidad);

        // ============================================================================
        // COMBOS
        // ============================================================================

        /// <summary>
        /// Obtiene todos los combos disponibles
        /// </summary>
        /// <returns>Lista de combos disponibles</returns>
        Task<IEnumerable<ComboBasicoResponse>> GetCombosDisponiblesAsync();

        /// <summary>
        /// Obtiene un combo específico con sus productos
        /// </summary>
        /// <param name="comboId">ID del combo</param>
        /// <returns>Datos del combo</returns>
        Task<ComboBasicoResponse?> GetComboByIdAsync(int comboId);

        /// <summary>
        /// Verifica disponibilidad de un combo
        /// </summary>
        /// <param name="comboId">ID del combo</param>
        /// <returns>True si todos los productos del combo están disponibles</returns>
        Task<bool> VerificarDisponibilidadComboAsync(int comboId);

        // ============================================================================
        // VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Valida que un producto puede ser creado
        /// </summary>
        /// <param name="crearProductoRequest">Datos del producto</param>
        /// <returns>Resultado de validación</returns>
        Task<ValidacionProductoResult> ValidarProductoAsync(CrearProductoRequest crearProductoRequest);

        /// <summary>
        /// Valida que una lista de productos es válida para una orden
        /// </summary>
        /// <param name="items">Lista de productos</param>
        /// <returns>Resultado de validación</returns>
        Task<ValidacionProductosResult> ValidarListaProductosAsync(List<ItemCalculoRequest> items);
    }

    // ============================================================================
    // MODELOS PARA RESULTADOS
    // ============================================================================

    /// <summary>
    /// Resultado de stock de producto
    /// </summary>
    public class StockProductoResult
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }
        public int CantidadMinima { get; set; }
        public bool RequiereRestock { get; set; }
        public string EstadoStock { get; set; } = string.Empty;
    }

    /// <summary>
    /// Producto con stock bajo
    /// </summary>
    public class ProductoStockBajoResponse
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }
        public int CantidadMinima { get; set; }
        public bool EsCritico { get; set; }
        public string Recomendacion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado de cálculo de precio
    /// </summary>
    public class CalculoPrecioResult
    {
        public decimal Subtotal { get; set; }
        public int TotalItems { get; set; }
        public List<DetalleCalculoItem> Detalles { get; set; } = new();
        public string SubtotalFormateado => $"RD$ {Subtotal:N2}";
    }

    /// <summary>
    /// Detalle de cálculo por item
    /// </summary>
    public class DetalleCalculoItem
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string SubtotalFormateado => $"RD$ {Subtotal:N2}";
    }

    /// <summary>
    /// Item de cálculo de precio
    /// </summary>
    public class ItemCalculoRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    /// <summary>
    /// Combo
    /// </summary>
    public class ComboBasicoResponse
    {
        public int ComboID { get; set; }
        public string NombreCombo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"RD$ {Precio:N2}";
        public bool Disponible { get; set; }
        public List<ProductoComboBasico> Productos { get; set; } = new();
    }

    /// <summary>
    /// Producto en combo
    /// </summary>
    public class ProductoComboBasico
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    /// <summary>
    /// Categoría
    /// </summary>
    public class CategoriaBasicaResponse
    {
        public int CategoriaID { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CantidadProductos { get; set; }
        public int ProductosDisponibles { get; set; }
    }

    /// <summary>
    /// Resultado de validación de producto
    /// </summary>
    public class ValidacionProductoResult
    {
        public bool EsValido { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
    }

    /// <summary>
    /// Resultado de validación de lista de productos
    /// </summary>
    public class ValidacionProductosResult
    {
        public bool TodosValidos { get; set; }
        public List<string> ProductosInvalidos { get; set; } = new();
        public List<string> ProductosNoDisponibles { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
    }

    /// <summary>
    /// Request para actualizar producto
    /// </summary>
    public class ActualizarProductoRequest
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? CategoriaId { get; set; }
        public bool? Disponible { get; set; }
    }
}