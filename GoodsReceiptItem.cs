using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.Utilities; // Assuming DatabaseHelper is here

namespace HumanitarianProjectManagement.Services
{
    public class GoodsReceiptService
    {
        public async Task<List<GoodsReceipt>> GetAllGoodsReceiptsAsync()
        {
            List<GoodsReceipt> receipts = new List<GoodsReceipt>();
            string query = @"SELECT GRNID, POID, ReceiptDate, ReceivedByUserID, Notes, CreatedAt, UpdatedAt 
                             FROM GoodsReceipts;";

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
                                GoodsReceipt gr = MapGoodsReceiptFromReader(reader);
                                gr.Items = await GetGoodsReceiptItemsAsync(gr.GRNID, connection); // Pass connection
                                receipts.Add(gr);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllGoodsReceiptsAsync: {ex.Message}");
            }
            return receipts;
        }

        public async Task<GoodsReceipt> GetGoodsReceiptByIdAsync(int id)
        {
            GoodsReceipt gr = null;
            string query = @"SELECT GRNID, POID, ReceiptDate, ReceivedByUserID, Notes, CreatedAt, UpdatedAt 
                             FROM GoodsReceipts WHERE GRNID = @GRNID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GRNID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                gr = MapGoodsReceiptFromReader(reader);
                                gr.Items = await GetGoodsReceiptItemsAsync(gr.GRNID, connection); // Pass connection
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetGoodsReceiptByIdAsync: {ex.Message}");
            }
            return gr;
        }

        public async Task AddGoodsReceiptAsync(GoodsReceipt gr)
        {
            string query = @"INSERT INTO GoodsReceipts (POID, ReceiptDate, ReceivedByUserID, Notes, CreatedAt, UpdatedAt) 
                             VALUES (@POID, @ReceiptDate, @ReceivedByUserID, @Notes, @CreatedAt, @UpdatedAt);
                             SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddGoodsReceiptParameters(command, gr);
                        gr.GRNID = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    foreach (var item in gr.Items)
                    {
                        item.GRNID = gr.GRNID;
                        await AddGoodsReceiptItemAsync(item, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in AddGoodsReceiptAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task UpdateGoodsReceiptAsync(GoodsReceipt gr)
        {
            string query = @"UPDATE GoodsReceipts SET POID = @POID, ReceiptDate = @ReceiptDate, ReceivedByUserID = @ReceivedByUserID, 
                             Notes = @Notes, UpdatedAt = @UpdatedAt WHERE GRNID = @GRNID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddGoodsReceiptParameters(command, gr);
                        command.Parameters.AddWithValue("@GRNID", gr.GRNID);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Delete existing items and re-add new ones
                    string deleteItemsQuery = "DELETE FROM GoodsReceiptItems WHERE GRNID = @GRNID;";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteItemsQuery, connection, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@GRNID", gr.GRNID);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    foreach (var item in gr.Items)
                    {
                        item.GRNID = gr.GRNID;
                        await AddGoodsReceiptItemAsync(item, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in UpdateGoodsReceiptAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task DeleteGoodsReceiptAsync(int id)
        {
            string query = "DELETE FROM GoodsReceipts WHERE GRNID = @GRNID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Delete associated items first
                    string deleteItemsQuery = "DELETE FROM GoodsReceiptItems WHERE GRNID = @GRNID;";
                    using (SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, connection, transaction))
                    {
                        deleteItemsCmd.Parameters.AddWithValue("@GRNID", id);
                        await deleteItemsCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@GRNID", id);
                        await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in DeleteGoodsReceiptAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task<List<GoodsReceipt>> SearchGoodsReceiptsAsync(string searchTerm)
        {
            List<GoodsReceipt> receipts = new List<GoodsReceipt>();
            string query = @"SELECT GRNID, POID, ReceiptDate, ReceivedByUserID, Notes, CreatedAt, UpdatedAt 
                             FROM GoodsReceipts ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE Notes LIKE @SearchTerm;";
                // Add search by POID, ReceivedByUserID if needed, will require JOINs
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
                                GoodsReceipt gr = MapGoodsReceiptFromReader(reader);
                                gr.Items = await GetGoodsReceiptItemsAsync(gr.GRNID, connection); // Pass connection
                                receipts.Add(gr);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchGoodsReceiptsAsync: {ex.Message}");
            }
            return receipts;
        }

        private GoodsReceipt MapGoodsReceiptFromReader(SqlDataReader reader)
        {
            return new GoodsReceipt
            {
                GRNID = (int)reader["GRNID"],
                POID = (int)reader["POID"],
                ReceiptDate = (DateTime)reader["ReceiptDate"],
                ReceivedByUserID = (int)reader["ReceivedByUserID"],
                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        private void AddGoodsReceiptParameters(SqlCommand command, GoodsReceipt gr)
        {
            command.Parameters.AddWithValue("@POID", gr.POID);
            command.Parameters.AddWithValue("@ReceiptDate", gr.ReceiptDate);
            command.Parameters.AddWithValue("@ReceivedByUserID", gr.ReceivedByUserID);
            command.Parameters.AddWithValue("@Notes", (object)gr.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", gr.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", gr.UpdatedAt);
        }

        private async Task<ICollection<GoodsReceiptItem>> GetGoodsReceiptItemsAsync(int grnId, SqlConnection connection)
        {
            List<GoodsReceiptItem> items = new List<GoodsReceiptItem>();
            string query = @"SELECT GRItemID, GRNID, POItemID, ReceivedQuantity, QualityStatus 
                             FROM GoodsReceiptItems WHERE GRNID = @GRNID;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@GRNID", grnId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new GoodsReceiptItem
                        {
                            GRItemID = (int)reader["GRItemID"],
                            GRNID = (int)reader["GRNID"],
                            POItemID = (int)reader["POItemID"],
                            ReceivedQuantity = (decimal)reader["ReceivedQuantity"],
                            QualityStatus = reader["QualityStatus"] != DBNull.Value ? reader["QualityStatus"].ToString() : null
                        });
                    }
                }
            }
            return items;
        }

        private async Task AddGoodsReceiptItemAsync(GoodsReceiptItem item, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"INSERT INTO GoodsReceiptItems (GRNID, POItemID, ReceivedQuantity, QualityStatus) 
                             VALUES (@GRNID, @POItemID, @ReceivedQuantity, @QualityStatus);";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@GRNID", item.GRNID);
                command.Parameters.AddWithValue("@POItemID", item.POItemID);
                command.Parameters.AddWithValue("@ReceivedQuantity", item.ReceivedQuantity);
                command.Parameters.AddWithValue("@QualityStatus", (object)item.QualityStatus ?? DBNull.Value);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}


