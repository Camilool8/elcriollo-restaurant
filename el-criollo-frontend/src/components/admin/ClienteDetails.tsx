import React from 'react';
import { User, Mail, Phone, MapPin, Calendar, Heart, Award, Hash } from 'lucide-react';
import { Cliente } from '@/types';
import { Modal } from '@/components/ui/Modal';
import { Badge } from '@/components/ui/Badge';

interface ClienteDetailsProps {
  cliente: Cliente;
  isOpen: boolean;
  onClose: () => void;
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
      <p className="text-base font-semibold text-gray-800">{value || 'No especificado'}</p>
    </div>
  </div>
);

export const ClienteDetails: React.FC<ClienteDetailsProps> = ({ cliente, isOpen, onClose }) => {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Detalles del Cliente" size="lg">
      <div className="space-y-4">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-dominican-blue">{cliente.nombreCompleto}</h2>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <DetailItem icon={<User />} label="Cédula" value={cliente.cedula} />
          <DetailItem icon={<Award />} label="Categoría" value={cliente.categoriaCliente} />
          <DetailItem icon={<Phone />} label="Teléfono" value={cliente.telefono} />
          <DetailItem icon={<Mail />} label="Email" value={cliente.email} />
          <DetailItem icon={<MapPin />} label="Dirección" value={cliente.direccion} />
          <DetailItem
            icon={<Calendar />}
            label="Fecha de Registro"
            value={new Date(cliente.fechaRegistro).toLocaleDateString()}
          />
          {cliente.fechaNacimiento && (
            <DetailItem
              icon={<Calendar />}
              label="Fecha de Nacimiento"
              value={new Date(cliente.fechaNacimiento).toLocaleDateString()}
            />
          )}
          {cliente.ultimaVisita && (
            <DetailItem
              icon={<Calendar />}
              label="Última Visita"
              value={new Date(cliente.ultimaVisita).toLocaleDateString()}
            />
          )}
        </div>
      </div>
    </Modal>
  );
};
