import { defineConfig } from 'vite';
export default defineConfig({ server: { proxy: { '/api': { target: 'http://localhost:5195', changeOrigin: true } } } });
