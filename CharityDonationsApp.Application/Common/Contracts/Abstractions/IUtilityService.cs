namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IUtilityService
{
    void SetInMemoryCache<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
    bool TryGetInMemoryCacheValue<TItem>(string key, out TItem? value);
    string ComputeSha256Hash(string input);
}