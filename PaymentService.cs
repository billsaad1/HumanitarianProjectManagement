using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.Utilities; // Assuming DatabaseHelper is here

namespace HumanitarianProjectManagement.Services
{
    public class PaymentService
    {
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            List<Payment> payments = new List<Payment>();
            string query = @"SELECT PaymentID, InvoiceID, PaymentDate, AmountPaid, PaymentMethod, ReferenceNumber, PaidByUserID, CreatedAt 
                             FROM Payments;";

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
                                payments.Add(MapPaymentFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllPaymentsAsync: {ex.Message}");
            }
            return payments;
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            Payment payment = null;
            string query = @"SELECT PaymentID, InvoiceID, PaymentDate, AmountPaid, PaymentMethod, ReferenceNumber, PaidByUserID, CreatedAt 
                             FROM Payments WHERE PaymentID = @PaymentID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentID", id);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                payment = MapPaymentFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetPaymentByIdAsync: {ex.Message}");
            }
            return payment;
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            string query = @"INSERT INTO Payments (InvoiceID, PaymentDate, AmountPaid, PaymentMethod, ReferenceNumber, PaidByUserID, CreatedAt) 
                             VALUES (@InvoiceID, @PaymentDate, @AmountPaid, @PaymentMethod, @ReferenceNumber, @PaidByUserID, @CreatedAt);";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddPaymentParameters(command, payment);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in AddPaymentAsync: {ex.Message}");
            }
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            string query = @"UPDATE Payments SET InvoiceID = @InvoiceID, PaymentDate = @PaymentDate, AmountPaid = @AmountPaid, 
                             PaymentMethod = @PaymentMethod, ReferenceNumber = @ReferenceNumber, PaidByUserID = @PaidByUserID, 
                             CreatedAt = @CreatedAt WHERE PaymentID = @PaymentID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddPaymentParameters(command, payment);
                        command.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in UpdatePaymentAsync: {ex.Message}");
            }
        }

        public async Task DeletePaymentAsync(int id)
        {
            string query = "DELETE FROM Payments WHERE PaymentID = @PaymentID;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentID", id);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in DeletePaymentAsync: {ex.Message}");
            }
        }

        public async Task<List<Payment>> SearchPaymentsAsync(string searchTerm)
        {
            List<Payment> payments = new List<Payment>();
            string query = @"SELECT PaymentID, InvoiceID, PaymentDate, AmountPaid, PaymentMethod, ReferenceNumber, PaidByUserID, CreatedAt 
                             FROM Payments ";
            string whereClause = "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause = "WHERE PaymentMethod LIKE @SearchTerm OR ReferenceNumber LIKE @SearchTerm;";
                // Add search by invoice number, paid by user if needed, will require JOINs
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
                                payments.Add(MapPaymentFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SearchPaymentsAsync: {ex.Message}");
            }
            return payments;
        }

        private Payment MapPaymentFromReader(SqlDataReader reader)
        {
            return new Payment
            {
                PaymentID = (int)reader["PaymentID"],
                InvoiceID = (int)reader["InvoiceID"],
                PaymentDate = (DateTime)reader["PaymentDate"],
                AmountPaid = (decimal)reader["AmountPaid"],
                PaymentMethod = reader["PaymentMethod"] != DBNull.Value ? reader["PaymentMethod"].ToString() : null,
                ReferenceNumber = reader["ReferenceNumber"] != DBNull.Value ? reader["ReferenceNumber"].ToString() : null,
                PaidByUserID = reader["PaidByUserID"] != DBNull.Value ? (int?)reader["PaidByUserID"] : null,
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }

        private void AddPaymentParameters(SqlCommand command, Payment payment)
        {
            command.Parameters.AddWithValue("@InvoiceID", payment.InvoiceID);
            command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
            command.Parameters.AddWithValue("@AmountPaid", payment.AmountPaid);
            command.Parameters.AddWithValue("@PaymentMethod", (object)payment.PaymentMethod ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReferenceNumber", (object)payment.ReferenceNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaidByUserID", (object)payment.PaidByUserID ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", payment.CreatedAt);
        }
    }
}


