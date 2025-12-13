# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Build args for environment variables (baked at build time)
ARG NEXT_PUBLIC_API_URL=""
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Production stage - standalone (optimized, smaller image)
FROM node:20-alpine AS production
WORKDIR /app

# Copy standalone output (includes node_modules and server)
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
COPY --from=build /app/public ./public

# Copy i18n runtime files (not included in standalone)
COPY --from=build /app/src/messages ./src/messages
COPY --from=build /app/src/i18n ./src/i18n

EXPOSE 3000

ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Start standalone server (not npm start)
CMD ["node", "server.js"]
