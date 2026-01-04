using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.Extensions.Options;
using Services.Hubs;
using Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Get Application version to Log
var productVersion = "";
var assembly = Assembly.GetExecutingAssembly();
var assemblyVersion = assembly.GetName().Version;
FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
productVersion = fvi.ProductVersion ?? "product version not found";

// For comments on OpenApi Swagger
var basePath = AppContext.BaseDirectory; //Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;
var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
var xmlCommentsPath = Path.Combine(basePath, fileName);
var usePostgreSQL = Environment.GetEnvironmentVariable("UsePostgreSQL")?.ToLower() == "true";

// Configuration
builder.Configuration.AddEnvironmentVariables(); // Add environment variables
// Write Log with Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.Enrich.FromLogContext();
    lc.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() })
                );
    lc.Enrich.WithMachineName();
    lc.WriteTo.Debug();
    lc.WriteTo.Console();
    lc.Enrich.WithProperty("Version", productVersion);
    lc.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddControllers().AddNewtonsoftJson(x =>
    {
        x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Microservicio APIs",
                    Description = "REST APIs del Microservicio",
                    TermsOfService = new Uri("http://www.DOMINIO.com.ar/"),
                    Contact = new OpenApiContact
                    {
                        Name = "Admin",
                        Email = "mail@DOMINIO.com.ar",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("http://www.DOMINIO.com.ar/"),
                    }
                });
                if (!string.IsNullOrWhiteSpace(xmlCommentsPath))
                    c.IncludeXmlComments(xmlCommentsPath);
            });

// CORS configuration for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy => policy
        .WithOrigins(
            "http://localhost:4200",
            "http://localhost:4201",
            "http://localhost:8089"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Content-Length", "Content-Type")
        .SetIsOriginAllowedToAllowWildcardSubdomains());
});
// SQL Server is default database
if (usePostgreSQL)
    builder.Services.AddDbContextPool<AppDBContext>(c =>
        {
            c.UseNpgsql(builder.Configuration.GetConnectionString("Default"), x => x.UseNetTopologySuite());
            c.UseLowerCaseNamingConvention(); //Pasa las tablas y columnas a minusculas
            // sin esto includes redundates dan error (ej.: GET de role/{id})
            c.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored));
#if DEBUG
            c.EnableSensitiveDataLogging();
#endif
        }, poolSize: 600);
else
    builder.Services.AddDbContextPool<AppDBContext>(c =>
        {
            c.UseSqlServer(builder.Configuration.GetConnectionString("Default"), x => x.UseNetTopologySuite());
            // sin esto includes redundates dan error (ej.: GET de role/{id})
            c.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored));

#if DEBUG
            c.EnableSensitiveDataLogging();
#endif
        }, poolSize: 600);


var key = Encoding.ASCII.GetBytes("DefaultJWT");
var dbBuilder = new DbContextOptionsBuilder<AppDBContext>();
// SQL Server is default database
if (usePostgreSQL)
{
    dbBuilder.UseLowerCaseNamingConvention().UseNpgsql(builder.Configuration.GetConnectionString("Default"), x => x.UseNetTopologySuite());
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}
else
    dbBuilder.UseSqlServer(builder.Configuration.GetConnectionString("Default"), x => x.UseNetTopologySuite());

List<SystemIntegration> systemIntegrations = new List<SystemIntegration>();
// WRN: Si la base no esta esto tira error
using (AppDBContext db = new AppDBContext(dbBuilder.Options))
{
    // db.Database.EnsureCreated(); // remove in production
    // JWT Secret
    key = Encoding.ASCII.GetBytes(db.SystemsParameters.FirstOrDefault(z => z.Code == "JwtSecret")?.Value ?? "Core2020-With-High-Security");
    systemIntegrations = db.SystemsIntegrations.Where(x => x.Active).ToList();
}
builder.Services.AddSingleton<List<SystemIntegration>>(systemIntegrations); // esto es para que se inyecte en los controllers deberiamos pensar lo mismo con parametros de sistema

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddHttpContextAccessor();

// OpenAI Options + HttpClient + SignalR + DI
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient("openai", (sp, c) =>
{
    var opts = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl) ? "https://api.openai.com/" : opts.BaseUrl;
    if (!baseUrl.EndsWith('/')) baseUrl += "/";
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = TimeSpan.FromSeconds(300);
});
builder.Services.AddSignalR();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();


// builder.Services.AddHttpClient<Services.Google.Maps>(c =>
// {
//     // Primero busca en la base si está configurada la URL del host y sino en archivo de configuracion
//     c.BaseAddress = new Uri("https://maps.googleapis.com");
// }); //.AddHttpMessageHandler<ServiceHandler>(); // Enrich Log when calling service;

var app = builder.Build();

// sets microservice default culture
var cultureInfo = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.RouteTemplate = "docs/{documentName}/swagger.json");
    app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "docs";
    c.SwaggerEndpoint("v1/swagger.json", "Documentación APIs");
    c.DocExpansion(DocExpansion.None);
});

}

// app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

// This allows to enrich without middleware but it does not have response data
// app.UseSerilogRequestLogging();
// app.UseMiddleware<ApiLogMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();