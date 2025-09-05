using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace CharityDonationsApp.Infrastructure.Services;

public class UtilityService(
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache memoryCache) : IUtilityService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    # region Claims

    public string GetSignedInUserEmail() => User?.FindFirstValue(ClaimTypes.Email) ??
                                            throw new InvalidOperationException("JWT missing 'email' claim.");

    #endregion

    #region Cache

    public void SetInMemoryCache<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        => memoryCache.Set(key, value,
            TimeSpan.FromSeconds(Math.Max(absoluteExpirationRelativeToNow.TotalSeconds,
                absoluteExpirationRelativeToNow.TotalSeconds - 60)));

    public bool TryGetInMemoryCacheValue<TItem>(string key, out TItem? value)
        => memoryCache.TryGetValue(key, out value);

    public void RemoveInMemoryCache(object key) => memoryCache.Remove(key);

    #endregion

    public static string ComputeSha256Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}