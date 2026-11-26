using System;
using System.Collections.Generic;

namespace CampusCafeOrderingSystem
{
    // --- Mandatory Interface ---
    public interface IProductCatalogue
    {
        Product GetById(string productId);
    }

    // --- Immutable Data Models ---
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

            // Defensive read-only wrapper ensures formatters cannot mutate summary lines
            Lines = new List<CalculatedLine>(lines).AsReadOnly();
            PolicyName = policyName;
            Subtotal = subtotal;
            DiscountAmount = discountAmount;
            FinalTotal = finalTotal;
        }
    }

    // --- Concrete Catalogue Implementation ---
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
}