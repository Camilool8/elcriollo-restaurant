import { useState, useEffect, useCallback } from 'react';
import type { FiltrosReporteVentas, ReporteVentasState, ResumenVentas } from '@/types/reportes';
import { facturaService } from '@/services/facturaService';
import { ordenesService } from '@/services/ordenesService';
import { clienteService } from '@/services/clienteService';
import { productosService } from '@/services/productosService';
import { categoriaService } from '@/services/categoriaService';
import type { Factura, Orden, Producto } from '@/types';

const estadoInicial: ReporteVentasState = {
  facturas: [],
  ordenes: [],
  clientes: [],
  productos: [],
  categorias: [],
  filtros: {},
  resumen: null,
  estadisticasPorPeriodo: [],
  estadisticasPorCliente: [],
  estadisticasPorProducto: [],
  estadisticasPorCategoria: [],
  vistaActual: 'FACTURAS',
  loading: false,
  error: null,
};

// Función auxiliar para convertir strings de moneda a números
const parseCurrency = (value: string | number): number => {
  if (typeof value === 'number') return value;
  if (!value) return 0;

  // Remover "RD$ " y convertir a número
  const cleanValue = value
    .toString()
    .replace(/RD\$\s*/g, '')
    .replace(/,/g, '');
  return parseFloat(cleanValue) || 0;
};

export const useReporteVentas = () => {
  const [state, setState] = useState<ReporteVentasState>(estadoInicial);

  // Fetch inicial de catálogos
  useEffect(() => {
    const fetchCatalogos = async () => {
      try {
        const [clientes, productos, categorias] = await Promise.all([
          clienteService.getClientes(),
          productosService.getAllProductos(),
          categoriaService.getCategorias(),
        ]);
        setState((prev) => ({ ...prev, clientes, productos, categorias }));
      } catch (error) {
        setState((prev) => ({ ...prev, error: 'Error cargando catálogos' }));
      }
    };
    fetchCatalogos();
  }, []);

  // Fetch de facturas y órdenes según filtros
  const fetchDatos = useCallback(async (filtros: FiltrosReporteVentas) => {
    setState((prev) => ({ ...prev, loading: true, error: null }));
    try {
      // Fetch facturas por rango o día
      let facturas = [];
      if (filtros.fechaInicio && filtros.fechaFin) {
        facturas = await facturaService.obtenerFacturasPorRango(
          filtros.fechaInicio,
          filtros.fechaFin
        );
      } else {
        facturas = await facturaService.obtenerFacturasDelDia();
      }

      // Procesar facturas para asegurar que los valores numéricos sean correctos
      const facturasProcesadas = facturas.map((factura) => ({
        ...factura,
        subtotal: parseCurrency(factura.subtotal),
        total: parseCurrency(factura.total),
        descuento: parseCurrency(factura.descuento),
        propina: parseCurrency(factura.propina),
        impuesto: parseCurrency(factura.impuesto),
      }));

      // Fetch órdenes relacionadas - obtener individualmente
      const ordenIds = Array.from(new Set(facturasProcesadas.map((f) => f.ordenID)));
      const ordenes = await Promise.all(ordenIds.map((id) => ordenesService.getOrdenById(id)));

      setState((prev) => ({ ...prev, facturas: facturasProcesadas, ordenes, loading: false }));
      return { facturas: facturasProcesadas, ordenes };
    } catch (error) {
      setState((prev) => ({ ...prev, loading: false, error: 'Error cargando datos' }));
      return { facturas: [], ordenes: [] };
    }
  }, []);

  // Aplicar filtros en frontend
  const filtrarFacturas = useCallback(
    (
      facturas: Factura[],
      ordenes: Orden[],
      filtros: FiltrosReporteVentas,
      productos: Producto[]
    ) => {
      return facturas.filter((factura: Factura) => {
        // Estado
        if (filtros.estadoFactura && factura.estado !== filtros.estadoFactura) return false;

        // Método de pago
        if (filtros.metodoPago && factura.metodoPago !== filtros.metodoPago) return false;

        // Monto
        if (filtros.montoMinimo && factura.total < filtros.montoMinimo) return false;
        if (filtros.montoMaximo && factura.total > filtros.montoMaximo) return false;

        // Producto
        if (filtros.productoId) {
          const orden = ordenes.find((o: Orden) => o.ordenID === factura.ordenID);
          if (
            !orden ||
            !orden.detalles?.some((d: any) => d.producto?.productoID === filtros.productoId)
          )
            return false;
        }

        // Categoría
        if (filtros.categoriaId) {
          const orden = ordenes.find((o: Orden) => o.ordenID === factura.ordenID);
          if (!orden) return false;

          // Obtener productos de la categoría seleccionada
          const productosDeCategoria = productos.filter(
            (p) => p.categoria.categoriaID === filtros.categoriaId
          );
          const productoIdsDeCategoria = productosDeCategoria.map((p) => p.productoID);

          // Verificar si la orden contiene algún producto de esa categoría
          const tieneProductoDeCategoria = orden.detalles?.some((d: any) => {
            // Verificar si el detalle tiene un producto y si ese producto está en la categoría
            return d.producto && productoIdsDeCategoria.includes(d.producto.productoID);
          });

          if (!tieneProductoDeCategoria) return false;
        }

        return true;
      });
    },
    []
  );

  // Calcular resumen general
  const calcularResumen = useCallback((facturas: Factura[]): ResumenVentas => {
    const totalFacturado = facturas.reduce((sum: number, f: Factura) => sum + f.total, 0);
    const totalPagado = facturas
      .filter((f: Factura) => f.estado === 'Pagada')
      .reduce((sum: number, f: Factura) => sum + f.total, 0);
    const totalPendiente = facturas
      .filter((f: Factura) => f.estado === 'Pendiente')
      .reduce((sum: number, f: Factura) => sum + f.total, 0);
    const cantidadFacturas = facturas.length;
    const cantidadOrdenes = new Set(facturas.map((f: Factura) => f.ordenID)).size;
    const promedioPorFactura = cantidadFacturas ? totalFacturado / cantidadFacturas : 0;
    const promedioPorOrden = cantidadOrdenes ? totalFacturado / cantidadOrdenes : 0;

    return {
      totalFacturado,
      totalPagado,
      totalPendiente,
      cantidadFacturas,
      cantidadOrdenes,
      promedioPorFactura,
      promedioPorOrden,
      metodoPagoMasUsado: '',
      clienteMasFrecuente: '',
      productoMasVendido: '',
      categoriaMasVendida: '',
    };
  }, []);

  // Cambiar filtros y refrescar
  const setFiltros = async (filtros: FiltrosReporteVentas) => {
    setState((prev) => ({ ...prev, filtros }));
    const { facturas, ordenes } = await fetchDatos(filtros);
    const facturasFiltradas = filtrarFacturas(facturas, ordenes, filtros, state.productos);
    const resumen = calcularResumen(facturasFiltradas);
    setState((prev) => ({ ...prev, facturas: facturasFiltradas, ordenes, resumen }));
  };

  // Inicializar con datos del día
  useEffect(() => {
    setFiltros({});
    // eslint-disable-next-line
  }, []);

  return {
    ...state,
    setFiltros,
  };
};
