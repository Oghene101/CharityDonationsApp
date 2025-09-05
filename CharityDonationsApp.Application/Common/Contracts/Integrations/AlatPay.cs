namespace CharityDonationsApp.Application.Contracts.Integrations;

public record AlatPayApiResponse<TData>(
    bool Status,
    string Message,
    TData Data);

#region Card Payment

public record AlatPayInitializeCardRequest(
    string CardNumber,
    string Currency, //NGN
    string BusinessId);

public record AlatPayInitializeCardData(
    string GatewayRecommendation, // PROCEED or DO NOT PROCEED
    string TransactionId,
    string OrderId);

public record AlatPayAuthenticateCardRequest(
    string CardNumber,
    string CardMonth,
    string CardYear,
    string SecurityCode,
    string BusinessId,
    string BusinessName,
    string Amount,
    string Currency, //NGN
    string OrderId,
    string Description,
    string Channel,
    AlatPayCardCustomer Customer,
    string TransactionId);

public record AlatPayCardCustomer(
    string Email,
    string Phone,
    string FirstName,
    string LastName,
    string Metadata);

public record AlatPayAuthenticateCardData(
    string RedirectHtml,
    string GatewayRecommendation,
    string TransactionId,
    string OrderId);

#endregion

#region Ussd

public record AlatPayInitializePhoneNumberRequest(
    string Amount,
    string Currency, //NGN
    AlatPayPhoneNumberCustomer Customer,
    string PhoneNumber,
    string BusinessId);

public record AlatPayPhoneNumberCustomer(
    string Email,
    string Phone,
    string FirstName,
    string LastName);

public record AlatPayInitializePhoneNumberData(
    string PhoneNumber,
    string TransactionId,
    decimal Amount,
    string BusinessId,
    int Status,
    string Currency); //NGN

#endregion