using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DjMrkos.Api.Middleware;

/// <summary>
/// The only place exceptions become HTTP responses. Handlers throw domain-meaningful
/// exceptions (NotFoundException, ValidationException, ...); this middleware is what
/// decides those map to 404, 400, 410, 429 — no controller/endpoint does that mapping itself.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (QrWindowClosedException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status410Gone, ex.Message);
        }
        catch (RateLimitExceededException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status429TooManyRequests, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, IDictionary<string, string[]>? errors = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails { Status = statusCode, Title = title };
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
