using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace HumanitarianProjectManagement.DataAccessLayer
{
    public class ProjectService
    {
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            List<Project> projects = new List<Project>();
            string query = "SELECT ProjectID, ProjectName, ProjectCode, SectionID, StartDate, EndDate, Location, OverallObjective, ManagerUserID, Status, TotalBudget, Donor, CreatedAt, UpdatedAt FROM Projects";

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
                                Project project = new Project
                                {
                                    ProjectID = (int)reader["ProjectID"],
                                    ProjectName = reader["ProjectName"].ToString(),
                                    ProjectCode = reader["ProjectCode"] != DBNull.Value ? reader["ProjectCode"].ToString() : null,
                                    SectionID = reader["SectionID"] != DBNull.Value ? (int?)reader["SectionID"] : null,
                                    StartDate = reader["StartDate"] != DBNull.Value ? (DateTime?)reader["StartDate"] : null,
                                    EndDate = reader["EndDate"] != DBNull.Value ? (DateTime?)reader["EndDate"] : null,
                                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : null,
                                    OverallObjective = reader["OverallObjective"] != DBNull.Value ? reader["OverallObjective"].ToString() : null,
                                    ManagerUserID = reader["ManagerUserID"] != DBNull.Value ? (int?)reader["ManagerUserID"] : null,
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    TotalBudget = reader["TotalBudget"] != DBNull.Value ? (decimal?)reader["TotalBudget"] : null,
                                    Donor = reader["Donor"] != DBNull.Value ? reader["Donor"].ToString() : null,
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)reader["UpdatedAt"] : null
                                };
                                projects.Add(project);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetAllProjectsAsync: {ex.Message}");
                // Return empty list or throw
            }
            return projects;
        }

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            Project project = null;
            string query = "SELECT ProjectID, ProjectName, ProjectCode, SectionID, StartDate, EndDate, Location, OverallObjective, ManagerUserID, Status, TotalBudget, Donor, CreatedAt, UpdatedAt FROM Projects WHERE ProjectID = @ProjectID";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectID", projectId);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                project = new Project
                                {
                                    ProjectID = (int)reader["ProjectID"],
                                    ProjectName = reader["ProjectName"].ToString(),
                                    ProjectCode = reader["ProjectCode"] != DBNull.Value ? reader["ProjectCode"].ToString() : null,
                                    SectionID = reader["SectionID"] != DBNull.Value ? (int?)reader["SectionID"] : null,
                                    StartDate = reader["StartDate"] != DBNull.Value ? (DateTime?)reader["StartDate"] : null,
                                    EndDate = reader["EndDate"] != DBNull.Value ? (DateTime?)reader["EndDate"] : null,
                                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : null,
                                    OverallObjective = reader["OverallObjective"] != DBNull.Value ? reader["OverallObjective"].ToString() : null,
                                    ManagerUserID = reader["ManagerUserID"] != DBNull.Value ? (int?)reader["ManagerUserID"] : null,
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    TotalBudget = reader["TotalBudget"] != DBNull.Value ? (decimal?)reader["TotalBudget"] : null,
                                    Donor = reader["Donor"] != DBNull.Value ? reader["Donor"].ToString() : null,
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)reader["UpdatedAt"] : null
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetProjectByIdAsync: {ex.Message}");
            }
            return project;
        }

        public async Task<List<Project>> GetProjectsBySectionIdAsync(int sectionId)
        {
            List<Project> projects = new List<Project>();
            string query = "SELECT ProjectID, ProjectName, ProjectCode, SectionID, StartDate, EndDate, Location, OverallObjective, ManagerUserID, Status, TotalBudget, Donor, CreatedAt, UpdatedAt FROM Projects WHERE SectionID = @SectionID";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SectionID", sectionId);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Project project = new Project
                                {
                                    ProjectID = (int)reader["ProjectID"],
                                    ProjectName = reader["ProjectName"].ToString(),
                                    ProjectCode = reader["ProjectCode"] != DBNull.Value ? reader["ProjectCode"].ToString() : null,
                                    SectionID = reader["SectionID"] != DBNull.Value ? (int?)reader["SectionID"] : null,
                                    StartDate = reader["StartDate"] != DBNull.Value ? (DateTime?)reader["StartDate"] : null,
                                    EndDate = reader["EndDate"] != DBNull.Value ? (DateTime?)reader["EndDate"] : null,
                                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : null,
                                    OverallObjective = reader["OverallObjective"] != DBNull.Value ? reader["OverallObjective"].ToString() : null,
                                    ManagerUserID = reader["ManagerUserID"] != DBNull.Value ? (int?)reader["ManagerUserID"] : null,
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    TotalBudget = reader["TotalBudget"] != DBNull.Value ? (decimal?)reader["TotalBudget"] : null,
                                    Donor = reader["Donor"] != DBNull.Value ? reader["Donor"].ToString() : null,
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)reader["UpdatedAt"] : null
                                };
                                projects.Add(project);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetProjectsBySectionIdAsync: {ex.Message}");
                // Return empty list or re-throw based on error handling policy
            }
            return projects;
        }

        public async Task<bool> SaveProjectAsync(Project project)
        {
            int rowsAffected = 0;
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    SqlCommand command;
                    if (project.ProjectID == 0) // New project
                    {
                        string insertQuery = @"
                            INSERT INTO Projects (ProjectName, ProjectCode, SectionID, StartDate, EndDate, Location, OverallObjective, ManagerUserID, Status, TotalBudget, Donor, CreatedAt, UpdatedAt)
                            VALUES (@ProjectName, @ProjectCode, @SectionID, @StartDate, @EndDate, @Location, @OverallObjective, @ManagerUserID, @Status, @TotalBudget, @Donor, @CreatedAt, @UpdatedAt);
                            SELECT CAST(SCOPE_IDENTITY() as int);"; // To get the new ProjectID
                        command = new SqlCommand(insertQuery, connection);
                        command.Parameters.AddWithValue("@CreatedAt", project.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : project.CreatedAt);
                    }
                    else // Existing project
                    {
                        string updateQuery = @"
                            UPDATE Projects SET 
                                ProjectName = @ProjectName, ProjectCode = @ProjectCode, SectionID = @SectionID, StartDate = @StartDate, EndDate = @EndDate, 
                                Location = @Location, OverallObjective = @OverallObjective, ManagerUserID = @ManagerUserID, Status = @Status, 
                                TotalBudget = @TotalBudget, Donor = @Donor, UpdatedAt = @UpdatedAt
                            WHERE ProjectID = @ProjectID";
                        command = new SqlCommand(updateQuery, connection);
                        command.Parameters.AddWithValue("@ProjectID", project.ProjectID);
                    }

                    command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                    command.Parameters.AddWithValue("@ProjectCode", (object)project.ProjectCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SectionID", (object)project.SectionID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartDate", (object)project.StartDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", (object)project.EndDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Location", (object)project.Location ?? DBNull.Value);
                    command.Parameters.AddWithValue("@OverallObjective", (object)project.OverallObjective ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ManagerUserID", (object)project.ManagerUserID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (object)project.Status ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalBudget", (object)project.TotalBudget ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Donor", (object)project.Donor ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow); // Always set/update UpdatedAt

                    await connection.OpenAsync();
                    if (project.ProjectID == 0)
                    {
                        // For INSERT, execute scalar to get new ID and assign back to object
                        object newId = await command.ExecuteScalarAsync();
                        if (newId != null && newId != DBNull.Value)
                        {
                            project.ProjectID = Convert.ToInt32(newId);
                            rowsAffected = 1; // SCOPE_IDENTITY() returning a value means 1 row was inserted.
                        }
                        else
                        {
                            rowsAffected = 0;
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
                Console.WriteLine($"SQL Error in SaveProjectAsync: {ex.Message}");
                return false;
            }
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            string sql = "DELETE FROM Projects WHERE ProjectID = @ProjectID";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProjectID", projectId);
                    try
                    {
                        await conn.OpenAsync();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                    catch (SqlException ex)
                    {
                        // TODO: Log exception (ex.ToString())
                        Console.WriteLine($"Error deleting project: {ex.Message}"); // Basic error output
                        return false;
                    }
                }
            }
        }
    }
}
