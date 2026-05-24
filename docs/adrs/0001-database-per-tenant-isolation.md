# ADR 0001: Database-per-Tenant Isolation

**Fecha**: 2026-05-23
**Estado**: Aceptado

## Contexto

El BAS ERP Framework está diseñado para escalar y soportar un entorno Multitenant (multi-inquilino) masivo, con un estimado inicial de 1,500 clientes concurrentes. Cada cliente manejará grandes volúmenes de datos transaccionales y catálogos con millones de productos.
Es imperativo asegurar el máximo rendimiento, privacidad de los datos de cada cliente de acuerdo con normativas legales (e.g., GDPR), y evitar que un pico de procesamiento de un cliente degrade la experiencia de los demás.
El marco de decisión se reduce principalmente a dos estrategias comunes:
1. **Shared Database, Shared Schema (Tabla Compartida)**: Todos los clientes en la misma BD física, separados lógicamente por un campo `TenantId`.
2. **Database-per-Tenant (Base de Datos Aislada)**: Cada cliente tiene su propia base de datos física independiente.

## Decisión

Hemos decidido adoptar la arquitectura **Database-per-Tenant**. Cada inquilino del ERP será aprovisionado con su propia base de datos SQL Server independiente en el motor administrado. El enrutamiento de la conexión a la base de datos se resuelve dinámicamente en tiempo de ejecución interceptando el contexto del request y reconfigurando el string de conexión de Entity Framework Core mediante el middleware de la aplicación.

## Justificación

1. **Mitigación del "Noisy Neighbor" (Vecino Ruidoso)**: Al estar físicamente separadas, las cargas computacionales pesadas (ej. generación masiva de reportes contables) de un tenant gigantesco no ahogarán los recursos (Locks, CPU, Memory Buffers) de un tenant más pequeño.
2. **Aislamiento Físico y Cumplimiento Legal**: Resulta imposible que, por un defecto en el código (olvido de un `WHERE TenantId = X`), la consulta de un cliente retorne registros de la competencia. Esto brinda una garantía de privacidad absoluta indispensable en entornos empresariales y financieros modernos.
3. **Escalabilidad Elástica**: Permite escalar independientemente el hardware o tier de Azure SQL Database para inquilinos específicos que demanden mayor capacidad sin incurrir en costos para toda la plataforma.
4. **Resiliencia y Recuperación**: Posibilita ejecutar restauraciones de puntos en el tiempo (Point-in-Time Restore) de manera individualizada en caso de corrupción de datos de un cliente, sin afectar el servicio del resto de los 1,499 tenants.

## Consecuencias

- **Gestión de Infraestructura**: Mayor sobrecarga administrativa en el despliegue y mantenimiento de los esquemas, mitigado altamente por la adopción de Infraestructura como Código (Bicep/Terraform) y Migraciones Automatizadas en el pipeline de CI/CD.
- **Costos Base**: Incurre en un costo inicial ligeramente superior debido al aprovisionamiento individual, el cual será optimizado implementando "Elastic Pools" de Azure SQL para compartir los DTUs/vCores a lo largo de las múltiples instancias menos exigentes.
- **Consultas Multi-Tenant**: Las consultas que requieran consolidación de datos entre diferentes inquilinos (ej. analíticas globales) se volverán más complejas, requiriendo mecanismos de replicación hacia un Data Warehouse centralizado.
