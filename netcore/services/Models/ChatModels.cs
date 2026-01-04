namespace Services.Models;

/// <summary>
/// Modelo para solicitudes al endpoint /ask
/// </summary>
public class AskRequest
{
    /// <summary>
    /// El prompt a enviar a OpenAI
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Temperatura del modelo (0.0 - 2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Máximo de tokens en la respuesta
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Si se debe hacer streaming de la respuesta
    /// </summary>
    public bool Stream { get; set; } = true;
}

/// <summary>
/// Modelo para respuestas del endpoint /ask (sin streaming)
/// </summary>
public class AskResponse
{
    /// <summary>
    /// Respuesta generada por OpenAI
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Tokens utilizados en la solicitud
    /// </summary>
    public int? TokensUsed { get; set; }

    /// <summary>
    /// Razón por la que terminó la generación: 'stop' (natural), 'length' (maxTokens alcanzado), etc.
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// Modelo para chunks de streaming
/// </summary>
public class StreamChunk
{
    /// <summary>
    /// Contenido del chunk
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Razón de finalización
    /// </summary>
    public string? FinishReason { get; set; }

    /// <summary>
    /// Mensaje de error si lo hay
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Modelo para el estado del servicio
/// </summary>
public class StatusResponse
{
    /// <summary>
    /// Estado del servicio
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Versión de la API
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Si hay conexión con OpenAI
    /// </summary>
    public bool OpenAIConnected { get; set; }
}
