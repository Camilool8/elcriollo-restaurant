export interface Empleado {
  empleadoID: number;
  nombreCompleto: string;
  cedula: string;
  email: string;
  telefono: string;
  telefonoFormateado: string;
  direccion: string;
  fechaNacimiento: string;
  fechaContratacion: string;
  cargo: string;
  salarioFormateado: string;
  tiempoEnEmpresa: string;
  esEmpleadoActivo: boolean;
  usuarioID?: number;
}
