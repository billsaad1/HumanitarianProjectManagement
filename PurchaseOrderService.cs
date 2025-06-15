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
    public class PurchaseOrderService
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
