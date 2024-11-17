using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopManagementApp.Pages
{
    public class AddCustomerModel : PageModel
    {
        [BindProperty]
        public CustomerInputModel NewCustomer { get; set; } = new();

        public string Message { get; set; }

        public void OnGet()
        {
            // Initialize an empty customer
            NewCustomer = new CustomerInputModel();
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

                // Insert the new customer into the database
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Customers (Name, Email)
                    VALUES (@Name, @Email)";
                command.Parameters.AddWithValue("@Name", NewCustomer.Name);
                command.Parameters.AddWithValue("@Email", NewCustomer.Email);
                command.ExecuteNonQuery();
            }

            Message = "Customer added successfully!";
            return RedirectToPage("/Customers");
        }
    }

    public class CustomerInputModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}
