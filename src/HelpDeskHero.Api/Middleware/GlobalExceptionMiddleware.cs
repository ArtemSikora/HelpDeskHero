using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHero.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(
                context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled request failure.");

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType =
                "application/problem+json";

            var problem =
                new ProblemDetails
                {
                    Title =
                        "An unexpected error occurred.",

                    Status =
                        context.Response.StatusCode,

                    Detail =
                        "The request could not be completed.",

                    Instance =
                        context.Request.Path
                };

            await context.Response
                .WriteAsJsonAsync(
                    problem);
        }
    }
}
