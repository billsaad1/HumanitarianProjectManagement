
using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using HumanitarianProjectManagement; // For AppContext

namespace HumanitarianProjectManagement.DataAccessLayer
{
    public class PurchaseOrderServiceAdoDal
    {
        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(int? projectId = null, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<PurchaseOrder> purchaseOrders = new List<PurchaseOrder>();
            StringBuilder queryBuilder = new StringBuilder(
                "SELECT PurchaseOrderID, ProjectID, PurchaseID, SupplierName, OrderDate, ExpectedDeliveryDate, ActualDeliveryDate, TotalAmount, Status, ShippingAddress, BillingAddress, Notes, CreatedByUserID, ApprovedByUserID FROM PurchaseOrders");

            List<string> conditions = new List<string>();

            if (projectId.HasValue)
            {
                conditions.Add("ProjectID = @ProjectID");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                conditions.Add("Status = @Status");
            }
            if (fromDate.HasValue)
            {
                conditions.Add("OrderDate >= @FromDate");
            }
            if (toDate.HasValue)
            {
                conditions.Add("OrderDate <= @ToDate"); // Assuming toDate is passed as end of day if time is relevant
            }

            if (conditions.Count > 0)
            {
                queryBuilder.Append(" WHERE ").Append(string.Join(" AND ", conditions));
            }
            queryBuilder.Append(" ORDER BY OrderDate DESC;");

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                    {
                        if (projectId.HasValue)
                        {
                            command.Parameters.AddWithValue("@ProjectID", projectId.Value);
                        }
                        if (!string.IsNullOrWhiteSpace(status))
                        {
                            command.Parameters.AddWithValue("@Status", status);
                        }
                        if (fromDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@FromDate", fromDate.Value);
                        }
                        if (toDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@ToDate", toDate.Value);
                        }

                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PurchaseOrder po = new PurchaseOrder
                                {
                                    PurchaseOrderID = (int)reader["PurchaseOrderID"],
                                    ProjectID = (int)reader["ProjectID"],
                                    PurchaseID = reader["PurchaseID"] != DBNull.Value ? (int?)reader["PurchaseID"] : null,
                                    SupplierName = reader["SupplierName"].ToString(),
                                    OrderDate = (DateTime)reader["OrderDate"],
                                    ExpectedDeliveryDate = reader["ExpectedDeliveryDate"] != DBNull.Value ? (DateTime?)reader["ExpectedDeliveryDate"] : null,
                                    ActualDeliveryDate = reader["ActualDeliveryDate"] != DBNull.Value ? (DateTime?)reader["ActualDeliveryDate"] : null,
                                    TotalAmount = (decimal)reader["TotalAmount"],
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    ShippingAddress = reader["ShippingAddress"] != DBNull.Value ? reader["ShippingAddress"].ToString() : null,
                                    BillingAddress = reader["BillingAddress"] != DBNull.Value ? reader["BillingAddress"].ToString() : null,
                                    Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                                    CreatedByUserID = reader["CreatedByUserID"] != DBNull.Value ? (int?)reader["CreatedByUserID"] : null,
                                    ApprovedByUserID = reader["ApprovedByUserID"] != DBNull.Value ? (int?)reader["ApprovedByUserID"] : null
                                };
                                purchaseOrders.Add(po);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetPurchaseOrdersAsync: {ex.Message}");
            }
            return purchaseOrders;
        }

        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int purchaseOrderId)
        {
            PurchaseOrder po = null;
            string query = "SELECT PurchaseOrderID, ProjectID, PurchaseID, SupplierName, OrderDate, ExpectedDeliveryDate, ActualDeliveryDate, TotalAmount, Status, ShippingAddress, BillingAddress, Notes, CreatedByUserID, ApprovedByUserID FROM PurchaseOrders WHERE PurchaseOrderID = @PurchaseOrderID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderId);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                po = new PurchaseOrder
                                {
                                    PurchaseOrderID = (int)reader["PurchaseOrderID"],
                                    ProjectID = (int)reader["ProjectID"],
                                    PurchaseID = reader["PurchaseID"] != DBNull.Value ? (int?)reader["PurchaseID"] : null,
                                    SupplierName = reader["SupplierName"].ToString(),
                                    OrderDate = (DateTime)reader["OrderDate"],
                                    ExpectedDeliveryDate = reader["ExpectedDeliveryDate"] != DBNull.Value ? (DateTime?)reader["ExpectedDeliveryDate"] : null,
                                    ActualDeliveryDate = reader["ActualDeliveryDate"] != DBNull.Value ? (DateTime?)reader["ActualDeliveryDate"] : null,
                                    TotalAmount = (decimal)reader["TotalAmount"],
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    ShippingAddress = reader["ShippingAddress"] != DBNull.Value ? reader["ShippingAddress"].ToString() : null,
                                    BillingAddress = reader["BillingAddress"] != DBNull.Value ? reader["BillingAddress"].ToString() : null,
                                    Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                                    CreatedByUserID = reader["CreatedByUserID"] != DBNull.Value ? (int?)reader["CreatedByUserID"] : null,
                                    ApprovedByUserID = reader["ApprovedByUserID"] != DBNull.Value ? (int?)reader["ApprovedByUserID"] : null
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetPurchaseOrderByIdAsync: {ex.Message}");
            }
            return po;
        }

        public async Task<bool> SavePurchaseOrderAsync(PurchaseOrder po)
        {
            int rowsAffected = 0;
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    SqlCommand command;
                    if (po.PurchaseOrderID == 0) // New PO
                    {
                        po.OrderDate = (po.OrderDate == DateTime.MinValue || po.OrderDate == default(DateTime)) ? DateTime.UtcNow : po.OrderDate;
                        po.CreatedByUserID = ApplicationState.CurrentUser?.UserID; // Changed to ApplicationState

                        string insertQuery = @"
                            INSERT INTO PurchaseOrders (ProjectID, PurchaseID, SupplierName, OrderDate, ExpectedDeliveryDate, ActualDeliveryDate, TotalAmount, Status, ShippingAddress, BillingAddress, Notes, CreatedByUserID, ApprovedByUserID)
                            VALUES (@ProjectID, @PurchaseID, @SupplierName, @OrderDate, @ExpectedDeliveryDate, @ActualDeliveryDate, @TotalAmount, @Status, @ShippingAddress, @BillingAddress, @Notes, @CreatedByUserID, @ApprovedByUserID);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                        command = new SqlCommand(insertQuery, connection);
                        command.Parameters.AddWithValue("@CreatedByUserID", (object)po.CreatedByUserID ?? DBNull.Value);
                    }
                    else // Existing PO
                    {
                        string updateQuery = @"
                            UPDATE PurchaseOrders SET
                                ProjectID = @ProjectID, PurchaseID = @PurchaseID, SupplierName = @SupplierName,
                                ExpectedDeliveryDate = @ExpectedDeliveryDate, ActualDeliveryDate = @ActualDeliveryDate,
                                TotalAmount = @TotalAmount, Status = @Status, ShippingAddress = @ShippingAddress,
                                BillingAddress = @BillingAddress, Notes = @Notes, ApprovedByUserID = @ApprovedByUserID
                            WHERE PurchaseOrderID = @PurchaseOrderID;";
                        command = new SqlCommand(updateQuery, connection);
                        command.Parameters.AddWithValue("@PurchaseOrderID", po.PurchaseOrderID);
                    }

                    command.Parameters.AddWithValue("@ProjectID", po.ProjectID);
                    command.Parameters.AddWithValue("@PurchaseID", (object)po.PurchaseID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SupplierName", po.SupplierName);
                    if (po.PurchaseOrderID == 0) // Only add OrderDate for new POs
                    {
                        command.Parameters.AddWithValue("@OrderDate", po.OrderDate);
                    }
                    command.Parameters.AddWithValue("@ExpectedDeliveryDate", (object)po.ExpectedDeliveryDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ActualDeliveryDate", (object)po.ActualDeliveryDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount", po.TotalAmount);
                    command.Parameters.AddWithValue("@Status", (object)po.Status ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ShippingAddress", (object)po.ShippingAddress ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BillingAddress", (object)po.BillingAddress ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)po.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ApprovedByUserID", (object)po.ApprovedByUserID ?? DBNull.Value);

                    await connection.OpenAsync();
                    if (po.PurchaseOrderID == 0)
                    {
                        object newId = await command.ExecuteScalarAsync();
                        if (newId != null && newId != DBNull.Value)
                        {
                            po.PurchaseOrderID = Convert.ToInt32(newId);
                            rowsAffected = 1;
                        }
                    }
                    else
                    {
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SavePurchaseOrderAsync: {ex.Message}");
                return false;
            }
            return rowsAffected > 0;
        }

        public async Task<bool> DeletePurchaseOrderAsync(int purchaseOrderId)
        {
            string sql = "DELETE FROM PurchaseOrders WHERE PurchaseOrderID = @PurchaseOrderID;";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderId);
                    try
                    {
                        await conn.OpenAsync();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"SQL Error in DeletePurchaseOrderAsync: {ex.Message}");
                        return false;
                    }
                }
            }
        }
    }
}


namespace HumanitarianProjectManagement.Services
{
    public class PurchaseOrderServiceAdo
    {
        public async Task<List<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            List<PurchaseOrder> orders = new List<PurchaseOrder>();
            string query = @"SELECT POID, PRID, OrderDate, SupplierID, IssuedByUserID, DeliveryDate, ShippingAddress, BillingAddress, PaymentTerms, TotalAmount, Status, CreatedAt, UpdatedAt
                             FROM PurchaseOrders;";

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
                                PurchaseOrder po = MapPurchaseOrderFromReader(reader);
                                po.Items = await GetPurchaseOrderItemsAsync(po.POID, connection); // Pass connection
                                orders.Add(po);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllPurchaseOrdersAsync: {ex.Message}");
            }
            return orders;
        }

        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int id)
        {
            PurchaseOrder po = null;
            string query = @"SELECT POID, PRID, OrderDate, SupplierID, IssuedByUserID, DeliveryDate, ShippingAddress, BillingAddress, PaymentTerms, TotalAmount, Status, CreatedAt, UpdatedAt
                             FROM PurchaseOrders WHERE POID = @POID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@POID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                po = MapPurchaseOrderFromReader(reader);
                                po.Items = await GetPurchaseOrderItemsAsync(po.POID, connection); // Pass connection
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetPurchaseOrderByIdAsync: {ex.Message}");
            }
            return po;
        }

        public async Task AddPurchaseOrderAsync(PurchaseOrder po)
        {
            string query = @"INSERT INTO PurchaseOrders (PRID, OrderDate, SupplierID, IssuedByUserID, DeliveryDate, ShippingAddress, BillingAddress, PaymentTerms, TotalAmount, Status, CreatedAt, UpdatedAt)
                             VALUES (@PRID, @OrderDate, @SupplierID, @IssuedByUserID, @DeliveryDate, @ShippingAddress, @BillingAddress, @PaymentTerms, @TotalAmount, @Status, @CreatedAt, @UpdatedAt);
                             SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddPurchaseOrderParameters(command, po);
                        po.POID = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    foreach (var item in po.Items)
                    {
                        item.POID = po.POID;
                        await AddPurchaseOrderItemAsync(item, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in AddPurchaseOrderAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task UpdatePurchaseOrderAsync(PurchaseOrder po)
        {
            string query = @"UPDATE PurchaseOrders SET PRID = @PRID, OrderDate = @OrderDate, SupplierID = @SupplierID, IssuedByUserID = @IssuedByUserID,
                             DeliveryDate = @DeliveryDate, ShippingAddress = @ShippingAddress, BillingAddress = @BillingAddress, PaymentTerms = @PaymentTerms,
                             TotalAmount = @TotalAmount, Status = @Status, UpdatedAt = @UpdatedAt WHERE POID = @POID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddPurchaseOrderParameters(command, po);
                        command.Parameters.AddWithValue("@POID", po.POID);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Delete existing items and re-add new ones
                    string deleteItemsQuery = "DELETE FROM PurchaseOrderItems WHERE POID = @POID;";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteItemsQuery, connection, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@POID", po.POID);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    foreach (var item in po.Items)
                    {
                        item.POID = po.POID;
                        await AddPurchaseOrderItemAsync(item, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in UpdatePurchaseOrderAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task DeletePurchaseOrderAsync(int id)
        {
            string query = "DELETE FROM PurchaseOrders WHERE POID = @POID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Delete associated items first
                    string deleteItemsQuery = "DELETE FROM PurchaseOrderItems WHERE POID = @POID;";
                    using (SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, connection, transaction))
                    {
                        deleteItemsCmd.Parameters.AddWithValue("@POID", id);
                        await deleteItemsCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@POID", id);
                        await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in DeletePurchaseOrderAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task<List<PurchaseOrder>> SearchPurchaseOrdersAsync(string searchTerm)
        {
            List<PurchaseOrder> orders = new List<PurchaseOrder>();
            string query = @"SELECT POID, PRID, OrderDate, SupplierID, IssuedByUserID, DeliveryDate, ShippingAddress, BillingAddress, PaymentTerms, TotalAmount, Status, CreatedAt, UpdatedAt
                             FROM PurchaseOrders ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE Status LIKE @SearchTerm;";
                // Add search by supplier name, user name, and product name if needed, will require JOINs
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
                                PurchaseOrder po = MapPurchaseOrderFromReader(reader);
                                po.Items = await GetPurchaseOrderItemsAsync(po.POID, connection); // Pass connection
                                orders.Add(po);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchPurchaseOrdersAsync: {ex.Message}");
            }
            return orders;
        }

        private PurchaseOrder MapPurchaseOrderFromReader(SqlDataReader reader)
        {
            return new PurchaseOrder
            {
                POID = (int)reader["POID"],
                PRID = reader["PRID"] != DBNull.Value ? (int?)reader["PRID"] : null,
                OrderDate = (DateTime)reader["OrderDate"],
                SupplierID = (int)reader["SupplierID"],
                IssuedByUserID = (int)reader["IssuedByUserID"],
                DeliveryDate = reader["DeliveryDate"] != DBNull.Value ? (DateTime?)reader["DeliveryDate"] : null,
                ShippingAddress = reader["ShippingAddress"] != DBNull.Value ? reader["ShippingAddress"].ToString() : null,
                BillingAddress = reader["BillingAddress"] != DBNull.Value ? reader["BillingAddress"].ToString() : null,
                PaymentTerms = reader["PaymentTerms"] != DBNull.Value ? reader["PaymentTerms"].ToString() : null,
                TotalAmount = (decimal)reader["TotalAmount"],
                Status = reader["Status"].ToString(),
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        private void AddPurchaseOrderParameters(SqlCommand command, PurchaseOrder po)
        {
            command.Parameters.AddWithValue("@PRID", (object)po.PRID ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDate", po.OrderDate);
            command.Parameters.AddWithValue("@SupplierID", po.SupplierID);
            command.Parameters.AddWithValue("@IssuedByUserID", po.IssuedByUserID);
            command.Parameters.AddWithValue("@DeliveryDate", (object)po.DeliveryDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@ShippingAddress", (object)po.ShippingAddress ?? DBNull.Value);
            command.Parameters.AddWithValue("@BillingAddress", (object)po.BillingAddress ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaymentTerms", (object)po.PaymentTerms ?? DBNull.Value);
            command.Parameters.AddWithValue("@TotalAmount", po.TotalAmount);
            command.Parameters.AddWithValue("@Status", po.Status);
            command.Parameters.AddWithValue("@CreatedAt", po.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", po.UpdatedAt);
        }

        private async Task<ICollection<PurchaseOrderItem>> GetPurchaseOrderItemsAsync(int poId, SqlConnection connection)
        {
            List<PurchaseOrderItem> items = new List<PurchaseOrderItem>();
            string query = @"SELECT POItemID, POID, ProductID, Quantity, UnitPrice, TotalPrice, Notes
                             FROM PurchaseOrderItems WHERE POID = @POID;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@POID", poId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new PurchaseOrderItem
                        {
                            POItemID = (int)reader["POItemID"],
                            POID = (int)reader["POID"],
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

        private async Task AddPurchaseOrderItemAsync(PurchaseOrderItem item, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"INSERT INTO PurchaseOrderItems (POID, ProductID, Quantity, UnitPrice, TotalPrice, Notes)
                             VALUES (@POID, @ProductID, @Quantity, @UnitPrice, @TotalPrice, @Notes);";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@POID", item.POID);
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
