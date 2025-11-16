// next.config.ts
import type { NextConfig } from "next";
import createNextIntlPlugin from 'next-intl/plugin';

const API_URL = process.env.API_URL ?? "http://localhost:7080"; 
const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts'); // ← THÊM PATH

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${API_URL}/api/:path*`,
      },
    ];
  },
};

export default withNextIntl(nextConfig);