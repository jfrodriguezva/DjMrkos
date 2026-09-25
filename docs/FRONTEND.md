# Frontend

React 19 + Vite + TypeScript. Sin framework de CSS — un sistema de diseño propio en `src/styles/tokens.css`, con módulos CSS por componente para estilos que no son de marca (layout, spacing local).

## Estructura

```
frontend/src/
├── styles/
│   ├── tokens.css      # Paleta de marca, tipografía, espaciado — la fuente de verdad visual
│   └── global.css       # Reset + clases utilitarias (.btn, .card, .badge, .field, .container)
├── lib/
│   ├── apiClient.ts      # fetch tipado, maneja la API key de admin y errores como ApiError
│   ├── queryClient.ts    # Configuración de React Query
│   └── signalr.ts        # Fábrica de la conexión al hub de solicitudes de canciones
├── types/api.ts           # Tipos que reflejan uno a uno los DTOs de DjMrkos.Application
├── components/
│   ├── layout/            # Header (menú dinámico), Footer, SiteLayout
│   └── ui/                 # Button, Badge, Equalizer, PageState (loading/error/empty)
└── pages/
    ├── HomePage, ServicesPage, ServiceDetailPage, AboutPage, TestimonialsPage, ContactPage
    ├── QuoteBuilderPage      # /cotizador — el presupuesto tipo carrito, drag & drop
    ├── SongRequestPage      # /evento/:token — la página que abre el QR, fuera del layout del sitio
    └── admin/
        ├── AdminLoginPage
        ├── AdminDashboardPage  # shell con pestañas
        └── panels/              # EventsPanel, LiveQueuePanel, CatalogPanel, TestimonialsPanel, LeadsPanel
```

## Sistema de diseño

Paleta de marca fija (negro / azul metálico / plateado / rojo) definida como tokens en `:root` — es una identidad visual deliberada, no depende de `prefers-color-scheme`. Tipografía: **Rajdhani** (encabezados, condensada, con carácter de equipo de audio/HUD), **Manrope** (texto), **JetBrains Mono** (etiquetas, badges, metadatos — todo lo que se lee como "dato técnico": horas, IDs cortos, estados).

El motivo visual recurrente es el ecualizador (`components/ui/Equalizer.tsx`) — aparece en el hero, en las pantallas de carga y en la página del QR. Nace del propio dominio (un DJ, un ecualizador de audio) en vez de un ícono genérico.

## El menú es de verdad configurable

`Header.tsx` no tiene una lista de servicios hardcodeada — hace `useQuery(['menu'], () => api.get('/menu'))` y renderiza lo que reciba. Agregar un módulo desde el panel admin lo hace aparecer en el header público en la siguiente carga, sin tocar una línea de este proyecto.

## Cotizador tipo carrito

`QuoteBuilderPage.tsx` (`/cotizador`) reutiliza `GET /api/menu` como catálogo — cada categoría con `price` es un artículo que se puede arrastrar (API nativa de HTML5 Drag and Drop: `draggable`, `onDragStart`/`onDrop`) a la zona de presupuesto, o agregar con el botón **+** de la tarjeta. Las dos vías existen a propósito: el drag-and-drop es la interacción vistosa en escritorio, pero el HTML5 DnD nativo no es confiable en touch, así que el botón **+** es el camino que de verdad funciona en el celular de la mayoría de los visitantes.

El campo **invitados esperados** no filtra ni oculta artículos — alimenta una recomendación textual (`lib/currency.ts#audienceTierFor`, una tabla de umbrales fija) sobre cuánto audio e iluminación conviene para ese tamaño de evento. Es una guía, no una automatización: el usuario sigue eligiendo todo a mano.

No existe un backend de "presupuestos" — al enviar, el carrito compone un resumen de texto (artículos, cantidades, total, invitados) y lo manda como `message` de un `Lead` normal (`POST /api/leads`). El panel admin ve la cotización completa en la pestaña **Cotizaciones**, sin una tabla ni endpoint nuevos.

## Tiempo real

`LiveQueuePanel.tsx` (el tablero del DJ) hace una carga inicial por REST (`GET /admin/events/{id}/song-requests`) que solo sirve de semilla — desde ahí, **toda** actualización del tablero viene de los eventos `songRequestCreated` / `songRequestUpdated` que emite `SongRequestHub` por SignalR. La conexión se reconecta sola (`withAutomaticReconnect`) porque el celular del DJ pierde wifi durante el evento más seguido de lo que uno quisiera.

Las solicitudes se muestran en cuatro columnas tipo kanban (Pendientes / En cola / Tocadas / Rechazadas) en vez de una lista plana — el DJ está operando, no leyendo: necesita ver el estado de un vistazo, no procesar texto.

## Autenticación admin

`apiClient.ts` guarda la API key en `sessionStorage` (se pierde al cerrar la pestaña — deliberado para un dispositivo compartido en cabina) y la manda como `X-Api-Key` en cada llamada marcada `admin: true`. `AdminLoginPage` no solo guarda la clave: la **verifica** llamando a un endpoint admin real antes de navegar al dashboard, así una clave incorrecta nunca deja al DJ viendo un panel que fallará en cada acción.

## Supuesto de despliegue

El cliente llama rutas relativas (`/api/...`, `/hubs/...`) — en desarrollo, Vite las reenvía al backend (`vite.config.ts`); en producción se asume que el frontend se sirve **detrás del mismo dominio/reverse proxy** que la API (Nginx, YARP, etc.), sin necesidad de configurar CORS ni una URL base por entorno. Si el frontend y la API van a vivir en dominios distintos, `apiClient.ts` es el único archivo que necesita una URL base configurable.
