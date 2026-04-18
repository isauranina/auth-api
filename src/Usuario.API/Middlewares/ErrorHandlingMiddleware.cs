using System.Net;
using System.Text.Json;
using Application.Exceptions;

namespace Usuario.API.Middlewares;

public sealed class ErrorHandlingMiddleware {
	private readonly RequestDelegate _next;
	private readonly ILogger<ErrorHandlingMiddleware> _logger;

	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger) {
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context) {
		try {
			await _next(context);
		} catch (Exception ex) {
			await HandleExceptionAsync(context, ex, _logger);
		}
	}

	private static async Task HandleExceptionAsync(
		HttpContext context,
		Exception exception,
		ILogger<ErrorHandlingMiddleware> logger) {
		logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);

		context.Response.ContentType = "application/json";
		var statusCode = HttpStatusCode.InternalServerError;
		var message = exception.Message;

		if (exception is CustomException customException) {
			statusCode = customException.StatusCode;
			message = customException.Message;
		} else if (exception is KeyNotFoundException or FileNotFoundException) {
			statusCode = HttpStatusCode.NotFound;
			if (string.IsNullOrWhiteSpace(exception.Message)) {
				message = "Recurso no encontrado";
			}
		} else if (exception is ArgumentException or ArgumentNullException or InvalidOperationException) {
			statusCode = HttpStatusCode.BadRequest;
		} else {
			if (context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() != true) {
				message = "Ha ocurrido un error interno del servidor.";
			}
		}

		context.Response.StatusCode = (int)statusCode;

		var result = JsonSerializer.Serialize(new {
			statusCode = (int)statusCode,
			message,
			timestamp = DateTime.UtcNow
		});

		await context.Response.WriteAsync(result);
	}
}
