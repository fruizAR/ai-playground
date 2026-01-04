using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ApiLogMiddleware
{
    private readonly RequestDelegate _next;
    public ApiLogMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }
    public async Task Invoke(HttpContext httpContext, IDiagnosticContext diagnosticContext, IConfiguration config)
    {
        // Read and log request body data
        bool saveBodies = config.GetValue<bool>("SaveLogBodies", false);
        if (saveBodies)
        {
            var bodyText = await ReadRequestBody(httpContext.Request) ?? "";
            diagnosticContext.Set("requestbody", bodyText);
        }
        diagnosticContext.Set("requestpath", httpContext.Request.Path);
        diagnosticContext.Set("querystring", httpContext.Request.QueryString.Value ?? "");
        if (httpContext.Request.Headers.TryGetValue("bearer", out StringValues values))
            diagnosticContext.Set("bearer", values.First() ?? "");
        diagnosticContext.Set("host", httpContext.Request.Host);
        diagnosticContext.Set("protocol", httpContext.Request.Protocol);
        diagnosticContext.Set("scheme", httpContext.Request.Scheme);

        var endpoint = httpContext.GetEndpoint();
        if (endpoint is object) // endpoint != null
            diagnosticContext.Set("endpointname", endpoint.DisplayName ?? "");

        // For reading response
        var originalResponseBody = httpContext.Response.Body;
        using var newResponseBody = new MemoryStream();
        httpContext.Response.Body = newResponseBody;

        // measure execution time
        var watch = System.Diagnostics.Stopwatch.StartNew();
        // Continue down the Middleware pipeline, eventually returning to this class        
        await _next(httpContext);

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        diagnosticContext.Set("elapsed", elapsedMs.ToString().Replace(",", "."));

        // Read response after execute call
        if (newResponseBody.CanSeek)
            newResponseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        // if (!httpContext.Request.Path.ToString().EndsWith("swagger.json") && !httpContext.Request.Path.ToString().EndsWith("index.html"))
        diagnosticContext.Set("contenttype", httpContext.Response.ContentType ?? "");
        diagnosticContext.Set("statuscode", httpContext.Response.StatusCode.ToString());
        if (saveBodies)
            diagnosticContext.Set("responsebody", responseBodyText);

        if (newResponseBody.CanSeek)
            newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);
    }
    private static async Task<string> ReadRequestBody(HttpRequest request)
    {
        request.EnableBuffering();

        var body = request.Body;
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        int bytesRead, totalBytesRead = 0;
        while (totalBytesRead < buffer.Length &&
               (bytesRead = await request.Body.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
        {
            totalBytesRead += bytesRead;
        }
        string requestBody = Encoding.UTF8.GetString(buffer);

        if (body.CanSeek)
            body.Seek(0, SeekOrigin.Begin);
        request.Body = body;

        // ofuscate password information
        if (requestBody.Contains("password"))
        {
            var startPassValue = requestBody.IndexOf("password") + 11;
            var endPassValue = requestBody.IndexOf("\"", startPassValue);
            var b1 = requestBody.Substring(0, startPassValue);
            var b2 = requestBody.Substring(endPassValue);
            requestBody = b1 + "******" + b2;
        }

        return $"{requestBody}";
    }
}