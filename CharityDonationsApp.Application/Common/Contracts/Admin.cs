namespace CharityDonationsApp.Application.Common.Contracts;

public static class Admin
{
    public record SendEmailConfirmationRequest(string Email);

    public record FastForwardLockOutEndRequest(string Email);
}