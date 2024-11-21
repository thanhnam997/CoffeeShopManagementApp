using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

public class CustomersModel : PageModel
{
    public required List<Customer> Customers { get; set; }

    public void OnGet()
    {
        LoadCustomers();
    }

    private void LoadCustomers()
    {
        Customers = new List<Customer>();
        using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Customers";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Customers.Add(new Customer
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2)
                    });
                }
            }
        }
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
