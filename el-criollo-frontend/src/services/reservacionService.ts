import { api } from './api';
import type {
  Reservacion,
  CrearReservacionRequest,
  ActualizarReservacionRequest,
  FiltrosReservacion,
  EstadisticasReservacion,
  DisponibilidadMesa,
  ReservacionConDetalles,
} from '@/types/reservacion';

class ReservacionService {
  private baseUrl = '/api/Reservacion';

  // ============================================================================
  // OPERACIONES CRUD
  // ============================================================================

  async getReservaciones(filtros?: FiltrosReservacion): Promise<ReservacionConDetalles[]> {
    try {
      const params = new URLSearchParams();
      if (filtros) {
        Object.entries(filtros).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString());
          }
        });
      }

      const response = await api.get<ReservacionConDetalles[]>(
        `${this.baseUrl}?${params.toString()}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo reservaciones:', error);
      throw new Error('Error al obtener las reservaciones');
    }
  }

  async getReservacionById(id: number): Promise<ReservacionConDetalles> {
    try {
      const response = await api.get<ReservacionConDetalles>(`${this.baseUrl}/${id}`);
      return response;
    } catch (error) {
      console.error('Error obteniendo reservación:', error);
      throw new Error('Error al obtener la reservación');
    }
  }

  async crearReservacion(data: CrearReservacionRequest): Promise<ReservacionConDetalles> {
    try {
      // Convertir los nombres de campos para que coincidan con el backend
      const backendData = {
        ClienteId: data.clienteID,
        MesaId: data.mesaID,
        CantidadPersonas: data.cantidadPersonas,
        FechaHora: data.fechaHora,
        DuracionMinutos: data.duracionEstimada || 120,
        NotasEspeciales: data.observaciones,
      };

      const response = await api.post<ReservacionConDetalles>(this.baseUrl, backendData);
      return response;
    } catch (error: any) {
      console.error('Error creando reservación:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al crear la reservación';
      throw new Error(message);
    }
  }

  async actualizarReservacion(
    id: number,
    data: ActualizarReservacionRequest
  ): Promise<ReservacionConDetalles> {
    try {
      // Convertir los nombres de campos para que coincidan con el backend
      const backendData: any = {};
      if (data.cantidadPersonas) backendData.CantidadPersonas = data.cantidadPersonas;
      if (data.fechaHora) backendData.FechaHora = data.fechaHora;
      if (data.duracionEstimada) backendData.DuracionMinutos = data.duracionEstimada;
      if (data.observaciones) backendData.NotasEspeciales = data.observaciones;

      const response = await api.put<ReservacionConDetalles>(`${this.baseUrl}/${id}`, backendData);
      return response;
    } catch (error: any) {
      console.error('Error actualizando reservación:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al actualizar la reservación';
      throw new Error(message);
    }
  }

  async cancelarReservacion(id: number, motivo?: string): Promise<boolean> {
    try {
      const response = await api.post<boolean>(`${this.baseUrl}/${id}/cancelar`, { motivo });
      return response;
    } catch (error: any) {
      console.error('Error cancelando reservación:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al cancelar la reservación';
      throw new Error(message);
    }
  }

  // ============================================================================
  // CONSULTAS ESPECÍFICAS
  // ============================================================================

  async getReservacionesDelDia(): Promise<ReservacionConDetalles[]> {
    try {
      const response = await api.get<ReservacionConDetalles[]>(`${this.baseUrl}/dia`);
      return response;
    } catch (error) {
      console.error('Error obteniendo reservaciones del día:', error);
      throw new Error('Error al obtener las reservaciones del día');
    }
  }

  async getReservacionesPendientes(): Promise<ReservacionConDetalles[]> {
    try {
      // Usar el endpoint de reservaciones del día y filtrar por estado
      const response = await api.get<ReservacionConDetalles[]>(`${this.baseUrl}/dia`);
      return response.filter((r) => r.estado === 'Pendiente');
    } catch (error) {
      console.error('Error obteniendo reservaciones pendientes:', error);
      throw new Error('Error al obtener las reservaciones pendientes');
    }
  }

  async getReservacionesPorMesa(mesaId: number): Promise<ReservacionConDetalles[]> {
    try {
      const response = await api.get<ReservacionConDetalles[]>(`${this.baseUrl}/mesa/${mesaId}`);
      return response;
    } catch (error) {
      console.error('Error obteniendo reservaciones por mesa:', error);
      throw new Error('Error al obtener las reservaciones de la mesa');
    }
  }

  async getReservacionesPorCliente(clienteId: number): Promise<ReservacionConDetalles[]> {
    try {
      const response = await api.get<ReservacionConDetalles[]>(
        `${this.baseUrl}/cliente/${clienteId}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo reservaciones por cliente:', error);
      throw new Error('Error al obtener las reservaciones del cliente');
    }
  }

  // ============================================================================
  // DISPONIBILIDAD
  // ============================================================================

  async getDisponibilidadMesa(mesaId: number, fecha: string): Promise<DisponibilidadMesa> {
    try {
      const params = new URLSearchParams({ fecha });
      const response = await api.get<DisponibilidadMesa>(
        `${this.baseUrl}/disponibilidad?${params.toString()}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo disponibilidad:', error);
      throw new Error('Error al obtener la disponibilidad de la mesa');
    }
  }

  async getMesasDisponibles(
    fecha: string,
    cantidadPersonas?: number
  ): Promise<DisponibilidadMesa[]> {
    try {
      const params = new URLSearchParams({ fecha });
      if (cantidadPersonas) {
        params.append('cantidadPersonas', cantidadPersonas.toString());
      }

      const response = await api.get<DisponibilidadMesa[]>(
        `${this.baseUrl}/disponibilidad?${params.toString()}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo mesas disponibles:', error);
      throw new Error('Error al obtener las mesas disponibles');
    }
  }

  async verificarDisponibilidad(
    mesaId: number,
    fechaHora: string,
    duracionEstimada: number
  ): Promise<boolean> {
    try {
      // Usar el servicio de mesas para verificar disponibilidad
      const response = await api.get<{ disponible: boolean }>(
        `/api/Mesas/${mesaId}/disponibilidad`
      );
      return response.disponible;
    } catch (error) {
      console.error('Error verificando disponibilidad:', error);
      throw new Error('Error al verificar la disponibilidad');
    }
  }

  // ============================================================================
  // ESTADÍSTICAS
  // ============================================================================

  async getEstadisticas(
    fechaDesde?: string,
    fechaHasta?: string
  ): Promise<EstadisticasReservacion> {
    try {
      const params = new URLSearchParams();
      if (fechaDesde) params.append('fechaInicio', fechaDesde);
      if (fechaHasta) params.append('fechaFin', fechaHasta);

      const response = await api.get<EstadisticasReservacion>(
        `${this.baseUrl}/estadisticas?${params.toString()}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo estadísticas:', error);
      throw new Error('Error al obtener las estadísticas');
    }
  }

  // ============================================================================
  // GESTIÓN DE ESTADOS
  // ============================================================================

  async confirmarReservacion(id: number): Promise<ReservacionConDetalles> {
    try {
      const response = await api.post<ReservacionConDetalles>(`${this.baseUrl}/${id}/confirmar`);
      return response;
    } catch (error: any) {
      console.error('Error confirmando reservación:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al confirmar la reservación';
      throw new Error(message);
    }
  }

  async iniciarReservacion(id: number): Promise<ReservacionConDetalles> {
    try {
      const response = await api.post<ReservacionConDetalles>(`${this.baseUrl}/${id}/iniciar`);
      return response;
    } catch (error: any) {
      console.error('Error iniciando reservación:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al iniciar la reservación';
      throw new Error(message);
    }
  }

  async completarReservacion(id: number): Promise<ReservacionConDetalles> {
    try {
      const response = await api.post<ReservacionConDetalles>(`${this.baseUrl}/${id}/completar`);
      return response;
    } catch (error: any) {
      console.error('Error completando reservación:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al completar la reservación';
      throw new Error(message);
    }
  }

  async marcarNoShow(id: number): Promise<ReservacionConDetalles> {
    try {
      const response = await api.post<ReservacionConDetalles>(`${this.baseUrl}/${id}/no-show`);
      return response;
    } catch (error: any) {
      console.error('Error marcando no show:', error);
      const message =
        error.response?.data?.detail || error.message || 'Error al marcar como no show';
      throw new Error(message);
    }
  }

  // ============================================================================
  // UTILIDADES
  // ============================================================================

  async getHorariosDisponibles(mesaId: number, fecha: string): Promise<string[]> {
    try {
      const params = new URLSearchParams({ fecha });
      const response = await api.get<string[]>(
        `${this.baseUrl}/horarios-disponibles/${mesaId}?${params.toString()}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo horarios disponibles:', error);
      throw new Error('Error al obtener los horarios disponibles');
    }
  }

  async getReservacionesRetrasadas(): Promise<ReservacionConDetalles[]> {
    try {
      const response = await api.get<ReservacionConDetalles[]>(`${this.baseUrl}/retrasadas`);
      return response;
    } catch (error) {
      console.error('Error obteniendo reservaciones retrasadas:', error);
      throw new Error('Error al obtener las reservaciones retrasadas');
    }
  }

  async getReservacionesProximas(minutos: number = 30): Promise<ReservacionConDetalles[]> {
    try {
      const params = new URLSearchParams({ minutos: minutos.toString() });
      const response = await api.get<ReservacionConDetalles[]>(
        `${this.baseUrl}/proximas?${params.toString()}`
      );
      return response;
    } catch (error) {
      console.error('Error obteniendo reservaciones próximas:', error);
      throw new Error('Error al obtener las reservaciones próximas');
    }
  }
}

export const reservacionService = new ReservacionService();
