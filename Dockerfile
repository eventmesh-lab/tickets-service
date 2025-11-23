# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias
COPY . ./

# Restaurar dependencias
WORKDIR /src/src/tickets-service.Api
RUN dotnet restore

# Publicar aplicación en modo Release
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Development (para desarrollo con hot reload)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

# Copiar archivos de proyecto
COPY . ./

# Restaurar dependencias
WORKDIR /app/src/tickets-service.Api
RUN dotnet restore

# Exponer puerto
EXPOSE 80

# Usar dotnet watch para hot reload en desarrollo
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:80", "--no-launch-profile"]

# Stage 3: Runtime (producción y docker-compose estándar)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar artefactos publicados desde el stage de build
COPY --from=build /app/publish ./

# Exponer puerto
EXPOSE 80

# Configurar variable de entorno para el puerto y ejecutar aplicación
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "tickets_service.Api.dll"]
