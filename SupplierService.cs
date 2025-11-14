using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.Utilities; // Assuming DatabaseHelper is here

namespace HumanitarianProjectManagement.Services
{
    public class SupplierService
    {
        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            List<Supplier> suppliers = new List<Supplier>();
            string query = "SELECT SupplierID, Name, ContactPerson, Email, Phone, Address, PaymentTerms, Category, Status, CreatedAt, UpdatedAt FROM Suppliers;";

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
                                suppliers.Add(MapSupplierFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllSuppliersAsync: {ex.Message}");
            }
            return suppliers;
        }

        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            Supplier supplier = null;
            string query = "SELECT SupplierID, Name, ContactPerson, Email, Phone, Address, PaymentTerms, Category, Status, CreatedAt, UpdatedAt FROM Suppliers WHERE SupplierID = @SupplierID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SupplierID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                supplier = MapSupplierFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetSupplierByIdAsync: {ex.Message}");
            }
            return supplier;
        }

        public async Task AddSupplierAsync(Supplier supplier)
        {
            string query = @"INSERT INTO Suppliers (Name, ContactPerson, Email, Phone, Address, PaymentTerms, Category, Status, CreatedAt, UpdatedAt) 
                             VALUES (@Name, @ContactPerson, @Email, @Phone, @Address, @PaymentTerms, @Category, @Status, @CreatedAt, @UpdatedAt);";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddSupplierParameters(command, supplier);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in AddSupplierAsync: {ex.Message}");
            }
        }

        public async Task UpdateSupplierAsync(Supplier supplier)
        {
            string query = @"UPDATE Suppliers SET Name = @Name, ContactPerson = @ContactPerson, Email = @Email, Phone = @Phone, 
                             Address = @Address, PaymentTerms = @PaymentTerms, Category = @Category, Status = @Status, UpdatedAt = @UpdatedAt 
                             WHERE SupplierID = @SupplierID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddSupplierParameters(command, supplier);
                        command.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in UpdateSupplierAsync: {ex.Message}");
            }
        }

        public async Task DeleteSupplierAsync(int id)
        {
            string query = "DELETE FROM Suppliers WHERE SupplierID = @SupplierID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SupplierID", id);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in DeleteSupplierAsync: {ex.Message}");
            }
        }

        public async Task<List<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            List<Supplier> suppliers = new List<Supplier>();
            string query = "SELECT SupplierID, Name, ContactPerson, Email, Phone, Address, PaymentTerms, Category, Status, CreatedAt, UpdatedAt FROM Suppliers ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE Name LIKE @SearchTerm OR ContactPerson LIKE @SearchTerm OR Email LIKE @SearchTerm OR Phone LIKE @SearchTerm;";
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
                                suppliers.Add(MapSupplierFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchSuppliersAsync: {ex.Message}");
            }
            return suppliers;
        }

        private Supplier MapSupplierFromReader(SqlDataReader reader)
        {
            return new Supplier
            {
                SupplierID = (int)reader["SupplierID"],
                Name = reader["Name"].ToString(),
                ContactPerson = reader["ContactPerson"] != DBNull.Value ? reader["ContactPerson"].ToString() : null,
                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                PaymentTerms = reader["PaymentTerms"] != DBNull.Value ? reader["PaymentTerms"].ToString() : null,
                Category = reader["Category"] != DBNull.Value ? reader["Category"].ToString() : null,
                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        private void AddSupplierParameters(SqlCommand command, Supplier supplier)
        {
            command.Parameters.AddWithValue("@Name", supplier.Name);
            command.Parameters.AddWithValue("@ContactPerson", (object)supplier.ContactPerson ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)supplier.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@Phone", (object)supplier.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object)supplier.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaymentTerms", (object)supplier.PaymentTerms ?? DBNull.Value);
            command.Parameters.AddWithValue("@Category", (object)supplier.Category ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", (object)supplier.Status ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", supplier.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", supplier.UpdatedAt);
        }
    }
}


