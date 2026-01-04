using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;

[Authorize]
public class BaseApiController : Controller
{
    protected readonly AppDBContext db;
    protected IConfiguration? Config { get; }
    public BaseApiController(AppDBContext context, IConfiguration config)
    {
        db = context;
        Config = config;
    }
    public BaseApiController(AppDBContext context)
    {
        db = context;
        Config = null;//TODO: instanciarlo por default
    }
    protected IActionResult HandleException(Exception ex, object? body = null)
    {
        //TODO: Implementar Log
        //TODO: Implementar envio del body
        while (ex.InnerException != null) ex = ex.InnerException;
        return StatusCode(500, ex);
    }

    /// <summary>
    /// Usuario extraido del jwt token
    /// </summary>
    protected JWTUser JWTUser
    {
        get
        {
            //TODO: Falta verificar el JWT si no tiene algun atributo que está como requerido
            if (HttpContext?.User != null)
            {
                var uid = HttpContext.User.Claims.FirstOrDefault(x => x.Type.ToLower() == "userid")?.Value;
                //Resuelve de los claims si en el contexto si tiene el IsAdmin para agregarlo al usuario logueado
                bool isAdmin = false;
                var rolesInUser = HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
                if (rolesInUser != null)
                    isAdmin = rolesInUser.Contains("admin");
                else
                    rolesInUser = new List<string>();

                var tenantId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.ToLower() == "tenantid")?.Value;

                var user = new JWTUser
                {
                    UserId = uid != null ? new Guid(uid!) : Guid.Empty,
                    Email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
                    IsAdmin = isAdmin, //Si tiene rol de administrador ID=1
                    Roles = rolesInUser,
                    TenantId = tenantId != null ? new Guid(tenantId) : null
                };

                return user;
            }
            return new JWTUser();
        }
    }

    protected string ModelErrors(ModelStateDictionary state)
    {
        string errors = "";
        foreach (var entry in state.Values)
            foreach (var error in entry.Errors)
                errors += "\n" + error.ErrorMessage;
        return errors;
    }
}
