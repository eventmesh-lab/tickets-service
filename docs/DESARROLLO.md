# Guía de Desarrollo - Tickets Service

Esta guía documenta los flujos de trabajo recomendados para el desarrollo del servicio `tickets-service` y cómo evitar problemas comunes de bloqueo de archivos durante la compilación.

## Tabla de Contenidos

- [Flujos de Trabajo](#flujos-de-trabajo)
- [Configuración del Entorno](#configuración-del-entorno)
- [Troubleshooting](#troubleshooting)
- [Scripts Helper](#scripts-helper)

## Flujos de Trabajo

### ⚠️ Regla de Oro

**NUNCA compilar manualmente (IDE o `dotnet build`) mientras Docker Compose tiene el servicio corriendo.**

Debes elegir **UNO** de los siguientes flujos por sesión de desarrollo:

### Escenario 1: Desarrollo Local con IDE (Recomendado)

**Cuándo usar**: Desarrollo activo con debugging, hot reload automático del IDE.

**Pasos**:

1. **Asegurarte de que Docker NO está corriendo el servicio**:
   ```bash
   docker-compose down
   ```

2. **Iniciar solo la base de datos**:
   ```bash
   docker-compose up db
   ```

3. **Ejecutar el servicio desde el IDE**:
   - Visual Studio: F5 o Debug > Start Debugging
   - Rider: Run > Debug 'tickets-service.Api'
   - VS Code: F5 con configuración de launch

4. **O ejecutar manualmente**:
   ```bash
   cd src/tickets-service.Api
   dotnet run
   ```

**Ventajas**:
- Hot reload automático del IDE
- Breakpoints y debugging completo
- Sin conflictos de archivos
- Compilación rápida e incremental

### Escenario 2: Desarrollo con Docker (Modo Producción)

**Cuándo usar**: Testing de la configuración Docker, testing de integración, CI/CD local.

**Pasos**:

1. **Detener cualquier instancia local**:
   - Cerrar el IDE si está ejecutando el servicio
   - Verificar que no hay procesos `dotnet` activos

2. **Construir y ejecutar con Docker**:
   ```bash
   docker-compose up --build
   ```

3. **El servicio estará disponible en**: http://localhost:5005

**Ventajas**:
- Replica el entorno de producción
- Testing de configuración Docker
- Aislamiento completo

### Escenario 3: Desarrollo con Docker (Modo Development + Hot Reload)

**Cuándo usar**: Desarrollo con Docker pero con hot reload habilitado.

**Pasos**:

1. **Detener cualquier instancia local**

2. **Ejecutar con perfil de desarrollo**:
   ```bash
   docker-compose --profile dev up api-dev
   ```

**Nota**: Este modo monta el código fuente como volumen y usa `dotnet watch` para hot reload.

### Escenario 4: Testing

**Cuándo usar**: Ejecutar pruebas unitarias, de integración o end-to-end.

**Pasos**:

1. **Detener Docker completamente**:
   ```bash
   docker-compose down
   ```

2. **Iniciar solo la base de datos** (si es necesario para tests de integración):
   ```bash
   docker-compose up db -d
   ```

3. **Ejecutar tests**:
   ```bash
   # Todos los tests
   dotnet test
   
   # Tests de un proyecto específico
   dotnet test tests/tickets-service.Domain.Tests/
   
   # Con cobertura
   dotnet test --collect:"XPlat Code Coverage"
   ```

## Configuración del Entorno

### Archivo Directory.Build.props

El proyecto incluye un archivo `Directory.Build.props` en la raíz que configura:

- `UseSharedCompilation=false`: Deshabilita el servidor de compilación compartida de Roslyn que puede mantener bloqueos en archivos DLL
- Configuraciones comunes aplicadas a todos los proyectos

### Dependencias Externas

El servicio depende de:

1. **PostgreSQL** (puerto 5432)
   - Usuario: `tickets`
   - Password: `tickets`
   - Base de datos: `tickets_service`

2. **RabbitMQ** (opcional, según configuración)
   - Para eventos de dominio

### Variables de Entorno

**Desarrollo Local**:
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__TicketsDb="Host=localhost;Port=5432;Database=tickets_service;Username=tickets;Password=tickets"
```

**Desarrollo con Docker**:
Las variables están configuradas en `docker-compose.yml`.

## Troubleshooting

### Error: "The file is locked by: tickets_service.Api"

**Causa**: Intentaste compilar mientras el servicio está corriendo en Docker o en otro proceso.

**Solución**:

1. Detener Docker:
   ```bash
   docker-compose down
   ```

2. Si persiste, identificar y terminar el proceso:
   ```bash
   # En Windows (Git Bash)
   taskkill //F //PID <PID_NUMBER>
   
   # En Linux/Mac
   kill -9 <PID_NUMBER>
   ```

3. Limpiar artefactos de compilación:
   ```bash
   dotnet clean tickets-service.sln
   ```

4. Recompilar:
   ```bash
   dotnet build tickets-service.sln
   ```

### Error: "Port 5005 is already in use"

**Causa**: El puerto está siendo usado por otra instancia del servicio.

**Solución**:

1. Verificar procesos:
   ```bash
   # Windows
   netstat -ano | findstr :5005
   
   # Linux/Mac
   lsof -i :5005
   ```

2. Terminar el proceso o cambiar el puerto en `launchSettings.json` o `docker-compose.yml`

### Error de Conexión a PostgreSQL

**Causa**: La base de datos no está disponible o las credenciales son incorrectas.

**Solución**:

1. Verificar que PostgreSQL está corriendo:
   ```bash
   docker-compose ps
   ```

2. Si no está corriendo:
   ```bash
   docker-compose up db -d
   ```

3. Verificar conexión:
   ```bash
   docker exec -it tickets-db psql -U tickets -d tickets_service
   ```

### Problemas con Hot Reload

**Si el hot reload no funciona en el IDE**:

1. Verificar que `UseSharedCompilation=false` está en `Directory.Build.props`
2. Reiniciar el IDE
3. Limpiar la solución: `dotnet clean`
4. Reconstruir: `dotnet build`

**Si el hot reload no funciona en Docker (modo dev)**:

1. Verificar que estás usando el perfil correcto:
   ```bash
   docker-compose --profile dev up api-dev
   ```

2. Verificar que los volúmenes están montados correctamente

## Scripts Helper

El proyecto incluye scripts helper en la raíz para simplificar operaciones comunes:

### `dev-start.sh` / `dev-start.ps1`

Detiene Docker y ejecuta el servicio localmente para desarrollo con IDE.

```bash
# Linux/Mac
./dev-start.sh

# Windows PowerShell
./dev-start.ps1
```

### `docker-start.sh` / `docker-start.ps1`

Limpia contenedores existentes y ejecuta con Docker en modo producción.

```bash
# Linux/Mac
./docker-start.sh

# Windows PowerShell
./docker-start.ps1
```

### `docker-dev-start.sh` / `docker-dev-start.ps1`

Ejecuta Docker en modo desarrollo con hot reload.

```bash
# Linux/Mac
./docker-dev-start.sh

# Windows PowerShell
./docker-dev-start.ps1
```

### `clean-all.sh` / `clean-all.ps1`

Limpieza profunda de artefactos de compilación y contenedores Docker.

```bash
# Linux/Mac
./clean-all.sh

# Windows PowerShell
./clean-all.ps1
```

## Mejores Prácticas

1. **Elige un flujo y mantente en él**: No cambies entre Docker e IDE sin detener el servicio primero.

2. **Limpia antes de cambiar**: Ejecuta `dotnet clean` antes de cambiar entre flujos de trabajo.

3. **Usa el script apropiado**: Los scripts helper facilitan el cambio entre flujos.

4. **Verifica el estado de Docker**: Usa `docker-compose ps` para verificar qué está corriendo.

5. **Debugging**: Usa el IDE para debugging intensivo, Docker para testing de integración.

6. **Testing**: Siempre detén Docker antes de ejecutar tests para evitar conflictos de puerto.

7. **Commit limpio**: No comitees carpetas `bin/` u `obj/` (ya están en `.gitignore`).

## Comandos Útiles

```bash
# Ver estado de contenedores
docker-compose ps

# Ver logs del servicio
docker-compose logs -f api

# Reconstruir imagen Docker
docker-compose build --no-cache api

# Limpiar todo Docker
docker-compose down -v
docker system prune -f

# Limpiar compilación .NET
dotnet clean tickets-service.sln

# Restaurar dependencias
dotnet restore tickets-service.sln

# Compilar todo
dotnet build tickets-service.sln

# Ejecutar tests
dotnet test

# Ejecutar con watch (hot reload)
dotnet watch run --project src/tickets-service.Api/tickets-service.Api.csproj
```

## Recursos Adicionales

- [Documentación de Arquitectura](./ARCHITECTURE.md)
- [Lenguaje Ubicuo](./lenguaje-ubicuo.md)
- [Ficha del Servicio](./tickets-service.md)
- [Guía Técnica Global](https://eventmesh-lab.github.io/org-docs/guia-tecnica/)

---

**Última actualización**: Noviembre 2025


