using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.Utilities; // Assuming DatabaseHelper is here

namespace HumanitarianProjectManagement.Services
{
    public class ProductServiceAdo
    {
        public async Task<List<Product>> GetAllProductsAsync()
        {
            List<Product> products = new List<Product>();
            string query = "SELECT ProductID, Name, Description, Unit, DefaultPrice, SKU, Category, CreatedAt, UpdatedAt FROM Products;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(MapProductFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllProductsAsync: {ex.Message}");
            }
            return products;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            Product product = null;
            string query = "SELECT ProductID, Name, Description, Unit, DefaultPrice, SKU, Category, CreatedAt, UpdatedAt FROM Products WHERE ProductID = @ProductID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                product = MapProductFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetProductByIdAsync: {ex.Message}");
            }
            return product;
        }

        public async Task AddProductAsync(Product product)
        {
            string query = @"INSERT INTO Products (Name, Description, Unit, DefaultPrice, SKU, Category, CreatedAt, UpdatedAt) 
                             VALUES (@Name, @Description, @Unit, @DefaultPrice, @SKU, @Category, @CreatedAt, @UpdatedAt);";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddProductParameters(command, product);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in AddProductAsync: {ex.Message}");
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            string query = @"UPDATE Products SET Name = @Name, Description = @Description, Unit = @Unit, DefaultPrice = @DefaultPrice, 
                             SKU = @SKU, Category = @Category, UpdatedAt = @UpdatedAt 
                             WHERE ProductID = @ProductID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddProductParameters(command, product);
                        command.Parameters.AddWithValue("@ProductID", product.ProductID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in UpdateProductAsync: {ex.Message}");
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            string query = "DELETE FROM Products WHERE ProductID = @ProductID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", id);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in DeleteProductAsync: {ex.Message}");
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            List<Product> products = new List<Product>();
            string query = "SELECT ProductID, Name, Description, Unit, DefaultPrice, SKU, Category, CreatedAt, UpdatedAt FROM Products ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE Name LIKE @SearchTerm OR SKU LIKE @SearchTerm OR Category LIKE @SearchTerm;";
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query + whereClause, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchTerm))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                        }
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(MapProductFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchProductsAsync: {ex.Message}");
            }
            return products;
        }

        private Product MapProductFromReader(SqlDataReader reader)
        {
            return new Product
            {
                ProductID = (int)reader["ProductID"],
                Name = reader["Name"].ToString(),
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                Unit = reader["Unit"] != DBNull.Value ? reader["Unit"].ToString() : null,
                DefaultPrice = reader["DefaultPrice"] != DBNull.Value ? (decimal)reader["DefaultPrice"] : 0,
                SKU = reader["SKU"] != DBNull.Value ? reader["SKU"].ToString() : null,
                Category = reader["Category"] != DBNull.Value ? reader["Category"].ToString() : null,
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        private void AddProductParameters(SqlCommand command, Product product)
        {
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Unit", (object)product.Unit ?? DBNull.Value);
            command.Parameters.AddWithValue("@DefaultPrice", product.DefaultPrice);
            command.Parameters.AddWithValue("@SKU", (object)product.SKU ?? DBNull.Value);
            command.Parameters.AddWithValue("@Category", (object)product.Category ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);
        }
    }
}
