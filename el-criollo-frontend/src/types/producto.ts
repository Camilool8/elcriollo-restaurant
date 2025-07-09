export interface Categoria {
  categoriaID: number;
  nombreCategoria: string;
  descripcion?: string;
  cantidadProductos: number;
  productosDisponibles: number;
}

export interface Producto {
  productoID: number;
  nombre: string;
  descripcion?: string;
  categoria: Categoria;
  precio: string;
  precioNumerico: number;
  tiempoPreparacion: string;
  imagen?: string;
  estaDisponible: boolean;
  inventario: {
    cantidadDisponible: number;
    nivelStock: string;
    colorIndicador: string;
    stockBajo: boolean;
  };
}

export interface Combo {
  comboID: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  descuento: number;
  estado: boolean;
  productos?: ComboProducto[];
}

export interface ComboProducto {
  comboProductoID: number;
  comboID: number;
  productoID: number;
  cantidad: number;
  producto?: Producto;
}
