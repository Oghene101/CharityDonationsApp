using Microsoft.Extensions.Caching.Memory;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IUtilityService
{
    void SetInMemoryCache<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow);

    public void SetInMemoryCache<TItem>(object key, TItem value, MemoryCacheEntryOptions options);

    bool TryGetInMemoryCacheValue<TItem>(string key, out TItem? value);
    void RemoveInMemoryCache(object key);
    string ComputeSha256Hash(string input);
}