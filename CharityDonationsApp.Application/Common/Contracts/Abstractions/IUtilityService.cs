namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IUtilityService
{
    string GetSignedInUserEmail();
    void SetInMemoryCache<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
    bool TryGetInMemoryCacheValue<TItem>(string key, out TItem? value);
}