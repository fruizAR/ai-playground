from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse
from app.models.schemas import AskRequest, AskResponse
from app.services.openai_service import openai_service
from loguru import logger
import json


router = APIRouter()


@router.post("/ask")
async def ask_openai(request: AskRequest):
    """
    Endpoint para enviar solicitudes a OpenAI
    
    Args:
        request: Solicitud con el prompt y configuración
        
    Returns:
        Respuesta de OpenAI (streaming o no-streaming según configuración)
    """
    try:
        if request.stream:
            # Retornar respuesta en streaming
            async def generate():
                try:
                    async for chunk in openai_service.generate_response(request):
                        # Enviar cada chunk como Server-Sent Event
                        yield f"data: {json.dumps(chunk)}\n\n"
                except Exception as e:
                    logger.error(f"Error en streaming: {str(e)}")
                    yield f"data: {json.dumps({'error': str(e)})}\n\n"
                finally:
                    yield "data: [DONE]\n\n"
            
            return StreamingResponse(
                generate(),
                media_type="text/event-stream",
                headers={
                    "Cache-Control": "no-cache",
                    "Connection": "keep-alive",
                }
            )
        else:
            # Retornar respuesta completa
            result = await openai_service.generate_response_non_stream(request)
            return AskResponse(
                response=result["response"],
                tokensused=result["tokens_used"],
                finishReason=result["finish_reason"]
            )
            
    except Exception as e:
        logger.error(f"Error en /ask: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
