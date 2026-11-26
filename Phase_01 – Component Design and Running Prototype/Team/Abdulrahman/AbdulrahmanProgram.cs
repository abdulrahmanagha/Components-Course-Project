using System;
using System.Collections.Generic;

namespace CampusCafeOrderingSystem
{
    /// <summary>
    /// Core order processing component.
    /// Depends only on IProductCatalogue and IPricingPolicy abstractions (Loose Coupling).
    /// </summary>
    public class OrderService
    {
        private const int MinLinesPerOrder = 1;
        private const int MaxLinesPerOrder = 4;
        private const int MinQuantityPerLine = 1;
        private const int MaxQuantityPerLine = 10;

        private readonly IProductCatalogue _catalogue;
        private readonly IPricingPolicy _pricingPolicy;

        /// <summary>
        /// Initializes a new instance of OrderService using Constructor Injection.
        /// </summary>
        public OrderService(IProductCatalogue catalogue, IPricingPolicy pricingPolicy)
        {
            _catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue), "Product catalogue cannot be null.");
            _pricingPolicy = pricingPolicy ?? throw new ArgumentNullException(nameof(pricingPolicy), "Pricing policy cannot be null.");
        }

        /// <summary>
        /// Validates requested lines, calculates totals using catalogue prices,
        /// applies the injected pricing policy, and returns an immutable OrderSummary.
        /// </summary>
        public OrderSummary ProcessOrder(IReadOnlyList<OrderLine> lines)
        {
            ValidateOrderLines(lines);

            var calculatedLines = new List<CalculatedLine>(lines.Count);
            decimal subtotal = 0m;

            foreach (OrderLine line in lines)
            {
                Product product = _catalogue.GetById(line.ProductId);
                if (product == null)
                {
                    throw new ArgumentException(
                        $"Product with ID '{line.ProductId}' was not found in the catalogue.",
                        nameof(lines));
                }

                decimal lineTotal = product.UnitPrice * line.Quantity;
                subtotal += lineTotal;

                calculatedLines.Add(new CalculatedLine(
                    product.Id,
                    product.Name,
                    line.Quantity,
                    product.UnitPrice,
                    lineTotal));
            }

            decimal rawPayable = _pricingPolicy.Apply(subtotal);

            // Post-condition contract guard: validate policy output BEFORE rounding
            if (rawPayable < 0m || rawPayable > subtotal)
            {
                throw new InvalidOperationException(
                    $"Pricing policy '{_pricingPolicy.Name}' returned an invalid payable amount ({rawPayable}). " +
                    $"Expected a value between 0 and {subtotal}.");
            }

            decimal finalTotal = Math.Round(rawPayable, 2, MidpointRounding.AwayFromZero);
            decimal discountAmount = subtotal - finalTotal;

            return new OrderSummary(
                calculatedLines.AsReadOnly(),
                _pricingPolicy.Name,
                subtotal,
                discountAmount,
                finalTotal);
        }

        private static void ValidateOrderLines(IReadOnlyList<OrderLine> lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines), "Order lines collection cannot be null.");

            if (lines.Count < MinLinesPerOrder)
                throw new ArgumentException("Order must contain at least one product line.", nameof(lines));

            if (lines.Count > MaxLinesPerOrder)
                throw new ArgumentException($"Order cannot exceed {MaxLinesPerOrder} product lines.", nameof(lines));

            var seenProductIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (OrderLine line in lines)
            {
                if (line == null)
                    throw new ArgumentException("Order cannot contain a null order line.", nameof(lines));

                if (string.IsNullOrWhiteSpace(line.ProductId))
                    throw new ArgumentException("Product ID cannot be null, empty, or whitespace.", nameof(lines));

                if (line.Quantity < MinQuantityPerLine || line.Quantity > MaxQuantityPerLine)
                {
                    throw new ArgumentException(
                        $"Quantity for product '{line.ProductId}' must be between {MinQuantityPerLine} and {MaxQuantityPerLine}.",
                        nameof(lines));
                }

                if (!seenProductIds.Add(line.ProductId))
                {
                    throw new ArgumentException(
                        $"Duplicate product ID '{line.ProductId}' is not permitted within the same order.",
                        nameof(lines));
                }
            }
        }
    }

    /// <summary>
    /// Application Entry Point and Composition Root.
    /// Instantiates concrete components, injects dependencies, runs the demo scenario and automated tests.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Campus Cafe Ordering System (Checkpoint 1 Prototype) ===\n");

            // 1. Composition Root: instantiate concrete components and inject via interfaces
            IProductCatalogue catalogue = new InMemoryCatalogue();
            IPricingPolicy noDiscountPolicy = new NoDiscount();
            IReceiptFormatter detailedFormatter = new DetailedReceiptFormatter();

            var orderService = new OrderService(catalogue, noDiscountPolicy);

            // 2. Run Worked Acceptance Example (2x P02 Sandwich, 1x P01 Coffee, No Discount)
            Console.WriteLine("--- Demonstration: Worked Example (No Discount) ---");
            try
            {
                var workedExampleOrder = new List<OrderLine>
                {
                    new OrderLine("P02", 2),
                    new OrderLine("P01", 1)
                };

                OrderSummary summary = orderService.ProcessOrder(workedExampleOrder);
                string receipt = detailedFormatter.Format(summary);
                Console.WriteLine(receipt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Order processing failed: {ex.Message}");
            }

            Console.WriteLine("\n--- Automated Verification Tests (Checkpoint 1) ---");
            TestRunner.RunAllTests();
        }
    }
}