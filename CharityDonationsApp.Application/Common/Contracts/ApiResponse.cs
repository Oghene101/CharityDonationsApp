using System.Net;
using System.Text.Json.Serialization;

namespace CharityDonationsApp.Application.Common.Contracts;

public class ApiResponse
{
    protected ApiResponse(bool isSuccess, HttpStatusCode statusCode, string message, Error[] errors)
    {
        switch (isSuccess)
        {
            case true when errors.Length != 0:
                throw new InvalidOperationException("cannot be successful with error");
            case false when errors.Length == 0:
                throw new InvalidOperationException("cannot be unsuccessful without error");
        }

        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public HttpStatusCode StatusCode { get; }
    public string Message { get; }
    public Error[] Errors { get; }

    public static ApiResponse Success(string message = "Completed successfully",
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(true, statusCode, message, Error.None);

    public static ApiResponse<TData> Success<TData>(TData data, string message = "Completed successfully",
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(data, true, statusCode, message, Error.None);

    public static ApiResponse Failure(Error error, string message = "Failed to complete successfully",
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new(false, statusCode, message, [error]);

    public static ApiResponse Failure(Error[] errors, string message = "Failed to complete successfully",
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new(false, statusCode, message, errors);

    public static ApiResponse<TData> Failure<TData>(Error error, string message = "Failed to complete successfully",
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(default!, false, statusCode, message, [error]);

    public static ApiResponse<TData> Failure<TData>(Error[] errors, string message = "Failed to complete successfully",
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(default!, false, statusCode, message, errors);

    public static implicit operator ApiResponse(Error error) => Failure(error);
    public static implicit operator ApiResponse(Error[] errors) => Failure(errors);

    public void Deconstruct(out bool isSuccess, out Error[] errors)
    {
        isSuccess = IsSuccess;
        errors = Errors;
    }
}

public class ApiResponse<TData>(TData data, bool isSuccess, HttpStatusCode statusCode, string message, Error[] errors)
    : ApiResponse(isSuccess, statusCode, message, errors)
{
    private readonly TData _data = isSuccess switch
    {
        true when data is null => throw new InvalidOperationException("Successful api response must have data"),
        false when data is not null => throw new InvalidOperationException("Failed api response must not have data"),
        _ => data
    };

    [JsonPropertyOrder(5)]
    public TData Data =>
        IsSuccess ? _data : throw new InvalidOperationException("Cannot access value of a failed result");

    public static implicit operator ApiResponse<TData>(TData data) => Success(data);

    public static implicit operator TData(ApiResponse<TData> response)
    {
        if (response.IsSuccess) return response._data;

        throw new InvalidOperationException("Cannot convert a failed result to a value.");
    }

    public static implicit operator ApiResponse<TData>(Error error) => Failure<TData>(error);

    public static implicit operator ApiResponse<TData>(Error[] errors) => Failure<TData>(errors);

    public void Deconstruct(out bool isSuccess, out TData data, out Error[] errors)
    {
        isSuccess = IsSuccess;
        data = _data;
        errors = Errors;
    }
}