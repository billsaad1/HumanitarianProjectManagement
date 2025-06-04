using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace HumanitarianProjectManagement.DataAccessLayer // Corrected namespace
{
    public class SectionService
    {
        public async Task<List<Section>> GetSectionsAsync()
        {
            List<Section> sections = new List<Section>();
            string query = "SELECT SectionID, SectionName, Description FROM Sections";
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
                                Section section = new Section
                                {
                                    SectionID = reader.GetInt32(reader.GetOrdinal("SectionID")),
                                    SectionName = reader.GetString(reader.GetOrdinal("SectionName")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                                };
                                sections.Add(section);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetSectionsAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in GetSectionsAsync: {ex.Message}");
            }
            return sections;
        }

        public async Task<int> AddSectionAsync(Section section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            if (string.IsNullOrWhiteSpace(section.SectionName))
                throw new ArgumentException("SectionName cannot be empty or whitespace.", nameof(section.SectionName));

            string query = @"
                INSERT INTO Sections (SectionName, Description)
                VALUES (@SectionName, @Description);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SectionName", section.SectionName);
                        command.Parameters.AddWithValue("@Description", (object)section.Description ?? DBNull.Value);

                        await connection.OpenAsync();
                        object newId = await command.ExecuteScalarAsync();
                        if (newId != null && newId != DBNull.Value)
                        {
                            return Convert.ToInt32(newId);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in AddSectionAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in AddSectionAsync: {ex.Message}");
            }
            return 0; // Return 0 to indicate failure
        }
    }
}
