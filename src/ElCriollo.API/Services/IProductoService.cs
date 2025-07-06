using ElCriollo.API.Models.DTOs.Response;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de productos de El Criollo
    /// Maneja el menú dominicano, inventario y lógica de negocio específica
    /// </summary>
    public interface IProductoService
    {
        // ============================================================================
        // GESTIÓN DEL MENÚ DIGITAL DOMINICANO
        // ============================================================================

        /// <summary>
        /// Obtiene el menú digital completo con productos dominicanos categorizados
        /// </summary>
        /// <param name="incluirNoDisponibles">Incluir productos sin stock</param>
        /// <returns>Menú digital organizado por categorías</returns>
        Task<MenuDigitalViewModel> GetMenuDigitalAsync(bool incluirNoDisponibles = false);

        /// <summary>
        /// Obtiene productos por categoría específica (ej: Platos Principales, Frituras)
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="incluirNoDisponibles">Incluir productos sin stock</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<IEnumerable<ProductoResponse>> GetProductosPorCategoriaAsync(int categoriaId, bool incluirNoDisponibles = false);

        /// <summary>
        /// Busca productos por nombre, categoría o características dominicanas
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <param name="categoriaId">Filtro por categoría (opcional)</param>
        /// <param name="precioMin">Precio mínimo (opcional)</param>
        /// <param name="precioMax">Precio máximo (opcional)</param>
        /// <returns>Productos que coinciden con la búsqueda</returns>
        Task<IEnumerable<ProductoResponse>> BuscarProductosAsync(string termino, int? categoriaId = null, decimal? precioMin = null, decimal? precioMax = null);

        /// <summary>
        /// Obtiene productos típicamente dominicanos (platos tradicionales)
        /// </summary>
        /// <returns>Lista de productos típicos de RD</returns>
        Task<IEnumerable<ProductoResponse>> GetProductosTradicionalsDominicanosAsync();

        /// <summary>
        /// Obtiene productos más populares/vendidos
        /// </summary>
        /// <param name="limit">Número máximo de productos a retornar</param>
        /// <returns>Productos más populares</returns>
        Task<IEnumerable<ProductoResponse>> GetProductosPopularesAsync(int limit = 10);

        // ============================================================================
        // GESTIÓN DE COMBOS ESPECIALES
        // ============================================================================

        /// <summary>
        /// Obtiene todos los combos disponibles
        /// </summary>
        /// <param name="incluirNoDisponibles">Incluir combos sin stock suficiente</param>
        /// <returns>Lista de combos especiales</returns>
        Task<IEnumerable<ComboResponse>> GetCombosDisponiblesAsync(bool incluirNoDisponibles = false);

        /// <summary>
        /// Obtiene un combo específico con sus productos
        /// </summary>
        /// <param name="comboId">ID del combo</param>
        /// <returns>Combo con detalles completos</returns>
        Task<ComboResponse?> GetComboByIdAsync(int comboId);

        /// <summary>
        /// Sugiere combos basados en productos seleccionados
        /// </summary>
        /// <param name="productosIds">IDs de productos ya seleccionados</param>
        /// <returns>Combos sugeridos que incluyen esos productos</returns>
        Task<IEnumerable<ComboResponse>> SugerirCombosAsync(List<int> productosIds);

        /// <summary>
        /// Valida si un combo tiene suficiente stock para ser ordenado
        /// </summary>
        /// <param name="comboId">ID del combo</param>
        /// <param name="cantidad">Cantidad de combos a ordenar</param>
        /// <returns>Resultado de validación con detalles</returns>
        Task<ComboValidationResult> ValidarStockComboAsync(int comboId, int cantidad = 1);

        // ============================================================================
        // CONTROL DE INVENTARIO Y STOCK
        // ============================================================================

        /// <summary>
        /// Verifica disponibilidad de un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidadRequerida">Cantidad necesaria</param>
        /// <returns>Información de disponibilidad</returns>
        Task<StockValidationResult> VerificarDisponibilidadAsync(int productoId, int cantidadRequerida = 1);

        /// <summary>
        /// Obtiene productos con stock bajo (alertas de inventario)
        /// </summary>
        /// <param name="umbralMinimo">Umbral mínimo personalizado</param>
        /// <returns>Productos que requieren reabastecimiento</returns>
        Task<IEnumerable<ProductoStockAlert>> GetProductosStockBajoAsync(int? umbralMinimo = null);

        /// <summary>
        /// Actualiza el stock de un producto (para administradores)
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="nuevaCantidad">Nueva cantidad en stock</param>
        /// <param name="usuarioId">ID del usuario que actualiza</param>
        /// <returns>Resultado de la actualización</returns>
        Task<bool> ActualizarStockAsync(int productoId, int nuevaCantidad, int usuarioId);

        /// <summary>
        /// Reserva stock temporalmente para una orden (no confirma)
        /// </summary>
        /// <param name="items">Items a reservar con cantidades</param>
        /// <returns>Resultado de la reserva temporal</returns>
        Task<StockReservationResult> ReservarStockTemporalAsync(List<ItemStock> items);

        /// <summary>
        /// Confirma reserva de stock (descuenta del inventario)
        /// </summary>
        /// <param name="reservaId">ID de la reserva temporal</param>
        /// <returns>Éxito de la confirmación</returns>
        Task<bool> ConfirmarReservaStockAsync(string reservaId);

        /// <summary>
        /// Libera reserva de stock (devuelve al inventario)
        /// </summary>
        /// <param name="reservaId">ID de la reserva temporal</param>
        /// <returns>Éxito de la liberación</returns>
        Task<bool> LiberarReservaStockAsync(string reservaId);

        // ============================================================================
        // GESTIÓN DE PRODUCTOS (CRUD)
        // ============================================================================

        /// <summary>
        /// Crea un nuevo producto en el menú
        /// </summary>
        /// <param name="crearProductoRequest">Datos del nuevo producto</param>
        /// <param name="usuarioId">ID del usuario que crea</param>
        /// <returns>Producto creado</returns>
        Task<ProductoResponse> CrearProductoAsync(CrearProductoRequest crearProductoRequest, int usuarioId);

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="actualizarProductoRequest">Datos actualizados</param>
        /// <param name="usuarioId">ID del usuario que actualiza</param>
        /// <returns>Producto actualizado</returns>
        Task<ProductoResponse?> ActualizarProductoAsync(int productoId, ActualizarProductoRequest actualizarProductoRequest, int usuarioId);

        /// <summary>
        /// Desactiva un producto (no lo elimina, lo marca como inactivo)
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="usuarioId">ID del usuario que desactiva</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> DesactivarProductoAsync(int productoId, int usuarioId);

        /// <summary>
        /// Reactiva un producto previamente desactivado
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="usuarioId">ID del usuario que reactiva</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> ReactivarProductoAsync(int productoId, int usuarioId);

        // ============================================================================
        // ANÁLISIS Y RECOMENDACIONES
        // ============================================================================

        /// <summary>
        /// Obtiene recomendaciones de productos basadas en un producto seleccionado
        /// </summary>
        /// <param name="productoId">ID del producto base</param>
        /// <param name="limit">Número máximo de recomendaciones</param>
        /// <returns>Productos recomendados</returns>
        Task<IEnumerable<ProductoResponse>> GetRecomendacionesAsync(int productoId, int limit = 5);

        /// <summary>
        /// Obtiene productos que típicamente se ordenan juntos
        /// </summary>
        /// <param name="productosIds">IDs de productos ya seleccionados</param>
        /// <returns>Productos que se suelen ordenar con los seleccionados</returns>
        Task<IEnumerable<ProductoResponse>> GetProductosComplementariosAsync(List<int> productosIds);

        /// <summary>
        /// Calcula el precio total con descuentos aplicables
        /// </summary>
        /// <param name="items">Items con cantidades</param>
        /// <param name="aplicarDescuentos">Si aplicar descuentos automáticos</param>
        /// <returns>Desglose de precios y descuentos</returns>
        Task<PrecioCalculationResult> CalcularPrecioTotalAsync(List<ItemPrecio> items, bool aplicarDescuentos = true);

        /// <summary>
        /// Valida una orden completa antes de procesarla
        /// </summary>
        /// <param name="items">Items de la orden</param>
        /// <returns>Resultado de validación con errores/advertencias</returns>
        Task<OrdenValidationResult> ValidarOrdenAsync(List<ItemOrden> items);

        // ============================================================================
        // CATEGORÍAS Y ORGANIZACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene todas las categorías de productos disponibles
        /// </summary>
        /// <param name="incluirVacias">Incluir categorías sin productos</param>
        /// <returns>Lista de categorías</returns>
        Task<IEnumerable<CategoriaResponse>> GetCategoriasAsync(bool incluirVacias = false);

        /// <summary>
        /// Obtiene estadísticas de una categoría específica
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <returns>Estadísticas de la categoría</returns>
        Task<CategoriaStatsResult> GetEstadisticasCategoriaAsync(int categoriaId);
    }

    // ============================================================================
    // MODELOS DE RESPUESTA ESPECÍFICOS DEL SERVICIO
    // ============================================================================

    /// <summary>
    /// Resultado de validación de stock de combo
    /// </summary>
    public class ComboValidationResult
    {
        public bool TieneStockSuficiente { get; set; }
        public List<string> ProductosSinStock { get; set; } = new();
        public List<string> ProductosStockBajo { get; set; } = new();
        public string? Mensaje { get; set; }
        public int CantidadMaximaPosible { get; set; }
    }

    /// <summary>
    /// Resultado de validación de stock individual
    /// </summary>
    public class StockValidationResult
    {
        public bool EstaDisponible { get; set; }
        public int StockActual { get; set; }
        public int CantidadRequerida { get; set; }
        public int CantidadDisponible { get; set; }
        public string? Mensaje { get; set; }
        public List<ProductoResponse>? Alternativas { get; set; }
    }

    /// <summary>
    /// Alerta de stock bajo
    /// </summary>
    public class ProductoStockAlert
    {
        public int ProductoId { get; set; }
        public string? NombreProducto { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string? Categoria { get; set; }
        public string? Urgencia { get; set; } // Crítico, Bajo, Advertencia
    }

    /// <summary>
    /// Item para reserva de stock
    /// </summary>
    public class ItemStock
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    /// <summary>
    /// Resultado de reserva de stock
    /// </summary>
    public class StockReservationResult
    {
        public bool Exitoso { get; set; }
        public string? ReservaId { get; set; }
        public DateTime? VenceEn { get; set; }
        public List<string> ProductosNoDisponibles { get; set; } = new();
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Item para cálculo de precio
    /// </summary>
    public class ItemPrecio
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public int? ComboId { get; set; }
    }

    /// <summary>
    /// Resultado de cálculo de precio
    /// </summary>
    public class PrecioCalculationResult
    {
        public decimal Subtotal { get; set; }
        public decimal Descuentos { get; set; }
        public decimal Total { get; set; }
        public List<DescuentoAplicado> DescuentosDetalle { get; set; } = new();
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Descuento aplicado
    /// </summary>
    public class DescuentoAplicado
    {
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }

    /// <summary>
    /// Item para validación de orden
    /// </summary>
    public class ItemOrden
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public int? ComboId { get; set; }
        public string? NotasEspeciales { get; set; }
    }

    /// <summary>
    /// Resultado de validación de orden
    /// </summary>
    public class OrdenValidationResult
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
        public List<string> Sugerencias { get; set; } = new();
        public decimal TotalEstimado { get; set; }
        public int TiempoPreparacionMinutos { get; set; }
    }

    /// <summary>
    /// Estadísticas de categoría
    /// </summary>
    public class CategoriaStatsResult
    {
        public int TotalProductos { get; set; }
        public int ProductosDisponibles { get; set; }
        public int ProductosAgotados { get; set; }
        public decimal PrecioPromedio { get; set; }
        public decimal PrecioMinimo { get; set; }
        public decimal PrecioMaximo { get; set; }
        public string? ProductoMasVendido { get; set; }
        public string? ProductoMenosVendido { get; set; }
    }

    /// <summary>
    /// DTO para actualizar producto
    /// </summary>
    public class ActualizarProductoRequest
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? CategoriaId { get; set; }
        public bool? Estado { get; set; }
        public int? TiempoPreparacion { get; set; }
    }
}