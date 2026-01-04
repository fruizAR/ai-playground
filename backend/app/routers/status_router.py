from fastapi import APIRouter
from app.models.schemas import StatusResponse
from app.services.openai_service import openai_service
from loguru import logger


router = APIRouter()


@router.get("/status", response_model=StatusResponse)
async def get_status():
    """
    Endpoint para obtener el estado del servicio de orquestación
    
    Returns:
        Estado del servicio y conexión con OpenAI
    """
    try:
        openai_connected = await openai_service.check_connection()
        
        return StatusResponse(
            status="running",
            version="1.0.0",
            openaiconnected=openai_connected
        )
    except Exception as e:
        logger.error(f"Error al obtener status: {str(e)}")
        return StatusResponse(
            status="error",
            version="1.0.0",
            openaiconnected=False
        )
