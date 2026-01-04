# Backend - OpenAI Orchestration API

API de orquestación entre aplicaciones Angular y servicios OpenAI usando FastAPI.

## 🚀 Características

- **Endpoint `/ask` (POST)**: Envía solicitudes a OpenAI con soporte de streaming
- **Endpoint `/status` (GET)**: Obtiene el estado del servicio y conexión con OpenAI
- **Endpoint `/logs` (GET)**: Accede a los logs de interacciones
- **Streaming SSE**: Respuestas en tiempo real usando Server-Sent Events
- **CORS configurado**: Listo para conectar con aplicaciones Angular
- **Logging avanzado**: Usando Loguru para mejor trazabilidad

## 📋 Requisitos

- Python 3.8+
- API Key de OpenAI

## 🔧 Instalación

1. **Crear entorno virtual:**

```bash
python3 -m venv venv
source venv/bin/activate  # En Windows: venv\Scripts\activate
```

2. **Instalar dependencias:**

```bash
pip install -r requirements.txt
```

3. **Configurar variables de entorno:**

Copia el archivo `.env.example` a `.env` y configura tu API Key de OpenAI:

```bash
cp .env.example .env
```

Edita `.env` y añade tu API Key:

```
OPENAI_API_KEY=tu_api_key_aqui
OPENAI_MODEL=gpt-4-turbo-preview
API_HOST=0.0.0.0
API_PORT=8000
LOG_LEVEL=INFO
```

## ▶️ Ejecución

**Modo desarrollo (con hot reload):**

```bash
python run.py
```

O usando uvicorn directamente:

```bash
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

**Modo producción:**

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000 --workers 4
```

## 📚 Documentación API

Una vez iniciado el servidor, accede a:

- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

## 🔌 Endpoints

### POST /ask

Envía un prompt a OpenAI y recibe la respuesta.

**Request Body:**

```json
{
  "prompt": "Explica qué es FastAPI",
  "temperature": 0.7,
  "max_tokens": 1000,
  "stream": true
}
```

**Response (streaming):**

- Content-Type: `text/event-stream`
- Formato SSE con chunks de texto

**Response (no-streaming):**

```json
{
  "response": "FastAPI es un framework moderno...",
  "tokens_used": 150
}
```

### GET /status

Obtiene el estado del servicio.

**Response:**

```json
{
  "status": "running",
  "version": "1.0.0",
  "openai_connected": true
}
```

### GET /logs

Obtiene los logs de las interacciones.

**Query Parameters:**

- `limit` (opcional): Número de logs a retornar (default: 100, max: 1000)
- `level` (opcional): Filtrar por nivel (INFO, ERROR, WARNING)

**Response:**

```json
[
  {
    "timestamp": "2026-01-04T12:00:00",
    "level": "INFO",
    "message": "Generando respuesta para prompt...",
    "metadata": null
  }
]
```

## 🏗️ Estructura del Proyecto

```
backend/
├── app/
│   ├── __init__.py
│   ├── main.py              # Aplicación FastAPI principal
│   ├── config.py            # Configuración y settings
│   ├── models/
│   │   ├── __init__.py
│   │   └── schemas.py       # Modelos Pydantic
│   ├── routers/
│   │   ├── __init__.py
│   │   ├── openai_router.py # Endpoint /ask
│   │   ├── status_router.py # Endpoint /status
│   │   └── logs_router.py   # Endpoint /logs
│   └── services/
│       ├── __init__.py
│       └── openai_service.py # Lógica de OpenAI
├── logs/                    # Directorio de logs
├── .env.example            # Ejemplo de variables de entorno
├── .gitignore
├── requirements.txt        # Dependencias
├── run.py                 # Script de inicio
└── README.md
```

## 🔒 Seguridad

- Las API Keys se almacenan en variables de entorno
- CORS configurado para orígenes específicos
- No se exponen claves en logs
- Validación de entrada con Pydantic

## 🧪 Pruebas

**Probar con curl:**

```bash
# Probar /status
curl http://localhost:8000/status

# Probar /ask (no-streaming)
curl -X POST http://localhost:8000/ask \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hola", "stream": false}'

# Probar /ask (streaming)
curl -X POST http://localhost:8000/ask \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hola", "stream": true}'

# Probar /logs
curl http://localhost:8000/logs?limit=10
```

## 📝 Notas

- El streaming usa Server-Sent Events (SSE) para compatibilidad con Angular
- Los logs se almacenan en memoria (para producción, usar base de datos)
- CORS está configurado para `localhost:4200` y `localhost:4201` (puertos típicos de Angular)
- En producción, usar un servidor proxy inverso (nginx) y HTTPS

## 🚀 Próximos Pasos

- Implementar autenticación JWT
- Añadir rate limiting
- Persistir logs en base de datos
- Añadir métricas con Prometheus
- Implementar LangGraph para flujos complejos
