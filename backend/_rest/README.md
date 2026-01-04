# Pruebas de API - OpenAI Orchestration

Este directorio contiene archivos de prueba para la API de orquestación OpenAI.

## 📄 Archivos

- **openai-api.http**: Colección completa de pruebas para todos los endpoints

## 🚀 Cómo usar

### Opción 1: VS Code con REST Client Extension

1. Instalar la extensión [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
2. Abrir el archivo `openai-api.http`
3. Hacer clic en "Send Request" sobre cualquier petición
4. Ver la respuesta en el panel lateral

### Opción 2: Usar curl desde terminal

Para el endpoint `/status`:

```bash
curl http://localhost:8000/status
```

Para el endpoint `/ask` sin streaming:

```bash
curl -X POST http://localhost:8000/ask \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "¿Qué es FastAPI?",
    "temperature": 0.7,
    "max_tokens": 100,
    "stream": false
  }'
```

Para el endpoint `/ask` con streaming:

```bash
curl -X POST http://localhost:8000/ask \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Explica FastAPI",
    "temperature": 0.7,
    "max_tokens": 500,
    "stream": true
  }'
```

Para los logs:

```bash
curl "http://localhost:8000/logs?limit=10"
```

## 📋 Pruebas incluidas

### Endpoints básicos:

1. ✅ GET `/` - Información de la API
2. ✅ GET `/status` - Estado del servicio
3. ✅ GET `/logs` - Logs del sistema

### Endpoint /ask:

4. ✅ POST `/ask` - Solicitud simple sin streaming
5. ✅ POST `/ask` - Solicitud con streaming (SSE)
6. ✅ POST `/ask` - Pregunta técnica
7. ✅ POST `/ask` - Solicitud creativa (temperatura alta)
8. ✅ POST `/ask` - Generación de código
9. ✅ POST `/ask` - Streaming largo

### Validaciones:

10. ❌ Prompt vacío (validación)
11. ❌ Sin campo prompt (validación)
12. ❌ Temperatura fuera de rango (validación)
13. ❌ Max tokens negativo (validación)

## 🔑 Requisitos

- Servidor FastAPI ejecutándose en `http://localhost:8000`
- API Key de OpenAI configurada en el archivo `.env`
- El servidor debe estar iniciado antes de ejecutar las pruebas

## 💡 Notas

- Las pruebas con `stream: true` retornan Server-Sent Events (SSE)
- Las pruebas con `stream: false` retornan JSON estándar
- Los parámetros por defecto son: `temperature=0.7`, `max_tokens=1000`, `stream=true`
- Las validaciones de Pydantic capturan automáticamente errores en los parámetros
