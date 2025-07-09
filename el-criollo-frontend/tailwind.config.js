/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        'dominican-red': {
          DEFAULT: '#C41E3A',
          50: '#FBE9EC',
          100: '#F6D3DA',
          200: '#EFA7B6',
          300: '#E87B91',
          400: '#E14F6D',
          500: '#C41E3A',
          600: '#A51931',
          700: '#861428',
          800: '#68101F',
          900: '#490B15',
        },
        'dominican-blue': {
          DEFAULT: '#002D62',
          50: '#E6EAF0',
          100: '#BFCBDB',
          200: '#99ACC7',
          300: '#738EB2',
          400: '#4D709E',
          500: '#002D62',
          600: '#002653',
          700: '#001E44',
          800: '#001735',
          900: '#001026',
        },
        'palm-green': {
          DEFAULT: '#3A7D44',
          50: '#ECF2ED',
          100: '#D8E5DA',
          200: '#B2CBBB',
          300: '#8CB09B',
          400: '#65967C',
          500: '#3A7D44',
          600: '#326B3B',
          700: '#2A5A32',
          800: '#224828',
          900: '#1A361F',
        },
        'warm-beige': '#F5F5DC',
        'stone-gray': '#6B7280',

        // Colores complementarios del Caribe
        'caribbean-gold': '#FFD700',
        'sunset-orange': '#FF6B35',

        // Variaciones para UI
        red: {
          50: '#fef2f2',
          100: '#fee2e2',
          200: '#fecaca',
          300: '#fca5a5',
          400: '#f87171',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c',
          800: '#991b1b',
          900: '#7f1d1d',
        },
        blue: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
        },
        orange: {
          50: '#fff7ed',
          100: '#ffedd5',
          200: '#fed7aa',
          300: '#fdba74',
          400: '#fb923c',
          500: '#f97316',
          600: '#ea580c',
          700: '#c2410c',
          800: '#9a3412',
          900: '#7c2d12',
        },
      },
      fontFamily: {
        sans: [
          'Inter',
          'ui-sans-serif',
          'system-ui',
          '-apple-system',
          'BlinkMacSystemFont',
          'Segoe UI',
          'Roboto',
          'Helvetica Neue',
          'Arial',
          'Noto Sans',
          'sans-serif',
        ],
        heading: [
          'Poppins',
          'ui-sans-serif',
          'system-ui',
          '-apple-system',
          'BlinkMacSystemFont',
          'Segoe UI',
          'Roboto',
          'Helvetica Neue',
          'Arial',
          'Noto Sans',
          'sans-serif',
        ],
        mono: [
          'JetBrains Mono',
          'ui-monospace',
          'SFMono-Regular',
          'Menlo',
          'Monaco',
          'Consolas',
          'Liberation Mono',
          'Courier New',
          'monospace',
        ],
      },
      spacing: {
        18: '4.5rem',
        88: '22rem',
        128: '32rem',
      },
      animation: {
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-down': 'slideDown 0.3s ease-out',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
      },
      boxShadow: {
        caribbean: '0 4px 6px -1px rgba(207, 20, 43, 0.1), 0 2px 4px -1px rgba(207, 20, 43, 0.06)',
        'caribbean-lg':
          '0 10px 15px -3px rgba(207, 20, 43, 0.1), 0 4px 6px -2px rgba(207, 20, 43, 0.05)',
      },
      backdropBlur: {
        xs: '2px',
      },
    },
  },
  plugins: [
    // Plugin para formularios
    function ({ addUtilities }) {
      addUtilities({
        '.smooth-transition': {
          transition: 'all 0.2s ease-in-out',
        },
        '.dominican-gradient': {
          background: 'linear-gradient(135deg, #CF142B 0%, #002D62 100%)',
        },
        '.caribbean-gradient': {
          background: 'linear-gradient(135deg, #FFD700 0%, #228B22 50%, #CF142B 100%)',
        },
        '.glass-effect': {
          'backdrop-filter': 'blur(10px)',
          'background-color': 'rgba(255, 255, 255, 0.8)',
        },
        '.scrollbar-hide': {
          '-ms-overflow-style': 'none',
          'scrollbar-width': 'none',
          '&::-webkit-scrollbar': {
            display: 'none',
          },
        },
      });
    },
  ],
};
