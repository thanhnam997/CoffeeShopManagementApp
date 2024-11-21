using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementApp.Pages
{
    public class AddCustomerModel : PageModel
    {
        [BindProperty]
        public CustomerInputModel NewCustomer { get; set; } = new();

        [BindProperty]
        public List<ProductSelection> SelectedProducts { get; set; } = new();

        public string? Message { get; set; }

        public void OnGet()
        {
            LoadProducts();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || !SelectedProducts.Any(p => p.Selected && p.Quantity > 0))
            {
                LoadProducts();
                ModelState.AddModelError("", "You must select at least one product.");
                return Page();
            }

            try
            {
                using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
                {
                    connection.Open();

                    // Add customer
                    var customerCommand = connection.CreateCommand();
                    customerCommand.CommandText = @"
                        INSERT INTO Customers (Name, Email)
                        VALUES (@Name, @Email)";
                    customerCommand.Parameters.AddWithValue("@Name", NewCustomer.Name);
                    customerCommand.Parameters.AddWithValue("@Email", NewCustomer.Email);
                    customerCommand.ExecuteNonQuery();

                    // Get the CustomerId of the newly added customer
                    customerCommand.CommandText = "SELECT last_insert_rowid()";
                    var customerId = (long)customerCommand.ExecuteScalar();

                    // Add order
                    var orderCommand = connection.CreateCommand();
                    orderCommand.CommandText = @"
                        INSERT INTO Orders (CustomerId, OrderDate, TotalAmount)
                        VALUES (@CustomerId, @OrderDate, @TotalAmount)";
                    orderCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now.ToString("yyyy-MM-dd"));
                    orderCommand.Parameters.AddWithValue("@TotalAmount", CalculateTotalAmount());
                    orderCommand.ExecuteNonQuery();

                    // Get the OrderId of the newly added order
                    orderCommand.CommandText = "SELECT last_insert_rowid()";
                    var orderId = (long)orderCommand.ExecuteScalar();

                    // Add order details
                    foreach (var product in SelectedProducts)
                    {
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

                Message = "Customer and order added successfully!";
                return RedirectToPage("/"); // Redirect to a relevant page
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                LoadProducts();
                return Page();
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

        public class CustomerInputModel
        {
            [Required(ErrorMessage = "Name is required.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; } = string.Empty;
        }

        public class ProductSelection
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public bool Selected { get; set; }
        }
    }
}
