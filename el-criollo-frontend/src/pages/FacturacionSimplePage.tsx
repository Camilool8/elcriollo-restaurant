import React, { useState, useEffect } from 'react';
import {
  Receipt,
  TrendingUp,
  RefreshCw,
  Plus,
  Eye,
  DollarSign,
  Trash2,
  AlertCircle,
  CheckCircle,
  XCircle,
  Clock,
  CreditCard,
  FileText,
  Filter,
} from 'lucide-react';

// Components
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

// Components de facturación
import { FacturaForm, ResumenFactura, PagoForm } from '@/components/facturacion';

// Hooks
import { useFacturacion } from '@/hooks/useFacturacion';
import { useDebounce } from '@/hooks/useDebounce';

// Types
import type { Factura, FacturaEstado, MetodoPago } from '@/types';

// Utils
import { formatearPrecio, formatearFecha } from '@/utils/dominicanValidations';

// ============================================================================
// INTERFACES
// ============================================================================

interface FiltrosFacturacion {
  estado?: FacturaEstado;
  metodoPago?: MetodoPago;
  busqueda?: string;
}

// ============================================================================
// COMPONENTE PRINCIPAL
// ============================================================================

export const FacturacionSimplePage: React.FC = () => {
  // Hook de facturación
  const {
    state: { facturas, facturasDelDia, estadisticas, facturaActual, isLoading, error, lastUpdated },
    anularFactura,
    seleccionarFactura,
    refrescarDatos,
    limpiarError,
    calcularTotalFacturado,
  } = useFacturacion({ autoRefresh: true, refreshInterval: 30000 });

  // Estados locales
  const [filtros, setFiltros] = useState<FiltrosFacturacion>({});
  const [busqueda, setBusqueda] = useState('');
  const [modalActivo, setModalActivo] = useState<string | null>(null);
  const [facturaSeleccionada, setFacturaSeleccionada] = useState<Factura | null>(null);

  // Debounce para búsqueda
  const busquedaDebounced = useDebounce(busqueda, 300);

  // ============================================================================
  // EFECTOS
  // ============================================================================

  useEffect(() => {
    if (busquedaDebounced) {
      setFiltros((prev) => ({ ...prev, busqueda: busquedaDebounced }));
    } else {
      setFiltros((prev) => ({ ...prev, busqueda: undefined }));
    }
  }, [busquedaDebounced]);

  // ============================================================================
  // FUNCIONES AUXILIARES
  // ============================================================================

  const obtenerFacturasFiltradas = (): Factura[] => {
    let facturasFiltradas = facturasDelDia;

    if (filtros.estado) {
      facturasFiltradas = facturasFiltradas.filter((f) => f.estado === filtros.estado);
    }

    if (filtros.metodoPago) {
      facturasFiltradas = facturasFiltradas.filter((f) => f.metodoPago === filtros.metodoPago);
    }

    if (filtros.busqueda) {
      const busquedaLower = filtros.busqueda.toLowerCase();
      facturasFiltradas = facturasFiltradas.filter(
        (f) =>
          f.numeroFactura.toLowerCase().includes(busquedaLower) ||
          f.cliente?.nombreCompleto.toLowerCase().includes(busquedaLower) ||
          f.orden?.numeroOrden.toLowerCase().includes(busquedaLower)
      );
    }

    return facturasFiltradas;
  };

  const obtenerIconoEstado = (estado: FacturaEstado) => {
    switch (estado) {
      case 'Pendiente':
        return <Clock className="w-4 h-4 text-amber-600" />;
      case 'Pagada':
        return <CheckCircle className="w-4 h-4 text-green-600" />;
      case 'Anulada':
        return <XCircle className="w-4 h-4 text-red-600" />;
      default:
        return <AlertCircle className="w-4 h-4 text-gray-600" />;
    }
  };

  const obtenerIconoMetodoPago = (metodo: MetodoPago) => {
    switch (metodo) {
      case 'Efectivo':
        return <DollarSign className="w-4 h-4 text-green-600" />;
      case 'Tarjeta de Crédito':
      case 'Tarjeta de Débito':
        return <CreditCard className="w-4 h-4 text-blue-600" />;
      default:
        return <FileText className="w-4 h-4 text-gray-600" />;
    }
  };

  const obtenerEstadoBadge = (estado: FacturaEstado) => {
    switch (estado) {
      case 'Pendiente':
        return <Badge variant="warning">Pendiente</Badge>;
      case 'Pagada':
        return <Badge variant="success">Pagada</Badge>;
      case 'Anulada':
        return <Badge variant="danger">Anulada</Badge>;
      default:
        return <Badge variant="secondary">{estado}</Badge>;
    }
  };

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const handleVerFactura = (factura: Factura) => {
    setFacturaSeleccionada(factura);
    seleccionarFactura(factura);
    setModalActivo('ver-factura');
  };

  const handlePagarFactura = (factura: Factura) => {
    setFacturaSeleccionada(factura);
    setModalActivo('pagar-factura');
  };

  const handleAnularFactura = (factura: Factura) => {
    setFacturaSeleccionada(factura);
    setModalActivo('anular-factura');
  };

  const handleConfirmarAnulacion = async (motivo: string) => {
    if (!facturaSeleccionada) return;

    try {
      await anularFactura(facturaSeleccionada.facturaID, motivo);
      setModalActivo(null);
      setFacturaSeleccionada(null);
    } catch (error) {
      console.error('Error anulando factura:', error);
    }
  };

  const handleLimpiarFiltros = () => {
    setFiltros({});
    setBusqueda('');
  };

  const cerrarModal = () => {
    setModalActivo(null);
    setFacturaSeleccionada(null);
    limpiarError();
  };

  // ============================================================================
  // RENDER
  // ============================================================================

  const facturasFiltradas = obtenerFacturasFiltradas();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-dominican-blue bg-opacity-10 rounded-lg">
            <Receipt className="w-8 h-8 text-dominican-blue" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Facturación</h1>
            <p className="text-gray-600">
              {facturasFiltradas.length} facturas • Total:{' '}
              {formatearPrecio(calcularTotalFacturado(facturasFiltradas))}
            </p>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <div className="text-sm text-gray-600">
            {lastUpdated && <span>Actualizado: {formatearFecha(lastUpdated)}</span>}
          </div>

          <Button variant="outline" onClick={refrescarDatos} disabled={isLoading}>
            <RefreshCw className={`w-4 h-4 mr-2 ${isLoading ? 'animate-spin' : ''}`} />
            Refrescar
          </Button>

          <Button
            onClick={() => setModalActivo('crear-factura')}
            className="bg-dominican-blue hover:bg-dominican-blue/90"
          >
            <Plus className="w-4 h-4 mr-2" />
            Nueva Factura
          </Button>
        </div>
      </div>

      {/* Estadísticas rápidas */}
      {estadisticas && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Facturas Hoy</p>
                <p className="text-2xl font-bold text-dominican-blue">
                  {estadisticas.totalFacturasHoy}
                </p>
              </div>
              <Receipt className="w-8 h-8 text-dominican-blue opacity-60" />
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Ventas del Día</p>
                <p className="text-2xl font-bold text-green-600">
                  {formatearPrecio(estadisticas.ventasDelDia)}
                </p>
              </div>
              <TrendingUp className="w-8 h-8 text-green-600 opacity-60" />
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Promedio Factura</p>
                <p className="text-2xl font-bold text-blue-600">
                  {formatearPrecio(estadisticas.promedioFactura)}
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-blue-600 opacity-60" />
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Pendientes</p>
                <p className="text-2xl font-bold text-amber-600">
                  {estadisticas.facturasPendientes}
                </p>
              </div>
              <Clock className="w-8 h-8 text-amber-600 opacity-60" />
            </div>
          </Card>
        </div>
      )}

      {/* Filtros */}
      <Card className="p-4">
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex-1 min-w-[200px]">
            <Input
              placeholder="Buscar por número, cliente u orden..."
              value={busqueda}
              onChange={(e) => setBusqueda(e.target.value)}
            />
          </div>

          <select
            value={filtros.estado || ''}
            onChange={(e) =>
              setFiltros((prev) => ({
                ...prev,
                estado: (e.target.value as FacturaEstado) || undefined,
              }))
            }
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
          >
            <option value="">Todos los estados</option>
            <option value="Pendiente">Pendiente</option>
            <option value="Pagada">Pagada</option>
            <option value="Anulada">Anulada</option>
          </select>

          <select
            value={filtros.metodoPago || ''}
            onChange={(e) =>
              setFiltros((prev) => ({
                ...prev,
                metodoPago: (e.target.value as MetodoPago) || undefined,
              }))
            }
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
          >
            <option value="">Todos los métodos</option>
            <option value="Efectivo">Efectivo</option>
            <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
            <option value="Tarjeta de Débito">Tarjeta de Débito</option>
            <option value="Transferencia Bancaria">Transferencia</option>
          </select>

          <Button variant="outline" onClick={handleLimpiarFiltros}>
            <Filter className="w-4 h-4 mr-2" />
            Limpiar
          </Button>
        </div>
      </Card>

      {/* Lista de facturas */}
      <Card>
        {isLoading ? (
          <div className="p-8 text-center">
            <LoadingSpinner size="lg" />
            <p className="text-gray-600 mt-2">Cargando facturas...</p>
          </div>
        ) : facturasFiltradas.length === 0 ? (
          <div className="p-8 text-center text-gray-500">
            <Receipt className="w-16 h-16 mx-auto mb-4 opacity-50" />
            <p>No hay facturas que coincidan con los filtros</p>
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {facturasFiltradas.map((factura) => (
              <div key={factura.facturaID} className="p-4 hover:bg-gray-50">
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-4">
                      <div>
                        <div className="font-medium text-dominican-blue">
                          {factura.numeroFactura}
                        </div>
                        <div className="text-sm text-gray-600">
                          {formatearFecha(factura.fechaFactura)}
                        </div>
                      </div>

                      <div>
                        <div className="font-medium">
                          {factura.cliente?.nombreCompleto || 'Cliente ocasional'}
                        </div>
                        <div className="text-sm text-gray-600">
                          Orden #{factura.orden?.numeroOrden}
                        </div>
                      </div>

                      <div className="flex items-center space-x-2">
                        {obtenerIconoMetodoPago(factura.metodoPago)}
                        <span className="text-sm">{factura.metodoPago}</span>
                      </div>

                      <div className="font-bold text-dominican-blue">
                        {formatearPrecio(factura.total)}
                      </div>

                      <div className="flex items-center space-x-2">
                        {obtenerIconoEstado(factura.estado)}
                        {obtenerEstadoBadge(factura.estado)}
                      </div>
                    </div>
                  </div>

                  <div className="flex space-x-2">
                    <Button size="sm" variant="outline" onClick={() => handleVerFactura(factura)}>
                      <Eye className="w-4 h-4" />
                    </Button>

                    {factura.estado === 'Pendiente' && (
                      <>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => handlePagarFactura(factura)}
                        >
                          <DollarSign className="w-4 h-4" />
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => handleAnularFactura(factura)}
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>

      {/* Modales */}
      {modalActivo === 'crear-factura' && (
        <Modal isOpen={true} onClose={cerrarModal} title="Nueva Factura" size="xl">
          <FacturaForm
            onFacturaCreada={(factura) => {
              console.log('Factura creada:', factura);
              cerrarModal();
            }}
            onClose={cerrarModal}
          />
        </Modal>
      )}

      {modalActivo === 'ver-factura' && facturaActual && (
        <Modal isOpen={true} onClose={cerrarModal} title="Detalles de Factura" size="xl">
          <ResumenFactura factura={facturaActual} onClose={cerrarModal} />
        </Modal>
      )}

      {modalActivo === 'pagar-factura' && facturaSeleccionada && (
        <Modal isOpen={true} onClose={cerrarModal} title="Procesar Pago" size="lg">
          <PagoForm factura={facturaSeleccionada} onClose={cerrarModal} />
        </Modal>
      )}

      {modalActivo === 'anular-factura' && facturaSeleccionada && (
        <Modal isOpen={true} onClose={cerrarModal} title="Anular Factura" size="md">
          <div className="space-y-4">
            <div className="flex items-center space-x-3 p-4 bg-red-50 rounded-lg">
              <AlertCircle className="w-6 h-6 text-red-600" />
              <div>
                <p className="font-medium text-red-800">¿Está seguro de anular esta factura?</p>
                <p className="text-sm text-red-600">Esta acción no se puede deshacer.</p>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Motivo de anulación
              </label>
              <textarea
                rows={3}
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
                placeholder="Describa el motivo de la anulación..."
              />
            </div>

            <div className="flex justify-end space-x-3">
              <Button variant="outline" onClick={cerrarModal}>
                Cancelar
              </Button>
              <Button
                onClick={() => handleConfirmarAnulacion('Motivo de anulación')}
                className="bg-red-600 hover:bg-red-700"
              >
                Anular Factura
              </Button>
            </div>
          </div>
        </Modal>
      )}

      {/* Error Display */}
      {error && (
        <div className="fixed bottom-4 right-4 max-w-md">
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded shadow-lg">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <AlertCircle className="w-5 h-5" />
                <span className="font-medium">Error</span>
              </div>
              <button onClick={limpiarError} className="text-red-700 hover:text-red-900">
                <XCircle className="w-4 h-4" />
              </button>
            </div>
            <p className="mt-1 text-sm">{error}</p>
          </div>
        </div>
      )}
    </div>
  );
};
