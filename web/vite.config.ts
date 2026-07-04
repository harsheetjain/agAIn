import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The .NET API (SignalR hub + REST) runs on :5215. Proxying keeps the browser
// on a single origin, which also means a dev tunnel on :5173 exposes the whole
// app (UI + hub + API) with no extra config.
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5215', changeOrigin: true },
      '/hub': { target: 'http://localhost:5215', changeOrigin: true, ws: true },
    },
  },
})
