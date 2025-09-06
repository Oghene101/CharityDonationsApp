namespace CharityDonationsApp.Application.Common.Contracts;

public static class User
{
    public record AddBvnRequest(string Bvn);

    public record AddNinRequest(string Nin);

    public record AddAddressRequest(
        string HouseNumber,
        string Landmark,
        string Street,
        string Lga,
        string City,
        string State,
        string Country);
}