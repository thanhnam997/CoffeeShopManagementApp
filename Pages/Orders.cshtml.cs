using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

public class OrdersModel : PageModel
{
    public List<Order> Orders { get; set; }

    public void OnGet()
    {
        LoadOrders();
    }

    private void LoadOrders()
    {
        Orders = new List<Order>();
        using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Orders";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Orders.Add(new Order
                    {
                        Id = reader.GetInt32(0),
                        CustomerId = reader.GetInt32(1),
                        OrderDate = reader.GetString(2),
                        TotalAmount = reader.GetDecimal(3)
                    });
                }
            }
        }
    }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}
