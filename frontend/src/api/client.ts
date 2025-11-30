import { client } from '@/src/api/database/client.gen';

client.setConfig({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7080',
  auth: () => {
    const token = localStorage.getItem('access_token');
    return token ? `Bearer ${token}` : undefined;
  },
});