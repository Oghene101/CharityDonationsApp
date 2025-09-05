using System.Net;
using CharityDonationsApp.Application.Common.Contracts;

namespace CharityDonationsApp.Application.Common.Exceptions;

public class ApiException(
    string title,
    string message,
    Error[] errors,
    HttpStatusCode statusCode) : Exception(message)
{
    public string Title => title;

    public Dictionary<string, object> Extensions => new() { ["errors"] = errors };

    public int StatusCode => (int)statusCode;

    public static ApiException NotFound(Error error, string message = "The requested resource was not found.")
        => new("Not Found", message, [error], HttpStatusCode.NotFound);
    
    public static ApiException BadRequest(Error error, string message = "Invalid request parameters.")
        => new("Bad Request", message, [error], HttpStatusCode.BadRequest);
    
    public static ApiException BadRequest(Error[] errors, string message = "Invalid request parameters.")
        => new("Bad Request", message, errors, HttpStatusCode.BadRequest);

    public static ApiException Unauthorized(Error error, string message = "Authentication required.")
        => new("Unauthorized", message, [error], HttpStatusCode.Unauthorized);

    public static ApiException Forbidden(Error error, string message = "Insufficient permissions.")
        => new("Forbidden", message, [error], HttpStatusCode.Forbidden);

    public static ApiException InternalServerError(Error[] errors, string message = "An unexpected error occurred.")
        => new("Internal Server Error", message, errors, HttpStatusCode.InternalServerError);
}