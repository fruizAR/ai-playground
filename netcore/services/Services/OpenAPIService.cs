using System.Runtime.CompilerServices;
using GrapeAI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Services.Hubs;
using Models = Services.Models;

namespace Services.Services;

public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; } // opcional para Azure OpenAI o proxy
    public string Model { get; set; } = "gpt-4o-mini"; // por defecto para streaming
}

public interface IOpenAIService
{
    IAsyncEnumerable<string> StreamChatAsync(string threadId, string userMessage, string? assistantId = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GetStreamChatAsync(string threadId, string userMessage, string? assistantId = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Models.StreamChunk> GenerateResponseStreamAsync(string prompt, double temperature, int maxTokens, CancellationToken cancellationToken = default);
    Task<(string response, int? tokensUsed, string? finishReason)> GenerateResponseAsync(string prompt, double temperature, int maxTokens, CancellationToken cancellationToken = default);
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
}

public class OpenAIService : IOpenAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppDBContext _db;
    private readonly IHubContext<ChatHub> _hub;
    private readonly OpenAIOptions _options;

    public OpenAIService(IHttpClientFactory httpClientFactory, AppDBContext db, IHubContext<ChatHub> hub, IOptions<OpenAIOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _db = db;
        _hub = hub;
        _options = options.Value;
    }

    public async IAsyncEnumerable<string> StreamChatAsync(string threadId, string userMessage, string? assistantId = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 1) Persist user message
        var userMsg = new BotConversation
        {
            MessageId = Guid.NewGuid().ToString("N"),
            AssistantId = assistantId,
            ThreadId = threadId,
            RunId = null,
            CreatedAt = DateTime.UtcNow,
            Role = "user",
            Message = userMessage
        };
        _db.BotConversations.Add(userMsg);
        // await _db.SaveChangesAsync(cancellationToken);

        // 2) Call OpenAI Chat Completions SSE
        using var client = _httpClientFactory.CreateClient("openai");
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        var payload = new
        {
            model = _options.Model,
            stream = true,
            messages = new object[]
            {
                new { role = "system", content = "Eres GrapeAI, un asistente útil y preciso. Responde SIEMPRE en Markdown, sin HTML ni postprocesamiento. Devuelve el contenido tal como lo generas, respetando espacios y saltos de línea." },
                new { role = "user", content = userMessage }
            }
        };
        req.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var assistantBuffer = new System.Text.StringBuilder();
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data:")) continue;
            // Evitar recortar: solo quitar un espacio opcional después de 'data:' según el formato SSE
            string data;
            if (line.Length > 5 && line[5] == ' ')
                data = line.Substring(6);
            else
                data = line.Substring(5);
            if (data == "[DONE]") break;

            string? tokenToYield = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(data);
                var root = doc.RootElement;
                var choices = root.TryGetProperty("choices", out var ch) ? ch : default;
                if (choices.ValueKind == System.Text.Json.JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var delta = choices[0].GetProperty("delta");
                    if (delta.TryGetProperty("content", out var contentEl))
                    {
                        // Preservar exactamente el contenido: si es string, úsalo tal cual
                        // Nota: GetString no altera whitespaces, solo retorna la cadena JSON decodificada
                        var token = contentEl.GetString();
                        if (token != null)
                        {
                            assistantBuffer.Append(token);
                            tokenToYield = token;
                            // Emit to SignalR group
                            await _hub.Clients.Group(threadId).SendAsync("token", new { threadId, token }, cancellationToken);
                        }
                    }
                }
            }
            catch
            {
                // skip malformed lines
            }
            if (!string.IsNullOrEmpty(tokenToYield))
            {
                yield return tokenToYield!;
            }
        }

        var fullAssistant = assistantBuffer.ToString();
        // 3) Persist assistant message
        var assistantMsg = new BotConversation
        {
            MessageId = Guid.NewGuid().ToString("N"),
            AssistantId = assistantId,
            ThreadId = threadId,
            RunId = null,
            CreatedAt = DateTime.UtcNow,
            Role = "assistant",
            Message = fullAssistant
        };
        _db.BotConversations.Add(assistantMsg);
        // await _db.SaveChangesAsync(cancellationToken);

        // 4) Notify completion
        await _hub.Clients.Group(threadId).SendAsync("completed", new { threadId }, cancellationToken);
    }


    public async IAsyncEnumerable<string> GetStreamChatAsync(string threadId, string userMessage, string? assistantId = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Nota: este método expone un stream de tokens directamente, sin SignalR
        using var client = _httpClientFactory.CreateClient("openai");
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        var payload = new
        {
            model = _options.Model,
            stream = true,
            messages = new object[]
            {
                new { role = "system", content = "Eres GrapeAI, un asistente útil y preciso. Responde SIEMPRE en Markdown, sin HTML ni postprocesamiento. Devuelve el contenido tal como lo generas, respetando espacios y saltos de línea." },
                new { role = "user", content = userMessage }
            }
        };
        req.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            if (string.IsNullOrEmpty(line)) continue;
            if (!line.StartsWith("data:")) continue;

            string data = (line.Length > 5 && line[5] == ' ')
                ? line.Substring(6)
                : line.Substring(5);
            if (data == "[DONE]") yield break;

            string? tokenToYield = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(data);
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == System.Text.Json.JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var delta = choices[0].GetProperty("delta");
                    if (delta.TryGetProperty("content", out var contentEl))
                    {
                        var token = contentEl.GetString();
                        if (token != null)
                        {
                            tokenToYield = token; // sin recortes ni postprocesamiento
                        }
                    }
                }
            }
            catch
            {
                // ignorar líneas mal formadas
            }

            if (!string.IsNullOrEmpty(tokenToYield))
                yield return tokenToYield!;
        }
    }

    /// <summary>
    /// Genera una respuesta con streaming para el endpoint /ask
    /// </summary>
    public async IAsyncEnumerable<Models.StreamChunk> GenerateResponseStreamAsync(string prompt, double temperature, int maxTokens, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient("openai");
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            model = _options.Model,
            stream = true,
            temperature = temperature,
            max_tokens = maxTokens,
            messages = new object[]
            {
                new { role = "user", content = prompt }
            }
        };

        req.Content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data:")) continue;

            string data = (line.Length > 5 && line[5] == ' ')
                ? line.Substring(6)
                : line.Substring(5);

            if (data == "[DONE]") yield break;

            Models.StreamChunk? chunkToYield = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(data);
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == System.Text.Json.JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    var delta = choice.GetProperty("delta");

                    string? content = null;
                    string? finishReason = null;

                    if (delta.TryGetProperty("content", out var contentEl))
                    {
                        content = contentEl.GetString();
                    }

                    if (choice.TryGetProperty("finish_reason", out var finishReasonEl) &&
                        finishReasonEl.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        finishReason = finishReasonEl.GetString();
                    }

                    if (content != null || finishReason != null)
                    {
                        chunkToYield = new Models.StreamChunk
                        {
                            Content = content,
                            FinishReason = finishReason
                        };
                    }
                }
            }
            catch
            {
                // ignorar líneas mal formadas
            }

            if (chunkToYield != null)
                yield return chunkToYield;
        }
    }

    /// <summary>
    /// Genera una respuesta sin streaming para el endpoint /ask
    /// </summary>
    public async Task<(string response, int? tokensUsed, string? finishReason)> GenerateResponseAsync(string prompt, double temperature, int maxTokens, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient("openai");
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            model = _options.Model,
            stream = false,
            temperature = temperature,
            max_tokens = maxTokens,
            messages = new object[]
            {
                new { role = "user", content = prompt }
            }
        };

        req.Content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        using var resp = await client.SendAsync(req, cancellationToken);
        resp.EnsureSuccessStatusCode();

        var responseContent = await resp.Content.ReadAsStringAsync(cancellationToken);

        using var doc = System.Text.Json.JsonDocument.Parse(responseContent);
        var root = doc.RootElement;

        var choice = root.GetProperty("choices")[0];
        var message = choice.GetProperty("message");
        var content = message.GetProperty("content").GetString() ?? string.Empty;

        string? finishReason = null;
        if (choice.TryGetProperty("finish_reason", out var finishReasonEl) &&
            finishReasonEl.ValueKind != System.Text.Json.JsonValueKind.Null)
        {
            finishReason = finishReasonEl.GetString();
        }

        int? tokensUsed = null;
        if (root.TryGetProperty("usage", out var usage) &&
            usage.TryGetProperty("total_tokens", out var totalTokens))
        {
            tokensUsed = totalTokens.GetInt32();
        }

        return (content, tokensUsed, finishReason);
    }

    /// <summary>
    /// Verifica la conexión con OpenAI
    /// </summary>
    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("openai");
            using var req = new HttpRequestMessage(HttpMethod.Get, "v1/models");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var resp = await client.SendAsync(req, cancellationToken);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
