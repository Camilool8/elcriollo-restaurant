import React, { useState } from 'react';
import { toast } from 'react-toastify';
import { Edit, Eye, Download, Mail, Phone, Calendar, MapPin } from 'lucide-react';
import Papa from 'papaparse';
import { useEmpleados } from '@/hooks/useEmpleados';
import { DataTable, Column } from '@/components/ui/DataTable';
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { formatearFechaCorta } from '@/utils/dominicanValidations';
import { Empleado } from '@/types';
import { EmpleadoDetails } from '@/components/admin/EmpleadoDetails';
import { EditEmpleadoForm } from '@/components/admin/EditEmpleadoForm';
import { Input } from '@/components/ui/Input';
import { Search } from 'lucide-react';

const EmpleadosPage: React.FC = () => {
  const { empleados, isLoading, searchEmpleados, refreshEmpleados } = useEmpleados();

  const [isViewModalOpen, setIsViewModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [selectedEmpleado, setSelectedEmpleado] = useState<Empleado | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // ====================================
  // FUNCIONES
  // ====================================

  const handleSearch = () => {
    searchEmpleados(searchQuery);
  };

  const handleViewEmpleado = (empleado: Empleado) => {
    setSelectedEmpleado(empleado);
    setIsViewModalOpen(true);
  };

  const handleEditEmpleado = (empleado: Empleado) => {
    setSelectedEmpleado(empleado);
    setIsEditModalOpen(true);
  };

  const handleCloseModals = () => {
    setIsViewModalOpen(false);
    setIsEditModalOpen(false);
    setSelectedEmpleado(null);
  };

  const handleExportEmpleados = () => {
    if (empleados.length === 0) {
      toast.warn('No hay empleados para exportar.');
      return;
    }

    const dataToExport = empleados.map((emp) => ({
      Nombre: emp.nombreCompleto,
      Cedula: emp.cedula,
      Email: emp.email,
      Telefono: emp.telefono,
      Salario: emp.salarioFormateado,
      'Fecha Contratacion': formatearFechaCorta(emp.fechaContratacion),
    }));

    const csv = Papa.unparse(dataToExport);
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute(
      'download',
      `empleados-el-criollo-${new Date().toISOString().split('T')[0]}.csv`
    );
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    toast.success('Lista de empleados exportada exitosamente.');
  };

  const handleUpdateSuccess = () => {
    handleCloseModals();
    refreshEmpleados();
  };

  // ====================================
  // CONFIGURACIÓN DE COLUMNAS
  // ====================================

  const columns: Column<Empleado>[] = [
    {
      key: 'nombreCompleto',
      label: 'Empleado',
      render: (_, empleado) => (
        <div>
          <div className="font-medium text-gray-900">{empleado.nombreCompleto}</div>
          <div className="text-sm text-stone-gray">{empleado.cedula || 'Sin cédula'}</div>
        </div>
      ),
    },
    {
      key: 'email',
      label: 'Contacto',
      render: (_, empleado) => (
        <div className="space-y-1">
          {empleado.telefonoFormateado && (
            <div className="flex items-center text-sm text-gray-600">
              <Phone className="w-3 h-3 mr-1" />
              {empleado.telefonoFormateado}
            </div>
          )}
          {empleado.email && (
            <div className="flex items-center text-sm text-gray-600">
              <Mail className="w-3 h-3 mr-1" />
              {empleado.email}
            </div>
          )}
        </div>
      ),
    },
    {
      key: 'direccion',
      label: 'Dirección',
      render: (_, empleado) =>
        empleado.direccion ? (
          <div className="flex items-center text-sm text-gray-600">
            <MapPin className="w-3 h-3 mr-1" />
            {empleado.direccion}
          </div>
        ) : (
          <span className="text-stone-gray text-sm">No especificado</span>
        ),
    },
    {
      key: 'fechaContratacion',
      label: 'Fecha Contratación',
      render: (_, empleado) => (
        <div className="flex items-center text-sm text-gray-600">
          <Calendar className="w-3 h-3 mr-1" />
          {formatearFechaCorta(empleado.fechaContratacion)}
        </div>
      ),
    },
    {
      key: 'acciones',
      label: 'Acciones',
      render: (_, empleado) => (
        <div className="flex items-center justify-center space-x-2">
          <Button variant="ghost" size="sm" onClick={() => handleViewEmpleado(empleado)}>
            <Eye className="w-4 h-4" />
          </Button>
          <Button variant="ghost" size="sm" onClick={() => handleEditEmpleado(empleado)}>
            <Edit className="w-4 h-4" />
          </Button>
        </div>
      ),
      align: 'center',
    },
  ];

  // ====================================
  // RENDER
  // ====================================

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-start">
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Gestión de Empleados
          </h1>
          <p className="text-stone-gray mt-1">
            Visualización del personal registrado en el sistema.
          </p>
        </div>
      </div>

      {/* Controles y Tabla */}
      <div className="bg-white rounded-lg shadow-sm border p-6">
        <div className="flex justify-between items-center mb-4">
          <div className="flex-1 flex gap-2">
            <Input
              placeholder="Buscar por nombre, cédula o email..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
              leftIcon={<Search className="w-4 h-4" />}
              fullWidth
            />
            <Button variant="primary" onClick={handleSearch}>
              Buscar
            </Button>
            <Button
              variant="outline"
              onClick={() => {
                setSearchQuery('');
                searchEmpleados('');
              }}
            >
              Limpiar
            </Button>
          </div>
          <div className="flex items-center space-x-2">
            <button
              onClick={handleExportEmpleados}
              className="px-4 py-2 text-sm text-dominican-blue border border-dominican-blue rounded-lg hover:bg-dominican-blue hover:text-white smooth-transition flex items-center"
            >
              <Download className="w-4 h-4 mr-2" />
              Exportar
            </button>
          </div>
        </div>

        <DataTable
          columns={columns}
          data={empleados}
          loading={isLoading}
          emptyMessage="No se encontraron empleados"
        />
      </div>

      {/* Modal de Vista */}
      {selectedEmpleado && (
        <Modal
          isOpen={isViewModalOpen}
          onClose={handleCloseModals}
          title={`Detalles de ${selectedEmpleado.nombreCompleto}`}
        >
          <EmpleadoDetails empleado={selectedEmpleado} />
        </Modal>
      )}

      {/* Modal de Edición */}
      {selectedEmpleado && (
        <Modal
          isOpen={isEditModalOpen}
          onClose={handleCloseModals}
          title={`Editar a ${selectedEmpleado.nombreCompleto}`}
        >
          <EditEmpleadoForm
            empleado={selectedEmpleado}
            onSuccess={handleUpdateSuccess}
            onCancel={handleCloseModals}
          />
        </Modal>
      )}
    </div>
  );
};

export default EmpleadosPage;
