from pydantic_settings import BaseSettings
from pathlib import Path


# Obtener el directorio base del proyecto (backend/)
BASE_DIR = Path(__file__).resolve().parent.parent


class Settings(BaseSettings):
    """Configuración de la aplicación"""
    openai_api_key: str
    openai_model: str = "gpt-4-turbo-preview"
    api_host: str = "0.0.0.0"
    api_port: int = 8000
    log_level: str = "INFO"
    
    class Config:
        env_file = str(BASE_DIR / ".env")
        case_sensitive = False


settings = Settings()
