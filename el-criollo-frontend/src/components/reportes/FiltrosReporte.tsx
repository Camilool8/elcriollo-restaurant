import React from 'react';
import { Filter, X, Calendar, Package, Tag, DollarSign } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import type { FiltrosReporteProps } from '@/types/reportes';

export const FiltrosReporte: React.FC<FiltrosReporteProps> = ({
  filtros,
  onFiltrosChange,
  onLimpiarFiltros,
  productos,
  categorias,
  loading = false,
}) => {
  const handleDateChange = (field: 'fechaInicio' | 'fechaFin', value: string) => {
    const date = value ? new Date(value) : undefined;
    onFiltrosChange({
      ...filtros,
      [field]: date,
    });
  };

  const handleSelectChange = (field: keyof typeof filtros, value: string | number) => {
    const parsedValue =
      typeof value === 'string' && value !== ''
        ? field.includes('Id')
          ? parseInt(value)
          : value
        : undefined;

    onFiltrosChange({
      ...filtros,
      [field]: parsedValue,
    });
  };

  const handleNumberChange = (field: 'montoMinimo' | 'montoMaximo', value: string) => {
    const number = value ? parseFloat(value) : undefined;
    onFiltrosChange({
      ...filtros,
      [field]: number,
    });
  };

  const limpiarFiltros = () => {
    onLimpiarFiltros();
  };

  const filtrosActivos = Object.values(filtros).filter((v) => v !== undefined).length;

  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center space-x-2">
          <Filter className="w-5 h-5 text-dominican-blue" />
          <h3 className="text-lg font-semibold text-gray-900">Filtros de Reporte</h3>
          {filtrosActivos > 0 && (
            <span className="bg-dominican-blue text-white text-xs px-2 py-1 rounded-full">
              {filtrosActivos} activos
            </span>
          )}
        </div>
        {filtrosActivos > 0 && (
          <Button
            variant="ghost"
            size="sm"
            onClick={limpiarFiltros}
            className="text-gray-500 hover:text-gray-700"
          >
            <X className="w-4 h-4 mr-1" />
            Limpiar
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {/* Filtro por fecha de inicio */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            <Calendar className="w-4 h-4 inline mr-1" />
            Fecha Inicio
          </label>
          <Input
            type="date"
            value={filtros.fechaInicio?.toISOString().split('T')[0] || ''}
            onChange={(e) => handleDateChange('fechaInicio', e.target.value)}
            className="w-full"
            disabled={loading}
          />
        </div>

        {/* Filtro por fecha de fin */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            <Calendar className="w-4 h-4 inline mr-1" />
            Fecha Fin
          </label>
          <Input
            type="date"
            value={filtros.fechaFin?.toISOString().split('T')[0] || ''}
            onChange={(e) => handleDateChange('fechaFin', e.target.value)}
            className="w-full"
            disabled={loading}
          />
        </div>

        {/* Filtro por producto */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            <Package className="w-4 h-4 inline mr-1" />
            Producto
          </label>
          <select
            value={filtros.productoId || ''}
            onChange={(e) => handleSelectChange('productoId', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
            disabled={loading}
          >
            <option value="">Todos los productos</option>
            {productos.map((producto) => (
              <option key={producto.productoID} value={producto.productoID}>
                {producto.nombre}
              </option>
            ))}
          </select>
        </div>

        {/* Filtro por categoría */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            <Tag className="w-4 h-4 inline mr-1" />
            Categoría
          </label>
          <select
            value={filtros.categoriaId || ''}
            onChange={(e) => handleSelectChange('categoriaId', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
            disabled={loading}
          >
            <option value="">Todas las categorías</option>
            {categorias.map((categoria) => (
              <option key={categoria.categoriaID} value={categoria.categoriaID}>
                {categoria.nombre}
              </option>
            ))}
          </select>
        </div>

        {/* Filtro por método de pago */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            <DollarSign className="w-4 h-4 inline mr-1" />
            Método de Pago
          </label>
          <select
            value={filtros.metodoPago || ''}
            onChange={(e) => handleSelectChange('metodoPago', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
            disabled={loading}
          >
            <option value="">Todos los métodos</option>
            <option value="Efectivo">Efectivo</option>
            <option value="Tarjeta de Débito">Tarjeta de Débito</option>
            <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
            <option value="Transferencia Bancaria">Transferencia Bancaria</option>
            <option value="Pago Móvil">Pago Móvil</option>
            <option value="Cheque">Cheque</option>
          </select>
        </div>

        {/* Filtro por estado de factura */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Estado de Factura</label>
          <select
            value={filtros.estadoFactura || ''}
            onChange={(e) => handleSelectChange('estadoFactura', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
            disabled={loading}
          >
            <option value="">Todos los estados</option>
            <option value="Pendiente">Pendiente</option>
            <option value="Pagada">Pagada</option>
            <option value="Anulada">Anulada</option>
          </select>
        </div>

        {/* Filtro por monto mínimo */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Monto Mínimo</label>
          <Input
            type="number"
            placeholder="0.00"
            value={filtros.montoMinimo || ''}
            onChange={(e) => handleNumberChange('montoMinimo', e.target.value)}
            className="w-full"
            disabled={loading}
          />
        </div>

        {/* Filtro por monto máximo */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Monto Máximo</label>
          <Input
            type="number"
            placeholder="0.00"
            value={filtros.montoMaximo || ''}
            onChange={(e) => handleNumberChange('montoMaximo', e.target.value)}
            className="w-full"
            disabled={loading}
          />
        </div>
      </div>
    </Card>
  );
};
