from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
from loguru import logger
import sys

from app.config import settings
from app.routers import openai_router, status_router, logs_router


# Configurar logger
logger.remove()
logger.add(
    sys.stdout,
    format="<green>{time:YYYY-MM-DD HH:mm:ss}</green> | <level>{level: <8}</level> | <cyan>{name}</cyan>:<cyan>{function}</cyan>:<cyan>{line}</cyan> - <level>{message}</level>",
    level=settings.log_level
)
logger.add(
    "logs/app.log",
    rotation="500 MB",
    retention="10 days",
    level=settings.log_level
)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Gestión del ciclo de vida de la aplicación"""
    # Startup
    logger.info("🚀 Iniciando API de orquestación OpenAI")
    logger.info(f"Modelo configurado: {settings.openai_model}")
    yield
    # Shutdown
    logger.info("👋 Cerrando API de orquestación OpenAI")


# Crear aplicación FastAPI
app = FastAPI(
    title="OpenAI Orchestration API",
    description="Capa de orquestación entre aplicaciones Angular y servicios OpenAI",
    version="1.0.0",
    lifespan=lifespan
)


# Configurar CORS para permitir solicitudes desde Angular
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4200", "http://localhost:4201"],  # Puertos típicos de Angular
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# Incluir routers
app.include_router(openai_router.router, tags=["OpenAI"])
app.include_router(status_router.router, tags=["Status"])
app.include_router(logs_router.router, tags=["Logs"])


@app.get("/")
async def root():
    """Endpoint raíz"""
    return {
        "message": "OpenAI Orchestration API",
        "version": "1.0.0",
        "docs": "/docs"
    }
