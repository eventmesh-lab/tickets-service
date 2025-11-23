# 📦 Explicación de Archivos Docker

**Archivos ESENCIALES** (necesarios):

- `Dockerfile` - Define cómo construir la imagen
- `docker-compose.yml` - Orquesta los servicios

**Scripts HELPER** (opcionales, para facilitar el uso):

- `docker-start.sh/ps1` - Inicia Docker en modo producción
- `docker-dev-start.sh/ps1` - Inicia Docker con hot reload

## **docker-dev-start.sh / docker-dev-start.ps1** (OPCIONAL - Helper)

**¿Qué es?** Script que inicia Docker en modo desarrollo con hot reload.

**¿Para qué sirve?**

- Similar a `docker-start.sh` pero en modo desarrollo
- Habilita hot reload (los cambios en código se reflejan automáticamente)
- Monta el código fuente como volumen

**¿Cuándo se usa?** Cuando quieres desarrollar con Docker pero con hot reload

**Alternativa manual:**

```bash
docker-compose --profile dev build api-dev
docker-compose --profile dev up api-dev
```

**¿Puedo eliminarlo?** ✅ SÍ - Es solo una conveniencia.

---

## 🤔 ¿Por qué hay tantos archivos?

### Razón 1: Versiones para diferentes sistemas

- `.sh` = Para Linux, Mac, Git Bash
- `.ps1` = Para Windows PowerShell

### Con Scripts (actual)

```bash
./docker-start.sh    # Un comando, todo automático
```

### Sin Scripts (manual)

```bash
docker-compose down
dotnet clean tickets-service.sln
docker-compose build --no-cache api
docker-compose up -d
sleep 5
docker-compose ps
```

- ⚠️ `docker-start.sh/ps1` (puedes usar `docker-compose up --build`)
- ⚠️ `docker-dev-start.sh/ps1` (puedes usar `docker-compose --profile dev up`)
