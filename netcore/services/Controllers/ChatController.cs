using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GrapeAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Services;
using Services.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;

namespace Services.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : BaseApiController
{
    private readonly IOpenAIService _openAI;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAIOptions _openAIOptions;

    public ChatController(AppDBContext context, IConfiguration config, IOpenAIService openAI, IHttpClientFactory httpClientFactory, IOptions<OpenAIOptions> openAIOptions) : base(context, config)
    {
        _openAI = openAI;
        _httpClientFactory = httpClientFactory;
        _openAIOptions = openAIOptions.Value;
    }

    /// <summary>
    /// Endpoint para enviar solicitudes a OpenAI
    /// </summary>
    /// <param name="request">Solicitud con el prompt y configuración</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta de OpenAI (streaming o no-streaming según configuración)</returns>
    [HttpPost("ask")]
    [AllowAnonymous]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { detail = "El prompt no puede estar vacío" });
            }

            if (request.Temperature < 0 || request.Temperature > 2)
            {
                return BadRequest(new { detail = "La temperatura debe estar entre 0 y 2" });
            }

            if (request.MaxTokens <= 0)
            {
                return BadRequest(new { detail = "MaxTokens debe ser mayor a 0" });
            }

            if (request.Stream)
            {
                // Retornar respuesta en streaming con Server-Sent Events
                Response.Headers["Cache-Control"] = "no-cache";
                Response.Headers["Connection"] = "keep-alive";
                Response.ContentType = "text/event-stream";

                await foreach (var chunk in _openAI.GenerateResponseStreamAsync(
                    request.Prompt,
                    request.Temperature,
                    request.MaxTokens,
                    cancellationToken))
                {
                    var data = JsonSerializer.Serialize(chunk, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    var sseMessage = $"data: {data}\n\n";
                    await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(sseMessage), cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }

                // Enviar mensaje de finalización
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("data: [DONE]\n\n"), cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                return new EmptyResult();
            }
            else
            {
                // Retornar respuesta completa
                var (response, tokensUsed, finishReason) = await _openAI.GenerateResponseAsync(
                    request.Prompt,
                    request.Temperature,
                    request.MaxTokens,
                    cancellationToken);

                return Ok(new AskResponse
                {
                    Response = response,
                    TokensUsed = tokensUsed,
                    FinishReason = finishReason
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el estado del servicio de orquestación
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado del servicio y conexión con OpenAI</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        try
        {
            var openAIConnected = await _openAI.CheckConnectionAsync(cancellationToken);

            return Ok(new StatusResponse
            {
                Status = "running",
                Version = "1.0.0",
                OpenAIConnected = openAIConnected
            });
        }
        catch (Exception)
        {
            return Ok(new StatusResponse
            {
                Status = "error",
                Version = "1.0.0",
                OpenAIConnected = false
            });
        }
    }
}
