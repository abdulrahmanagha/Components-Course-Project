using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CampusCafeOrderingSystem
{
    // =========================================================================
    // Ehab - Product Catalogue Contract, Implementation & Data Models
    // =========================================================================

    public interface IProductCatalogue
    {
        Product GetById(string productId);
    }

    public class Product
    {
        public string Id { get; }
        public string Name { get; }
        public decimal UnitPrice { get; }

        public Product(string id, string name, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Product ID cannot be null or whitespace.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be null or whitespace.", nameof(name));

            if (unitPrice < 0m)
                throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

            Id = id;
            Name = name;
            UnitPrice = unitPrice;
        }
    }

    public class OrderLine
    {
        public string ProductId { get; }
        public int Quantity { get; }

        // Kept as a pure DTO without range validation so OrderService enforces business rules
        public OrderLine(string productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }

    public class CalculatedLine
    {
        public string ProductId { get; }
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }
        public decimal LineTotal { get; }

        public CalculatedLine(string productId, string productName, int quantity, decimal unitPrice, decimal lineTotal)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            LineTotal = lineTotal;
        }
    }

    public class OrderSummary
    {
        public IReadOnlyList<CalculatedLine> Lines { get; }
        public string PolicyName { get; }
        public decimal Subtotal { get; }
        public decimal DiscountAmount { get; }
        public decimal FinalTotal { get; }

        public OrderSummary(
            IReadOnlyList<CalculatedLine> lines,
            string policyName,
            decimal subtotal,
            decimal discountAmount,
            decimal finalTotal)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines), "Calculated lines cannot be null.");

            Lines = new List<CalculatedLine>(lines).AsReadOnly();
            PolicyName = policyName;
            Subtotal = subtotal;
            DiscountAmount = discountAmount;
            FinalTotal = finalTotal;
        }
    }

    public class InMemoryCatalogue : IProductCatalogue
    {
        private readonly Dictionary<string, Product> _products;

        public InMemoryCatalogue()
        {
            // Case-sensitive lookup as required by the specification (e.g., "P01" != "p01")
            _products = new Dictionary<string, Product>(StringComparer.Ordinal)
            {
                { "P01", new Product("P01", "Coffee", 2.50m) },
                { "P02", new Product("P02", "Sandwich", 5.00m) },
                { "P03", new Product("P03", "Juice", 3.00m) },
                { "P04", new Product("P04", "Cookie", 1.25m) }
            };
        }

        public Product GetById(string productId)
        {
            if (productId == null)
                throw new ArgumentNullException(nameof(productId), "Product ID cannot be null.");

            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product ID cannot be blank or whitespace.", nameof(productId));

            if (!_products.TryGetValue(productId, out Product product))
                throw new ArgumentException($"Unknown product ID '{productId}'.", nameof(productId));

            return product;
        }
    }

    // =========================================================================
    // Omar - Pricing Policy Contract & Checkpoint 1 Implementation
    // =========================================================================

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
            if (subtotal < 0m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(subtotal),
                    "Subtotal cannot be negative.");
            }

            return subtotal;
        }
    }

    // =========================================================================
    // Saud - Receipt Formatter & Automated Verification Suite
    // =========================================================================

    public interface IReceiptFormatter
    {
        string Format(OrderSummary summary);
    }

    public class DetailedReceiptFormatter : IReceiptFormatter
    {
        public string Format(OrderSummary summary)
        {
            if (summary == null)
                throw new ArgumentNullException(nameof(summary), "OrderSummary cannot be null.");

            var sb = new StringBuilder();
            sb.AppendLine("==========================================");
            sb.AppendLine("           CAMPUS CAFE RECEIPT");
            sb.AppendLine("==========================================");

            if (summary.Lines != null)
            {
                foreach (var line in summary.Lines)
                {
                    string unitPrice = line.UnitPrice.ToString("F2", CultureInfo.InvariantCulture);
                    string lineTotal = line.LineTotal.ToString("F2", CultureInfo.InvariantCulture);
                    sb.AppendLine($"{line.ProductName} ({line.ProductId}) x{line.Quantity} @ {unitPrice} = {lineTotal}");
                }
            }

            sb.AppendLine("------------------------------------------");
            sb.AppendLine($"Subtotal:       {summary.Subtotal.ToString("F2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Pricing Policy: {summary.PolicyName}");
            sb.AppendLine($"Discount:       {summary.DiscountAmount.ToString("F2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Final Total:    {summary.FinalTotal.ToString("F2", CultureInfo.InvariantCulture)}");
            sb.AppendLine("==========================================");

            return sb.ToString();
        }
    }

    public static class TestRunner
    {
        public static void RunAllTests()
        {
            int passed = 0;
            int failed = 0;

            Console.WriteLine("--- Automated Verification Tests (Checkpoint 1) ---");

            if (RunT01_ValidOrderNoDiscount()) passed++; else failed++;
            if (RunT04_EmptyOrder_ThrowsArgumentException()) passed++; else failed++;
            if (RunT06_ZeroQuantity_ThrowsArgumentException()) passed++; else failed++;

            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine($"Test Summary: {passed} Passed, {failed} Failed (Total: {passed + failed})");
        }

        private static bool RunT01_ValidOrderNoDiscount()
        {
            try
            {
                IProductCatalogue catalogue = new InMemoryCatalogue();
                IPricingPolicy policy = new NoDiscount();
                var service = new OrderService(catalogue, policy);

                var lines = new List<OrderLine>
                {
                    new OrderLine("P02", 2),
                    new OrderLine("P01", 1)
                };

                OrderSummary summary = service.ProcessOrder(lines);

                bool isMatch = summary.Lines.Count == 2 &&
                               summary.Subtotal == 12.50m &&
                               summary.DiscountAmount == 0.00m &&
                               summary.FinalTotal == 12.50m &&
                               summary.PolicyName == policy.Name;

                Console.WriteLine($"T01 (ValidOrderNoDiscount): {(isMatch ? "PASS" : "FAIL")}");
                return isMatch;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"T01 (ValidOrderNoDiscount): FAIL - {ex.Message}");
                return false;
            }
        }

        private static bool RunT04_EmptyOrder_ThrowsArgumentException()
        {
            try
            {
                IProductCatalogue catalogue = new InMemoryCatalogue();
                IPricingPolicy policy = new NoDiscount();
                var service = new OrderService(catalogue, policy);

                var emptyLines = new List<OrderLine>();
                service.ProcessOrder(emptyLines);

                Console.WriteLine("T04 (EmptyOrder_ThrowsArgumentException): FAIL - Expected ArgumentException was not thrown.");
                return false;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("T04 (EmptyOrder_ThrowsArgumentException): PASS");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"T04 (EmptyOrder_ThrowsArgumentException): FAIL - Threw {ex.GetType().Name} instead of ArgumentException.");
                return false;
            }
        }

        private static bool RunT06_ZeroQuantity_ThrowsArgumentException()
        {
            try
            {
                IProductCatalogue catalogue = new InMemoryCatalogue();
                IPricingPolicy policy = new NoDiscount();
                var service = new OrderService(catalogue, policy);

                var invalidLines = new List<OrderLine>
                {
                    new OrderLine("P01", 0)
                };

                service.ProcessOrder(invalidLines);

                Console.WriteLine("T06 (ZeroQuantity_ThrowsArgumentException): FAIL - Expected ArgumentException was not thrown.");
                return false;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("T06 (ZeroQuantity_ThrowsArgumentException): PASS");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"T06 (ZeroQuantity_ThrowsArgumentException): FAIL - Threw {ex.GetType().Name} instead of ArgumentException.");
                return false;
            }
        }
    }

    // =========================================================================
    // Abdulrahman - Core OrderService & Application Composition Root (Main)
    // =========================================================================

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
            Console.WriteLine("______________________________________________________________________\n");
            Console.WriteLine("\t  Campus Cafe Ordering System (Checkpoint 1 Prototype) ");
            Console.WriteLine("______________________________________________________________________\n");

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

            Console.WriteLine();

            // 3. Execute automated test suite (T01, T04, T06)
            TestRunner.RunAllTests();
        }
    }
}