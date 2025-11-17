using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.Utilities; // Assuming DatabaseHelper is here

namespace HumanitarianProjectManagement.Services
{
    public class InvoiceServiceAdo
    {
        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            List<Invoice> invoices = new List<Invoice>();
            string query = @"SELECT InvoiceID, POID, SupplierID, InvoiceNumber, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmountDue, Status, CreatedAt, UpdatedAt 
                             FROM Invoices;";

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
                                Invoice invoice = MapInvoiceFromReader(reader);
                                invoice.Payments = await GetInvoicePaymentsAsync(invoice.InvoiceID, connection); // Pass connection
                                invoices.Add(invoice);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllInvoicesAsync: {ex.Message}");
            }
            return invoices;
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            Invoice invoice = null;
            string query = @"SELECT InvoiceID, POID, SupplierID, InvoiceNumber, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmountDue, Status, CreatedAt, UpdatedAt 
                             FROM Invoices WHERE InvoiceID = @InvoiceID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@InvoiceID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                invoice = MapInvoiceFromReader(reader);
                                invoice.Payments = await GetInvoicePaymentsAsync(invoice.InvoiceID, connection); // Pass connection
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetInvoiceByIdAsync: {ex.Message}");
            }
            return invoice;
        }

        public async Task AddInvoiceAsync(Invoice invoice)
        {
            string query = @"INSERT INTO Invoices (POID, SupplierID, InvoiceNumber, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmountDue, Status, CreatedAt, UpdatedAt) 
                             VALUES (@POID, @SupplierID, @InvoiceNumber, @InvoiceDate, @DueDate, @Amount, @TaxAmount, @TotalAmountDue, @Status, @CreatedAt, @UpdatedAt);
                             SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddInvoiceParameters(command, invoice);
                        invoice.InvoiceID = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    foreach (var payment in invoice.Payments)
                    {
                        payment.InvoiceID = invoice.InvoiceID;
                        await AddPaymentAsync(payment, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in AddInvoiceAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            string query = @"UPDATE Invoices SET POID = @POID, SupplierID = @SupplierID, InvoiceNumber = @InvoiceNumber, InvoiceDate = @InvoiceDate, 
                             DueDate = @DueDate, Amount = @Amount, TaxAmount = @TaxAmount, TotalAmountDue = @TotalAmountDue, Status = @Status, 
                             UpdatedAt = @UpdatedAt WHERE InvoiceID = @InvoiceID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        AddInvoiceParameters(command, invoice);
                        command.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Delete existing payments and re-add new ones
                    string deletePaymentsQuery = "DELETE FROM Payments WHERE InvoiceID = @InvoiceID;";
                    using (SqlCommand deleteCmd = new SqlCommand(deletePaymentsQuery, connection, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    foreach (var payment in invoice.Payments)
                    {
                        payment.InvoiceID = invoice.InvoiceID;
                        await AddPaymentAsync(payment, connection, transaction);
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in UpdateInvoiceAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            string query = "DELETE FROM Invoices WHERE InvoiceID = @InvoiceID;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Delete associated payments first
                    string deletePaymentsQuery = "DELETE FROM Payments WHERE InvoiceID = @InvoiceID;";
                    using (SqlCommand deletePaymentsCmd = new SqlCommand(deletePaymentsQuery, connection, transaction))
                    {
                        deletePaymentsCmd.Parameters.AddWithValue("@InvoiceID", id);
                        await deletePaymentsCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@InvoiceID", id);
                        await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"SQL Error in DeleteInvoiceAsync: {ex.Message}");
                    throw; // Re-throw to indicate failure
                }
            }
        }

        public async Task<List<Invoice>> SearchInvoicesAsync(string searchTerm)
        {
            List<Invoice> invoices = new List<Invoice>();
            string query = @"SELECT InvoiceID, POID, SupplierID, InvoiceNumber, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmountDue, Status, CreatedAt, UpdatedAt 
                             FROM Invoices ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE InvoiceNumber LIKE @SearchTerm OR Status LIKE @SearchTerm;";
                // Add search by supplier name if needed, will require JOINs
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
                                Invoice invoice = MapInvoiceFromReader(reader);
                                invoice.Payments = await GetInvoicePaymentsAsync(invoice.InvoiceID, connection); // Pass connection
                                invoices.Add(invoice);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchInvoicesAsync: {ex.Message}");
            }
            return invoices;
        }

        private Invoice MapInvoiceFromReader(SqlDataReader reader)
        {
            return new Invoice
            {
                InvoiceID = (int)reader["InvoiceID"],
                POID = (int)reader["POID"],
                SupplierID = (int)reader["SupplierID"],
                InvoiceNumber = reader["InvoiceNumber"].ToString(),
                InvoiceDate = (DateTime)reader["InvoiceDate"],
                DueDate = reader["DueDate"] != DBNull.Value ? (DateTime?)reader["DueDate"] : null,
                Amount = (decimal)reader["Amount"],
                TaxAmount = (decimal)reader["TaxAmount"],
                TotalAmountDue = (decimal)reader["TotalAmountDue"],
                Status = reader["Status"].ToString(),
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        private void AddInvoiceParameters(SqlCommand command, Invoice invoice)
        {
            command.Parameters.AddWithValue("@POID", invoice.POID);
            command.Parameters.AddWithValue("@SupplierID", invoice.SupplierID);
            command.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
            command.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
            command.Parameters.AddWithValue("@DueDate", (object)invoice.DueDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Amount", invoice.Amount);
            command.Parameters.AddWithValue("@TaxAmount", invoice.TaxAmount);
            command.Parameters.AddWithValue("@TotalAmountDue", invoice.TotalAmountDue);
            command.Parameters.AddWithValue("@Status", invoice.Status);
            command.Parameters.AddWithValue("@CreatedAt", invoice.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", invoice.UpdatedAt);
        }

        private async Task<ICollection<Payment>> GetInvoicePaymentsAsync(int invoiceId, SqlConnection connection)
        {
            List<Payment> payments = new List<Payment>();
            string query = @"SELECT PaymentID, InvoiceID, PaymentDate, AmountPaid, PaymentMethod, ReferenceNumber, PaidByUserID, CreatedAt 
                             FROM Payments WHERE InvoiceID = @InvoiceID;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        payments.Add(new Payment
                        {
                            PaymentID = (int)reader["PaymentID"],
                            InvoiceID = (int)reader["InvoiceID"],
                            PaymentDate = (DateTime)reader["PaymentDate"],
                            AmountPaid = (decimal)reader["AmountPaid"],
                            PaymentMethod = reader["PaymentMethod"] != DBNull.Value ? reader["PaymentMethod"].ToString() : null,
                            ReferenceNumber = reader["ReferenceNumber"] != DBNull.Value ? reader["ReferenceNumber"].ToString() : null,
                            PaidByUserID = reader["PaidByUserID"] != DBNull.Value ? (int?)reader["PaidByUserID"] : null,
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
                }
            }
            return payments;
        }

        private async Task AddPaymentAsync(Payment payment, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"INSERT INTO Payments (InvoiceID, PaymentDate, AmountPaid, PaymentMethod, ReferenceNumber, PaidByUserID, CreatedAt) 
                             VALUES (@InvoiceID, @PaymentDate, @AmountPaid, @PaymentMethod, @ReferenceNumber, @PaidByUserID, @CreatedAt);";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@InvoiceID", payment.InvoiceID);
                command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                command.Parameters.AddWithValue("@AmountPaid", payment.AmountPaid);
                command.Parameters.AddWithValue("@PaymentMethod", (object)payment.PaymentMethod ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReferenceNumber", (object)payment.ReferenceNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@PaidByUserID", (object)payment.PaidByUserID ?? DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", payment.CreatedAt);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
