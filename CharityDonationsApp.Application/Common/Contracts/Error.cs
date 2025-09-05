namespace CharityDonationsApp.Application.Common.Contracts;

public record Error(string Code, string Message)
{
    public static Error[] None => [];
};