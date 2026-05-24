# ADR 0002: Application Facade Pattern

**Fecha**: 2026-05-23
**Estado**: Aceptado

## Contexto

El ecosistema del Backend (.NET 9 + Aspire) está construido siguiendo los principios de CQRS (Command Query Responsibility Segregation) a través de la librería MediatR. Esta separación otorga un núcleo de dominio limpio y casos de uso altamente especializados (Commands y Queries). 
Sin embargo, los requisitos del sistema dictan flujos de negocio modernos complejos. Por ejemplo, al crear o actualizar un producto, no solo se debe escribir en la base de datos mediante un Command, sino que simultáneamente se requiere disparar eventos en tiempo real a los clientes conectados a través de WebSockets (SignalR) para mantener reactivas sus interfaces.
Hacer que los Handlers de CQRS inyecten y dependan directamente de los `HubContext` de SignalR generaría un fuerte acoplamiento del Core de negocio con detalles específicos de la infraestructura de presentación.

## Decisión

Se introduce un **Application Facade Pattern** (`ProductFacade` y similares) que actuará como una capa superior integradora ubicada entre los Controladores (Endpoints HTTP/API) y el núcleo CQRS / Infraestructura.

## Justificación

1. **Desacoplamiento del Core**: Los Command Handlers y Query Handlers se mantienen puros, dedicándose exclusivamente a validar y alterar el estado del dominio. No "saben" que existe SignalR, lo que facilita enormemente su prueba unitaria.
2. **Coordinación de Transacciones y Eventos**: La `ProductFacade` se encarga del flujo orquestado: 
   - Ejecuta el Command de negocio vía MediatR.
   - Si es exitoso, invoca al servicio de SignalR para despachar el evento de notificación (`OnProductCreated`, `OnProductUpdated`) a los clientes asociados a ese Tenant.
3. **Simplicidad en la Capa de Exposición**: Los Controladores de la API se vuelven extremadamente delgados, relegando la lógica de coordinación a la Fachada y limitándose al enrutamiento HTTP y las respuestas de códigos de estado.
4. **Reutilización**: Si en el futuro se incorpora un Worker de fondo (Background Service) o un disparador mediante colas de mensajería (RabbitMQ / Azure Service Bus) que necesite realizar el mismo flujo, simplemente inyectará y consumirá el `ProductFacade`.

## Consecuencias

- **Nueva Capa en la Arquitectura**: Incrementa ligeramente la abstracción del código, requiriendo que los nuevos desarrolladores entiendan el rol de esta capa mediadora para no saltársela accidentalmente inyectando Handlers de forma directa en los Controladores.
- **Claridad Organizacional**: Se logra una clara separación de responsabilidades: *MediatR* ejecuta la acción atómica de dominio, y *Facade* orquesta los side-effects o ramificaciones técnicas requeridas por la experiencia de usuario en tiempo real.
