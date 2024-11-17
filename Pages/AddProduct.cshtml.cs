using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementApp.Pages
{
    public class AddProductModel : PageModel
    {
        [BindProperty]
        public ProductInputModel NewProduct { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            // Initialize an empty product
            NewProduct = new ProductInputModel();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();

                // Insert the new product into the database
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Products (Name, Price, Stock)
                    VALUES (@Name, @Price, @Stock)";
                command.Parameters.AddWithValue("@Name", NewProduct.Name);
                command.Parameters.AddWithValue("@Price", NewProduct.Price);
                command.Parameters.AddWithValue("@Stock", NewProduct.Stock);
                command.ExecuteNonQuery();
            }

            Message = "Product added successfully!";
            return RedirectToPage("/Products");
        }
    }

    public class ProductInputModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
    }
}
