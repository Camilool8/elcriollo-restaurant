import React, { useState } from 'react';
import { useReporteVentas } from '@/hooks/useReporteVentas';
import { FiltrosReporte } from '@/components/reportes/FiltrosReporte';
import { FacturaCard } from '@/components/reportes/FacturaCard';
import { FacturaDetalleModal } from '@/components/reportes/FacturaDetalleModal';
import { Card } from '@/components/ui/Card';
import type { Factura, Orden, Cliente } from '@/types';

const ReportesVentasPage: React.FC = () => {
  const {
    facturas,
    ordenes,
    clientes,
    productos,
    categorias,
    filtros,
    resumen,
    loading,
    error,
    setFiltros,
  } = useReporteVentas();

  // Estado para el modal de detalles
  const [facturaSeleccionada, setFacturaSeleccionada] = useState<Factura | null>(null);
  const [ordenSeleccionada, setOrdenSeleccionada] = useState<Orden | null>(null);
  const [clienteSeleccionado, setClienteSeleccionado] = useState<Cliente | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleFiltrosChange = (nuevosFiltros: any) => {
    setFiltros(nuevosFiltros);
  };

  const handleLimpiarFiltros = () => {
    setFiltros({});
  };

  const handleVerDetalle = (factura: Factura) => {
    const orden = ordenes.find((o) => o.ordenID === factura.ordenID);
    const cliente = clientes.find((c) => c.clienteID === factura.clienteID);

    setFacturaSeleccionada(factura);
    setOrdenSeleccionada(orden || null);
    setClienteSeleccionado(cliente || null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setFacturaSeleccionada(null);
    setOrdenSeleccionada(null);
    setClienteSeleccionado(null);
  };

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold text-dominican-blue mb-2">Reportes de Ventas</h1>
      <p className="text-gray-600 mb-6">
        Visualiza y filtra todas las ventas del restaurante con estilo de facturas reales.
      </p>

      <FiltrosReporte
        filtros={filtros}
        onFiltrosChange={handleFiltrosChange}
        onLimpiarFiltros={handleLimpiarFiltros}
        productos={productos}
        categorias={categorias}
        loading={loading}
      />

      {/* Resumen general */}
      {resumen && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 my-6">
          <Card className="p-4 bg-gradient-to-br from-green-50 to-green-100 border-green-200">
            <div className="text-lg font-bold text-green-700">
              RD$ {resumen.totalFacturado.toLocaleString('es-DO', { minimumFractionDigits: 2 })}
            </div>
            <div className="text-xs text-gray-600">Total Facturado</div>
          </Card>
          <Card className="p-4 bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
            <div className="text-lg font-bold text-blue-700">{resumen.cantidadFacturas}</div>
            <div className="text-xs text-gray-600">Facturas</div>
          </Card>
          <Card className="p-4 bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200">
            <div className="text-lg font-bold text-yellow-700">
              RD$ {resumen.totalPendiente.toLocaleString('es-DO', { minimumFractionDigits: 2 })}
            </div>
            <div className="text-xs text-gray-600">Pendiente de Pago</div>
          </Card>
        </div>
      )}

      {/* Grid de facturas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {facturas.map((factura) => {
          const orden = ordenes.find((o) => o.ordenID === factura.ordenID);

          // Buscar cliente en la factura primero, luego en la orden
          let cliente =
            clientes.find((c) => c.clienteID === factura.clienteID) ||
            (orden?.clienteID ? clientes.find((c) => c.clienteID === orden.clienteID) : undefined);

          // Si no se encuentra cliente, crear uno por defecto
          if (!cliente) {
            cliente = {
              clienteID: factura.clienteID || 1,
              nombreCompleto: 'Cliente General',
              cedula: undefined,
              telefono: undefined,
              email: undefined,
              direccion: undefined,
              fechaNacimiento: undefined,
              preferenciasComida: undefined,
              fechaRegistro: new Date().toISOString(),
              estado: true,
              categoriaCliente: 'General',
              totalOrdenes: 0,
              totalReservaciones: 0,
              totalFacturas: 0,
              promedioConsumo: '0.00',
            };
          }

          // Debug: mostrar información de la factura
          if (process.env.NODE_ENV === 'development') {
            console.log('Factura:', {
              facturaID: factura.facturaID,
              clienteID: factura.clienteID,
              ordenID: factura.ordenID,
              ordenClienteID: orden?.clienteID,
              clienteEncontrado: cliente?.nombreCompleto,
              clienteFinal: cliente?.nombreCompleto,
            });
          }

          return (
            <FacturaCard
              key={factura.facturaID}
              factura={factura}
              orden={orden || null}
              cliente={cliente}
              onVerDetalle={handleVerDetalle}
              onExportar={() => {}}
            />
          );
        })}
      </div>

      {loading && (
        <div className="text-center text-gray-500 py-8">
          <div className="animate-spin w-8 h-8 border-2 border-dominican-blue border-t-transparent rounded-full mx-auto mb-4"></div>
          <p>Cargando datos...</p>
        </div>
      )}

      {error && (
        <div className="text-center text-red-500 py-8">
          <p className="bg-red-50 border border-red-200 rounded-lg p-4">{error}</p>
        </div>
      )}

      {!loading && facturas.length === 0 && (
        <div className="text-center text-gray-500 py-8">
          <p className="bg-gray-50 border border-gray-200 rounded-lg p-4">
            No se encontraron facturas con los filtros aplicados.
          </p>
        </div>
      )}

      {/* Modal de detalles */}
      <FacturaDetalleModal
        factura={facturaSeleccionada}
        orden={ordenSeleccionada}
        cliente={clienteSeleccionado}
        isOpen={isModalOpen}
        onClose={handleCloseModal}
      />
    </div>
  );
};

export default ReportesVentasPage;
