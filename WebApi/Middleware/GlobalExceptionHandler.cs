using Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace WebApi.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ошибка валидации",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        if (exception is UnauthorizedAccessException unauthorized)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status401Unauthorized,
                "Требуется авторизация",
                unauthorized.Message,
                "https://tools.ietf.org/html/rfc7235#section-3.1");
            return true;
        }

        if (exception is ForbiddenException forbidden)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status403Forbidden,
                "Доступ запрещён",
                forbidden.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.3");
            return true;
        }

        if (exception is NotFoundException notFound)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status404NotFound,
                "Не найдено",
                notFound.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.4");
            return true;
        }

        if (exception is KeyNotFoundException keyNotFound)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status404NotFound,
                "Не найдено",
                keyNotFound.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.4");
            return true;
        }

        if (exception is ConflictException conflict)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status409Conflict,
                "Конфликт",
                conflict.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.8");
            return true;
        }

        if (exception is BadRequestException badRequest)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status400BadRequest,
                "Некорректный запрос",
                badRequest.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1");
            return true;
        }

        if (exception is InvalidOperationException invalidOp)
        {
            var msg = invalidOp.Message;
            if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                await WriteProblemAsync(
                    httpContext,
                    cancellationToken,
                    StatusCodes.Status404NotFound,
                    "Не найдено",
                    msg,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4");
                return true;
            }

            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status400BadRequest,
                "Некорректная операция",
                msg,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1");
            return true;
        }

        if (exception is ArgumentException or ArgumentNullException)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status400BadRequest,
                "Некорректные аргументы",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1");
            return true;
        }

        if (exception is DbUpdateException dbUpdateException)
        {
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ошибка сохранения данных",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = dbUpdateException.InnerException?.Message ?? dbUpdateException.Message
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
            return true;
        }

        if (exception is SmtpException smtpException)
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status503ServiceUnavailable,
                "Сервис отправки почты недоступен",
                smtpException.InnerException?.Message ?? smtpException.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.6.4");
            return true;
        }

        if (exception is TimeoutException timeoutException &&
            timeoutException.Message.Contains("SMTP", StringComparison.OrdinalIgnoreCase))
        {
            await WriteProblemAsync(
                httpContext,
                cancellationToken,
                StatusCodes.Status503ServiceUnavailable,
                "Сервис отправки почты недоступен",
                timeoutException.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.6.4");
            return true;
        }

        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        return false;
    }

    private static async Task WriteProblemAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        int statusCode,
        string title,
        string? detail,
        string type)
    {
        httpContext.Response.StatusCode = statusCode;
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type
        };
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }
}
