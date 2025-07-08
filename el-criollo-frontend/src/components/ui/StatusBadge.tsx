import { Badge } from './Badge';

interface StatusBadgeProps {
  status: string;
  type?: 'user' | 'client' | 'employee' | 'order' | 'payment';
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, type = 'user' }) => {
  const getVariant = () => {
    const statusLower = status.toLowerCase();

    switch (statusLower) {
      case 'activo':
      case 'active':
      case 'completado':
      case 'pagada':
        return 'success';
      case 'inactivo':
      case 'inactive':
      case 'cancelado':
      case 'anulada':
        return 'danger';
      case 'pendiente':
      case 'en preparacion':
        return 'warning';
      case 'en proceso':
      case 'preparando':
        return 'info';
      default:
        return 'secondary';
    }
  };

  const getDisplayText = () => {
    switch (status.toLowerCase()) {
      case 'active':
        return 'Activo';
      case 'inactive':
        return 'Inactivo';
      case 'pending':
        return 'Pendiente';
      case 'completed':
        return 'Completado';
      case 'cancelled':
        return 'Cancelado';
      default:
        return status;
    }
  };

  return <Badge variant={getVariant()}>{getDisplayText()}</Badge>;
};

export default StatusBadge;
