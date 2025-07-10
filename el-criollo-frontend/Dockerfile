FROM --platform=linux/amd64 node:18-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build

FROM --platform=linux/amd64 nginx:alpine

RUN mkdir -p /tmp/nginx_client_temp \
    /tmp/nginx_proxy_temp \
    /tmp/nginx_fastcgi_temp \
    /tmp/nginx_uwsgi_temp \
    /tmp/nginx_scgi_temp \
    /var/cache/nginx \
    /var/log/nginx && \
    chmod 700 /tmp/nginx_*_temp && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /tmp/nginx_*_temp

COPY nginx.conf /etc/nginx/nginx.conf

COPY --from=builder /app/dist /usr/share/nginx/html

RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"] 