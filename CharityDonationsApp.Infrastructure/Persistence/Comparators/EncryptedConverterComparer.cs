using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CharityDonationsApp.Infrastructure.Persistence.Comparators;

public class EncryptedConverterComparer() : ValueComparer<string>(
    (l, r) => string.Equals(l, r),
    v => v == null ? 0 : v.GetHashCode(),
    v => v)
{
}