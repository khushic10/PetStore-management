using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using System.Data;

namespace Project.Controllers
{
    public class OldPetController : Controller
    {
        private readonly string _connectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=pets;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        [HttpGet]
        public IActionResult Index()
        {
            List<PetModel> pets = new List<PetModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string command = "SELECT * FROM petData WHERE PetStatus = 'Sold'";
                using (SqlCommand sqlCommand = new SqlCommand(command, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        foreach (DataRow pet in dataTable.Rows)
                        {
                            pets.Add(MapToPetModel(pet));
                        }
                    }
                }
            }

            return View(pets);
        }
        public IActionResult Details(int id)
        {
            PetModel pet = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "SELECT * FROM petData WHERE Id = @Id";
                    using (SqlCommand sqlCommand = new SqlCommand(command, conn))
                    {
                        sqlCommand.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pet = MapToPetModel(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500); // Internal Server Error
            }

            if (pet == null)
            {
                return NotFound(); // Return 404 Not Found if pet with the given id is not found
            }

            return View(pet);
        }
        public IActionResult DeletePet(int Id)
        {
            try
            {

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "DELETE FROM petData WHERE Id = @Id";

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
        private PetModel MapToPetModel(DataRow petRow)
        {
            return new PetModel
            {
                Id = Convert.ToInt32(petRow["Id"]),
                PetName = petRow["PetName"].ToString(),
                PetDescription = petRow["PetDescription"].ToString(),
                PetType = petRow["PetType"].ToString(),
                PetBreed = petRow["PetBreed"].ToString(),
                PetCost = Convert.ToInt32(petRow["PetCost"]),
                PetStatus = petRow["PetStatus"].ToString(),
                PetImage = petRow["PetImage"] as byte[],
            };
        }
        private PetModel MapToPetModel(SqlDataReader reader)
        {
            return new PetModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                PetName = reader["PetName"].ToString(),
                PetDescription = reader["PetDescription"].ToString(),
                PetType = reader["PetType"].ToString(),
                PetBreed = reader["PetBreed"].ToString(),
                PetCost = Convert.ToInt32(reader["PetCost"]),
                PetStatus = reader["PetStatus"].ToString(),
                // Note: You may need to handle DBNull.Value for nullable columns
                // Example: PetImage = reader["PetImage"] != DBNull.Value ? (byte[])reader["PetImage"] : null,
                PetImage = reader["PetImage"] as byte[],
            };
        }
    }
}
