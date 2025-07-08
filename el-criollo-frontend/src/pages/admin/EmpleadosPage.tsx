import React, { useState } from 'react';
import { toast } from 'react-toastify';
import {
  Plus,
  Edit,
  Eye,
  Phone,
  Mail,
  MapPin,
  Calendar,
  DollarSign,
  Briefcase,
  UserCheck,
  UserX,
  Download,
} from 'lucide-react';
import { useEmpleados } from '@/hooks/useEmpleados';
import { DataTable, Column } from '@/components/ui/DataTable';
import { SearchFilter } from '@/components/ui/SearchFilter';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { ActionMenu } from '@/components/ui/ActionMenu';
import { CreateEmpleadoForm } from '@/components/admin/CreateEmpleadoForm';
import {
  formatearPrecio,
  formatearFechaCorta,
  formatearNombreCompleto,
} from '@/utils/dominicanValidations';
import { Empleado } from '@/types';

const EmpleadosPage: React.FC = () => {
  const { empleados, departamentos, isLoading, totalCount, searchEmpleados, refreshEmpleados } =
    useEmpleados();

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedDepartamento, setSelectedDepartamento] = useState('');
  const [selectedEstado, setSelectedEstado] = useState('');

  // ====================================
  // FUNCIONES
  // ====================================

  const handleSearch = (query: string) => {
    searchEmpleados(query);
  };

  const handleFilterChange = () => {
    // TODO: Implementar filtrado por departamento y estado
    refreshEmpleados();
  };

  const handleEmpleadoCreated = () => {
    refreshEmpleados();
  };

  const handleViewEmpleado = (empleado: Empleado) => {
    // TODO: Abrir modal de detalles
    toast.info(`Ver detalles de ${empleado.nombre} ${empleado.apellido}`);
  };

  const handleEditEmpleado = (empleado: Empleado) => {
    // TODO: Abrir modal de edición
    toast.info(`Editar ${empleado.nombre} ${empleado.apellido}`);
  };

  const handleToggleStatus = (empleado: Empleado) => {
    // TODO: Implementar cambio de estado
    toast.info(`Cambiar estado de ${empleado.nombre} ${empleado.apellido}`);
  };

  const handleExportEmpleados = () => {
    // TODO: Implementar exportación
    toast.info('Exportando lista de empleados...');
  };

  // ====================================
  // CONFIGURACIÓN DE COLUMNAS
  // ====================================

  const columns: Column<Empleado>[] = [
    {
      key: 'nombre',
      label: 'Empleado',
      render: (_, empleado) => (
        <div>
          <div className="font-medium text-gray-900">
            {formatearNombreCompleto(empleado.nombre, empleado.apellido)}
          </div>
          <div className="text-sm text-stone-gray">{empleado.cedula || 'Sin cédula'}</div>
        </div>
      ),
    },
    {
      key: 'email',
      label: 'Contacto',
      render: (_, empleado) => (
        <div className="space-y-1">
          {empleado.telefono && (
            <div className="flex items-center text-sm text-gray-600">
              <Phone className="w-3 h-3 mr-1" />
              {empleado.telefono}
            </div>
          )}
          {empleado.email && (
            <div className="flex items-center text-sm text-gray-600">
              <Mail className="w-3 h-3 mr-1" />
              {empleado.email}
            </div>
          )}
          {empleado.direccion && (
            <div className="flex items-center text-sm text-gray-600">
              <MapPin className="w-3 h-3 mr-1" />
              {empleado.direccion}
            </div>
          )}
        </div>
      ),
    },
    {
      key: 'departamento',
      label: 'Departamento',
      render: (_, empleado) => (
        <div className="flex items-center">
          <Briefcase className="w-4 h-4 mr-2 text-dominican-blue" />
          <span className="font-medium text-dominican-blue">
            {empleado.departamento || 'Sin asignar'}
          </span>
        </div>
      ),
    },
    {
      key: 'salario',
      label: 'Salario',
      render: (_, empleado) =>
        empleado.salario ? (
          <div className="flex items-center font-medium text-green-600">
            <DollarSign className="w-4 h-4 mr-1" />
            {formatearPrecio(empleado.salario)}
          </div>
        ) : (
          <span className="text-stone-gray text-sm">No especificado</span>
        ),
      align: 'right',
    },
    {
      key: 'estado',
      label: 'Estado',
      render: (_, empleado) => <StatusBadge status={empleado.estado} type="employee" />,
      align: 'center',
    },
    {
      key: 'fechaIngreso',
      label: 'Fecha Ingreso',
      render: (_, empleado) => (
        <div className="flex items-center text-sm text-gray-600">
          <Calendar className="w-3 h-3 mr-1" />
          {formatearFechaCorta(empleado.fechaIngreso)}
        </div>
      ),
    },
    {
      key: 'acciones',
      label: 'Acciones',
      render: (_, empleado) => (
        <ActionMenu
          items={[
            {
              label: 'Ver Detalles',
              icon: <Eye className="w-4 h-4" />,
              onClick: () => handleViewEmpleado(empleado),
            },
            {
              label: 'Editar',
              icon: <Edit className="w-4 h-4" />,
              onClick: () => handleEditEmpleado(empleado),
            },
            {
              label: empleado.estado === 'Activo' ? 'Desactivar' : 'Activar',
              icon:
                empleado.estado === 'Activo' ? (
                  <UserX className="w-4 h-4" />
                ) : (
                  <UserCheck className="w-4 h-4" />
                ),
              onClick: () => handleToggleStatus(empleado),
              variant: empleado.estado === 'Activo' ? 'danger' : 'default',
            },
          ]}
        />
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
          <p className="text-stone-gray mt-1">Personal y recursos humanos del restaurante</p>
        </div>
      </div>

      {/* Métricas rápidas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg p-4 shadow-sm border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-dominican-blue">{empleados.length}</p>
              <p className="text-sm text-stone-gray">Total Empleados</p>
            </div>
            <Briefcase className="w-8 h-8 text-dominican-blue opacity-20" />
          </div>
        </div>

        <div className="bg-white rounded-lg p-4 shadow-sm border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-green-600">
                {empleados.filter((e) => e.estado === 'Activo').length}
              </p>
              <p className="text-sm text-stone-gray">Activos</p>
            </div>
            <UserCheck className="w-8 h-8 text-green-600 opacity-20" />
          </div>
        </div>

        <div className="bg-white rounded-lg p-4 shadow-sm border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-yellow-600">{departamentos.length}</p>
              <p className="text-sm text-stone-gray">Departamentos</p>
            </div>
            <Briefcase className="w-8 h-8 text-yellow-600 opacity-20" />
          </div>
        </div>

        <div className="bg-white rounded-lg p-4 shadow-sm border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-blue-600">
                {empleados.filter((e) => e.salario).length}
              </p>
              <p className="text-sm text-stone-gray">Con Salario</p>
            </div>
            <DollarSign className="w-8 h-8 text-blue-600 opacity-20" />
          </div>
        </div>
      </div>

      {/* Búsqueda y filtros */}
      <SearchFilter
        placeholder="Buscar por nombre, cédula, teléfono o departamento..."
        onSearch={handleSearch}
        filters={[
          {
            label: 'Departamento',
            options: departamentos.map((dep) => ({ label: dep, value: dep })),
            value: selectedDepartamento,
            onChange: setSelectedDepartamento,
          },
          {
            label: 'Estado',
            options: [
              { label: 'Activo', value: 'Activo' },
              { label: 'Inactivo', value: 'Inactivo' },
            ],
            value: selectedEstado,
            onChange: setSelectedEstado,
          },
        ]}
        actions={[
          {
            label: 'Agregar Empleado',
            icon: <Plus className="w-4 h-4" />,
            onClick: () => setShowCreateForm(true),
            variant: 'primary',
          },
          {
            label: 'Exportar',
            icon: <Download className="w-4 h-4" />,
            onClick: handleExportEmpleados,
            variant: 'outline',
          },
        ]}
      />

      {/* Tabla de empleados */}
      <DataTable
        data={empleados}
        columns={columns}
        loading={isLoading}
        emptyMessage="No se encontraron empleados"
      />

      {/* Modal crear empleado */}
      <CreateEmpleadoForm
        isOpen={showCreateForm}
        onClose={() => setShowCreateForm(false)}
        onSuccess={handleEmpleadoCreated}
      />
    </div>
  );
};

export default EmpleadosPage;
