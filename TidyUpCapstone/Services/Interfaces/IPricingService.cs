namespace TidyUpCapstone.Services.Interfaces
{
    public interface IPricingService
    {
        decimal CalculateAdjustedPrice(int categoryId, int conditionId);
        decimal CalculateFinalPriceAfterTax(decimal adjustedPrice);
    }
}
