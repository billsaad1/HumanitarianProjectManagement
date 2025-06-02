using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq; // For potential use with collections

namespace HumanitarianProjectManagement.DataAccessLayer
{
    public class ProjectService
    {
        private readonly LogFrameService _logFrameService; // Added LogFrameService field

        public ProjectService() // Constructor
        {
            _logFrameService = new LogFrameService(); // Initialize LogFrameService
        }

        #region DetailedBudgetLine CRUD
        public async Task<List<DetailedBudgetLine>> GetDetailedBudgetLinesByProjectIdAsync(int projectId)
        {
            List<DetailedBudgetLine> budgetLines = new List<DetailedBudgetLine>();
            string query = "SELECT DetailedBudgetLineID, ProjectID, BudgetCategory, Description, Unit, Quantity, UnitCost, Duration, PercentChargedToCBPF, TotalCost FROM DetailedBudgetLines WHERE ProjectID = @ProjectID";

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
                            while (await reader.ReadAsync())
                            {
                                budgetLines.Add(new DetailedBudgetLine
                                {
                                    DetailedBudgetLineID = (int)reader["DetailedBudgetLineID"],
                                    ProjectID = (int)reader["ProjectID"],
                                    BudgetCategory = (BudgetCategory)Enum.Parse(typeof(BudgetCategory), reader["BudgetCategory"].ToString()),
                                    Description = reader["Description"].ToString(),
                                    Unit = reader["Unit"] != DBNull.Value ? reader["Unit"].ToString() : null,
                                    Quantity = (int)reader["Quantity"],
                                    UnitCost = (decimal)reader["UnitCost"],
                                    Duration = (int)reader["Duration"],
                                    PercentChargedToCBPF = (decimal)reader["PercentChargedToCBPF"],
                                    TotalCost = (decimal)reader["TotalCost"]
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetDetailedBudgetLinesByProjectIdAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in GetDetailedBudgetLinesByProjectIdAsync: {ex.Message}");
            }
            return budgetLines;
        }

        public async Task<int> AddDetailedBudgetLineAsync(DetailedBudgetLine budgetLine)
        {
            if (budgetLine == null) throw new ArgumentNullException(nameof(budgetLine));

            string query = @"INSERT INTO DetailedBudgetLines
                                (ProjectID, BudgetCategory, Description, Unit, Quantity, UnitCost, Duration, PercentChargedToCBPF, TotalCost)
                             VALUES
                                (@ProjectID, @BudgetCategory, @Description, @Unit, @Quantity, @UnitCost, @Duration, @PercentChargedToCBPF, @TotalCost);
                             SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectID", budgetLine.ProjectID);
                        command.Parameters.AddWithValue("@BudgetCategory", budgetLine.BudgetCategory.ToString()); // Store enum as string or int
                        command.Parameters.AddWithValue("@Description", budgetLine.Description);
                        command.Parameters.AddWithValue("@Unit", (object)budgetLine.Unit ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", budgetLine.Quantity);
                        command.Parameters.AddWithValue("@UnitCost", budgetLine.UnitCost);
                        command.Parameters.AddWithValue("@Duration", budgetLine.Duration);
                        command.Parameters.AddWithValue("@PercentChargedToCBPF", budgetLine.PercentChargedToCBPF);
                        command.Parameters.AddWithValue("@TotalCost", budgetLine.TotalCost);

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
                Console.WriteLine($"SQL Error in AddDetailedBudgetLineAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in AddDetailedBudgetLineAsync: {ex.Message}");
            }
            return 0;
        }

        public async Task<bool> UpdateDetailedBudgetLineAsync(DetailedBudgetLine budgetLine)
        {
            if (budgetLine == null) throw new ArgumentNullException(nameof(budgetLine));

            string query = @"UPDATE DetailedBudgetLines SET
                                ProjectID = @ProjectID,
                                BudgetCategory = @BudgetCategory,
                                Description = @Description,
                                Unit = @Unit,
                                Quantity = @Quantity,
                                UnitCost = @UnitCost,
                                Duration = @Duration,
                                PercentChargedToCBPF = @PercentChargedToCBPF,
                                TotalCost = @TotalCost
                             WHERE DetailedBudgetLineID = @DetailedBudgetLineID;";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectID", budgetLine.ProjectID);
                        command.Parameters.AddWithValue("@BudgetCategory", budgetLine.BudgetCategory.ToString());
                        command.Parameters.AddWithValue("@Description", budgetLine.Description);
                        command.Parameters.AddWithValue("@Unit", (object)budgetLine.Unit ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", budgetLine.Quantity);
                        command.Parameters.AddWithValue("@UnitCost", budgetLine.UnitCost);
                        command.Parameters.AddWithValue("@Duration", budgetLine.Duration);
                        command.Parameters.AddWithValue("@PercentChargedToCBPF", budgetLine.PercentChargedToCBPF);
                        command.Parameters.AddWithValue("@TotalCost", budgetLine.TotalCost);
                        command.Parameters.AddWithValue("@DetailedBudgetLineID", budgetLine.DetailedBudgetLineID);

                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in UpdateDetailedBudgetLineAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in UpdateDetailedBudgetLineAsync: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DeleteDetailedBudgetLineAsync(int detailedBudgetLineId)
        {
            string query = "DELETE FROM DetailedBudgetLines WHERE DetailedBudgetLineID = @DetailedBudgetLineID;";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DetailedBudgetLineID", detailedBudgetLineId);
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in DeleteDetailedBudgetLineAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in DeleteDetailedBudgetLineAsync: {ex.Message}");
            }
            return false;
        }
        #endregion

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

                                // Load related entities
                                project.Outcomes = await _logFrameService.GetOutcomesByProjectIdAsync(projectId);
                                foreach (var outcome in project.Outcomes)
                                {
                                    outcome.Outputs = await _logFrameService.GetOutputsByOutcomeIdAsync(outcome.OutcomeID);
                                    foreach (var output in outcome.Outputs)
                                    {
                                        output.Activities = await _logFrameService.GetActivitiesByOutputIdAsync(output.OutputID);
                                        output.ProjectIndicators = await _logFrameService.GetProjectIndicatorsByOutputIdAsync(output.OutputID);
                                    }
                                }
                                project.DetailedBudgetLines = await GetDetailedBudgetLinesByProjectIdAsync(projectId);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetProjectByIdAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"General Error in GetProjectByIdAsync: {ex.Message}");
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
                                // For lists, typically only main project info is loaded. Full details via GetProjectByIdAsync.
                                projects.Add(new Project
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
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetProjectsBySectionIdAsync: {ex.Message}");
            }
            return projects;
        }

        public async Task<bool> SaveProjectAsync(Project project)
        {
            // Acknowledge limitation: This does not use a single transaction for all operations.
            // If any step fails, previous steps are not rolled back.
            int rowsAffected = 0;
            bool mainProjectSaved = false;

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync(); // Open connection once if possible, or manage per command.
                                                // For simplicity here, each service method call will manage its own connection.

                    SqlCommand command;
                    if (project.ProjectID == 0) // New project
                    {
                        string insertQuery = @"
                            INSERT INTO Projects (ProjectName, ProjectCode, SectionID, StartDate, EndDate, Location, OverallObjective, ManagerUserID, Status, TotalBudget, Donor, CreatedAt, UpdatedAt)
                            VALUES (@ProjectName, @ProjectCode, @SectionID, @StartDate, @EndDate, @Location, @OverallObjective, @ManagerUserID, @Status, @TotalBudget, @Donor, @CreatedAt, @UpdatedAt);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
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
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                    // Re-open connection if it was closed by previous operations or manage it outside
                    if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                    if (project.ProjectID == 0)
                    {
                        object newId = await command.ExecuteScalarAsync();
                        if (newId != null && newId != DBNull.Value)
                        {
                            project.ProjectID = Convert.ToInt32(newId);
                            mainProjectSaved = true;
                        }
                    }
                    else
                    {
                        rowsAffected = await command.ExecuteNonQueryAsync();
                        mainProjectSaved = rowsAffected > 0;
                    }
                } // Connection is disposed here.

                if (!mainProjectSaved && project.ProjectID == 0) // If new project failed to save, ProjectID is still 0
                {
                    Console.WriteLine("Failed to save the main project details. Aborting save of child entities.");
                    return false;
                }
                if (!mainProjectSaved && project.ProjectID !=0) // If existing project not found or failed to update
                {
                     Console.WriteLine($"Failed to update main project details for ProjectID: {project.ProjectID}. Child entities will not be processed.");
                     return false; // Or handle as a partial success if desired.
                }


                // Save Outcomes and their children
                if (project.Outcomes != null)
                {
                    foreach (var outcome in project.Outcomes)
                    {
                        outcome.ProjectID = project.ProjectID;
                        if (outcome.OutcomeID == 0) outcome.OutcomeID = await _logFrameService.AddOutcomeAsync(outcome);
                        else await _logFrameService.UpdateOutcomeAsync(outcome);

                        if (outcome.OutcomeID > 0 && outcome.Outputs != null)
                        {
                            foreach (var output in outcome.Outputs)
                            {
                                output.OutcomeID = outcome.OutcomeID;
                                if (output.OutputID == 0) output.OutputID = await _logFrameService.AddOutputAsync(output);
                                else await _logFrameService.UpdateOutputAsync(output);

                                if (output.OutputID > 0)
                                {
                                    if (output.Activities != null)
                                    {
                                        foreach (var activity in output.Activities)
                                        {
                                            activity.OutputID = output.OutputID;
                                            if (activity.ActivityID == 0) await _logFrameService.AddActivityAsync(activity);
                                            else await _logFrameService.UpdateActivityAsync(activity);
                                        }
                                    }
                                    if (output.ProjectIndicators != null)
                                    {
                                        foreach (var indicator in output.ProjectIndicators)
                                        {
                                            indicator.OutputID = output.OutputID;
                                            indicator.ProjectID = project.ProjectID; // Ensure ProjectID is set
                                            if (indicator.IndicatorID == 0) await _logFrameService.AddProjectIndicatorToOutputAsync(indicator);
                                            else await _logFrameService.UpdateProjectIndicatorAsync(indicator);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Save DetailedBudgetLines
                if (project.DetailedBudgetLines != null)
                {
                    foreach (var budgetLine in project.DetailedBudgetLines)
                    {
                        budgetLine.ProjectID = project.ProjectID;
                        if (budgetLine.DetailedBudgetLineID == 0) await this.AddDetailedBudgetLineAsync(budgetLine);
                        else await this.UpdateDetailedBudgetLineAsync(budgetLine);
                    }
                }
                return true; // Indicates main project and attempted child saves
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SaveProjectAsync (overall): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in SaveProjectAsync (overall): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            // Acknowledge limitation: This does not use a single transaction.
            // Order of deletion is important to avoid FK violations if cascade delete is not set up in DB.
            try
            {
                // 1. Delete DetailedBudgetLines for the project
                var budgetLines = await GetDetailedBudgetLinesByProjectIdAsync(projectId);
                foreach (var line in budgetLines)
                {
                    await DeleteDetailedBudgetLineAsync(line.DetailedBudgetLineID);
                }

                // 2. Delete LogFrame structure (Activities, Indicators, then Outputs, then Outcomes)
                var outcomes = await _logFrameService.GetOutcomesByProjectIdAsync(projectId);
                foreach (var outcome in outcomes)
                {
                    var outputs = await _logFrameService.GetOutputsByOutcomeIdAsync(outcome.OutcomeID);
                    foreach (var output in outputs)
                    {
                        var activities = await _logFrameService.GetActivitiesByOutputIdAsync(output.OutputID);
                        foreach (var activity in activities)
                        {
                            await _logFrameService.DeleteActivityAsync(activity.ActivityID);
                        }
                        var indicators = await _logFrameService.GetProjectIndicatorsByOutputIdAsync(output.OutputID);
                        foreach (var indicator in indicators)
                        {
                            await _logFrameService.DeleteProjectIndicatorAsync(indicator.IndicatorID);
                        }
                        await _logFrameService.DeleteOutputAsync(output.OutputID);
                    }
                    await _logFrameService.DeleteOutcomeAsync(outcome.OutcomeID);
                }

                // Delete other direct child entities of Project if they were not handled by cascade (e.g. ProjectReports, Feedbacks etc.)
                // For brevity, these are omitted here but would follow a similar pattern.


                // Finally, delete the project itself
                string sql = "DELETE FROM Projects WHERE ProjectID = @ProjectID";
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectID", projectId);
                        await conn.OpenAsync();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in DeleteProjectAsync (overall): {ex.Message}");
                return false;
            }
             catch (Exception ex)
            {
                Console.WriteLine($"General Error in DeleteProjectAsync (overall): {ex.Message}");
                return false;
            }
        }
    }
}
