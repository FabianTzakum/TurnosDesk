using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Support.Errors;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(
        IWebHostEnvironment environment,
        ILogger<ApiExceptionFilter> logger
    )
    {
        _environment = environment;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(
            context.Exception,
            "Ocurrió un error no controlado al procesar la solicitud."
        );

        context.Result = context.Exception switch
        {
            DbUpdateException => BuildDatabaseErrorResult(),
            InvalidOperationException => BuildBadRequestResult(context.Exception),
            ArgumentException => BuildBadRequestResult(context.Exception),
            _ => BuildInternalServerErrorResult(context.Exception)
        };

        context.ExceptionHandled = true;
    }

    private static ObjectResult BuildDatabaseErrorResult()
    {
        var response = ApiResponse<object>.Fail(
            "No se pudo completar la operación en la base de datos.",
            new[]
            {
                "Revisa que los datos enviados sean válidos y que no exista un conflicto con registros relacionados."
            }
        );

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status409Conflict
        };
    }

    private static BadRequestObjectResult BuildBadRequestResult(Exception exception)
    {
        var response = ApiResponse<object>.Fail(
            "No se pudo procesar la solicitud.",
            new[] { exception.Message }
        );

        return new BadRequestObjectResult(response);
    }

    private ObjectResult BuildInternalServerErrorResult(Exception exception)
    {
        var errors = _environment.IsDevelopment()
            ? new[] { exception.Message }
            : new[] { "Ocurrió un error inesperado. Intenta nuevamente o contacta al administrador." };

        var response = ApiResponse<object>.Fail(
            "Ocurrió un error interno en el servidor.",
            errors
        );

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
