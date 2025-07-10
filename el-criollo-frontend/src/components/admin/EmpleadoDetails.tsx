import React from 'react';
import { User, Mail, Phone, Calendar, DollarSign, Info } from 'lucide-react';
import { Empleado } from '@/types';
import { formatearFechaCorta } from '@/utils/dominicanValidations';

interface EmpleadoDetailsProps {
  empleado: Empleado;
}

const DetailItem = ({
  icon,
  label,
  value,
}: {
  icon: React.ReactNode;
  label: string;
  value: string | React.ReactNode;
}) => (
  <div className="flex items-start py-3 px-4 bg-gray-50 rounded-lg">
    <div className="text-dominican-blue mr-4 mt-1">{icon}</div>
    <div>
      <p className="text-sm font-medium text-stone-gray">{label}</p>
      <p className="text-base font-semibold text-gray-800">{value}</p>
    </div>
  </div>
);

export const EmpleadoDetails: React.FC<EmpleadoDetailsProps> = ({ empleado }) => {
  return (
    <div className="space-y-4">
      <DetailItem icon={<User />} label="Nombre Completo" value={empleado.nombreCompleto} />
      <DetailItem icon={<Info />} label="Cédula" value={empleado.cedula} />
      <DetailItem icon={<Mail />} label="Email" value={empleado.email} />
      <DetailItem icon={<Phone />} label="Teléfono" value={empleado.telefonoFormateado} />
      <DetailItem
        icon={<DollarSign />}
        label="Salario"
        value={empleado.salarioFormateado || 'No especificado'}
      />
      <DetailItem
        icon={<Calendar />}
        label="Fecha de Contratación"
        value={formatearFechaCorta(empleado.fechaContratacion)}
      />
      <DetailItem icon={<Info />} label="Tiempo en la Empresa" value={empleado.tiempoEnEmpresa} />
    </div>
  );
};
