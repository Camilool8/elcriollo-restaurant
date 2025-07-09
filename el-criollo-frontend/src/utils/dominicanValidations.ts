// ====================================
// VALIDACIONES DOMINICANAS
// ====================================

export const DOMINICAN_REGEX = {
  // Cédula dominicana: 123-1234567-1
  cedula: /^\d{3}-\d{7}-\d{1}$/,

  // Teléfonos dominicanos: 809-123-4567, 829-123-4567, 849-123-4567
  telefono: /^(\+1\s?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})$/,

  // Código de área dominicano
  codigoAreaDominicano: /^(809|829|849)$/,

  // Email estándar
  email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,

  // Precio en pesos dominicanos
  precio: /^\d+(\.\d{1,2})?$/,

  // Número de mesa
  numeroMesa: /^\d{1,3}$/,

  // Número de factura dominicano
  numeroFactura: /^FACT-\d{8}-\d{4}$/,
};

// ====================================
// FUNCIONES DE VALIDACIÓN
// ====================================

export const validarCedulaDominicana = (cedula: string): boolean => {
  if (!cedula || !DOMINICAN_REGEX.cedula.test(cedula)) {
    return false;
  }

  // Extraer números sin guiones
  const numeros = cedula.replace(/-/g, '');

  // Validar que son 11 dígitos
  if (numeros.length !== 11) {
    return false;
  }

  // Algoritmo de validación de cédula dominicana
  const digitoVerificador = parseInt(numeros.charAt(10));
  const secuencia = numeros.substring(0, 10);

  let suma = 0;
  for (let i = 0; i < secuencia.length; i++) {
    const digito = parseInt(secuencia.charAt(i));
    const multiplicador = i % 2 === 0 ? 1 : 2;
    let producto = digito * multiplicador;

    if (producto > 9) {
      producto = Math.floor(producto / 10) + (producto % 10);
    }

    suma += producto;
  }

  const modulo = suma % 10;
  const verificador = modulo === 0 ? 0 : 10 - modulo;

  return verificador === digitoVerificador;
};

export const validarTelefonoDominicano = (telefono: string): boolean => {
  if (!telefono || !DOMINICAN_REGEX.telefono.test(telefono)) {
    return false;
  }

  // Extraer código de área
  const match = telefono.match(/([0-9]{3})/);
  if (!match) return false;

  const codigoArea = match[1];
  return DOMINICAN_REGEX.codigoAreaDominicano.test(codigoArea);
};

export const validarEmail = (email: string): boolean => {
  return email ? DOMINICAN_REGEX.email.test(email) : true; // Email es opcional
};

export const validarPrecio = (precio: string | number): boolean => {
  const precioStr = precio.toString();
  return DOMINICAN_REGEX.precio.test(precioStr) && parseFloat(precioStr) > 0;
};

// ====================================
// FORMATTERS DOMINICANOS
// ====================================

export const formatearCedula = (cedula: string): string => {
  // Remover todo excepto números
  const numeros = cedula.replace(/\D/g, '');

  // Aplicar formato XXX-XXXXXXX-X
  if (numeros.length >= 11) {
    return `${numeros.slice(0, 3)}-${numeros.slice(3, 10)}-${numeros.slice(10, 11)}`;
  } else if (numeros.length >= 3) {
    const parte1 = numeros.slice(0, 3);
    const parte2 = numeros.slice(3);
    return parte2.length > 0 ? `${parte1}-${parte2}` : parte1;
  }

  return numeros;
};

export const formatearTelefono = (telefono: string): string => {
  // Remover todo excepto números
  const numeros = telefono.replace(/\D/g, '');

  // Aplicar formato XXX-XXX-XXXX
  if (numeros.length >= 10) {
    return `${numeros.slice(0, 3)}-${numeros.slice(3, 6)}-${numeros.slice(6, 10)}`;
  } else if (numeros.length >= 6) {
    return `${numeros.slice(0, 3)}-${numeros.slice(3)}`;
  } else if (numeros.length >= 3) {
    return `${numeros.slice(0, 3)}-${numeros.slice(3)}`;
  }

  return numeros;
};

export const formatearPrecio = (precio: number | undefined | null): string => {
  if (precio === undefined || precio === null || isNaN(precio)) {
    return 'RD$ 0.00';
  }
  return `RD$ ${precio.toLocaleString('es-DO', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
};

export const formatearFecha = (fecha: string | Date): string => {
  const fechaObj = typeof fecha === 'string' ? new Date(fecha) : fecha;
  return fechaObj.toLocaleDateString('es-DO', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
};

export const formatearFechaCorta = (fecha: string | Date): string => {
  const fechaObj = typeof fecha === 'string' ? new Date(fecha) : fecha;
  return fechaObj.toLocaleDateString('es-DO');
};

export const formatearHora = (fecha: string | Date): string => {
  const fechaObj = typeof fecha === 'string' ? new Date(fecha) : fecha;
  return fechaObj.toLocaleTimeString('es-DO', {
    hour: '2-digit',
    minute: '2-digit',
  });
};

// ====================================
// GENERADORES DOMINICANOS
// ====================================

export const generarNumeroFactura = (): string => {
  const fecha = new Date();
  const year = fecha.getFullYear();
  const month = String(fecha.getMonth() + 1).padStart(2, '0');
  const day = String(fecha.getDate()).padStart(2, '0');
  const secuencial = Math.floor(Math.random() * 9999) + 1;

  return `FACT-${year}${month}${day}-${String(secuencial).padStart(4, '0')}`;
};

export const generarNumeroOrden = (): string => {
  const fecha = new Date();
  const hora = String(fecha.getHours()).padStart(2, '0');
  const minuto = String(fecha.getMinutes()).padStart(2, '0');
  const secuencial = Math.floor(Math.random() * 999) + 1;

  return `ORD-${hora}${minuto}-${String(secuencial).padStart(3, '0')}`;
};

// ====================================
// CONSTANTES DOMINICANAS
// ====================================

export const DEPARTAMENTOS_RD = [
  'Administración',
  'Cocina',
  'Servicio al Cliente',
  'Caja y Facturación',
  'Recepción y Reservas',
  'Limpieza y Mantenimiento',
  'Seguridad',
  'Delivery',
  'Marketing',
  'Recursos Humanos',
];

export const PROVINCIAS_RD = [
  'Santo Domingo',
  'Santiago',
  'La Vega',
  'San Cristóbal',
  'Puerto Plata',
  'San Pedro de Macorís',
  'La Romana',
  'Moca',
  'Higüey',
  'Baní',
  'Azua',
  'Mao',
  'Bonao',
  'San Francisco de Macorís',
  'Cotuí',
  'Jarabacoa',
  'Constanza',
  'Nagua',
  'Barahona',
  'Neiba',
  'Monte Cristi',
  'Dajabón',
  'Elías Piña',
  'Jimani',
  'Pedernales',
  'Cabral',
  'Villa Altagracia',
  'Yamasa',
  'Hato Mayor',
  'El Seibo',
  'Miches',
  'Samaná',
  'Las Terrenas',
];

export const METODOS_PAGO_RD = [
  'Efectivo',
  'Tarjeta de Débito',
  'Tarjeta de Crédito',
  'Transferencia Bancaria',
  'Pago Móvil',
  'Cheque',
];

export const SEXOS = ['Masculino', 'Femenino', 'Otro'];

// ====================================
// CÁLCULOS DOMINICANOS
// ====================================

export const calcularITBIS = (subtotal: number): number => {
  return subtotal * 0.18; // 18% ITBIS en República Dominicana
};

export const calcularTotal = (
  subtotal: number,
  descuento: number = 0,
  propina: number = 0
): number => {
  const subtotalConDescuento = subtotal - descuento;
  const itbis = calcularITBIS(subtotalConDescuento);
  return subtotalConDescuento + itbis + propina;
};

export const desglosarFactura = (subtotal: number, descuento: number = 0, propina: number = 0) => {
  const subtotalConDescuento = subtotal - descuento;
  const itbis = calcularITBIS(subtotalConDescuento);
  const total = subtotalConDescuento + itbis + propina;

  return {
    subtotal: subtotalConDescuento,
    itbis,
    propina,
    total,
    descuento,
  };
};

// ====================================
// MENSAJES EN ESPAÑOL DOMINICANO
// ====================================

export const MENSAJES_DOMINICANOS = {
  // Saludos
  bienvenida: '¡Bienvenido a El Criollo! 🇩🇴',
  despedida: '¡Que tengas un buen día! ¡Vuelve pronto!',

  // Validaciones
  cedulaInvalida: 'La cédula debe tener el formato: 123-1234567-1',
  telefonoInvalido: 'El teléfono debe ser dominicano: 809-123-4567',
  emailInvalido: 'El email no tiene un formato válido',
  precioInvalido: 'El precio debe ser mayor a cero',

  // Confirmaciones
  usuarioCreado: '¡Usuario creado exitosamente! 🎉',
  clienteRegistrado: '¡Cliente registrado con éxito! 👥',
  empleadoAgregado: '¡Empleado agregado al equipo! 💼',

  // Errores comunes
  errorConexion: 'Problemas de conexión. Verifica tu internet.',
  errorServidor: 'Error en el servidor. Intenta más tarde.',
  errorPermisos: 'No tienes permisos para esta acción.',
  errorDatos: 'Verifica los datos ingresados.',

  // Estados
  activo: 'Activo',
  inactivo: 'Inactivo',
  pendiente: 'Pendiente',
  completado: 'Completado',
  cancelado: 'Cancelado',
};

// ====================================
// UTILIDADES DE TEXTO
// ====================================

export const capitalizarTexto = (texto: string): string => {
  return texto.charAt(0).toUpperCase() + texto.slice(1).toLowerCase();
};

export const formatearNombreCompleto = (nombre: string, apellido: string): string => {
  return `${capitalizarTexto(nombre)} ${capitalizarTexto(apellido)}`;
};

export const extraerIniciales = (nombre: string, apellido: string): string => {
  const inicial1 = nombre.charAt(0).toUpperCase();
  const inicial2 = apellido.charAt(0).toUpperCase();
  return `${inicial1}${inicial2}`;
};

export const truncarTexto = (texto: string, longitud: number = 50): string => {
  if (texto.length <= longitud) return texto;
  return `${texto.substring(0, longitud)}...`;
};

// ====================================
// EXPORTAR TODO
// ====================================

export default {
  DOMINICAN_REGEX,
  validarCedulaDominicana,
  validarTelefonoDominicano,
  validarEmail,
  validarPrecio,
  formatearCedula,
  formatearTelefono,
  formatearPrecio,
  formatearFecha,
  formatearFechaCorta,
  formatearHora,
  generarNumeroFactura,
  generarNumeroOrden,
  calcularITBIS,
  calcularTotal,
  desglosarFactura,
  capitalizarTexto,
  formatearNombreCompleto,
  extraerIniciales,
  truncarTexto,
  DEPARTAMENTOS_RD,
  PROVINCIAS_RD,
  METODOS_PAGO_RD,
  SEXOS,
  MENSAJES_DOMINICANOS,
};
