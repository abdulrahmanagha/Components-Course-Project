
using System;

namespace CampusCafeOrderingSystem
{
 
    public interface IPricingPolicy
    {
        string Name { get; }
        decimal Apply(decimal subtotal);
    }

    public class NoDiscount : IPricingPolicy
    {
        public string Name
        {
            get { return "No Discount"; }
        }

        public decimal Apply(decimal subtotal)
        {
            if (subtotal < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(subtotal),
                    "Subtotal cannot be negative."
                );
            }

            return subtotal;
        }
    }
}
