import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: Number(process.env.PORT) || 5173,
    proxy: {
      '/api': { target: 'http://localhost:5027', changeOrigin: true },
      '/hubs': { target: 'http://localhost:5027', changeOrigin: true, ws: true },
    },
  },
})
