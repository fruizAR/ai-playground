using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;

public class ServiceHandler : DelegatingHandler
{
    private readonly Serilog.ILogger _logger = Log.ForContext<ServiceHandler>();
    public ServiceHandler() : base()
    {
    }
    /// <summary>
    /// It's Allows to add general params to http calls, Log All Calls with details events and more
    /// Is executing each time a request, GET, POST, PUT is made.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        sw.Stop();

        var requestBody = "";
        var responseBody = "";
        if (_logger.IsEnabled(LogEventLevel.Warning))
        {
            if (request.Content != null)
                requestBody = await request.Content.ReadAsStringAsync() ?? "";
            if (response.Content != null)
                responseBody = await response.Content.ReadAsStringAsync() ?? "";
        }
        // if password is passed in url it's removed for security reasons
        string uri = request.RequestUri?.AbsoluteUri ?? "";
        if (uri.ToLower().Contains("password"))
            uri = uri.Substring(0, uri.ToLower().IndexOf("password"));

        _logger.ForContext(new PropertyBagEnricher()
                .Add("elapsed", sw.ElapsedMilliseconds.ToString())
                .Add("StatusCode", ((int)response.StatusCode).ToString())
                .Add("RequestMethod", request.Method.ToString())
                .Add("RequestPath", uri)
                .Add("RequestBody", requestBody)
                .Add("ResponseBody", responseBody)
        ).Error("ERROR API");

        return response;
    }
}
public class PropertyBagEnricher : ILogEventEnricher
{
    private readonly Dictionary<string, Tuple<object, bool>> _properties;

    /// <summary>
    /// Creates a new <see cref="PropertyBagEnricher" /> instance.
    /// </summary>
    public PropertyBagEnricher()
    {
        _properties = new Dictionary<string, Tuple<object, bool>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Enriches the <paramref name="logEvent" /> using the values from the property bag.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The factory used to create the property.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (KeyValuePair<string, Tuple<object, bool>> prop in _properties)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(prop.Key, prop.Value.Item1, prop.Value.Item2));
        }
    }

    /// <summary>
    /// Add a property that will be added to all log events enriched by this enricher.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    /// <param name="destructureObject">
    /// Whether to destructure the value. See https://github.com/serilog/serilog/wiki/Structured-Data
    /// </param>
    /// <returns>The enricher instance, for chaining Add operations together.</returns>
    public PropertyBagEnricher Add(string key, object value, bool destructureObject = false)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        if (!_properties.ContainsKey(key)) _properties.Add(key, Tuple.Create(value, destructureObject));

        return this;
    }
}