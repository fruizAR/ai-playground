using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Services.Models;
using Services.Services;
using Microsoft.Extensions.Options;

[Route("[controller]")]
[AllowAnonymous]
public class StatusController : Controller
{
    protected readonly AppDBContext db;
    protected IConfiguration Config { get; }
    private readonly IOpenAIService _openAI;

    public StatusController(AppDBContext context, IConfiguration config, IOpenAIService openAI)
    {
        db = context;
        Config = config;
        _openAI = openAI;
    }
    /// <summary>
    /// Dummy API to know if service is UP (legacy)
    /// </summary>
    [HttpGet("info")]
    public IActionResult StatusInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version;
        System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
        DateTime lastModified = fileInfo.LastWriteTime;

        // var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        // var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        var version = Assembly.GetEntryAssembly()?.GetName().Version;

        string response = $"{assembly.GetName()}\nVersion: {assemblyVersion}.\nDate: {lastModified.ToString()}";
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el estado del servicio de orquestación
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado del servicio y conexión con OpenAI</returns>
    [HttpGet]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;
            var version = assemblyVersion?.ToString() ?? "1.0.0";

            var openAIConnected = await _openAI.CheckConnectionAsync(cancellationToken);

            return Ok(new StatusResponse
            {
                Status = "running",
                Version = version,
                OpenAIConnected = openAIConnected
            });
        }
        catch (Exception)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;
            var version = assemblyVersion?.ToString() ?? "1.0.0";

            return Ok(new StatusResponse
            {
                Status = "error",
                Version = version,
                OpenAIConnected = false
            });
        }
    }
}