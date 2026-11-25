public interface IReceiptFormatter
{
    string Format(OrderSummary summary);
}

// --- Implementation (Checkpoint 1) ---
public class DetailedReceiptFormatter : IReceiptFormatter
{
    public string Format(OrderSummary summary)
    {
        if (summary == null)
            throw new ArgumentNullException(nameof(summary), "OrderSummary cannot be null.");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("==========================================");
        sb.AppendLine("           CAMPUS CAFÉ RECEIPT");
        sb.AppendLine("==========================================");

        if (summary.Lines != null)
        {
            foreach (var line in summary.Lines)
            {
                sb.AppendLine($"{line.ProductName} ({line.ProductId}) x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2}");
            }
        }

        sb.AppendLine("------------------------------------------");
        sb.AppendLine($"Subtotal:       {summary.Subtotal:F2}");
        sb.AppendLine($"Pricing Policy: {summary.PolicyName}");
        sb.AppendLine($"Discount:      -{summary.DiscountAmount:F2}");
        sb.AppendLine($"Final Total:    {summary.FinalTotal:F2}");
        sb.AppendLine("==========================================");

        return sb.ToString();
    }
}

// --- Test Runner Helper ---
public static class TestRunner
{
    public static void RunAllTests()
    {
        Console.WriteLine("\n================ AUTOMATED TESTS ================");
        RunT01_ValidOrderNoDiscount();
        RunT04_EmptyOrder_ThrowsArgumentException();
        RunT06_ZeroQuantity_ThrowsArgumentException();
        Console.WriteLine("=================================================");
    }

    private static void RunT01_ValidOrderNoDiscount()
    {
        try
        {
            IProductCatalogue catalogue = new InMemoryCatalogue();
            IPricingPolicy policy = new NoDiscount();
            IReceiptFormatter formatter = new DetailedReceiptFormatter();
            var service = new OrderService(catalogue, policy, formatter);

            var lines = new List<OrderLine>
            {
                new OrderLine { ProductId = "P01", Quantity = 2 },
                new OrderLine { ProductId = "P02", Quantity = 1 }
            };

            OrderSummary summary = service.CreateOrder(lines);

            decimal expectedSubtotal = 0m;
            foreach (var item in summary.Lines)
            {
                expectedSubtotal += item.LineTotal;
            }

            bool isMatch = summary.Subtotal == expectedSubtotal &&
                           summary.DiscountAmount == 0m &&
                           summary.FinalTotal == expectedSubtotal &&
                           summary.PolicyName == policy.Name;

            Console.WriteLine($"T01 (ValidOrderNoDiscount): {(isMatch ? "PASS" : "FAIL")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"T01 (ValidOrderNoDiscount): FAIL - {ex.Message}");
        }
    }

    private static void RunT04_EmptyOrder_ThrowsArgumentException()
    {
        try
        {
            IProductCatalogue catalogue = new InMemoryCatalogue();
            IPricingPolicy policy = new NoDiscount();
            IReceiptFormatter formatter = new DetailedReceiptFormatter();
            var service = new OrderService(catalogue, policy, formatter);

            var emptyLines = new List<OrderLine>();
            service.CreateOrder(emptyLines);

            Console.WriteLine("T04 (EmptyOrder_ThrowsArgumentException): FAIL - Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("T04 (EmptyOrder_ThrowsArgumentException): PASS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"T04 (EmptyOrder_ThrowsArgumentException): FAIL - Threw {ex.GetType().Name} instead of ArgumentException.");
        }
    }

    private static void RunT06_ZeroQuantity_ThrowsArgumentException()
    {
        try
        {
            IProductCatalogue catalogue = new InMemoryCatalogue();
            IPricingPolicy policy = new NoDiscount();
            IReceiptFormatter formatter = new DetailedReceiptFormatter();
            var service = new OrderService(catalogue, policy, formatter);

            var invalidLines = new List<OrderLine>
            {
                new OrderLine { ProductId = "P01", Quantity = 0 }
            };

            service.CreateOrder(invalidLines);

            Console.WriteLine("T06 (ZeroQuantity_ThrowsArgumentException): FAIL - Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("T06 (ZeroQuantity_ThrowsArgumentException): PASS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"T06 (ZeroQuantity_ThrowsArgumentException): FAIL - Threw {ex.GetType().Name} instead of ArgumentException.");
        }
    }
}
