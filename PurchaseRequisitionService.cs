using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.Utilities; // Assuming DatabaseHelper is here

namespace HumanitarianProjectManagement.Services
{
    public class PurchaseRequisitionService
    {
        public async Task<List<PurchaseRequisition>> GetAllPurchaseRequisitionsAsync()
        {
            List<PurchaseRequisition> requisitions = new List<PurchaseRequisition>();
            string query = @"SELECT PRID, RequestDate, RequestedByUserID, Department, BudgetCode, Status, ApprovalByUserID, ApprovalDate, Notes, CreatedAt, UpdatedAt 
                             FROM PurchaseRequisitions;";

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
                                PurchaseRequisition pr = MapPurchaseRequisitionFromReader(reader);
                                pr.Items = await GetPurchaseRequisitionItemsAsync(pr.PRID, connection); // Pass connection to avoid multiple openings
                                requisitions.Add(pr);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllPurchaseRequisitionsAsync: {ex.Message}");
            }
            return requisitions;
        }

        public async Task<PurchaseRequisition> GetPurchaseRequisitionByIdAsync(int id)
        {
            PurchaseRequisition pr = null;
            string query = @"SELECT PRID, RequestDate, RequestedByUserID, Department, BudgetCode, Status, ApprovalByUserID, ApprovalDate, Notes, CreatedAt, UpdatedAt 
                             FROM PurchaseRequisitions WHERE PRID = @PRID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PRID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                pr = MapPurchaseRequisitionFromReader(reader);
                                pr.Items = await GetPurchaseRequisitionItemsAsync(pr.PRID, connection); // Pass connection
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetPurchaseRequisitionByIdAsync: {ex.Message}");
            }
            return pr;
        }

        public async Task AddPurchaseRequisitionAsync(PurchaseRequisition pr)
        {
            string query = @"INSERT INTO PurchaseRequisitions (RequestDate, RequestedByUserID, Department, BudgetCode, Status, ApprovalByUserID, ApprovalDate, Notes, CreatedAt, UpdatedAt) 
                             VALUES (@RequestDate, @RequestedByUserID, @Department, @BudgetCode, @Status, @ApprovalByUserID, @ApprovalDate, @Notes, @CreatedAt, @UpdatedAt);
                             SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddPurchaseRequisitionParameters(command, pr);
                        pr.PRID = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    foreach (var item in pr.Items)
                    {
                        item.PRID = pr.PRID;
                        await AddPurchaseRequisitionItemAsync(item, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in AddPurchaseRequisitionAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task UpdatePurchaseRequisitionAsync(PurchaseRequisition pr)
        {
            string query = @"UPDATE PurchaseRequisitions SET RequestDate = @RequestDate, RequestedByUserID = @RequestedByUserID, Department = @Department, 
                             BudgetCode = @BudgetCode, Status = @Status, ApprovalByUserID = @ApprovalByUserID, ApprovalDate = @ApprovalDate, 
                             Notes = @Notes, UpdatedAt = @UpdatedAt WHERE PRID = @PRID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddPurchaseRequisitionParameters(command, pr);
                        command.Parameters.AddWithValue("@PRID", pr.PRID);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Delete existing items and re-add new ones
                    string deleteItemsQuery = "DELETE FROM PurchaseRequisitionItems WHERE PRID = @PRID;";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteItemsQuery, connection, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@PRID", pr.PRID);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    foreach (var item in pr.Items)
                    {
                        item.PRID = pr.PRID;
                        await AddPurchaseRequisitionItemAsync(item, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in UpdatePurchaseRequisitionAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task DeletePurchaseRequisitionAsync(int id)
        {
            string query = "DELETE FROM PurchaseRequisitions WHERE PRID = @PRID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Delete associated items first
                    string deleteItemsQuery = "DELETE FROM PurchaseRequisitionItems WHERE PRID = @PRID;";
                    using (SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, connection, transaction))
                    {
                        deleteItemsCmd.Parameters.AddWithValue("@PRID", id);
                        await deleteItemsCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@PRID", id);
                        await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in DeletePurchaseRequisitionAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task<List<PurchaseRequisition>> SearchPurchaseRequisitionsAsync(string searchTerm)
        {
            List<PurchaseRequisition> requisitions = new List<PurchaseRequisition>();
            string query = @"SELECT PRID, RequestDate, RequestedByUserID, Department, BudgetCode, Status, ApprovalByUserID, ApprovalDate, Notes, CreatedAt, UpdatedAt 
                             FROM PurchaseRequisitions ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE Department LIKE @SearchTerm OR BudgetCode LIKE @SearchTerm OR Status LIKE @SearchTerm;";
                // Add search by user name and product name if needed, will require JOINs
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
                                PurchaseRequisition pr = MapPurchaseRequisitionFromReader(reader);
                                pr.Items = await GetPurchaseRequisitionItemsAsync(pr.PRID, connection); // Pass connection
                                requisitions.Add(pr);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchPurchaseRequisitionsAsync: {ex.Message}");
            }
            return requisitions;
        }

        public async Task ApprovePurchaseRequisitionAsync(int prId, int approverUserId)
        {
            string query = "UPDATE PurchaseRequisitions SET Status = @Status, ApprovalByUserID = @ApprovalByUserID, ApprovalDate = @ApprovalDate, UpdatedAt = @UpdatedAt WHERE PRID = @PRID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", "معتمد");
                        command.Parameters.AddWithValue("@ApprovalByUserID", approverUserId);
                        command.Parameters.AddWithValue("@ApprovalDate", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@PRID", prId);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in ApprovePurchaseRequisitionAsync: {ex.Message}");
            }
        }

        public async Task RejectPurchaseRequisitionAsync(int prId, int approverUserId)
        {
            string query = "UPDATE PurchaseRequisitions SET Status = @Status, ApprovalByUserID = @ApprovalByUserID, ApprovalDate = @ApprovalDate, UpdatedAt = @UpdatedAt WHERE PRID = @PRID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", "مرفوض");
                        command.Parameters.AddWithValue("@ApprovalByUserID", approverUserId);
                        command.Parameters.AddWithValue("@ApprovalDate", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@PRID", prId);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in RejectPurchaseRequisitionAsync: {ex.Message}");
            }
        }

        private PurchaseRequisition MapPurchaseRequisitionFromReader(SqlDataReader reader)
        {
            return new PurchaseRequisition
            {
                PRID = (int)reader["PRID"],
                RequestDate = (DateTime)reader["RequestDate"],
                RequestedByUserID = (int)reader["RequestedByUserID"],
                Department = reader["Department"] != DBNull.Value ? reader["Department"].ToString() : null,
                BudgetCode = reader["BudgetCode"] != DBNull.Value ? reader["BudgetCode"].ToString() : null,
                Status = reader["Status"].ToString(),
                ApprovalByUserID = reader["ApprovalByUserID"] != DBNull.Value ? (int?)reader["ApprovalByUserID"] : null,
                ApprovalDate = reader["ApprovalDate"] != DBNull.Value ? (DateTime?)reader["ApprovalDate"] : null,
                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        private void AddPurchaseRequisitionParameters(SqlCommand command, PurchaseRequisition pr)
        {
            command.Parameters.AddWithValue("@RequestDate", pr.RequestDate);
            command.Parameters.AddWithValue("@RequestedByUserID", pr.RequestedByUserID);
            command.Parameters.AddWithValue("@Department", (object)pr.Department ?? DBNull.Value);
            command.Parameters.AddWithValue("@BudgetCode", (object)pr.BudgetCode ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", pr.Status);
            command.Parameters.AddWithValue("@ApprovalByUserID", (object)pr.ApprovalByUserID ?? DBNull.Value);
            command.Parameters.AddWithValue("@ApprovalDate", (object)pr.ApprovalDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Notes", (object)pr.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", pr.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", pr.UpdatedAt);
        }

        private async Task<ICollection<PurchaseRequisitionItem>> GetPurchaseRequisitionItemsAsync(int prId, SqlConnection connection)
        {
            List<PurchaseRequisitionItem> items = new List<PurchaseRequisitionItem>();
            string query = @"SELECT PRItemID, PRID, ProductID, Quantity, UnitPrice, TotalPrice, Notes 
                             FROM PurchaseRequisitionItems WHERE PRID = @PRID;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PRID", prId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new PurchaseRequisitionItem
                        {
                            PRItemID = (int)reader["PRItemID"],
                            PRID = (int)reader["PRID"],
                            ProductID = (int)reader["ProductID"],
                            Quantity = (decimal)reader["Quantity"],
                            UnitPrice = (decimal)reader["UnitPrice"],
                            TotalPrice = (decimal)reader["TotalPrice"],
                            Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null
                        });
                    }
                }
            }
            return items;
        }

        private async Task AddPurchaseRequisitionItemAsync(PurchaseRequisitionItem item, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"INSERT INTO PurchaseRequisitionItems (PRID, ProductID, Quantity, UnitPrice, TotalPrice, Notes) 
                             VALUES (@PRID, @ProductID, @Quantity, @UnitPrice, @TotalPrice, @Notes);";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@PRID", item.PRID);
                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                command.Parameters.AddWithValue("@Notes", (object)item.Notes ?? DBNull.Value);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}


