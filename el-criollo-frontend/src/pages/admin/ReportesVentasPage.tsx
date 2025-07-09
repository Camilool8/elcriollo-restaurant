import React from 'react';
import { useReporteVentas } from '@/hooks/useReporteVentas';
import { FiltrosReporte } from '@/components/reportes/FiltrosReporte';
import { FacturaCard } from '@/components/reportes/FacturaCard';
import { Card } from '@/components/ui/Card';

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

  const handleFiltrosChange = (nuevosFiltros: any) => {
    setFiltros(nuevosFiltros);
  };

  const handleLimpiarFiltros = () => {
    setFiltros({});
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
        clientes={clientes}
        productos={productos}
        categorias={categorias}
        loading={loading}
      />

      {/* Resumen general */}
      {resumen && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 my-6">
          <Card className="p-4 bg-green-50 border-green-200">
            <div className="text-lg font-bold text-green-700">
              RD$ {resumen.totalFacturado.toLocaleString('es-DO', { minimumFractionDigits: 2 })}
            </div>
            <div className="text-xs text-gray-600">Total Facturado</div>
          </Card>
          <Card className="p-4 bg-blue-50 border-blue-200">
            <div className="text-lg font-bold text-blue-700">{resumen.cantidadFacturas}</div>
            <div className="text-xs text-gray-600">Facturas</div>
          </Card>
          <Card className="p-4 bg-yellow-50 border-yellow-200">
            <div className="text-lg font-bold text-yellow-700">
              RD$ {resumen.totalPendiente.toLocaleString('es-DO', { minimumFractionDigits: 2 })}
            </div>
            <div className="text-xs text-gray-600">Pendiente de Pago</div>
          </Card>
          <Card className="p-4 bg-dominican-blue text-white">
            <div className="text-lg font-bold">{resumen.cantidadOrdenes}</div>
            <div className="text-xs">Órdenes</div>
          </Card>
        </div>
      )}

      {/* Grid de facturas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {facturas.map((factura) => {
          const orden = ordenes.find((o) => o.ordenID === factura.ordenID);
          const cliente = clientes.find((c) => c.clienteID === factura.clienteID);
          return (
            <FacturaCard
              key={factura.facturaID}
              factura={factura}
              orden={orden}
              cliente={cliente}
            />
          );
        })}
      </div>

      {loading && <div className="text-center text-gray-500 py-8">Cargando datos...</div>}
      {error && <div className="text-center text-red-500 py-8">{error}</div>}
    </div>
  );
};

export default ReportesVentasPage;
