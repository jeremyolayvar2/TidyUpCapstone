using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public class PricingService : IPricingService
    {
        public decimal CalculateAdjustedPrice(int categoryId, int conditionId)
        {
            var baseCap = GetBaseCapForCategory(categoryId);
            var modifier = GetConditionModifier(conditionId);
            return baseCap + (baseCap * modifier);
        }

        public decimal CalculateFinalPriceAfterTax(decimal adjustedPrice)
        {
            decimal taxRate = adjustedPrice switch
            {
                <= 50 => 0.05m,
                <= 100 => 0.10m,
                <= 200 => 0.15m,
                _ => 0.15m
            };

            return adjustedPrice - (adjustedPrice * taxRate);
        }

        private decimal GetBaseCapForCategory(int categoryId)
        {
         
            return categoryId switch
            {
                1 => 50,   // Books & Stationery
                2 => 200,  // Electronics & Gadgets
                3 => 80,   // Toys & Games
                4 => 100,  // Home & Kitchen
                5 => 150,  // Furniture
                6 => 180,  // Appliances
                7 => 70,   // Health & Beauty
                8 => 60,   // Crafts & DIY
                9 => 60,   // School & Office
                10 => 90,  // Sentimental Items
                11 => 75,  // Miscellaneous
                _ => 50
            };
        }

        private decimal GetConditionModifier(int conditionId)
        {
      
            return conditionId switch
            {
                1 => 0.25m,   // Brand New
                2 => 0.15m,   // Like New
                3 => 0.05m,   // Gently Used
                4 => -0.10m,  // Visible Wear
                5 => -0.25m,  // For Repair/Parts
                _ => 0m
            };
        }
    }
}
