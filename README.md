# AI Playground - OpenAI Chat Orchestration

Proyecto completo de orquestación entre aplicaciones Angular y servicios OpenAI, con implementaciones en **Python (FastAPI)** y **.NET (ASP.NET Core)**.

## 🏗️ Arquitectura del Proyecto

```
ai-playground/
├── backend/              # Backend Python con FastAPI
│   ├── app/
│   │   ├── routers/     # Endpoints REST
│   │   ├── services/    # Lógica de negocio
│   │   └── models/      # Modelos Pydantic
│   └── _rest/           # Pruebas REST
│
├── netcore/             # Backend .NET con ASP.NET Core
│   └── services/
│       ├── Controllers/ # Endpoints REST
│       ├── Services/    # Lógica de negocio
│       ├── Models/      # Modelos C#
│       └── _rest/       # Pruebas REST
│
└── frontend/            # Frontend Angular
    └── src/
        ├── app/
        │   ├── components/  # Componentes UI
        │   └── services/    # Servicios HTTP
        └── environments/    # Configuración
```

## 🚀 Características

### Backend (Python & .NET)

- ✅ **Endpoint `/ask`** - Enviar prompts a OpenAI con soporte de streaming
- ✅ **Endpoint `/status`** - Estado del servicio y conexión con OpenAI
- ✅ **Streaming SSE** - Server-Sent Events para respuestas en tiempo real
- ✅ **CORS configurado** - Listo para conectar con Angular
- ✅ **Logging avanzado** - Serilog (.NET) / Loguru (Python)
- ✅ **Validación de datos** - Pydantic (Python) / C# nativo (.NET)

### Frontend (Angular)

- ✅ **Interfaz de chat moderna** - UI intuitiva y responsiva
- ✅ **Streaming en tiempo real** - Muestra la respuesta mientras se genera
- ✅ **Modo sin streaming** - Respuestas completas
- ✅ **Configuración ajustable** - Temperature y Max Tokens
- ✅ **Indicador de estado** - Conexión con backend en tiempo real
- ✅ **Compatible con ambos backends** - Python y .NET

## 🔧 Instalación Rápida

### 1. Backend Python (FastAPI)

```bash
cd backend
python3 -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt

# Configurar API Key
cp .env.example .env
# Editar .env y añadir OPENAI_API_KEY

# Ejecutar
python run.py
```

El backend estará en `http://localhost:8000`

### 2. Backend .NET (ASP.NET Core)

```bash
cd netcore/services

# Configurar API Key en appsettings.json
# Editar la sección "OpenAI" -> "ApiKey"

# Ejecutar
dotnet run
```

El backend estará en `https://localhost:7001`

### 3. Frontend Angular

```bash
cd frontend
npm install

# Configurar backend en src/environments/environment.ts
# apiUrl: 'http://localhost:8000' para Python
# apiUrl: 'https://localhost:7001/api/chat' para .NET

# Ejecutar
npm start
```

El frontend estará en `http://localhost:4200`

## 📚 Documentación

- **[Backend Python (FastAPI)](backend/README.md)** - Documentación completa del backend Python
- **[Backend .NET (ASP.NET Core)](netcore/services/README.md)** - Documentación completa del backend .NET
- **[Frontend Angular](frontend/README.md)** - Documentación completa del frontend

## 🔌 Endpoints Disponibles

### Python (FastAPI)

| Método | Endpoint  | Descripción             |
| ------ | --------- | ----------------------- |
| POST   | `/ask`    | Enviar prompts a OpenAI |
| GET    | `/status` | Estado del servicio     |
| GET    | `/logs`   | Logs del sistema        |

### .NET (ASP.NET Core)

| Método | Endpoint           | Descripción             |
| ------ | ------------------ | ----------------------- |
| POST   | `/api/chat/ask`    | Enviar prompts a OpenAI |
| GET    | `/api/chat/status` | Estado del servicio     |

## 🧪 Pruebas

### Con VS Code REST Client

Abre los archivos `.http` en VS Code con la extensión REST Client:

- **Python**: `backend/_rest/openai-api.http`
- **.NET**: `netcore/services/_rest/chat-api.http`

### Con curl

**Python:**

```bash
curl -X POST http://localhost:8000/ask \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hola", "stream": false}'
```

**.NET:**

```bash
curl -k -X POST https://localhost:7001/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hola", "stream": false}'
```

## 🎯 Cambiar entre Backends

El frontend Angular es compatible con ambos backends. Solo necesitas cambiar la URL en `frontend/src/environments/environment.ts`:

**Para usar Python:**

```typescript
apiUrl: "http://localhost:8000";
```

**Para usar .NET:**

```typescript
apiUrl: "https://localhost:7001/api/chat";
```

## 🔑 Configuración de API Keys

### Python (FastAPI)

Edita `backend/.env`:

```bash
OPENAI_API_KEY=tu_api_key_aqui
OPENAI_MODEL=gpt-4-turbo-preview
```

### .NET (ASP.NET Core)

Edita `netcore/services/appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "tu_api_key_aqui",
    "Model": "gpt-4o-mini"
  }
}
```

## 📊 Comparación de Backends

| Característica | Python (FastAPI)  | .NET (ASP.NET Core) |
| -------------- | ----------------- | ------------------- |
| Lenguaje       | Python 3.8+       | C# / .NET 8.0+      |
| Framework      | FastAPI           | ASP.NET Core        |
| Puerto         | 8000              | 7001 (HTTPS)        |
| Documentación  | `/docs` (Swagger) | `/docs` (Swagger)   |
| Streaming SSE  | ✅                | ✅                  |
| Performance    | Excelente         | Excelente           |
| Tipado         | Pydantic          | C# nativo           |
| Logging        | Loguru            | Serilog             |

## 🛠️ Stack Tecnológico

### Backend Python

- FastAPI
- Uvicorn
- OpenAI Python SDK
- Pydantic
- Loguru

### Backend .NET

- ASP.NET Core 8.0
- OpenAI .NET (HttpClient)
- Serilog
- Entity Framework Core

### Frontend

- Angular 17
- RxJS
- TypeScript
- Server-Sent Events (EventSource)

## 🚀 Características Avanzadas

- **Streaming en Tiempo Real**: Ambos backends soportan Server-Sent Events
- **Validación de Datos**: Automática con Pydantic (Python) y Data Annotations (.NET)
- **CORS Configurado**: Listo para desarrollo y producción
- **Manejo de Errores**: Robusto en ambos backends
- **Logging Estructurado**: Para debugging y monitoreo
- **Type Safety**: En todos los niveles (TypeScript + Pydantic/C#)

## 📝 Notas

- Ambos backends son 100% compatibles con el frontend Angular
- El streaming funciona de manera idéntica en Python y .NET
- Los modelos de datos son equivalentes en ambas implementaciones
- CORS está configurado para permitir conexiones desde localhost:4200

## 🐛 Troubleshooting

### Backend Python no inicia

- Verifica que tengas Python 3.8+
- Asegúrate de activar el entorno virtual
- Revisa que todas las dependencias estén instaladas

### Backend .NET no inicia

- Verifica que tengas .NET 8.0 SDK
- Ejecuta `dotnet dev-certs https --trust` para certificados HTTPS
- Revisa que el puerto 7001 no esté en uso

### Frontend no conecta

- Verifica que el backend esté ejecutándose
- Revisa la URL en `environment.ts`
- Verifica la configuración de CORS en el backend
- Mira la consola del navegador para errores

## 🎓 Recursos Adicionales

- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Angular Documentation](https://angular.io/docs)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)

## 📄 Licencia

Este proyecto es de código abierto para fines educativos.

---

## Jugando con LlaMA

### Instalación

https://www.llama.com/docs/llama-everywhere/running-meta-llama-on-mac/
