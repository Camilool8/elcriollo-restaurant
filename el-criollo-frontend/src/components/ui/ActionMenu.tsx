import { useState, useRef, useEffect, ReactNode } from 'react';
import { createPortal } from 'react-dom';
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
  const [menuPosition, setMenuPosition] = useState({ top: 0, left: 0 });
  const triggerRef = useRef<HTMLDivElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        menuRef.current &&
        !menuRef.current.contains(event.target as Node) &&
        triggerRef.current &&
        !triggerRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const calculateMenuPosition = () => {
    if (!triggerRef.current) return;

    const triggerRect = triggerRef.current.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const viewportWidth = window.innerWidth;

    // Dimensiones del menú (aproximadas)
    const menuWidth = 192; // w-48 = 12rem = 192px
    const menuHeight = items.length * 40 + 16; // altura aproximada por item + padding

    let top = triggerRect.bottom + 4; // 4px de espacio
    let left = triggerRect.right - menuWidth; // alinear a la derecha

    // Si el menú se sale por la derecha, alinear a la izquierda
    if (left < 0) {
      left = triggerRect.left;
    }

    // Si el menú se sale por abajo, mostrarlo arriba
    if (top + menuHeight > viewportHeight) {
      top = triggerRect.top - menuHeight - 4;
    }

    // Si el menú se sale por arriba, ajustar
    if (top < 0) {
      top = 4;
    }

    setMenuPosition({ top, left });
  };

  const handleToggle = () => {
    if (!isOpen) {
      calculateMenuPosition();
    }
    setIsOpen(!isOpen);
  };

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
    <>
      <div ref={triggerRef}>
        <Button variant="ghost" size="sm" onClick={handleToggle}>
          {trigger}
        </Button>
      </div>

      {isOpen &&
        createPortal(
          <div
            ref={menuRef}
            className="fixed bg-white border border-gray-200 rounded-lg shadow-xl z-[9999] transform transition-all duration-200 ease-out"
            style={{
              top: `${menuPosition.top}px`,
              left: `${menuPosition.left}px`,
              width: '192px', // w-48
            }}
          >
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
          </div>,
          document.body
        )}
    </>
  );
};

export default ActionMenu;
