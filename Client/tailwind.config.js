import forms from '@tailwindcss/forms'
import containerQueries from '@tailwindcss/container-queries'

/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        "primary": "#059669",
        "primary-light": "#34d399",
        "primary-dark": "#064e3b",
        "secondary": "#1e3a8a",
        "success": "#16a34a",
        "danger": "#dc2626",
        "warning": "#d97706",
        "background-light": "#F8FAFC",
        "surface-light": "#ffffff",
        "text-dark": "#1e293b",
        "text-muted": "#64748b",
      },
      fontFamily: {
        "display": ["Manrope", "sans-serif"],
        "arabic": ["Cairo", "sans-serif"],
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/container-queries'),
  ],
}
