import React, { ReactNode } from 'react';
import { ChevronLeft, ChevronRight, Search } from 'lucide-react';
import { Button } from './Button';
import { Input } from './Input';
import { Card } from './Card';

// ====================================
// TIPOS
// ====================================

export interface Column<T> {
  key: keyof T | string;
  label: string;
  sortable?: boolean;
  render?: (value: any, item: T, index: number) => ReactNode;
  width?: string;
  align?: 'left' | 'center' | 'right';
}

export interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  loading?: boolean;
  searchable?: boolean;
  searchPlaceholder?: string;
  onSearch?: (query: string) => void;
  pagination?: {
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalItems: number;
    onPageChange: (page: number) => void;
    onPageSizeChange?: (size: number) => void;
  };
  actions?: {
    label: string;
    icon?: ReactNode;
    onClick: () => void;
    variant?: 'primary' | 'secondary' | 'outline';
  }[];
  emptyMessage?: string;
  className?: string;
}

// ====================================
// COMPONENTE PRINCIPAL
// ====================================

export function DataTable<T extends Record<string, any>>({
  data,
  columns,
  loading = false,
  searchable = false,
  searchPlaceholder = 'Buscar...',
  onSearch,
  pagination,
  actions,
  emptyMessage = 'No hay datos disponibles',
  className = '',
}: DataTableProps<T>) {
  const [searchQuery, setSearchQuery] = React.useState('');
  const [sortColumn, setSortColumn] = React.useState<string | null>(null);
  const [sortDirection, setSortDirection] = React.useState<'asc' | 'desc'>('asc');

  // ====================================
  // FUNCIONES
  // ====================================

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    onSearch?.(query);
  };

  const handleSort = (columnKey: string) => {
    if (sortColumn === columnKey) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(columnKey);
      setSortDirection('asc');
    }
  };

  const getCellValue = (item: T, column: Column<T>) => {
    if (column.render) {
      return column.render(item[column.key as keyof T], item, 0);
    }
    return item[column.key as keyof T];
  };

  const getAlignmentClass = (align?: string) => {
    switch (align) {
      case 'center':
        return 'text-center';
      case 'right':
        return 'text-right';
      default:
        return 'text-left';
    }
  };

  // ====================================
  // RENDER
  // ====================================

  return (
    <Card className={className}>
      {/* Header con búsqueda y acciones */}
      {(searchable || actions) && (
        <div className="flex flex-col md:flex-row justify-between items-center gap-4 mb-6">
          {/* Búsqueda */}
          {searchable && (
            <div className="flex-1 max-w-md">
              <Input
                placeholder={searchPlaceholder}
                value={searchQuery}
                onChange={(e) => handleSearch(e.target.value)}
                leftIcon={<Search className="w-4 h-4" />}
                fullWidth
              />
            </div>
          )}

          {/* Acciones */}
          {actions && (
            <div className="flex gap-2">
              {actions.map((action, index) => (
                <Button
                  key={index}
                  variant={action.variant || 'primary'}
                  leftIcon={action.icon}
                  onClick={action.onClick}
                >
                  {action.label}
                </Button>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Tabla */}
      <div className="overflow-x-auto">
        <table className="w-full">
          {/* Header */}
          <thead>
            <tr className="border-b border-gray-200">
              {columns.map((column) => (
                <th
                  key={String(column.key)}
                  className={`
                    p-4 font-heading font-semibold text-dominican-blue
                    ${getAlignmentClass(column.align)}
                    ${column.sortable ? 'cursor-pointer hover:bg-gray-50' : ''}
                  `}
                  style={{ width: column.width }}
                  onClick={() => column.sortable && handleSort(String(column.key))}
                >
                  <div className="flex items-center gap-2">
                    {column.label}
                    {column.sortable && (
                      <div className="flex flex-col">
                        <div
                          className={`w-0 h-0 border-l-2 border-r-2 border-b-2 border-transparent ${
                            sortColumn === column.key && sortDirection === 'asc'
                              ? 'border-b-dominican-blue'
                              : 'border-b-gray-300'
                          }`}
                        />
                        <div
                          className={`w-0 h-0 border-l-2 border-r-2 border-t-2 border-transparent ${
                            sortColumn === column.key && sortDirection === 'desc'
                              ? 'border-t-dominican-blue'
                              : 'border-t-gray-300'
                          }`}
                        />
                      </div>
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>

          {/* Body */}
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={columns.length} className="text-center p-8 text-stone-gray">
                  <div className="flex items-center justify-center">
                    <div className="animate-spin w-6 h-6 border-2 border-dominican-red border-t-transparent rounded-full mr-2" />
                    Cargando...
                  </div>
                </td>
              </tr>
            ) : data.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="text-center p-8 text-stone-gray">
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              data.map((item, index) => (
                <tr
                  key={index}
                  className="border-b border-gray-100 hover:bg-gray-50 smooth-transition"
                >
                  {columns.map((column) => (
                    <td
                      key={String(column.key)}
                      className={`p-4 ${getAlignmentClass(column.align)}`}
                    >
                      {getCellValue(item, column)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Paginación */}
      {pagination && (
        <div className="flex flex-col md:flex-row justify-between items-center gap-4 mt-6 pt-4 border-t">
          {/* Información */}
          <div className="text-sm text-stone-gray">
            Mostrando {(pagination.currentPage - 1) * pagination.pageSize + 1} -{' '}
            {Math.min(pagination.currentPage * pagination.pageSize, pagination.totalItems)} de{' '}
            {pagination.totalItems} resultados
          </div>

          {/* Controles de paginación */}
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={pagination.currentPage === 1}
              onClick={() => pagination.onPageChange(pagination.currentPage - 1)}
            >
              <ChevronLeft className="w-4 h-4" />
            </Button>

            {/* Páginas */}
            <div className="flex gap-1">
              {Array.from({ length: Math.min(5, pagination.totalPages) }, (_, i) => {
                const page = i + 1;
                return (
                  <Button
                    key={page}
                    variant={page === pagination.currentPage ? 'primary' : 'outline'}
                    size="sm"
                    onClick={() => pagination.onPageChange(page)}
                  >
                    {page}
                  </Button>
                );
              })}
            </div>

            <Button
              variant="outline"
              size="sm"
              disabled={pagination.currentPage === pagination.totalPages}
              onClick={() => pagination.onPageChange(pagination.currentPage + 1)}
            >
              <ChevronRight className="w-4 h-4" />
            </Button>
          </div>
        </div>
      )}
    </Card>
  );
}

export default DataTable;
