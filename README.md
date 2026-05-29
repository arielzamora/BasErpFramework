# PoC del Framework ERP Multitenant para BAS

Bienvenido al repositorio oficial del **Framework ERP Multitenant**. Esta Prueba de Concepto (PoC) demuestra una arquitectura robusta, reactiva y escalable diseñada para soportar hasta 1,500 clientes concurrentes, manejando catálogos de millones de productos con aislamiento estricto de datos y actualizaciones en tiempo real.

<!-- 🚀 **Despliegue en Vivo (Azure)**: [https://ca-bas-erp-frontend.blackcoast-ff28a7a6.westus2.azurecontainerapps.io/](https://ca-bas-erp-frontend.blackcoast-ff28a7a6.westus2.azurecontainerapps.io/) -->

## Arquitectura del Sistema

El sistema implementa una arquitectura unificada que combina las mejores tecnologías modernas:

- **Frontend Reactivo**: Angular 19 impulsado por **Signals** y un motor de renderizado ultra-rápido (`ChangeDetectionStrategy.OnPush`). Diseñado para actualizar grillas masivas de datos en tiempo real sin congelar la interfaz de usuario.
- **Backend Modular**: .NET 9 orquestado con **.NET Aspire**. Utiliza un enfoque tradicional de **Arquitectura en N-Capas (N-Tier)** donde los servicios de aplicación (`ProductoService`) orquestan las transacciones a través de un patrón *Unit of Work* y *Repository*, manteniendo la simplicidad y alta cohesión del código.
- **Tiempo Real**: **SignalR** integrado nativamente para enviar actualizaciones incrementales al cliente, evitando el costoso "Long Polling" y garantizando que los clientes siempre vean los datos en vivo.
- **Base de Datos**: Microsoft SQL Server.

## Diagramas de Arquitectura (C4 Model)

Para mantener este archivo principal ágil y limpio, los diagramas detallados del modelo C4 se encuentran organizados en la carpeta de documentación del repositorio. 

Puedes consultarlos directamente en los siguientes enlaces:
- 🗺️ [Diagrama de Contexto (Nivel 1)](docs/diagrams/c4-context.png)
- 📦 [Diagrama de Contenedores (Nivel 2)](docs/diagrams/c4-container.png)
- ⚙️ [Diagrama de Componentes del Backend (Nivel 3)](docs/diagrams/c4-component.png)

## Guía de Inicio Rápido (Local) - Ejecución con Docker Compose

Para facilitar la evaluación de esta prueba técnica y asegurar que el entorno sea completamente reproducible sin requerir instalaciones previas de SDKs, se ha provisto un entorno Dockerizado completo.

**Instrucciones claras para ejecutar el entorno:**

1. Clona el repositorio y navega a la raíz del proyecto.
2. Abre una terminal y ejecuta el siguiente comando:

```bash
docker-compose up --build
```

**¿Qué incluye este entorno?**
- `db`: Contenedor oficial de Microsoft SQL Server 2022.
- `backend`: API REST en .NET 9. Al arrancar, se conecta automáticamente a la base de datos SQL y auto-genera el esquema a través de Entity Framework Core Migrations (`context.Database.EnsureCreated()` en `Program.cs`), poblando la base de datos `BasErpBd_Default`.
- `frontend`: SPA en Angular 19 servida a través de NGINX, escuchando en el puerto `4200` y configurada dinámicamente para conectarse al backend local expuesto en el puerto `8080`.

**Una vez finalizado el comando, accede a:**
👉 **http://localhost:4200**

---

## Explicación de la Estrategia Multitenant Elegida

Para resolver el requerimiento de Multitenancy y la escala de alto rendimiento, **se ha elegido la estrategia de Bases de Datos Separadas (Database-per-Tenant)**, descartando el enfoque de "base de datos única con TenantId por tabla".

**Justificación de la estrategia de Bases de Datos Separadas:**
1. **Aislamiento de Datos (Data Isolation)**: Se garantiza física y legalmente que la información de un inquilino jamás colisione o se exponga a otro inquilino por una omisión en un `WHERE TenantId = X`.
2. **Mitigación del Vecino Ruidoso (Noisy Neighbor)**: Las cargas computacionales intensivas sobre millones de registros de un inquilino grande no bloquearán los recursos ni la memoria de la base de datos de los inquilinos más pequeños.
3. **Mantenibilidad Continua**: Cada base de datos puede ser respaldada o restaurada independientemente (Point-in-Time Restore) sin forzar un rollback masivo de todo el sistema.

*(Para más detalles, consultar el documento `docs/adrs/0001-database-per-tenant-isolation.md`).*

---

## Integración y Despliegue Continuo (CI/CD)

El repositorio incluye un pipeline completo configurado en `.github/workflows/deploy.yml` que valida automáticamente el código mediante pruebas unitarias en cada push.

### ¿Cómo se realizaría el push de las imágenes hacia Docker Hub?
Actualmente, por orgullo de ingeniería y profesionalismo de esta PoC, las imágenes se envían a un registro privado corporativo en la nube (**Azure Container Registry**) y se despliegan de manera automatizada usando **Infraestructura como Código (Bicep)**.

Sin embargo, para adaptar el pipeline y hacer el push de las imágenes hacia el registro público **Docker Hub**, los cambios a realizar en el archivo YAML serían mínimos:

1. Modificar la acción de Autenticación de Docker para apuntar a Docker Hub:
```yaml
      - name: Log in to Docker Hub
        uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}
```
2. Modificar las etiquetas (Tags) de construcción en los jobs para utilizar el namespace de Docker Hub en lugar del servidor ACR:
```yaml
      - name: Build and Push Backend
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./src/backend/Dockerfile
          push: true
          tags: tu-usuario-dockerhub/bas-erp-backend:latest
```

De esta manera, las imágenes quedarían publicadas globalmente y listas para ser extraídas en cualquier otro entorno en la nube.
