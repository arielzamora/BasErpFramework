# Stage 1: Build the Angular App
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
# Construir la aplicación para producción
RUN npm run build --configuration=production

# Stage 2: Serve with NGINX
FROM nginx:alpine
# El nuevo application builder de Angular deja los estáticos en la subcarpeta 'browser'
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

# Exponer el puerto estándar de Nginx
EXPOSE 80

# Comando por defecto para arrancar Nginx, inyectando la variable dinámica primero
CMD ["/bin/sh", "-c", "sed -i \"s|</head>|<script>window.env = { apiUrl: '${BACKEND_URL}' };</script></head>|g\" /usr/share/nginx/html/index.html && nginx -g 'daemon off;'"]
