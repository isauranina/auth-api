#!/bin/bash
set -e
sleep 30

# Los placeholders __IMAGE_TAG__ y __JWT_SECRET__ los reemplaza el workflow con sed antes de user-data.
sudo mkdir -p /home/deploy
cd /home/deploy

sudo tee compose.yml > /dev/null <<'DEPLOY_COMPOSE_EOF'
services:
  api:
    image: inina14/auth-api:__IMAGE_TAG__
    container_name: usuario-api
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: "production"
      ASPNETCORE_URLS: "http://+:5000"
      JWT_SECRET_KEY: "__JWT_SECRET__"
      LOKI_URL: "http://localhost:3100"
    ports:
      - "80:5000"
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "curl -fsS http://127.0.0.1:5000/health/live || exit 1",
        ]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 40s
DEPLOY_COMPOSE_EOF

echo "Desplegando contenedores..."
docker compose -f compose.yml up -d || docker-compose -f compose.yml up -d
echo "¡Despliegue finalizado!"
