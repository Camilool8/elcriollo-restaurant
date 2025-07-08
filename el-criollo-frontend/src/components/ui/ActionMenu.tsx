import { useState, useRef, useEffect, ReactNode } from 'react';
import { MoreHorizontal } from 'lucide-react';
import { Button } from './Button';
import type { ActionMenuItem } from '@/types';

interface ActionMenuProps {
  items: ActionMenuItem[];
  trigger?: ReactNode;
}

export const ActionMenu: React.FC<ActionMenuProps> = ({
  items,
  trigger = <MoreHorizontal className="w-4 h-4" />,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const getItemClasses = (variant?: string) => {
    const base =
      'flex items-center w-full px-3 py-2 text-sm text-left hover:bg-gray-100 smooth-transition';

    switch (variant) {
      case 'danger':
        return `${base} text-red-600 hover:bg-red-50`;
      case 'warning':
        return `${base} text-yellow-600 hover:bg-yellow-50`;
      default:
        return `${base} text-gray-700`;
    }
  };

  return (
    <div className="relative" ref={menuRef}>
      <Button variant="ghost" size="sm" onClick={() => setIsOpen(!isOpen)}>
        {trigger}
      </Button>

      {isOpen && (
        <div className="absolute right-0 mt-1 w-48 bg-white border border-gray-200 rounded-lg shadow-lg z-50">
          <div className="py-1">
            {items.map((item, index) => (
              <button
                key={index}
                className={getItemClasses(item.variant)}
                onClick={() => {
                  item.onClick();
                  setIsOpen(false);
                }}
                disabled={item.disabled}
              >
                {item.icon && <span className="mr-2">{item.icon}</span>}
                {item.label}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default ActionMenu;
