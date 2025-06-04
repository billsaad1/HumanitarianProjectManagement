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

private async Task<List<int>> GetBudgetLineIDsForProjectAsync(SqlConnection connection, SqlTransaction transaction, int projectID)
{
    var ids = new List<int>();
    string query = "SELECT BudgetLineID FROM DetailedBudgetLines WHERE ProjectID = @ProjectID;";
    using (var command = new SqlCommand(query, connection, transaction))
    {
        command.Parameters.AddWithValue("@ProjectID", projectID);
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetInt32(0));
            }
        } // Reader is automatically closed here
    }
    return ids;
}

private async Task AddDetailedBudgetLineAsync(SqlConnection connection, SqlTransaction transaction, DetailedBudgetLine budgetLine)
{
    string query = @"INSERT INTO DetailedBudgetLines
                     (ProjectID, Category, Remarks, Unit, Quantity, UnitCost, Duration, PercentageChargedToCBPF)
                     VALUES
                     (@ProjectID, @Category, @Remarks, @Unit, @Quantity, @UnitCost, @Duration, @PercentageChargedToCBPF);
                     SELECT CAST(SCOPE_IDENTITY() as int);"; // Get new ID
    using (var command = new SqlCommand(query, connection, transaction))
    {
        command.Parameters.AddWithValue("@ProjectID", budgetLine.ProjectID);
        command.Parameters.AddWithValue("@Category", (int)budgetLine.Category); // Store enum as int
        command.Parameters.AddWithValue("@Remarks", budgetLine.Remarks);
        command.Parameters.AddWithValue("@Unit", (object)budgetLine.Unit ?? DBNull.Value);
        command.Parameters.AddWithValue("@Quantity", budgetLine.Quantity);
        command.Parameters.AddWithValue("@UnitCost", budgetLine.UnitCost);
        command.Parameters.AddWithValue("@Duration", budgetLine.Duration);
        command.Parameters.AddWithValue("@PercentageChargedToCBPF", budgetLine.PercentageChargedToCBPF);

        object newId = await command.ExecuteScalarAsync();
        if (newId != null && newId != DBNull.Value)
        {
            budgetLine.BudgetLineID = Convert.ToInt32(newId);
        }
    }
}

private async Task UpdateDetailedBudgetLineAsync(SqlConnection connection, SqlTransaction transaction, DetailedBudgetLine budgetLine)
{
    string query = @"UPDATE DetailedBudgetLines SET
                     Category = @Category, Remarks = @Remarks, Unit = @Unit, Quantity = @Quantity,
                     UnitCost = @UnitCost, Duration = @Duration, PercentageChargedToCBPF = @PercentageChargedToCBPF
                     WHERE BudgetLineID = @BudgetLineID AND ProjectID = @ProjectID;";
    using (var command = new SqlCommand(query, connection, transaction))
    {
        command.Parameters.AddWithValue("@BudgetLineID", budgetLine.BudgetLineID);
        command.Parameters.AddWithValue("@ProjectID", budgetLine.ProjectID); // Important for where clause
        command.Parameters.AddWithValue("@Category", (int)budgetLine.Category);
        command.Parameters.AddWithValue("@Remarks", budgetLine.Remarks);
        command.Parameters.AddWithValue("@Unit", (object)budgetLine.Unit ?? DBNull.Value);
        command.Parameters.AddWithValue("@Quantity", budgetLine.Quantity);
        command.Parameters.AddWithValue("@UnitCost", budgetLine.UnitCost);
        command.Parameters.AddWithValue("@Duration", budgetLine.Duration);
        command.Parameters.AddWithValue("@PercentageChargedToCBPF", budgetLine.PercentageChargedToCBPF);
        await command.ExecuteNonQueryAsync();
    }
}

private async Task DeleteDetailedBudgetLineAsync(SqlConnection connection, SqlTransaction transaction, int budgetLineID)
{
    string query = "DELETE FROM DetailedBudgetLines WHERE BudgetLineID = @BudgetLineID;";
    using (var command = new SqlCommand(query, connection, transaction))
    {
        command.Parameters.AddWithValue("@BudgetLineID", budgetLineID);
        await command.ExecuteNonQueryAsync();
    }
}

        public async Task<bool> SaveProjectAsync(Project project)
        {
            if (project == null) return false;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command;
                        if (project.ProjectID == 0) // New project
                        {
                            string insertQuery = @"
                                INSERT INTO Projects (ProjectName, ProjectCode, StartDate, EndDate, Location, OverallObjective, Status, Donor, TotalBudget, CreatedAt, UpdatedAt, SectionID, ManagerUserID)
                                VALUES (@ProjectName, @ProjectCode, @StartDate, @EndDate, @Location, @OverallObjective, @Status, @Donor, @TotalBudget, @CreatedAt, @UpdatedAt, @SectionID, @ManagerUserID);
                                SELECT CAST(SCOPE_IDENTITY() as int);";
                            command = new SqlCommand(insertQuery, connection, transaction);
                        }
                        else // Existing project
                        {
                            string updateQuery = @"
                                UPDATE Projects SET
                                    ProjectName = @ProjectName, ProjectCode = @ProjectCode, StartDate = @StartDate, EndDate = @EndDate, Location = @Location,
                                    OverallObjective = @OverallObjective, Status = @Status, Donor = @Donor, TotalBudget = @TotalBudget, UpdatedAt = @UpdatedAt,
                                    SectionID = @SectionID, ManagerUserID = @ManagerUserID
                                WHERE ProjectID = @ProjectID;";
                            command = new SqlCommand(updateQuery, connection, transaction);
                            command.Parameters.AddWithValue("@ProjectID", project.ProjectID);
                        }

                        command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                        command.Parameters.AddWithValue("@ProjectCode", (object)project.ProjectCode ?? DBNull.Value);
                        command.Parameters.AddWithValue("@StartDate", (object)project.StartDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EndDate", (object)project.EndDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Location", (object)project.Location ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OverallObjective", (object)project.OverallObjective ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Status", (object)project.Status ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Donor", (object)project.Donor ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TotalBudget", (object)project.TotalBudget ?? DBNull.Value);
                        project.UpdatedAt = DateTime.UtcNow;
                        command.Parameters.AddWithValue("@UpdatedAt", project.UpdatedAt);
                        command.Parameters.AddWithValue("@SectionID", (object)project.SectionID ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ManagerUserID", (object)project.ManagerUserID ?? DBNull.Value);

                        if (project.ProjectID == 0)
                        {
                            project.CreatedAt = project.UpdatedAt;
                            command.Parameters.AddWithValue("@CreatedAt", project.CreatedAt);
                            object newId = await command.ExecuteScalarAsync();
                            if (newId != null && newId != DBNull.Value) project.ProjectID = Convert.ToInt32(newId);
                            else throw new Exception("Failed to create project or retrieve new ProjectID.");
                        }
                        else
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        // === BUDGET LINE PROCESSING ===
                        if (project.ProjectID > 0 && project.DetailedBudgetLines != null)
                        {
                            List<int> existingDbBudgetLineIDs = await GetBudgetLineIDsForProjectAsync(connection, transaction, project.ProjectID);
                            List<int> processedBudgetLineIDs = new List<int>();

                            foreach (var line in project.DetailedBudgetLines)
                            {
                                line.ProjectID = project.ProjectID;
                                if (line.BudgetLineID == 0)
                                {
                                    await AddDetailedBudgetLineAsync(connection, transaction, line);
                                }
                                else
                                {
                                    await UpdateDetailedBudgetLineAsync(connection, transaction, line);
                                }
                                processedBudgetLineIDs.Add(line.BudgetLineID);
                            }

                            List<int> linesToDelete = existingDbBudgetLineIDs.Except(processedBudgetLineIDs).ToList();
                            foreach (int idToDelete in linesToDelete)
                            {
                                await DeleteDetailedBudgetLineAsync(connection, transaction, idToDelete);
                            }
                        }
                        // === END BUDGET LINE PROCESSING ===

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving project or budget lines: {ex.Message}");
                        try { transaction.Rollback(); } catch { /* ignore rollback error */ }
                        return false;
                    }
                } // Connection automatically closed here by using
            }
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
