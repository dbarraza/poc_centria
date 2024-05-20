FROM node:16.13-alpine AS build

WORKDIR /react-app

COPY package*.json ./

RUN npm i

COPY . .

RUN npm run build

FROM nginx:alpine

COPY ./nginx/nginx.conf /etc/nginx/nginx.conf

COPY --from=build /react-app/build /usr/share/nginx/html