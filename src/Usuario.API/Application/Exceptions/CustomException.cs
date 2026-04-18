using System.Net;

namespace Application.Exceptions;

public sealed class CustomException : ApplicationException {
	public HttpStatusCode StatusCode { get; }

	public CustomException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
		: base(message) {
		StatusCode = statusCode;
	}
}
