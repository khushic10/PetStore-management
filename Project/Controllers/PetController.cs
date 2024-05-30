using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;

namespace Project.Controllers
{
    public class PetController : Controller
    {
        private readonly string _connectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=pets;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        [HttpGet]
        public IActionResult Index()
        {
            List<PetModel> pets = new List<PetModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string command = "SELECT * FROM petData WHERE PetStatus <> 'Sold'";
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
        [HttpGet]
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


        [HttpGet]
        public IActionResult AddPet()
        {
            PetModel pet = new PetModel();
            return View(pet);
        }
        [HttpPost]
        public IActionResult AddPet(PetModel pet, IFormFile petImage)
        {
            try
            {
                if (petImage != null && petImage.Length > 0)
                {
                    // Read the uploaded file into a byte array
                    byte[] imageData;
                    using (var stream = new MemoryStream())
                    {
                        petImage.CopyTo(stream);
                        imageData = stream.ToArray();
                    }

                    // Save the image data to the PetModel
                    pet.PetImage = imageData;
                }
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "INSERT INTO petData (PetName, PetDescription, PetType, PetBreed, PetCost, PetImage) " +
                                     "VALUES (@PetName, @PetDescription, @PetType, @PetBreed, @PetCost, @PetImage)";
                    SqlCommand sqlCommand = new SqlCommand(command, conn);
                    sqlCommand.Parameters.AddWithValue("@PetName", pet.PetName);
                    sqlCommand.Parameters.AddWithValue("@PetDescription", pet.PetDescription);
                    sqlCommand.Parameters.AddWithValue("@PetType", pet.PetType);
                    sqlCommand.Parameters.AddWithValue("@PetBreed", pet.PetBreed);
                    sqlCommand.Parameters.AddWithValue("@PetCost", pet.PetCost);
                    sqlCommand.Parameters.AddWithValue("@PetImage", pet.PetImage);
                    int rowsAffected = sqlCommand.ExecuteNonQuery();
                    if (rowsAffected == 1)
                    {
                        // Redirect to the index page with a success message
                        TempData["SuccessMessage"] = "Pet added successfully!";
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
                Console.WriteLine("Error adding pet: " + ex.Message);

                // Return the same view with the model to display validation errors or other messages
                ModelState.AddModelError("", "An error occurred while adding the pet. Please try again.");
                return View(pet);
            }
        }
        [HttpGet]
        public IActionResult EditPet(int id)
        {
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
                                PetModel pet = MapToPetModel(reader);
                                return View(pet);
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

            return NotFound(); // Return 404 Not Found if pet with the given id is not found
        }

        [HttpPost]
        public IActionResult EditPet(PetModel pet)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "UPDATE petData SET PetName = @PetName, PetDescription = @PetDescription, PetType = @PetType, PetBreed = @PetBreed, PetCost = @PetCost WHERE Id = @Id";
                    SqlCommand sqlCommand = new SqlCommand(command, conn);
                    sqlCommand.Parameters.AddWithValue("@PetName", pet.PetName);
                    sqlCommand.Parameters.AddWithValue("@PetDescription", pet.PetDescription);
                    sqlCommand.Parameters.AddWithValue("@PetType", pet.PetType);
                    sqlCommand.Parameters.AddWithValue("@PetBreed", pet.PetBreed);
                    sqlCommand.Parameters.AddWithValue("@PetCost", pet.PetCost);
                    sqlCommand.Parameters.AddWithValue("@Id", pet.Id);
                    int rowsAffected = sqlCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Redirect to the index page with a success message
                        TempData["SuccessMessage"] = "Pet updated successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Handle case where pet with the given id is not found
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500); // Internal Server Error
            }
        }

        [HttpPost]
        public IActionResult MarkAsSold(int Id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string command = "UPDATE petData SET PetStatus = 'Sold' WHERE Id = @Id";
                    using (SqlCommand sqlCommand = new SqlCommand(command, conn))
                    {
                        sqlCommand.Parameters.AddWithValue("@Id", Id);
                        int rowsAffected = sqlCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Successfully marked as sold
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            // Pet record not found or not updated
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
