using System.Security.Cryptography;
using System.Text;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace CharityDonationsApp.Infrastructure.Services;

public class UtilityService(
    IMemoryCache memoryCache) : IUtilityService
{
    #region Cache

    public void SetInMemoryCache<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        => memoryCache.Set(key, value,
            TimeSpan.FromSeconds(Math.Max(absoluteExpirationRelativeToNow.TotalSeconds,
                absoluteExpirationRelativeToNow.TotalSeconds - 60)));

    public void SetInMemoryCache<TItem>(object key, TItem value, MemoryCacheEntryOptions options)
    {
        memoryCache.Set(key, value, options);
    }

    public bool TryGetInMemoryCacheValue<TItem>(string key, out TItem? value)
        => memoryCache.TryGetValue(key, out value);

    public void RemoveInMemoryCache(object key) => memoryCache.Remove(key);

    #endregion

    public string ComputeSha256Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}