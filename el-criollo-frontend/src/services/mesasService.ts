import { api, getErrorMessage } from './api';
import type {
  Mesa,
  EstadoMesa,
  CambioEstadoMesaRequest,
  MantenimientoMesaRequest,
} from '@/types/mesa';

class MesasService {
  /**
   * Obtiene todas las mesas del restaurante
   */
  async getAllMesas(): Promise<Mesa[]> {
    try {
      const response = await api.get<Mesa[]>('/Mesas');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo mesas: ${message}`);
    }
  }

  /**
   * Obtiene una mesa específica por ID
   */
  async getMesaById(mesaId: number): Promise<Mesa> {
    try {
      const response = await api.get<Mesa>(`/Mesas/${mesaId}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo mesa: ${message}`);
    }
  }

  /**
   * Obtiene mesas filtradas por estado
   */
  async getMesasByEstado(estado: EstadoMesa): Promise<Mesa[]> {
    try {
      const response = await api.get<Mesa[]>(`/Mesas/estado/${estado}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo mesas por estado: ${message}`);
    }
  }

  /**
   * Libera una mesa (la marca como libre)
   */
  async liberarMesa(mesaId: number): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.post<{ success: boolean; message: string }>(
        `/Mesas/${mesaId}/liberar`
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error liberando mesa: ${message}`);
    }
  }

  /**
   * Ocupa una mesa (la marca como ocupada)
   */
  async ocuparMesa(mesaId: number): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.post<{ success: boolean; message: string }>(
        `/Mesas/${mesaId}/ocupar`
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error ocupando mesa: ${message}`);
    }
  }

  /**
   * Cambia el estado de una mesa manualmente
   */
  async cambiarEstadoMesa(
    mesaId: number,
    request: CambioEstadoMesaRequest
  ): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.put<{ success: boolean; message: string }>(
        `/Mesas/${mesaId}/estado`,
        request
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error cambiando estado: ${message}`);
    }
  }

  /**
   * Marca una mesa en mantenimiento
   */
  async marcarMantenimiento(
    mesaId: number,
    request: MantenimientoMesaRequest
  ): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.post<{ success: boolean; message: string }>(
        `/Mesas/${mesaId}/mantenimiento`,
        request
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error marcando mantenimiento: ${message}`);
    }
  }

  /**
   * Obtiene estadísticas generales de ocupación
   */
  async getEstadisticasMesas(): Promise<{
    totalMesas: number;
    mesasLibres: number;
    mesasOcupadas: number;
    mesasReservadas: number;
    mesasMantenimiento: number;
    porcentajeOcupacion: number;
  }> {
    try {
      const mesas = await this.getAllMesas();

      const stats = {
        totalMesas: mesas.length,
        mesasLibres: mesas.filter((m) => m.estado === 'Libre').length,
        mesasOcupadas: mesas.filter((m) => m.estado === 'Ocupada').length,
        mesasReservadas: mesas.filter((m) => m.estado === 'Reservada').length,
        mesasMantenimiento: mesas.filter((m) => m.estado === 'Mantenimiento').length,
        porcentajeOcupacion: 0,
      };

      const mesasOperativas = stats.totalMesas - stats.mesasMantenimiento;
      if (mesasOperativas > 0) {
        stats.porcentajeOcupacion =
          ((stats.mesasOcupadas + stats.mesasReservadas) / mesasOperativas) * 100;
      }

      return stats;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo estadísticas: ${message}`);
    }
  }

  /**
   * Busca mesas disponibles con filtros opcionales
   */
  async buscarMesasDisponibles(filtros?: {
    capacidadMinima?: number;
    ubicacion?: string;
  }): Promise<Mesa[]> {
    try {
      let mesas = await this.getMesasByEstado('Libre');

      if (filtros?.capacidadMinima) {
        mesas = mesas.filter((mesa) => mesa.capacidad >= filtros.capacidadMinima!);
      }

      if (filtros?.ubicacion) {
        mesas = mesas.filter((mesa) => mesa.ubicacion === filtros.ubicacion);
      }

      return mesas;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error buscando mesas disponibles: ${message}`);
    }
  }

  /**
   * Obtiene el resumen de una mesa (para tooltips y vistas rápidas)
   */
  getResumenMesa(mesa: Mesa): string {
    let resumen = `Mesa ${mesa.numeroMesa} (${mesa.capacidad} personas)`;

    if (mesa.ubicacion) {
      resumen += ` - ${mesa.ubicacion}`;
    }

    switch (mesa.estado) {
      case 'Ocupada':
        if (mesa.clienteActual) {
          resumen += `\n👤 ${mesa.clienteActual.nombreCompleto}`;
        }
        if (mesa.ordenActual) {
          resumen += `\n🍽️ Orden: ${mesa.ordenActual.numeroOrden}`;
          resumen += `\n💰 Total: RD$${mesa.ordenActual.totalCalculado.toFixed(2)}`;
        }
        if (mesa.tiempoOcupada) {
          resumen += `\n⏱️ Tiempo: ${mesa.tiempoOcupada}`;
        }
        break;

      case 'Reservada':
        if (mesa.reservacionActual) {
          resumen += `\n📅 Reserva: ${mesa.reservacionActual.numeroReservacion}`;
          resumen += `\n👥 ${mesa.reservacionActual.cantidadPersonas} personas`;
          if (mesa.tiempoHastaReserva) {
            resumen += `\n⏰ En: ${mesa.tiempoHastaReserva}`;
          }
        }
        break;

      case 'Libre':
        resumen += '\n✅ Disponible';
        break;

      case 'Mantenimiento':
        resumen += '\n🔧 En mantenimiento';
        break;
    }

    if (mesa.necesitaLimpieza) {
      resumen += '\n🧹 Necesita limpieza';
    }

    if (mesa.requiereAtencion) {
      resumen += '\n⚠️ Requiere atención';
    }

    return resumen;
  }
}

// Instancia singleton exportada
export const mesasService = new MesasService();
