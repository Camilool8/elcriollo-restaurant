import { useState } from 'react';
import { Card } from './Card';
import { Input } from './Input';
import { Search } from 'lucide-react';
import { Button } from './Button';
import { ReactNode } from 'react';

interface FilterOption {
  label: string;
  value: string;
}

interface SearchFilterProps {
  placeholder?: string;
  onSearch: (query: string) => void;
  filters?: {
    label: string;
    options: FilterOption[];
    value: string;
    onChange: (value: string) => void;
  }[];
  actions?: {
    label: string;
    icon?: ReactNode;
    onClick: () => void;
    variant?: 'primary' | 'secondary' | 'outline';
  }[];
}

export const SearchFilter: React.FC<SearchFilterProps> = ({
  placeholder = 'Buscar...',
  onSearch,
  filters,
  actions,
}) => {
  const [searchQuery, setSearchQuery] = useState('');

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    onSearch(query);
  };

  return (
    <Card className="mb-6">
      <div className="flex flex-col lg:flex-row gap-4">
        {/* Búsqueda principal */}
        <div className="flex-1">
          <Input
            placeholder={placeholder}
            value={searchQuery}
            onChange={(e) => handleSearch(e.target.value)}
            leftIcon={<Search className="w-4 h-4" />}
            fullWidth
          />
        </div>

        {/* Filtros */}
        {filters && (
          <div className="flex gap-3">
            {filters.map((filter, index) => (
              <div key={index} className="min-w-40">
                <select
                  value={filter.value}
                  onChange={(e) => filter.onChange(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
                >
                  <option value="">{filter.label}</option>
                  {filter.options.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
            ))}
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
    </Card>
  );
};
