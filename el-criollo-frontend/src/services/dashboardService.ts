import { mesasService } from './mesasService';
import { ordenesService } from './ordenesService';
import { facturaService } from './facturaService';

export interface DashboardResponse {
  ventasHoy: number;
  ventasAyer: number;
  ordenesActivas: number;
  ordenesHoy: number;
  mesasOcupadas: number;
  totalMesas: number;
  porcentajeOcupacion: number;
}

export interface VentasPorHora {
  hora: string;
  ventas: number;
  ordenes: number;
}

export interface ProductoMasVendido {
  nombre: string;
  cantidad: number;
  ingresos: number;
  categoria: string;
}

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

class DashboardService {
  async getDashboardStats(): Promise<DashboardResponse> {
    try {
      // Obtener datos básicos en paralelo
      const [estadisticasMesas, ordenesActivas, facturasHoy] = await Promise.all([
        mesasService.getEstadisticasMesas(),
        ordenesService.getOrdenesActivas(),
        facturaService.obtenerFacturasDelDia(),
      ]);

      console.log('📊 Datos obtenidos:', {
        ordenesActivas: ordenesActivas.length,
        facturasHoy: facturasHoy.length,
        estadisticasMesas,
      });

      // Procesar facturas para asegurar valores numéricos correctos
      const facturasProcesadas = facturasHoy.map((factura) => ({
        ...factura,
        subtotal: parseCurrency(factura.subtotal),
        total: parseCurrency(factura.total),
        descuento: parseCurrency(factura.descuento),
        propina: parseCurrency(factura.propina),
        impuesto: parseCurrency(factura.impuesto),
      }));

      // Obtener órdenes relacionadas a las facturas
      const ordenIds = Array.from(new Set(facturasProcesadas.map((f) => f.ordenID)));
      const ordenesRelacionadas = await Promise.all(
        ordenIds.map(async (id) => {
          try {
            return await ordenesService.getOrdenById(id);
          } catch (error) {
            console.warn(`Error obteniendo orden ${id}:`, error);
            return null;
          }
        })
      );

      const ordenesValidas = ordenesRelacionadas.filter((orden) => orden !== null);

      console.log('💰 Facturas procesadas:', facturasProcesadas);
      console.log('📋 Órdenes relacionadas:', ordenesValidas);

      // Calcular ventas del día - usando la misma lógica que ReportesVentasPage
      const ventasHoy = facturasProcesadas.reduce((sum, factura) => sum + factura.total, 0);
      const ventasPagadas = facturasProcesadas
        .filter((factura) => factura.estado === 'Pagada')
        .reduce((sum, factura) => sum + factura.total, 0);

      console.log('💵 Ventas hoy (total):', ventasHoy);
      console.log('💵 Ventas pagadas:', ventasPagadas);

      // Calcular ventas de ayer
      const ventasAyer = await this.calcularVentasAyer();

      // Contar órdenes del día - usando facturas como fuente
      const ordenesHoy = facturasProcesadas.length;

      // Contar órdenes activas (pendientes + en preparación)
      const ordenesActivas_count = ordenesActivas.filter(
        (orden) => orden.estado === 'Pendiente' || orden.estado === 'En Preparacion'
      ).length;

      const resultado = {
        ventasHoy: ventasPagadas, // Usar solo ventas pagadas para ser más preciso
        ventasAyer,
        ordenesActivas: ordenesActivas_count,
        ordenesHoy,
        mesasOcupadas: estadisticasMesas.mesasOcupadas,
        totalMesas: estadisticasMesas.totalMesas,
        porcentajeOcupacion: estadisticasMesas.porcentajeOcupacion,
      };

      console.log('📈 Resultado final dashboard:', resultado);
      return resultado;
    } catch (error: any) {
      console.warn('Error obteniendo estadísticas del dashboard:', error);
      // Fallback con datos por defecto
      return {
        ventasHoy: 0,
        ventasAyer: 0,
        ordenesActivas: 0,
        ordenesHoy: 0,
        mesasOcupadas: 0,
        totalMesas: 0,
        porcentajeOcupacion: 0,
      };
    }
  }

  private async calcularVentasAyer(): Promise<number> {
    try {
      const ayer = new Date();
      ayer.setDate(ayer.getDate() - 1);

      const facturasAyer = await facturaService.obtenerFacturasDelDia(ayer);

      // Procesar facturas de ayer
      const facturasProcesadas = facturasAyer.map((factura) => ({
        ...factura,
        total: parseCurrency(factura.total),
      }));

      // Calcular solo ventas pagadas
      return facturasProcesadas
        .filter((factura) => factura.estado === 'Pagada')
        .reduce((sum, factura) => sum + factura.total, 0);
    } catch (error) {
      console.warn('Error calculando ventas de ayer:', error);
      return 0;
    }
  }

  async getVentasPorHora(): Promise<VentasPorHora[]> {
    try {
      const facturasHoy = await facturaService.obtenerFacturasDelDia();

      // Procesar facturas
      const facturasProcesadas = facturasHoy.map((factura) => ({
        ...factura,
        total: parseCurrency(factura.total),
      }));

      const ventasPorHora = new Map<string, { ventas: number; ordenes: number }>();

      // Procesar facturas por hora
      facturasProcesadas.forEach((factura) => {
        if (factura.estado === 'Pagada' && factura.fechaFactura) {
          const fecha = new Date(factura.fechaFactura);
          const hora = fecha.getHours().toString().padStart(2, '0') + ':00';

          if (!ventasPorHora.has(hora)) {
            ventasPorHora.set(hora, { ventas: 0, ordenes: 0 });
          }

          const actual = ventasPorHora.get(hora)!;
          actual.ventas += factura.total;
          actual.ordenes += 1;
        }
      });

      // Convertir a array y ordenar por hora
      return Array.from(ventasPorHora.entries())
        .map(([hora, datos]) => ({
          hora,
          ventas: datos.ventas,
          ordenes: datos.ordenes,
        }))
        .sort((a, b) => a.hora.localeCompare(b.hora));
    } catch (error: any) {
      console.warn('Error obteniendo ventas por hora:', error);
      return [];
    }
  }

  async getProductosMasVendidos(): Promise<ProductoMasVendido[]> {
    try {
      // Obtener facturas del día
      const facturasHoy = await facturaService.obtenerFacturasDelDia();

      // Obtener órdenes relacionadas
      const ordenIds = Array.from(new Set(facturasHoy.map((f) => f.ordenID)));
      const ordenesRelacionadas = await Promise.all(
        ordenIds.map(async (id) => {
          try {
            return await ordenesService.getOrdenById(id);
          } catch (error) {
            console.warn(`Error obteniendo orden ${id}:`, error);
            return null;
          }
        })
      );

      const ordenesValidas = ordenesRelacionadas.filter((orden) => orden !== null);

      // Procesar productos vendidos
      const productosVendidos = new Map<
        string,
        { cantidad: number; ingresos: number; categoria: string }
      >();

      ordenesValidas.forEach((orden) => {
        if (orden.detalles && Array.isArray(orden.detalles)) {
          orden.detalles.forEach((detalle: any) => {
            const nombreProducto =
              detalle.producto?.nombre || detalle.nombreProducto || 'Producto desconocido';
            const categoria = detalle.producto?.categoria?.nombre || 'Sin categoría';
            const cantidad = detalle.cantidad || 0;
            const precio = detalle.precio || detalle.precioUnitario || 0;

            if (!productosVendidos.has(nombreProducto)) {
              productosVendidos.set(nombreProducto, { cantidad: 0, ingresos: 0, categoria });
            }

            const actual = productosVendidos.get(nombreProducto)!;
            actual.cantidad += cantidad;
            actual.ingresos += cantidad * precio;
          });
        }
      });

      // Convertir a array y ordenar por cantidad vendida
      return Array.from(productosVendidos.entries())
        .map(([nombre, datos]) => ({
          nombre,
          cantidad: datos.cantidad,
          ingresos: datos.ingresos,
          categoria: datos.categoria,
        }))
        .sort((a, b) => b.cantidad - a.cantidad)
        .slice(0, 10); // Top 10 productos
    } catch (error: any) {
      console.warn('Error obteniendo productos más vendidos:', error);
      return [];
    }
  }
}

export const dashboardService = new DashboardService();
