namespace CloudRental.Services;
public static class RentalPricing {
    public static readonly int[] AllowedMonths = { 1, 3, 6, 12 };
    public static decimal Total(decimal monthlyPrice, int months) {
        if (monthlyPrice <= 0 || !AllowedMonths.Contains(months)) throw new ArgumentOutOfRangeException(nameof(months));
        return monthlyPrice * months;
    }
}
