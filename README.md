# PoC del Framework ERP Multitenant para BAS

Bienvenido al repositorio oficial del **Framework ERP Multitenant**. Esta Prueba de Concepto (PoC) demuestra una arquitectura robusta, reactiva y escalable diseñada para soportar hasta 1,500 clientes concurrentes, manejando catálogos de millones de productos con aislamiento estricto de datos y actualizaciones en tiempo real.

## Arquitectura del Sistema

El sistema implementa una arquitectura unificada que combina las mejores tecnologías modernas:

- **Frontend Reactivo**: Angular 19 impulsado por **Signals** y un motor de renderizado ultra-rápido (`ChangeDetectionStrategy.OnPush`). Diseñado para actualizar grillas masivas de datos en tiempo real sin congelar la interfaz de usuario.
- **Backend Modular**: .NET 9 orquestado con **.NET Aspire**. Utiliza un enfoque basado en **CQRS** (Command Query Responsibility Segregation) para separar lecturas de escrituras, y un patrón `Facade` en la capa de aplicación para coordinar flujos complejos sin acoplar el núcleo de negocio.
- **Tiempo Real**: **SignalR** integrado nativamente para enviar actualizaciones incrementales al cliente, evitando el costoso "Long Polling" y garantizando que los clientes siempre vean los datos en vivo.
- **Base de Datos**: Microsoft SQL Server.

## Guía de Inicio Rápido (Local)

Para facilitar la vida de los desarrolladores y el equipo de pruebas, hemos encapsulado todo el entorno (Frontend, Backend y Base de Datos) en contenedores de Docker.

Para levantar todo el entorno con un solo comando:

```bash
docker-compose up --build
```

**¿Qué sucede al ejecutar este comando?**
1. Se levanta la **UI de Angular** lista para recibir conexiones.
2. Se levanta el contenedor de **SQL Server** nativo.
3. Se enciende el **Backend API** en .NET.
4. **Magia de Autoconfiguración**: En tiempo de ejecución (al arrancar), el Backend detecta la conexión al SQL Server y, a través de `context.Database.EnsureCreated()` (o `Migrate()`), auto-genera físicamente las bases de datos para nuestros 3 tenants de prueba. ¡No necesitas correr scripts de SQL manualmente!

## Estrategia de Multitenancy Seleccionada

Para este proyecto hemos descartado el enfoque clásico de "Tabla Compartida con columna Discriminadora" (ej. `TenantId` en cada tabla) y hemos apostado fuertemente por la estrategia de **Database-per-Tenant (Base de Datos Independiente por Tenant)**.

**Justificación Técnica:**
- **Aislamiento Físico y Legal**: Al tener una base de datos física por cada cliente, garantizamos que los datos de un tenant jamás se crucen con los de otro por un error de programación. Esto facilita el cumplimiento de normativas de privacidad (GDPR).
- **Mitigación del "Noisy Neighbor" (Vecino Ruidoso)**: En un ERP con 1,500 clientes, si un cliente enorme lanza un reporte pesadísimo sobre millones de registros de productos, no bloqueará las transacciones ni consumirá la CPU/RAM de la base de datos de los clientes más pequeños.
- **Mantenibilidad**: Facilita los respaldos individuales (Point-in-Time Restore por cliente) y permite migrar un cliente específico a hardware dedicado si su volumen de negocio lo requiere.

## Flujo de CI/CD

El proyecto cuenta con un flujo de integración y entrega continua (CI/CD) completamente automatizado a través de **GitHub Actions** (`.github/workflows/deploy.yml`).

### Infraestructura como Código (IaC)
Nos enorgullece destacar que para esta PoC no nos conformamos con despliegues manuales. Hemos implementado la arquitectura real utilizando **Azure Bicep**. En cada push a la rama `main`:
1. **Test**: Se compila el código y se corren los Unit Tests en el runner de GitHub.
2. **Build & Push**: Se construyen las imágenes de Docker de Angular y .NET y se empujan de forma segura a nuestro **Azure Container Registry (ACR)**.
3. **Deploy Automático**: Bicep toma el control, aprovisiona un entorno serverless en **Azure Container Apps**, crea/actualiza la instancia de **Azure SQL Database**, inyecta secretamente la cadena de conexión cifrada y reinicia los contenedores sin tiempo de inactividad (Zero-Downtime Deployment).

> **Nota sobre Docker Hub**: Si en el futuro se requiriera exportar el proyecto a un entorno no-Azure, el pipeline está diseñado modularmente. Bastaría con reemplazar la acción de `docker/login-action` para que apunte a `docker.io` con sus respectivas credenciales, y las imágenes se publicarían en el registro público o privado de Docker Hub.
