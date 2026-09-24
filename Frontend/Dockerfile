# Stage 1: Build React application
FROM node:20-alpine AS build
WORKDIR /app

COPY task-management-ui/package*.json ./
RUN npm install

COPY task-management-ui/ ./
RUN npm run build

# Stage 2: Serve static app via Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
