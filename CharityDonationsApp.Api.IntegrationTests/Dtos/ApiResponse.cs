using System.Net;
using CharityDonationsApp.Application.Common.Contracts;

namespace CharityDonationsApp.Api.IntegrationTests.Dtos;

public record ApiResponse(bool IsSuccess, HttpStatusCode StatusCode, string Message, Error[] Errors);

public record ApiResponse<TData>(bool IsSuccess, HttpStatusCode StatusCode, string Message, Error[] Errors, TData Data);