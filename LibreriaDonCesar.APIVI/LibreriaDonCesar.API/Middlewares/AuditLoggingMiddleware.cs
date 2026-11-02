using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LibreriaDonCesar.API.Models;
using LibreriaDonCesar.API.Services;

namespace LibreriaDonCesar.API.Middlewares;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public AuditLoggingMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        var timer = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        context.Request.EnableBuffering();

        string requestBody = string.Empty;

        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        requestBody = SanitizarPayload(requestBody);

        Exception? caughtException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                $"{{\"error\":\"Error interno en el servidor.\",\"traceId\":\"{traceId}\"}}");
        }
        finally
        {
            timer.Stop();

            var entity = ObtenerEntidad(context.Request.Path);
            var userName = ObtenerNombreUsuario(context, requestBody);
            var requestType = ObtenerTipoSolicitud(context.Request.Method);
            var statusCode = context.Response.StatusCode;

            var log = new SystemLog
            {
                EntryId = Guid.NewGuid().ToString(),
                Entity = entity,
                RequestType = requestType,
                Description = ObtenerDescripcion(
                    context.Request.Method,
                    context.Request.Path,
                    entity),
                Status = statusCode >= 200 && statusCode < 400
                    ? "Exitoso"
                    : "Fallido",
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Environment = _env.EnvironmentName,
                Level = caughtException != null
                    ? "Error"
                    : statusCode >= 400
                        ? "Warning"
                        : "Information",
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                UserId = ObtenerUserId(context),
                UserName = userName,
                PerformedBy = userName,
                Request = new RequestDetail
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue
                        ? context.Request.QueryString.Value
                        : null,
                    Headers = ObtenerHeadersSeguros(context.Request.Headers),
                    Payload = string.IsNullOrWhiteSpace(requestBody)
                        ? null
                        : requestBody
                },
                Response = new ResponseDetail
                {
                    StatusCode = statusCode,
                    ElapsedMilliseconds = timer.ElapsedMilliseconds
                }
            };

            if (caughtException != null)
            {
                log.Exception = new ExceptionDetail
                {
                    Message = caughtException.Message,
                    StackTrace = caughtException.StackTrace,
                    Source = caughtException.Source ?? string.Empty
                };
            }

            _ = logService.InsertLogAsync(log);
        }
    }

    private static string ObtenerEntidad(PathString path)
    {
        var segments = path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments == null || segments.Length < 2)
        {
            return "API";
        }

        return segments[1].ToLowerInvariant() switch
        {
            "products" => "Producto",
            "purchases" => "Compra",
            "suppliers" => "Proveedor",
            "users" => "Usuario",
            "inventorymovements" => "Movimiento de inventario",
            "inventories" => "Inventario",
            "sales" => "Venta",
            "invoices" => "Factura",
            "categories" => "Categoría",
            "roles" => "Rol",
            "auth" => "Autenticación",
            _ => segments[1]
        };
    }

    private static string ObtenerTipoSolicitud(string method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return string.Empty;
        }

        return char.ToUpperInvariant(method[0]) +
            method[1..].ToLowerInvariant();
    }

    private static string ObtenerDescripcion(
        string method,
        PathString path,
        string entity)
    {
        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            path.Value?.EndsWith(
                "/login",
                StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Inicio de sesión";
        }

        var action = method.ToUpperInvariant() switch
        {
            "POST" => "Creó un registro",
            "PUT" => "Actualizó un registro",
            "PATCH" => "Actualizó parcialmente un registro",
            "DELETE" => "Eliminó un registro",
            "GET" => "Consultó información",
            _ => "Realizó una solicitud"
        };

        return $"{action} de {entity}";
    }

    private static string? ObtenerUserId(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;
    }

    private static string ObtenerNombreUsuario(
        HttpContext context,
        string requestBody)
    {
        var userName =
            context.User.Identity?.Name
            ?? context.User.FindFirst(ClaimTypes.Name)?.Value
            ?? context.User.FindFirst("name")?.Value
            ?? context.User.FindFirst("preferred_username")?.Value
            ?? context.User.FindFirst("unique_name")?.Value;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            return userName;
        }

        if (context.Request.Path.Value?.EndsWith(
                "/login",
                StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                using var document = JsonDocument.Parse(requestBody);

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (property.Name.Equals(
                            "userName",
                            StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Equals(
                            "username",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return property.Value.GetString() ?? "Anonimo";
                    }
                }
            }
            catch (JsonException)
            {
                // Si el cuerpo no es JSON válido, se usa "Anonimo".
            }
        }

        return "Anonimo";
    }

    private static Dictionary<string, string> ObtenerHeadersSeguros(
        IHeaderDictionary requestHeaders)
    {
        var headers = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var header in requestHeaders)
        {
            var value = header.Value.ToString();

            if (header.Key.Equals(
                    "Authorization",
                    StringComparison.OrdinalIgnoreCase))
            {
                var scheme = value
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                value = string.IsNullOrWhiteSpace(scheme)
                    ? "******"
                    : $"{scheme} ******";
            }
            else if (header.Key.Contains(
                         "cookie",
                         StringComparison.OrdinalIgnoreCase) ||
                     header.Key.Contains(
                         "token",
                         StringComparison.OrdinalIgnoreCase) ||
                     header.Key.Contains(
                         "secret",
                         StringComparison.OrdinalIgnoreCase) ||
                     header.Key.Contains(
                         "password",
                         StringComparison.OrdinalIgnoreCase) ||
                     header.Key.Contains(
                         "api-key",
                         StringComparison.OrdinalIgnoreCase))
            {
                value = "******";
            }

            headers[header.Key] = value;
        }

        return headers;
    }

    private static string SanitizarPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return payload;
        }

        var pattern =
            @"(?i)""(password|contraseña|token|secret|clave)""\s*:\s*""([^""]+)""";

        return Regex.Replace(payload, pattern, "\"$1\": \"******\"");
    }
}
