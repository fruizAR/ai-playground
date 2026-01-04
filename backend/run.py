#!/usr/bin/env python3
"""
Script de inicio para el servidor de orquestación OpenAI
"""
import uvicorn
from app.config import settings


if __name__ == "__main__":
    uvicorn.run(
        "app.main:app",
        host=settings.api_host,
        port=settings.api_port,
        reload=True,  # Hot reload en desarrollo
        log_level=settings.log_level.lower()
    )
