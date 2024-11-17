using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementApp.Pages
{
    public class AddOrderModel : PageModel
    {
        [BindProperty]
        public OrderInputModel NewOrder { get; set; } = new();

        [BindProperty]
        public List<ProductSelection> SelectedProducts { get; set; } = new();

        public List<Customer> Customers { get; set; } = new();
        public required string Message { get; set; }

        public void OnGet()
        {
            LoadCustomers();
            LoadProducts();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || !SelectedProducts.Any(p => p.Selected && p.Quantity > 0))
            {
                LoadCustomers();
                LoadProducts();
                ModelState.AddModelError("", "You must select at least one product.");
                return Page();
            }

            try
            {
                using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
                {
                    connection.Open();

                    // Insert the new order
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Orders (CustomerId, OrderDate, TotalAmount)
                        VALUES (@CustomerId, @OrderDate, @TotalAmount)";
                    command.Parameters.AddWithValue("@CustomerId", NewOrder.CustomerId);
                    command.Parameters.AddWithValue("@OrderDate", DateTime.Now.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@TotalAmount", CalculateTotalAmount());
                    command.ExecuteNonQuery();

                    // Get the last inserted OrderId
                    command.CommandText = "SELECT last_insert_rowid()";
                   var orderId = command.ExecuteScalar() as long?;
if (!orderId.HasValue)
{
    throw new Exception("Failed to retrieve the OrderId after inserting the order.");
}

                    Console.WriteLine($"New OrderId: {orderId}");
                    
                    // Insert order details
                    foreach (var product in SelectedProducts)
                    
                    
                    
                    {
                        Console.WriteLine($"Processing ProductId: {product.ProductId}, Selected: {product.Selected}, Quantity: {product.Quantity}");
                        if (product.Selected && product.Quantity > 0)
                        {
                            var detailCommand = connection.CreateCommand();
                            detailCommand.CommandText = @"
                                INSERT INTO OrderDetails (OrderId, ProductId, Quantity)
                                VALUES (@OrderId, @ProductId, @Quantity)";
                            detailCommand.Parameters.AddWithValue("@OrderId", orderId);
                            detailCommand.Parameters.AddWithValue("@ProductId", product.ProductId);
                            detailCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
                            detailCommand.ExecuteNonQuery();
                        }
                    }
                }
                 Console.WriteLine("Order and OrderDetails successfully added.");
                return RedirectToPage("/Orders"); // Redirect to Orders page
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                LoadCustomers();
                LoadProducts();
                return Page();
            }
        }

        private void LoadCustomers()
        {
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name FROM Customers";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customers.Add(new Customer
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
        }

        private void LoadProducts()
        {
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, Price FROM Products";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SelectedProducts.Add(new ProductSelection
                        {
                            ProductId = reader.GetInt32(0),
                            ProductName = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            Quantity = 0
                        });
                    }
                }
            }
        }

        private decimal CalculateTotalAmount()
        {
            decimal total = 0;
            foreach (var product in SelectedProducts)
            {
                if (product.Selected)
                {
                    total += product.Price * product.Quantity;
                }
            }
            return total;
        }
    }

    public class OrderInputModel
    {
        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerId { get; set; }
    }

    public class ProductSelection
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool Selected { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
