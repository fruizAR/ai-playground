from pydantic import BaseModel, Field
from typing import Optional, List


class AskRequest(BaseModel):
    """Modelo para solicitudes al endpoint /ask"""
    prompt: str = Field(..., description="El prompt a enviar a OpenAI")
    temperature: Optional[float] = Field(0.7, ge=0.0, le=2.0, description="Temperatura del modelo")
    maxtokens: Optional[int] = Field(1000, gt=0, description="Máximo de tokens en la respuesta")
    stream: Optional[bool] = Field(True, description="Si se debe hacer streaming de la respuesta")


class AskResponse(BaseModel):
    """Modelo para respuestas del endpoint /ask"""
    response: str = Field(..., description="Respuesta generada por OpenAI")
    tokensused: Optional[int] = Field(None, description="Tokens utilizados en la solicitud")
    finishReason: Optional[str] = Field(None, description="Razón de finalización: 'stop', 'length', 'content_filter', etc.")


class StatusResponse(BaseModel):
    """Modelo para el estado del servicio"""
    status: str = Field(..., description="Estado del servicio")
    version: str = Field(..., description="Versión de la API")
    openaiconnected: bool = Field(..., description="Si hay conexión con OpenAI")


class LogEntry(BaseModel):
    """Modelo para entradas de log"""
    timestamp: str
    level: str
    message: str
    metadata: Optional[dict] = None
