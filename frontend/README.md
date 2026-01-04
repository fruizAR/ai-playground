# Frontend - OpenAI Chat Assistant

Aplicación Angular para interactuar con el backend de orquestación OpenAI con soporte de streaming en tiempo real.

## 🚀 Características

- ✅ **Interfaz de chat intuitiva** - UI moderna y responsiva
- ✅ **Streaming en tiempo real** - Respuestas usando Server-Sent Events (SSE)
- ✅ **Renderizado Markdown** - Soporte completo para Markdown en respuestas
- ✅ **Modo sin streaming** - Respuestas completas en una sola petición
- ✅ **Configuración ajustable** - Temperature y Max Tokens personalizables
- ✅ **Estado del backend** - Indicador de conexión con el servicio
- ✅ **Manejo de errores** - Gestión robusta de errores y estados de carga
- ✅ **Historial de chat** - Guarda todas las conversaciones durante la sesión

## 📋 Requisitos

- Node.js 18.x o superior
- npm 9.x o superior
- **Backend** - Uno de los siguientes:
  - Python FastAPI en `http://localhost:8000` (por defecto)
  - .NET ASP.NET Core en `https://localhost:7001/api/chat`

## 🔧 Instalación

1. **Instalar dependencias:**

```bash
cd frontend
npm install
```

**Dependencias principales:**

- `@angular/core`: Framework Angular
- `ngx-markdown`: Renderizado de Markdown
- `marked`: Parser de Markdown
- `rxjs`: Programación reactiva

## ▶️ Ejecución

**Modo desarrollo:**

```bash
npm start
# o
ng serve
```

La aplicación estará disponible en `http://localhost:4200`

**Modo producción:**

```bash
npm run build
# Los archivos compilados estarán en dist/openai-chat
```

## 🏗️ Estructura del Proyecto

```
frontend/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   └── chat/
│   │   │       ├── chat.component.ts      # Lógica del componente
│   │   │       ├── chat.component.html    # Template HTML
│   │   │       └── chat.component.css     # Estilos
│   │   ├── services/
│   │   │   └── openai.service.ts          # Servicio HTTP
│   │   ├── models/
│   │   │   └── chat.models.ts             # Interfaces TypeScript
│   │   ├── app.module.ts                  # Módulo principal
│   │   └── app.component.ts               # Componente raíz
│   ├── environments/
│   │   ├── environment.ts                 # Config desarrollo
│   │   └── environment.prod.ts            # Config producción
│   ├── index.html                         # HTML principal
│   ├── main.ts                            # Bootstrap de Angular
│   └── styles.css                         # Estilos globales
├── angular.json                           # Configuración Angular CLI
├── package.json                           # Dependencias npm
└── tsconfig.json                          # Configuración TypeScript
```

## 🎨 Características del Componente Chat

### Configuración

- **Streaming toggle**: Activa/desactiva el modo streaming
- **Temperature**: Control de creatividad (0.0 - 2.0)
- **Max Tokens**: Límite de tokens en la respuesta (100 - 2000)

### Funcionalidades

1. **Envío de mensajes**:

   - Enter para enviar
   - Shift+Enter para nueva línea

2. **Visualización en tiempo real**:

   - Con streaming: los caracteres aparecen a medida que se generan
   - Sin streaming: la respuesta completa aparece cuando está lista

3. **Indicadores visuales**:

   - 🟢 Conectado / 🔴 Desconectado
   - Indicador de streaming activo
   - Spinner de carga para modo sin streaming

4. **Historial**:
   - Botón para limpiar el chat
   - Timestamps en cada mensaje
   - Auto-scroll al último mensaje

## 🔌 Integración con el Backend

El servicio `OpenAIService` maneja todas las comunicaciones:

```typescript
// Streaming
this.openAIService
  .askWithStreaming(request)
  .subscribe((chunk) => console.log(chunk));

// Sin streaming
this.openAIService
  .askWithoutStreaming(request)
  .subscribe((response) => console.log(response));

// Status del backend
this.openAIService.getStatus().subscribe((status) => console.log(status));
```

## 🎯 Endpoints Utilizados

### Backend Python (FastAPI)

- `POST /ask` - Enviar prompts (streaming y no-streaming)
- `GET /status` - Estado del servicio backend
- `GET /logs` - Logs del sistema

### Backend .NET (ASP.NET Core)

- `POST /api/chat/ask` - Enviar prompts (streaming y no-streaming)
- `GET /api/chat/status` - Estado del servicio backend

## 🌍 Cambiar entre Backends

### Usar Backend Python (FastAPI)

Edita `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: "http://localhost:8000", // Python FastAPI
};
```

Ejecuta el backend Python:

```bash
cd backend
python run.py
```

### Usar Backend .NET (ASP.NET Core)

Edita `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: "https://localhost:7001/api/chat", // .NET ASP.NET Core
};
```

Ejecuta el backend .NET:

```bash
cd netcore/services
dotnet run
```

**Nota**: Ambos backends son 100% compatibles con el frontend. El streaming funciona igual en ambos.

## 🎨 Personalización

### Colores

Los colores principales están definidos en `chat.component.css`:

```css
/* Gradiente principal */
linear-gradient(135deg, #667eea 0%, #764ba2 100%)

/* Puedes cambiarlos según tus preferencias */
```

### Estilos

Modifica `src/styles.css` para cambios globales.

## 🧪 Pruebas

```bash
npm test
# Ejecuta las pruebas unitarias con Karma
```

## 📦 Build para Producción

```bash
npm run build
# Genera archivos optimizados en dist/openai-chat
```

Luego puedes servir los archivos con cualquier servidor web:

```bash
# Ejemplo con Python
cd dist/openai-chat
python -m http.server 8080
```

## 🔍 Troubleshooting

### Error de CORS

Si ves errores de CORS, verifica que el backend tenga configurado:

```python
allow_origins=["http://localhost:4200"]
```

### Backend no conecta

1. Verifica que el backend esté corriendo en `http://localhost:8000`
2. Revisa la configuración en `environment.ts`
3. Mira la consola del navegador para más detalles

### Streaming no funciona

1. Verifica que el backend soporte SSE
2. Revisa que el endpoint `/ask` devuelva `Content-Type: text/event-stream`
3. Asegúrate de que el navegador soporte EventSource API

### Markdown no se renderiza

1. Verifica que `ngx-markdown` esté instalado: `npm install`
2. Asegúrate de que `MarkdownModule` esté importado en `app.module.ts`
3. Revisa los estilos de markdown en `chat.component.css`

## 📝 Notas

- El componente usa `FormsModule` para two-way binding con `[(ngModel)]`
- El streaming usa la Fetch API con ReadableStream
- **Markdown rendering** con `ngx-markdown` y `marked` para formateo rico
- Los estilos son responsive y funcionan en móviles
- El proyecto usa Angular 17 standalone components compatible

## 📚 Documentación Adicional

- [MARKDOWN.md](MARKDOWN.md) - Guía completa de soporte Markdown

## 🚀 Próximos Pasos

- [x] Implementar markdown rendering para las respuestas
- [x] Añadir syntax highlighting para código
- [ ] Agregar persistencia de conversaciones (localStorage)
- [ ] Implementar modo oscuro
- [ ] Agregar exportación de conversaciones
- [ ] Implementar autenticación de usuarios
