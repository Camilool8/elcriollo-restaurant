export interface Cliente {
  clienteID: number;
  nombreCompleto: string;
  cedula?: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  fechaNacimiento?: string;
  preferenciasComida?: string;
  estado: boolean;
  fechaRegistro: string;
  categoriaCliente: string;
  totalOrdenes: number;
  totalReservaciones: number;
  totalFacturas: number;
  promedioConsumo: string;
  ultimaVisita?: string;
}
