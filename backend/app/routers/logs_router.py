from fastapi import APIRouter, Query
from typing import List
from app.models.schemas import LogEntry
from loguru import logger
from datetime import datetime
import os
import json


router = APIRouter()

# Lista en memoria para almacenar logs (en producción usar base de datos o sistema de logs)
log_entries: List[LogEntry] = []


def add_log_entry(level: str, message: str, metadata: dict = None):
    """Añade una entrada de log a la lista"""
    entry = LogEntry(
        timestamp=datetime.utcnow().isoformat(),
        level=level,
        message=message,
        metadata=metadata
    )
    log_entries.append(entry)
    # Mantener solo los últimos 1000 logs en memoria
    if len(log_entries) > 1000:
        log_entries.pop(0)


@router.get("/logs", response_model=List[LogEntry])
async def get_logs(
    limit: int = Query(100, ge=1, le=1000, description="Número máximo de logs a retornar"),
    level: str = Query(None, description="Filtrar por nivel de log (INFO, ERROR, WARNING)")
):
    """
    Endpoint para obtener los logs de las interacciones
    
    Args:
        limit: Número máximo de logs a retornar
        level: Filtrar por nivel de log
        
    Returns:
        Lista de entradas de log
    """
    try:
        filtered_logs = log_entries
        
        # Filtrar por nivel si se especifica
        if level:
            filtered_logs = [log for log in filtered_logs if log.level.upper() == level.upper()]
        
        # Retornar los últimos N logs
        return filtered_logs[-limit:]
        
    except Exception as e:
        logger.error(f"Error al obtener logs: {str(e)}")
        return []
