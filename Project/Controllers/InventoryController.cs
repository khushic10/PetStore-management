using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using System.Data;

namespace Project.Controllers
{
    public class InventoryController : Controller
    {
        private readonly string _connectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=pets;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        [HttpGet]
        public IActionResult Index()
        {
            List<InventoryModel> products = new List<InventoryModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string command = "SELECT * FROM Inventory";
                using (SqlCommand sqlCommand = new SqlCommand(command, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        foreach (DataRow item in dataTable.Rows)
                        {
                            products.Add(MapToInventoryModel(item));
                        }
                    }
                }
            }

            return View(products);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            InventoryModel product = new InventoryModel();
            return View(product);
        }
        [HttpPost]
        public IActionResult AddProduct(InventoryModel Product, IFormFile ProductImage)
        {
            try
            {
                if (ProductImage != null && ProductImage.Length > 0)
                {
                    // Read the uploaded file into a byte array
                    byte[] imageData;
                    using (var stream = new MemoryStream())
                    {
                        ProductImage.CopyTo(stream);
                        imageData = stream.ToArray();
                    }

                    // Save the image data to the InventoryModel
                    Product.ProductImage = imageData;
                }
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "INSERT INTO Inventory (ProductName, ProductCost, Quantity,ProductImage) " +
                                     "VALUES (@ProductName, @ProductCost, @Quantity, @ProductImage)";
                    SqlCommand sqlCommand = new SqlCommand(command, conn);
                    sqlCommand.Parameters.AddWithValue("@ProductName", Product.ProductName);
                    sqlCommand.Parameters.AddWithValue("@ProductCost", Product.ProductCost);
                    sqlCommand.Parameters.AddWithValue("@Quantity", Product.Quantity);
                    sqlCommand.Parameters.AddWithValue("@ProductImage", Product.ProductImage);
                    int rowsAffected = sqlCommand.ExecuteNonQuery();
                    if (rowsAffected == 1)
                    {
                        TempData["SuccessMessage"] = "Product added successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw new Exception("No rows were affected by the INSERT operation.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine("Error adding Product: " + ex.Message);

                // Return the same view with the model to display validation errors or other messages
                ModelState.AddModelError("", "An error occurred while adding the Product. Please try again.");
                return View(Product);
            }
        }
        public IActionResult DeleteProduct(int Id)
        {
            try
            {

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "DELETE FROM Inventory WHERE Id = @Id";

                    using (SqlCommand sqlCommand = new SqlCommand(command, conn))
                    {
                        sqlCommand.Parameters.AddWithValue("@Id", Id);
                        sqlCommand.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Log or handle the exception gracefully
                throw;
            }
        }
        public IActionResult EditProduct(int id, int quantity)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "UPDATE Inventory SET Quantity = @Quantity WHERE Id = @Id";
                    using (SqlCommand sqlCommand = new SqlCommand(command, conn))
                    {
                        sqlCommand.Parameters.AddWithValue("@Id", id);
                        sqlCommand.Parameters.AddWithValue("@Quantity", quantity);
                        int rowsAffected = sqlCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Successfully updated the quantity
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            // Product record not found or not updated
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500); // Internal Server Error
            }
        }
        private InventoryModel MapToInventoryModel(DataRow itemRow)
        {
            return new InventoryModel
            {
                Id = Convert.ToInt32(itemRow["Id"]),
                ProductName = itemRow["ProductName"].ToString(),
                ProductCost = Convert.ToInt32(itemRow["ProductCost"]),
                Quantity = Convert.ToInt32(itemRow["Quantity"]),
                ProductImage = itemRow["ProductImage"] as byte[],
            };
        }
    }
}
