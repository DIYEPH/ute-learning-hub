// next.config.ts
import type { NextConfig } from "next";
import createNextIntlPlugin from 'next-intl/plugin';

const API_URL = process.env.API_URL ?? "http://localhost:7080";
const NEXT_PUBLIC_API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:7080";
const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts');

const nextConfig: NextConfig = {
  async rewrites() {
    if (NEXT_PUBLIC_API_URL.startsWith('/')) {
      return [];
    }
    
    return [
      {
        source: "/api/:path*",
        destination: `${API_URL}/api/:path*`,
      },
      {
        source: "/images/:path*",
        destination: `${API_URL}/images/:path*`,
      },
    ];
  },
  images: {
    // Cho phép load ảnh từ cùng domain (qua Nginx hoặc rewrite)
    remotePatterns: [
      {
        protocol: 'http',
        hostname: 'localhost',
        pathname: '/images/**',
      },
      {
        protocol: 'https',
        hostname: 'localhost',
        pathname: '/images/**',
      },
    ],
    // Cho phép load ảnh từ relative paths
    dangerouslyAllowSVG: true,
    contentDispositionType: 'attachment',
    contentSecurityPolicy: "default-src 'self'; script-src 'none'; sandbox;",
  },
};

export default withNextIntl(nextConfig);