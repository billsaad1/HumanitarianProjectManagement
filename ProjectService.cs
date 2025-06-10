using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel; // For BindingList

namespace HumanitarianProjectManagement.DataAccessLayer
{
    public class ProjectService
    {
        private readonly LogFrameService _logFrameService;

        public ProjectService()
        {
            _logFrameService = new LogFrameService();
        }

        #region DetailedBudgetLine CRUD
        public async Task<List<DetailedBudgetLine>> GetDetailedBudgetLinesByProjectIdAsync(int projectId)
        {
            List<DetailedBudgetLine> budgetLines = new List<DetailedBudgetLine>();
            string query = "SELECT DetailedBudgetLineID, ProjectID, BudgetCategory, Code, Description, Unit, Quantity, UnitCost, Duration, PercentChargedToCBPF, TotalCost FROM DetailedBudgetLines WHERE ProjectID = @ProjectID";

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
                                DetailedBudgetLine budgetLine = new DetailedBudgetLine
                                {
                                    DetailedBudgetLineID = reader["DetailedBudgetLineID"] != DBNull.Value ? (Guid)reader["DetailedBudgetLineID"] : Guid.Empty,
                                    ProjectId = (int)reader["ProjectID"],
                                    Category = (BudgetCategoriesEnum)Enum.Parse(typeof(BudgetCategoriesEnum), reader["BudgetCategory"].ToString()),
                                    Code = reader["Code"] != DBNull.Value ? reader["Code"].ToString() : string.Empty,
                                    Description = reader["Description"].ToString(),
                                    Unit = reader["Unit"] != DBNull.Value ? reader["Unit"].ToString() : null,
                                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                                    UnitCost = (decimal)reader["UnitCost"],
                                    Duration = Convert.ToDecimal(reader["Duration"]),
                                    PercentageChargedToCBPF = (decimal)reader["PercentChargedToCBPF"],
                                    TotalCost = (decimal)reader["TotalCost"]
                                };
                                // Populate ItemizedDetails
                                if (budgetLine.DetailedBudgetLineID != Guid.Empty)
                                {
                                    List<ItemizedBudgetDetail> itemizedDetails = await GetItemizedBudgetDetailsByParentIdAsync(budgetLine.DetailedBudgetLineID);
                                    budgetLine.ItemizedDetails = new System.ComponentModel.BindingList<ItemizedBudgetDetail>(itemizedDetails);
                                }
                                budgetLines.Add(budgetLine);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDetailedBudgetLinesByProjectIdAsync: {ex.Message}");
            }
            return budgetLines;
        }

        public async Task<bool> AddDetailedBudgetLineWithDetailsAsync(DetailedBudgetLine budgetLine)
        {
            if (budgetLine == null) throw new ArgumentNullException(nameof(budgetLine));
            if (budgetLine.DetailedBudgetLineID == Guid.Empty) budgetLine.DetailedBudgetLineID = Guid.NewGuid();

            string query = @"INSERT INTO DetailedBudgetLines
                                (DetailedBudgetLineID, ProjectID, BudgetCategory, Code, Description, Unit, Quantity, UnitCost, Duration, PercentChargedToCBPF, TotalCost)
                             VALUES
                                (@DetailedBudgetLineID, @ProjectID, @BudgetCategory, @Code, @Description, @Unit, @Quantity, @UnitCost, @Duration, @PercentChargedToCBPF, @TotalCost);";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DetailedBudgetLineID", budgetLine.DetailedBudgetLineID);
                        command.Parameters.AddWithValue("@ProjectID", budgetLine.ProjectId);
                        command.Parameters.AddWithValue("@BudgetCategory", budgetLine.Category.ToString());
                        command.Parameters.AddWithValue("@Code", (object)budgetLine.Code ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Description", budgetLine.Description);
                        command.Parameters.AddWithValue("@Unit", (object)budgetLine.Unit ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", budgetLine.Quantity);
                        command.Parameters.AddWithValue("@UnitCost", budgetLine.UnitCost);
                        command.Parameters.AddWithValue("@Duration", budgetLine.Duration);
                        command.Parameters.AddWithValue("@PercentChargedToCBPF", budgetLine.PercentageChargedToCBPF);
                        command.Parameters.AddWithValue("@TotalCost", budgetLine.TotalCost);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            if (budgetLine.ItemizedDetails != null)
                            {
                                foreach (var itemDetail in budgetLine.ItemizedDetails)
                                {
                                    itemDetail.ParentBudgetLineID = budgetLine.DetailedBudgetLineID;
                                    await AddItemizedBudgetDetailAsync(itemDetail);
                                }
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddDetailedBudgetLineWithDetailsAsync: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> UpdateDetailedBudgetLineWithDetailsAsync(DetailedBudgetLine budgetLine)
        {
            if (budgetLine == null) throw new ArgumentNullException(nameof(budgetLine));
            if (budgetLine.DetailedBudgetLineID == Guid.Empty)
            {
                Console.WriteLine("Error in UpdateDetailedBudgetLineWithDetailsAsync: DetailedBudgetLineID cannot be empty for an update.");
                return false;
            }

            string query = @"UPDATE DetailedBudgetLines SET
                                ProjectID = @ProjectID, BudgetCategory = @BudgetCategory, Code = @Code, Description = @Description, Unit = @Unit, 
                                Quantity = @Quantity, UnitCost = @UnitCost, Duration = @Duration, PercentChargedToCBPF = @PercentChargedToCBPF, TotalCost = @TotalCost
                             WHERE DetailedBudgetLineID = @DetailedBudgetLineID;";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectID", budgetLine.ProjectId);
                        command.Parameters.AddWithValue("@BudgetCategory", budgetLine.Category.ToString());
                        command.Parameters.AddWithValue("@Code", (object)budgetLine.Code ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Description", budgetLine.Description);
                        command.Parameters.AddWithValue("@Unit", (object)budgetLine.Unit ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", budgetLine.Quantity);
                        command.Parameters.AddWithValue("@UnitCost", budgetLine.UnitCost);
                        command.Parameters.AddWithValue("@Duration", budgetLine.Duration);
                        command.Parameters.AddWithValue("@PercentChargedToCBPF", budgetLine.PercentageChargedToCBPF);
                        command.Parameters.AddWithValue("@TotalCost", budgetLine.TotalCost);
                        command.Parameters.AddWithValue("@DetailedBudgetLineID", budgetLine.DetailedBudgetLineID);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            // Synchronize ItemizedDetails
                            List<ItemizedBudgetDetail> dbItems = await GetItemizedBudgetDetailsByParentIdAsync(budgetLine.DetailedBudgetLineID);
                            HashSet<Guid> incomingItemIds = new HashSet<Guid>(budgetLine.ItemizedDetails?.Select(i => i.ItemizedBudgetDetailID) ?? Enumerable.Empty<Guid>());

                            foreach (var dbItem in dbItems)
                            {
                                if (!incomingItemIds.Contains(dbItem.ItemizedBudgetDetailID))
                                {
                                    await DeleteItemizedBudgetDetailAsync(dbItem.ItemizedBudgetDetailID);
                                }
                            }

                            if (budgetLine.ItemizedDetails != null)
                            {
                                foreach (var itemDetail in budgetLine.ItemizedDetails)
                                {
                                    itemDetail.ParentBudgetLineID = budgetLine.DetailedBudgetLineID;
                                    var existingDbItem = dbItems.FirstOrDefault(db => db.ItemizedBudgetDetailID == itemDetail.ItemizedBudgetDetailID && itemDetail.ItemizedBudgetDetailID != Guid.Empty);
                                    if (existingDbItem != null)
                                    {
                                        await UpdateItemizedBudgetDetailAsync(itemDetail);
                                    }
                                    else if (itemDetail.ItemizedBudgetDetailID == Guid.Empty)
                                    {
                                        await AddItemizedBudgetDetailAsync(itemDetail);
                                    }
                                }
                            }
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateDetailedBudgetLineWithDetailsAsync: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DeleteDetailedBudgetLineAsync(Guid detailedBudgetLineId)
        {
            await DeleteAllItemizedDetailsByParentIdAsync(detailedBudgetLineId);

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteDetailedBudgetLineAsync: {ex.Message}");
            }
            return false;
        }
        #endregion

        #region ItemizedBudgetDetail CRUD
        public async Task<List<ItemizedBudgetDetail>> GetItemizedBudgetDetailsByParentIdAsync(Guid parentBudgetLineId)
        {
            List<ItemizedBudgetDetail> items = new List<ItemizedBudgetDetail>();
            string query = "SELECT ItemizedBudgetDetailID, ParentBudgetLineID, Description, Quantity, UnitPrice, TotalCost FROM ItemizedBudgetDetails WHERE ParentBudgetLineID = @ParentBudgetLineID";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ParentBudgetLineID", parentBudgetLineId);
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ItemizedBudgetDetail item = new ItemizedBudgetDetail
                                {
                                    ItemizedBudgetDetailID = (Guid)reader["ItemizedBudgetDetailID"],
                                    ParentBudgetLineID = (Guid)reader["ParentBudgetLineID"],
                                    Description = reader["Description"].ToString(),
                                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"])
                                    // TotalCost removed from direct assignment
                                };
                                item.UpdateTotalCost(); // Calculate TotalCost
                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetItemizedBudgetDetailsByParentIdAsync: {ex.Message}");
            }
            return items;
        }

        public async Task<bool> AddItemizedBudgetDetailAsync(ItemizedBudgetDetail itemDetail)
        {
            if (itemDetail == null) throw new ArgumentNullException(nameof(itemDetail));
            if (itemDetail.ItemizedBudgetDetailID == Guid.Empty) itemDetail.ItemizedBudgetDetailID = Guid.NewGuid();

            string query = @"INSERT INTO ItemizedBudgetDetails 
                                (ItemizedBudgetDetailID, ParentBudgetLineID, Description, Quantity, UnitPrice, TotalCost)
                             VALUES 
                                (@ItemizedBudgetDetailID, @ParentBudgetLineID, @Description, @Quantity, @UnitPrice, @TotalCost);";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemizedBudgetDetailID", itemDetail.ItemizedBudgetDetailID);
                        command.Parameters.AddWithValue("@ParentBudgetLineID", itemDetail.ParentBudgetLineID);
                        command.Parameters.AddWithValue("@Description", itemDetail.Description);
                        command.Parameters.AddWithValue("@Quantity", itemDetail.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", itemDetail.UnitPrice);
                        command.Parameters.AddWithValue("@TotalCost", itemDetail.TotalCost);

                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddItemizedBudgetDetailAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateItemizedBudgetDetailAsync(ItemizedBudgetDetail itemDetail)
        {
            if (itemDetail == null) throw new ArgumentNullException(nameof(itemDetail));

            string query = @"UPDATE ItemizedBudgetDetails SET
                                ParentBudgetLineID = @ParentBudgetLineID,
                                Description = @Description,
                                Quantity = @Quantity,
                                UnitPrice = @UnitPrice,
                                TotalCost = @TotalCost
                             WHERE ItemizedBudgetDetailID = @ItemizedBudgetDetailID;";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ParentBudgetLineID", itemDetail.ParentBudgetLineID);
                        command.Parameters.AddWithValue("@Description", itemDetail.Description);
                        command.Parameters.AddWithValue("@Quantity", itemDetail.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", itemDetail.UnitPrice);
                        command.Parameters.AddWithValue("@TotalCost", itemDetail.TotalCost);
                        command.Parameters.AddWithValue("@ItemizedBudgetDetailID", itemDetail.ItemizedBudgetDetailID);

                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateItemizedBudgetDetailAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllItemizedDetailsByParentIdAsync(Guid parentBudgetLineId)
        {
            string query = "DELETE FROM ItemizedBudgetDetails WHERE ParentBudgetLineID = @ParentBudgetLineID;";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ParentBudgetLineID", parentBudgetLineId);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAllItemizedDetailsByParentIdAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteItemizedBudgetDetailAsync(Guid itemizedBudgetDetailId)
        {
            string query = "DELETE FROM ItemizedBudgetDetails WHERE ItemizedBudgetDetailID = @ItemizedBudgetDetailID;";
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemizedBudgetDetailID", itemizedBudgetDetailId);
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteItemizedBudgetDetailAsync: {ex.Message}");
                return false;
            }
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProjectByIdAsync: {ex.Message}");
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProjectsBySectionIdAsync: {ex.Message}");
            }
            return projects;
        }

        public async Task<bool> SaveProjectAsync(Project project)
        {
            int rowsAffected = 0;
            bool mainProjectSaved = false;

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync();

                    SqlCommand command;
                    if (project.ProjectID == 0)
                    {
                        string insertQuery = @"
                            INSERT INTO Projects (ProjectName, ProjectCode, SectionID, StartDate, EndDate, Location, OverallObjective, ManagerUserID, Status, TotalBudget, Donor, CreatedAt, UpdatedAt)
                            VALUES (@ProjectName, @ProjectCode, @SectionID, @StartDate, @EndDate, @Location, @OverallObjective, @ManagerUserID, @Status, @TotalBudget, @Donor, @CreatedAt, @UpdatedAt);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                        command = new SqlCommand(insertQuery, connection);
                        command.Parameters.AddWithValue("@CreatedAt", project.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : project.CreatedAt);
                    }
                    else
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
                }

                if (!mainProjectSaved)
                {
                    Console.WriteLine("Failed to save/update the main project details. Aborting save of child entities.");
                    return false;
                }

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
                                            indicator.ProjectID = project.ProjectID;
                                            if (indicator.ProjectIndicatorID == 0) await _logFrameService.AddProjectIndicatorToOutputAsync(indicator);
                                            else await _logFrameService.UpdateProjectIndicatorAsync(indicator);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (project.DetailedBudgetLines != null)
                {
                    List<DetailedBudgetLine> existingDbLines = new List<DetailedBudgetLine>();
                    if (project.ProjectID > 0)
                    {
                        existingDbLines = await GetDetailedBudgetLinesByProjectIdAsync(project.ProjectID);
                    }

                    var incomingLineIds = new HashSet<Guid>(project.DetailedBudgetLines.Where(bl => bl.DetailedBudgetLineID != Guid.Empty).Select(bl => bl.DetailedBudgetLineID));

                    foreach (var dbLine in existingDbLines)
                    {
                        if (!incomingLineIds.Contains(dbLine.DetailedBudgetLineID))
                        {
                            await DeleteDetailedBudgetLineAsync(dbLine.DetailedBudgetLineID);
                        }
                    }

                    foreach (var budgetLine in project.DetailedBudgetLines)
                    {
                        budgetLine.ProjectId = project.ProjectID;
                        if (budgetLine.DetailedBudgetLineID == Guid.Empty)
                        {
                            await AddDetailedBudgetLineWithDetailsAsync(budgetLine);
                        }
                        else
                        {
                            await UpdateDetailedBudgetLineWithDetailsAsync(budgetLine);
                        }
                    }
                }
                else
                {
                    if (project.ProjectID > 0)
                    {
                        var existingDbLines = await GetDetailedBudgetLinesByProjectIdAsync(project.ProjectID);
                        foreach (var dbLine in existingDbLines)
                        {
                            await DeleteDetailedBudgetLineAsync(dbLine.DetailedBudgetLineID);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveProjectAsync (overall): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            try
            {
                var budgetLines = await GetDetailedBudgetLinesByProjectIdAsync(projectId);
                foreach (var line in budgetLines)
                {
                    await DeleteDetailedBudgetLineAsync(line.DetailedBudgetLineID);
                }

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
                            await _logFrameService.DeleteProjectIndicatorAsync(indicator.ProjectIndicatorID);
                        }
                        await _logFrameService.DeleteOutputAsync(output.OutputID);
                    }
                    await _logFrameService.DeleteOutcomeAsync(outcome.OutcomeID);
                }

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProjectAsync: {ex.Message}");
                return false;
            }
        }
    }
}
