/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#E98300',
          dark: '#b86800',
        },
        secondary: '#0D7A74',
        background: '#F3F4F6',
      },
    },
  },
  plugins: [],
}

