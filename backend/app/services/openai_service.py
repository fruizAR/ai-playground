import openai
from openai import AsyncOpenAI
from typing import AsyncGenerator
from app.config import settings
from app.models.schemas import AskRequest
from loguru import logger


class OpenAIService:
    """Servicio para interactuar con OpenAI"""
    
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.openai_api_key)
        self.model = settings.openai_model
    
    async def check_connection(self) -> bool:
        """Verifica la conexión con OpenAI"""
        try:
            await self.client.models.list()
            return True
        except Exception as e:
            logger.error(f"Error al conectar con OpenAI: {str(e)}")
            return False
    
    async def generate_response(self, request: AskRequest) -> AsyncGenerator[dict, None]:
        """
        Genera una respuesta usando OpenAI con streaming
        
        Args:
            request: Solicitud con el prompt y configuración
            
        Yields:
            Diccionarios con chunks de texto y finishReason
        """
        try:
            logger.info(f"Generando respuesta para prompt: {request.prompt[:50]}...")
            
            stream = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {"role": "user", "content": request.prompt}
                ],
                temperature=request.temperature,
                max_tokens=request.maxtokens,
                stream=True
            )
            
            async for chunk in stream:
                if chunk.choices[0].delta.content is not None:
                    yield {"content": chunk.choices[0].delta.content}
                
                # Capturar finish_reason cuando esté disponible
                if chunk.choices[0].finish_reason is not None:
                    yield {"finishReason": chunk.choices[0].finish_reason}
                    
        except Exception as e:
            logger.error(f"Error al generar respuesta: {str(e)}")
            raise
    
    async def generate_response_non_stream(self, request: AskRequest) -> dict:
        """
        Genera una respuesta usando OpenAI sin streaming
        
        Args:
            request: Solicitud con el prompt y configuración
            
        Returns:
            Diccionario con la respuesta y metadatos
        """
        try:
            logger.info(f"Generando respuesta (no-stream) para prompt: {request.prompt[:50]}...")
            
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {"role": "user", "content": request.prompt}
                ],
                temperature=request.temperature,
                max_tokens=request.maxtokens,
                stream=False
            )
            
            return {
                "response": response.choices[0].message.content,
                "tokens_used": response.usage.total_tokens,
                "finish_reason": response.choices[0].finish_reason
            }
            
        except Exception as e:
            logger.error(f"Error al generar respuesta: {str(e)}")
            raise


# Instancia global del servicio
openai_service = OpenAIService()
