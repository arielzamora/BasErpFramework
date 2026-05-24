# ADR 0003: Angular Signals for Real-Time Grids

**Fecha**: 2026-05-23
**Estado**: Aceptado

## Contexto

El entorno del ERP requerirá renderizar pantallas con un altísimo nivel de densidad de información. Las tablas de datos (Datatables genéricos) no solo deben mostrar cientos o miles de registros, sino que también deben ser capaces de procesar ordenamientos locales, búsquedas instantáneas y recibir notificaciones push en tiempo real a través de WebSockets (SignalR) para reflejar cambios ejecutados por otros usuarios (por ejemplo, cambios de stock o actualización de precios) de manera inmediata.
Históricamente, en arquitecturas Angular clásicas basadas en Zone.js, inyectar miles de actualizaciones por segundo a una grilla provocaba cuellos de botella severos de renderizado ("Jank"), debido a que cada actualización desencadenaba un ciclo de verificación de cambios completo a lo largo de todo el árbol de componentes (Change Detection).

## Decisión

Adoptamos el paradigma de reactividad granular introducido en **Angular 19 utilizando Signals**, junto con la estrategia de renderizado **`ChangeDetectionStrategy.OnPush`** a nivel de todos los componentes compartidos (especialmente el `GenericTableComponent`).

## Justificación

1. **Reactividad Granular Extrema**: A diferencia de Zone.js que detecta cambios de forma transversal, *Signals* rastrea dependencias de datos con precisión milimétrica. Si un evento de SignalR actualiza el precio de un producto específico, Angular sabrá exactamente qué nodo exacto del DOM debe ser re-renderizado, ignorando el resto del árbol de la grilla.
2. **Performance Computada**: Las operaciones pesadas como la búsqueda de texto en tiempo real sobre miles de registros locales y los ordenamientos de columnas, se delegan a `computed()`. Los valores computados en Signals almacenan en caché (memoización) sus resultados y solo se recalculan de forma diferida si (y solo si) las dependencias directas (`data`, `searchQuery`, `sortDirection`) se ven alteradas, liberando el hilo principal del navegador.
3. **Escalabilidad del Componente**: Construir un Datatable verdaderamente agnóstico y premium exige un rendimiento que no degrade la experiencia de usuario bajo estrés masivo de datos. Esta arquitectura de reactividad permite alcanzar los ansiados 60 FPS (Frames por Segundo) requeridos por interfaces modernas de alto nivel.
4. **Simplificación del Código**: Reduce enormemente la verbosidad de RxJS (elimina las múltiples llamadas a `.subscribe()`, tuberías `async` complejas y fugas de memoria) a la hora de enlazar la UI local con el estado del componente. 

## Consecuencias

- **Curva de Aprendizaje**: El equipo de desarrollo Frontend debe migrar mentalmente del uso intensivo de BehaviorSubjects/RxJS para el estado UI a la mentalidad imperativa-reactiva de Signals (`set()`, `update()`, `computed()`).
- **Integración con Servicios**: Las capas de transporte y servicio (`product.service.ts`, `signalr.service.ts`) continuarán explotando el modelo de streams con RxJS (ya que los observables son óptimos para eventos asíncronos en red), convirtiéndose en Signals únicamente en el punto final de consumo dentro de los Componentes (`toSignal()`), estableciendo un claro límite entre el transporte (Asíncrono) y el estado (Sincrónico/Reactivo).
