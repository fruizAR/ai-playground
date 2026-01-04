# Backend .NET - OpenAI Orchestration API

API de orquestación entre aplicaciones Angular y servicios OpenAI usando ASP.NET Core.

## 🚀 Características

- **Endpoint `/api/chat/ask` (POST)**: Envía solicitudes a OpenAI con soporte de streaming SSE
- **Endpoint `/api/chat/status` (GET)**: Estado del servicio y conexión con OpenAI
- **Streaming SSE**: Respuestas en tiempo real usando Server-Sent Events
- **CORS configurado**: Listo para conectar con aplicaciones Angular
- **Logging avanzado**: Usando Serilog para trazabilidad

## 📋 Requisitos

- .NET 8.0 SDK o superior
- API Key de OpenAI (configurada en appsettings.json)
- SQL Server o PostgreSQL (opcional, para logs)

## 🔧 Configuración

### 1. Configurar API Key de OpenAI

Edita `appsettings.json` y configura tu API Key:

```json
{
  "OpenAI": {
    "ApiKey": "tu_api_key_aqui",
    "BaseUrl": "https://api.openai.com/",
    "Model": "gpt-4o-mini"
  }
}
```

### 2. Configurar Base de Datos (Opcional)

Si quieres habilitar el logging en base de datos, configura el connection string:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=OpenAIChat;..."
  }
}
```

## ▶️ Ejecución

**Modo desarrollo:**

```bash
cd netcore/services
dotnet run
```

O usando Visual Studio / VS Code con F5.

**Modo producción:**

```bash
dotnet publish -c Release
cd bin/Release/net8.0/publish
dotnet services.dll
```

El servidor estará disponible en:

- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5000`

## 📚 Documentación API

Una vez iniciado el servidor, accede a Swagger:

- **Swagger UI**: https://localhost:7001/docs

## 🔌 Endpoints

### POST /api/chat/ask

Envía un prompt a OpenAI y recibe la respuesta.

**Request Body:**

```json
{
  "prompt": "Explica qué es ASP.NET Core",
  "temperature": 0.7,
  "maxTokens": 1000,
  "stream": true
}
```

**Response (streaming):**

- Content-Type: `text/event-stream`
- Formato SSE con chunks de texto

**Response (no-streaming):**

```json
{
  "response": "ASP.NET Core es un framework moderno...",
  "tokensUsed": 150
}
```

### GET /api/chat/status

Obtiene el estado del servicio.

**Response:**

```json
{
  "status": "running",
  "version": "1.0.0",
  "openAIConnected": true
}
```

## 🏗️ Estructura del Proyecto

```
netcore/services/
├── Controllers/
│   ├── ChatController.cs        # Endpoints /ask y /status
│   └── BaseController.cs        # Controlador base
├── Services/
│   └── OpenAPIService.cs        # Lógica de OpenAI
├── Models/
│   └── ChatModels.cs            # Modelos de request/response
├── _rest/
│   └── chat-api.http            # Pruebas REST
├── Program.cs                   # Configuración de la aplicación
├── appsettings.json             # Configuración
└── services.csproj              # Archivo de proyecto
```

## 🔒 Seguridad

- Las API Keys se almacenan en appsettings.json (NO commitear con keys reales)
- CORS configurado con `SetIsOriginAllowed(origin => true)` para desarrollo
- Los endpoints tienen `[AllowAnonymous]` para facilitar desarrollo
- En producción, configurar CORS específico y autenticación

## 🧪 Pruebas

**Probar con curl:**

```bash
# Probar /status
curl -k https://localhost:7001/api/chat/status

# Probar /ask (no-streaming)
curl -k -X POST https://localhost:7001/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hola", "stream": false}'

# Probar /ask (streaming)
curl -k -X POST https://localhost:7001/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hola", "stream": true}'
```

**Usando archivo .http:**

Abre `_rest/chat-api.http` con la extensión REST Client en VS Code.

## 📝 Integración con Frontend Angular

Para conectar el frontend Angular con este backend .NET:

1. Edita `frontend/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: "https://localhost:7001/api/chat",
};
```

2. Ejecuta el backend .NET:

```bash
cd netcore/services
dotnet run
```

3. Ejecuta el frontend Angular:

```bash
cd frontend
npm start
```

El frontend ahora usará el backend .NET en lugar de Python.

## 🔧 Configuración Adicional

### Cambiar Puerto

Edita `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:7001;http://localhost:5000"
    }
  }
}
```

### Habilitar HTTPS en Desarrollo

```bash
dotnet dev-certs https --trust
```

### Configurar CORS Específico

En `Program.cs`, reemplaza:

```csharp
.SetIsOriginAllowed(origin => true)
```

Por:

```csharp
.WithOrigins("http://localhost:4200", "http://localhost:4201")
```

## 📊 Características Implementadas

- ✅ Streaming con Server-Sent Events (SSE)
- ✅ Respuestas no-streaming completas
- ✅ Validación de parámetros (temperature, maxTokens)
- ✅ Manejo de errores robusto
- ✅ Logging con Serilog
- ✅ CORS configurado
- ✅ Swagger/OpenAPI documentation
- ✅ Compatibilidad 100% con frontend Angular existente

## 🚀 Diferencias con Backend Python

| Característica     | Python (FastAPI) | .NET (ASP.NET Core)       |
| ------------------ | ---------------- | ------------------------- |
| Puerto por defecto | 8000             | 7001 (HTTPS), 5000 (HTTP) |
| Ruta base          | `/`              | `/api/chat/`              |
| Documentación      | `/docs`          | `/docs`                   |
| Streaming          | ✅ SSE           | ✅ SSE                    |
| Performance        | Excelente        | Excelente                 |
| Type Safety        | Pydantic         | C# nativo                 |

## 📝 Notas

- El streaming usa Server-Sent Events (SSE) igual que la versión Python
- CORS está configurado para permitir cualquier origen en desarrollo
- Los logs se almacenan en SQL Server si está configurado
- Compatible con Azure OpenAI cambiando `BaseUrl` en appsettings.json

## 🐛 Troubleshooting

### Error de certificado HTTPS

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Error de CORS

Verifica que CORS esté habilitado en `Program.cs` con `app.UseCors("CorsPolicy")`.

### OpenAI no conecta

1. Verifica la API Key en `appsettings.json`
2. Revisa que `BaseUrl` sea correcta
3. Prueba el endpoint `/api/chat/status` para ver el estado de conexión

## 🚀 Próximos Pasos

- [ ] Implementar rate limiting
- [ ] Añadir autenticación JWT
- [ ] Persistir conversaciones en base de datos
- [ ] Implementar métricas con Application Insights
- [ ] Añadir cache de respuestas
