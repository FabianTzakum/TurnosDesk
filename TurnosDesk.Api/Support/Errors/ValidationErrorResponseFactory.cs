using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Support.Errors;

public static class ValidationErrorResponseFactory
{
    public static IActionResult CreateResponse(ActionContext context)
    {
        var errors = ExtractErrors(context.ModelState);

        var response = ApiResponse<object>.Fail(
            "La solicitud contiene errores de validación.",
            errors
        );

        return new BadRequestObjectResult(response);
    }

    private static IReadOnlyCollection<string> ExtractErrors(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(item => item.Value?.Errors.Count > 0)
            .SelectMany(item => item.Value!.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "Uno de los campos enviados no tiene un valor válido."
                : error.ErrorMessage)
            .Distinct()
            .ToArray();

        return errors.Length == 0
            ? new[] { "La solicitud contiene datos inválidos." }
            : errors;
    }
}
