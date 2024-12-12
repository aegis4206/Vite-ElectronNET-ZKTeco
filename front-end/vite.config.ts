import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': 'http://localhost:8081', // API 代理到後端
    },
  },
  build: {
    emptyOutDir: true,
    outDir: '../wwwroot', // 發布目錄
    rollupOptions: {
      external: ['electron'],
    },
  },
  base:"./"
})
