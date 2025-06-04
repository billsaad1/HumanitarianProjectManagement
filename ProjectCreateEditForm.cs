using HumanitarianProjectManagement.DataAccessLayer;
using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using HumanitarianProjectManagement.UI;
using System.Globalization; // Added for Activity Plan

namespace HumanitarianProjectManagement.Forms
{
    public partial class ProjectCreateEditForm : Form
    {
        private readonly ProjectService _projectService;
        private readonly SectionService _sectionService;
        private Project _currentProject;
        private readonly bool _isEditMode;
        private int? _initialSectionId;

        // Constants & Fields for Activity Plan
        private const int NumberOfMonthsToPlan = 24;
        private const string ActivityPlanMonthColumnPrefix = "MonthCol_";

        // Fields for Budget UI Control Mapping
        private Dictionary<string, Panel> categoryContentPanels;
        private Dictionary<string, DataGridView> categoryDgvs;
        private Dictionary<string, BudgetCategory> categoryEnumMap;
        private Dictionary<BudgetCategory, string> reverseCategoryEnumMap;

        private class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }

        public ProjectCreateEditForm(Project projectToEdit = null)
            : this(projectToEdit, null)
        {
        }

        public ProjectCreateEditForm(Project projectToEdit = null, int? initialSectionId = null)
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToForm(this);
            _projectService = new ProjectService();
            _sectionService = new SectionService();
            _initialSectionId = initialSectionId;

            _isEditMode = (projectToEdit != null);

            if (_isEditMode)
            {
                _currentProject = projectToEdit ?? new Project();
                if (_currentProject.Outcomes == null) _currentProject.Outcomes = new HashSet<Outcome>();
                this.Text = $"Edit Project - {_currentProject.ProjectName}";
                PopulateControls();
            }
            else
            {
                _currentProject = new Project();
                if (_currentProject.Outcomes == null) _currentProject.Outcomes = new HashSet<Outcome>();
                this.Text = "Add New Project";
                if (dtpStartDate != null) {
                    dtpStartDate.Value = DateTime.Now;
                    dtpStartDate.Checked = false;
                }
                if (dtpEndDate != null) {
                    dtpEndDate.Checked = false;
                }
            }
            SetAccessibilityProperties();
            this.Load += new System.EventHandler(this.ProjectCreateEditForm_Load);
        }

        private async void ProjectCreateEditForm_Load(object sender, EventArgs e)
        {
            await LoadComboBoxesAsync();
            InitializeLogFrameUI();
            InitializeBudgetControls(); // New method call
            if (_isEditMode && _currentProject != null)
            {
                PopulateBudgetControls();
            }
            if (tabControlMain != null)
            {
                tabControlMain.SelectedIndexChanged += TabControlMain_SelectedIndexChanged;
            }
            // this.Size = new Size(1200, 800); // Replaced by Maximized
            // this.MinimumSize = new Size(1024, 768); // Still relevant if user restores down
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen; // Keep centered before maximizing
        }

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPagePlan)
            {
                InitializeActivityPlanGrid();
                PopulateActivityPlanGrid();
            }
        }

        #region Activity Plan Methods

        private void InitializeActivityPlanGrid()
        {
            if (dgvActivityPlan == null) return;

            dgvActivityPlan.Columns.Clear();
            dgvActivityPlan.AutoGenerateColumns = false;
            dgvActivityPlan.AllowUserToAddRows = false;

            var activityDescCol = new DataGridViewTextBoxColumn
            {
                Name = "ActivityDescription",
                HeaderText = "Activity Description",
                ReadOnly = true,
                Frozen = true,
                Width = 300,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            dgvActivityPlan.Columns.Add(activityDescCol);

            var activityStoreCol = new DataGridViewTextBoxColumn
            {
                Name = "ActivityReference",
                Visible = false
            };
            dgvActivityPlan.Columns.Add(activityStoreCol);

            DateTime startDate = _currentProject?.StartDate ?? DateTime.Now;

            for (int i = 0; i < NumberOfMonthsToPlan; i++)
            {
                DateTime currentMonth = startDate.AddMonths(i);
                var monthCol = new DataGridViewCheckBoxColumn
                {
                    Name = ActivityPlanMonthColumnPrefix + currentMonth.ToString("yyyyMM"),
                    HeaderText = currentMonth.ToString("MMM yyyy"),
                    Tag = currentMonth,
                    Width = 70
                };
                dgvActivityPlan.Columns.Add(monthCol);
            }
            dgvActivityPlan.CellValueChanged -= dgvActivityPlan_CellValueChanged;
            dgvActivityPlan.CellValueChanged += dgvActivityPlan_CellValueChanged;
            dgvActivityPlan.CurrentCellDirtyStateChanged -= dgvActivityPlan_CurrentCellDirtyStateChanged;
            dgvActivityPlan.CurrentCellDirtyStateChanged += dgvActivityPlan_CurrentCellDirtyStateChanged;
        }

        private void dgvActivityPlan_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvActivityPlan.IsCurrentCellDirty && dgvActivityPlan.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvActivityPlan.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void PopulateActivityPlanGrid()
        {
            if (dgvActivityPlan == null || _currentProject == null) return;

            dgvActivityPlan.CellValueChanged -= dgvActivityPlan_CellValueChanged;

            dgvActivityPlan.Rows.Clear();

            if (_currentProject.Outcomes == null) return;

            List<Activity> allActivities = new List<Activity>();
            foreach (var outcome in _currentProject.Outcomes)
            {
                if (outcome.Outputs == null) continue;
                foreach (var output in outcome.Outputs)
                {
                    if (output.Activities != null)
                    {
                        allActivities.AddRange(output.Activities);
                    }
                }
            }

            foreach (var activity in allActivities.OrderBy(a => a.ActivityDescription))
            {
                int rowIndex = dgvActivityPlan.Rows.Add();
                DataGridViewRow row = dgvActivityPlan.Rows[rowIndex];
                row.Cells["ActivityDescription"].Value = activity.ActivityDescription;
                row.Cells["ActivityReference"].Value = activity;

                List<string> plannedMonthsForActivity = new List<string>();
                if (!string.IsNullOrWhiteSpace(activity.PlannedMonths))
                {
                    plannedMonthsForActivity = activity.PlannedMonths.Split(',').ToList();
                }

                foreach (DataGridViewColumn col in dgvActivityPlan.Columns)
                {
                    if (col is DataGridViewCheckBoxColumn && col.Tag is DateTime monthDate)
                    {
                        string monthYearString = monthDate.ToString("yyyy-MM");
                        if (plannedMonthsForActivity.Contains(monthYearString))
                        {
                            row.Cells[col.Name].Value = true;
                        }
                        else
                        {
                            row.Cells[col.Name].Value = false;
                        }
                    }
                }
            }
            dgvActivityPlan.CellValueChanged += dgvActivityPlan_CellValueChanged;
        }

        private void dgvActivityPlan_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvActivityPlan == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataGridViewColumn changedColumn = dgvActivityPlan.Columns[e.ColumnIndex];
            DataGridViewRow changedRow = dgvActivityPlan.Rows[e.RowIndex];

            if (!(changedColumn is DataGridViewCheckBoxColumn) || !(changedColumn.Tag is DateTime monthDate))
            {
                return;
            }

            Activity activity = changedRow.Cells["ActivityReference"].Value as Activity;
            if (activity == null) return;

            bool isChecked = false;
            if (changedRow.Cells[e.ColumnIndex].Value != null && changedRow.Cells[e.ColumnIndex].Value != DBNull.Value)
            {
                 isChecked = Convert.ToBoolean(changedRow.Cells[e.ColumnIndex].Value);
            }

            string monthYearString = monthDate.ToString("yyyy-MM");

            List<string> plannedMonthsList = new List<string>();
            if (!string.IsNullOrWhiteSpace(activity.PlannedMonths))
            {
                plannedMonthsList = activity.PlannedMonths.Split(',').ToList();
            }

            if (isChecked)
            {
                if (!plannedMonthsList.Contains(monthYearString))
                {
                    plannedMonthsList.Add(monthYearString);
                }
            }
            else
            {
                plannedMonthsList.Remove(monthYearString);
            }

            plannedMonthsList.Sort();
            activity.PlannedMonths = string.Join(",", plannedMonthsList);
        }


        #endregion

        #region Budget Section Methods

        private void InitializeBudgetControls()
        {
            categoryContentPanels = new Dictionary<string, Panel> {
                {"A", pnlBudgetCatAContent}, {"B", pnlBudgetCatBContent}, {"C", pnlBudgetCatCContent},
                {"D", pnlBudgetCatDContent}, {"E", pnlBudgetCatEContent}, {"F", pnlBudgetCatFContent},
                {"G", pnlBudgetCatGContent}
            };
            categoryDgvs = new Dictionary<string, DataGridView> {
                {"A", dgvBudgetCatA}, {"B", dgvBudgetCatB}, {"C", dgvBudgetCatC},
                {"D", dgvBudgetCatD}, {"E", dgvBudgetCatE}, {"F", dgvBudgetCatF},
                {"G", dgvBudgetCatG}
            };
            categoryEnumMap = new Dictionary<string, BudgetCategory> {
                {"A", BudgetCategory.StaffAndPersonnel}, {"B", BudgetCategory.SuppliesCommoditiesMaterials},
                {"C", BudgetCategory.Equipment}, {"D", BudgetCategory.ContractualServices},
                {"E", BudgetCategory.Travel}, {"F", BudgetCategory.TransfersAndGrants},
                {"G", BudgetCategory.GeneralOperatingAndOtherDirectCosts}
            };
            reverseCategoryEnumMap = categoryEnumMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            Button[] toggleButtons = { btnToggleCatA, btnToggleCatB, btnToggleCatC, btnToggleCatD, btnToggleCatE, btnToggleCatF, btnToggleCatG };
            Button[] addLineButtons = { btnAddLineCatA, btnAddLineCatB, btnAddLineCatC, btnAddLineCatD, btnAddLineCatE, btnAddLineCatF, btnAddLineCatG };

            foreach (var entry in categoryDgvs)
            {
                if (entry.Value == null) {
                    Console.WriteLine($"Error: DataGridView control for category key '{entry.Key}' is null in InitializeBudgetControls.");
                    continue;
                }
                InitializeBudgetDataGridView(entry.Value, categoryEnumMap[entry.Key]);
                entry.Value.CellContentClick += dgvBudget_CellContentClick;
                entry.Value.CellValueChanged += dgvBudget_CellValueChanged;
                entry.Value.DataError += dgvBudget_DataError;
            }

            foreach (var btn in toggleButtons) { if(btn != null) btn.Click += BudgetCategory_ToggleExpand; else Console.WriteLine("A toggle button is null.");}
            foreach (var btn in addLineButtons) { if(btn != null) btn.Click += BudgetCategory_AddLine; else Console.WriteLine("An add line button is null.");}

            foreach(var entry in categoryContentPanels)
            {
                Button toggleBtn = toggleButtons.FirstOrDefault(b => b != null && b.Tag?.ToString() == entry.Key);
                bool isVisible = (entry.Key == "A"); // First category expanded by default
                entry.Value.Visible = isVisible;
                if(toggleBtn != null) toggleBtn.Text = isVisible ? "-" : "+";
            }
        }

        private void InitializeBudgetDataGridView(DataGridView dgv, BudgetCategory category)
        {
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.Tag = category;
            dgv.Columns.Clear();

            var idCol = new DataGridViewTextBoxColumn { Name = "BudgetLineID", DataPropertyName = "BudgetLineID", HeaderText = "ID", Visible = false, Width = 50 };
            dgv.Columns.Add(idCol);

            var remarksCol = new DataGridViewTextBoxColumn { Name = "Remarks", DataPropertyName = "Remarks", HeaderText = "Description/Remarks", Width = 300, AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill, MinimumWidth=200 };
            remarksCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.Columns.Add(remarksCol);

            var unitCol = new DataGridViewTextBoxColumn { Name = "Unit", DataPropertyName = "Unit", HeaderText = "Unit", Width = 70 };
            dgv.Columns.Add(unitCol);

            var qtyCol = new DataGridViewTextBoxColumn { Name = "Quantity", DataPropertyName = "Quantity", HeaderText = "Qty", Width = 60 };
            qtyCol.DefaultCellStyle.Format = "N2"; qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(qtyCol);

            var unitCostCol = new DataGridViewTextBoxColumn { Name = "UnitCost", DataPropertyName = "UnitCost", HeaderText = "Unit Cost", Width = 90 };
            unitCostCol.DefaultCellStyle.Format = "C2"; unitCostCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(unitCostCol);

            var durationCol = new DataGridViewTextBoxColumn { Name = "Duration", DataPropertyName = "Duration", HeaderText = "Duration", Width = 80 };
            dgv.Columns.Add(durationCol);

            var chargedCol = new DataGridViewTextBoxColumn { Name = "PercentageChargedToCBPF", DataPropertyName = "PercentageChargedToCBPF", HeaderText = "% Charged", Width = 70 };
            chargedCol.DefaultCellStyle.Format = "N0"; chargedCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(chargedCol);

            var totalCostCol = new DataGridViewTextBoxColumn { Name = "TotalCost", DataPropertyName = "TotalCost", HeaderText = "Total Cost", Width = 100, ReadOnly = true };
            totalCostCol.DefaultCellStyle.Format = "C2"; totalCostCol.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray; totalCostCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(totalCostCol);

            var deleteButtonCol = new DataGridViewButtonColumn { Name = "DeleteAction", HeaderText = "Action", Text = "Delete", UseColumnTextForButtonValue = true, Width = 65, FlatStyle = FlatStyle.Popup };
            deleteButtonCol.DefaultCellStyle.BackColor = System.Drawing.Color.Coral;
            deleteButtonCol.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgv.Columns.Add(deleteButtonCol);
        }

        private void BudgetCategory_ToggleExpand(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;
            string categoryKey = btn.Tag.ToString();

            if (categoryContentPanels.TryGetValue(categoryKey, out Panel contentPanel))
            {
                contentPanel.Visible = !contentPanel.Visible;
                btn.Text = contentPanel.Visible ? "-" : "+";
            }
        }

        private void BudgetCategory_AddLine(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;
            string categoryKey = btn.Tag.ToString();

            if (!categoryEnumMap.TryGetValue(categoryKey, out BudgetCategory categoryEnum)) return;
            if (_currentProject == null) { MessageBox.Show("Current project is not initialized.", "Error"); return; }


            var newBudgetLine = new DetailedBudgetLine
            {
                ProjectID = _currentProject.ProjectID,
                Project = _currentProject,
                Category = categoryEnum,
                Remarks = "New Item",
                Quantity = 1,
                UnitCost = 0.01m,
                Duration = "1",
                PercentageChargedToCBPF = 100
            };

            if (_currentProject.DetailedBudgetLines == null)
                _currentProject.DetailedBudgetLines = new HashSet<DetailedBudgetLine>();

            _currentProject.DetailedBudgetLines.Add(newBudgetLine);
            RefreshBudgetCategoryDGV(categoryEnum);
        }

        private void dgvBudget_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0 || e.ColumnIndex != dgv.Columns["DeleteAction"]?.Index) return;
            if (!(dgv.Tag is BudgetCategory categoryEnum)) return;

            DetailedBudgetLine lineToRemove = dgv.Rows[e.RowIndex].DataBoundItem as DetailedBudgetLine;

            if (lineToRemove != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete budget line: '{lineToRemove.Remarks}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    bool removedDirectly = _currentProject.DetailedBudgetLines.Remove(lineToRemove);
                    if (!removedDirectly && lineToRemove.BudgetLineID != 0)
                    {
                        var itemInCollection = _currentProject.DetailedBudgetLines.FirstOrDefault(i => i.BudgetLineID == lineToRemove.BudgetLineID);
                        if(itemInCollection != null) _currentProject.DetailedBudgetLines.Remove(itemInCollection);
                    }
                    RefreshBudgetCategoryDGV(categoryEnum);
                }
            }
        }

        private void dgvBudget_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count) return;

            DataGridViewRow row = dgv.Rows[e.RowIndex];
            DetailedBudgetLine line = row.DataBoundItem as DetailedBudgetLine;
            if (line == null) return;

            string colName = dgv.Columns[e.ColumnIndex].Name;

            if (colName == "Quantity" || colName == "UnitCost")
            {
                if (dgv.Columns.Contains("TotalCost"))
                {
                    dgv.InvalidateCell(dgv.Columns["TotalCost"].Index, e.RowIndex);
                }
            }
        }

        private void dgvBudget_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            string dgvName = "Budget Grid";
            if (dgv != null && dgv.Tag is BudgetCategory cat) { dgvName = $"Budget Grid for Category {reverseCategoryEnumMap[cat]}"; }

            MessageBox.Show($"Error in budget data for {dgvName}.
Column: {dgv.Columns[e.ColumnIndex].HeaderText} (Row: {e.RowIndex + 1})
Error: {e.Exception.Message}
Please enter a valid value.", "Data Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.ThrowException = false;
            e.Cancel = false;
        }

        private void PopulateBudgetControls()
        {
            if (_currentProject == null) return;
            if (_currentProject.DetailedBudgetLines == null)
               _currentProject.DetailedBudgetLines = new HashSet<DetailedBudgetLine>();

            foreach(var dgv in categoryDgvs.Values) { if(dgv != null) dgv.CellValueChanged -= dgvBudget_CellValueChanged; }

            foreach (BudgetCategory catEnum in Enum.GetValues(typeof(BudgetCategory)))
            {
                RefreshBudgetCategoryDGV(catEnum);
            }

            foreach(var dgv in categoryDgvs.Values) { if(dgv != null) dgv.CellValueChanged += dgvBudget_CellValueChanged; }
        }

        private void RefreshBudgetCategoryDGV(BudgetCategory category)
        {
            if (!reverseCategoryEnumMap.TryGetValue(category, out string categoryKey)) return;
            if (!categoryDgvs.TryGetValue(categoryKey, out DataGridView dgv)) return;
            if (dgv == null) return;

            var budgetLinesForCategory = _currentProject.DetailedBudgetLines
                                            .Where(bl => bl.Category == category)
                                            .OrderBy(bl => bl.BudgetLineID == 0 ? int.MaxValue : bl.BudgetLineID)
                                            .ThenBy(bl => bl.Remarks)
                                            .ToList();

            dgv.DataSource = null;
            dgv.DataSource = new BindingList<DetailedBudgetLine>(budgetLinesForCategory);
        }

        #endregion

        private void InitializeLogFrameUI()
        {
            if (this.btnAddOutcome == null)
            {
                Console.WriteLine("Error: btnAddOutcome control is null during InitializeLogFrameUI.");
                return;
            }
            this.btnAddOutcome.Click += new System.EventHandler(this.btnAddOutcome_Click);

            if (_isEditMode && _currentProject.Outcomes != null && _currentProject.Outcomes.Any())
            {
                RenderAllOutcomes();
            }
            else
            {
                ClearOutcomePanels();
            }
        }

        private void btnAddOutcome_Click(object sender, EventArgs e)
        {
            Outcome newOutcome = new Outcome { ProjectID = _currentProject.ProjectID, Project = _currentProject };
            if (_currentProject.Outcomes == null) _currentProject.Outcomes = new HashSet<Outcome>();
            _currentProject.Outcomes.Add(newOutcome);
            RenderAllOutcomes();
        }

        private void ClearOutcomePanels()
        {
            if (pnlLogFrameMain == null) return;

            var outcomePanels = pnlLogFrameMain.Controls.OfType<Panel>().ToList();
            foreach (var panel in outcomePanels)
            {
                 pnlLogFrameMain.Controls.Remove(panel);
                 panel.Dispose();
            }
        }

        private void RenderAllOutcomes()
        {
            if (pnlLogFrameMain == null) {
                 Console.WriteLine("Error: pnlLogFrameMain is null in RenderAllOutcomes.");
                 return;
            }
            ClearOutcomePanels();
            pnlLogFrameMain.SuspendLayout();

            int outcomeCounter = 0;
            if (_currentProject.Outcomes == null) _currentProject.Outcomes = new HashSet<Outcome>();

            foreach (var outcome in _currentProject.Outcomes.ToList())
            {
                outcomeCounter++;
                Panel pnlDynamicOutcome = new Panel
                {
                    Name = $"pnlOutcome_{outcomeCounter}",
                    AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5),
                    Margin = new Padding(3, 3, 3, 10), Dock = DockStyle.Top
                };

                TableLayoutPanel tlpOutcomeHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 3, Padding = new Padding(0,0,0,5) };
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                Label lblOutcomeTitle = new Label { Text = $"Outcome {outcomeCounter}:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 3, 0) };

                PlaceholderTextBox txtOutcomeDesc = new PlaceholderTextBox
                {
                    Text = outcome.OutcomeDescription, Multiline = true, ScrollBars = ScrollBars.Vertical,
                    Height = 40, Dock = DockStyle.Fill, Tag = outcome, PlaceholderText = "Enter outcome description"
                };
                txtOutcomeDesc.TextChanged += (s, ev) => {
                    var tb = s as PlaceholderTextBox; var oc = tb.Tag as Outcome;
                    if (oc != null) oc.OutcomeDescription = tb.Text;
                };

                Button btnDeleteOutcome = new Button { Text = "Delete Outcome", Tag = outcome, ForeColor = Color.Red, Width = 120, Anchor = AnchorStyles.Right | AnchorStyles.Top, AutoSize=true };
                btnDeleteOutcome.Click += BtnDeleteOutcome_Click;

                tlpOutcomeHeader.Controls.Add(lblOutcomeTitle, 0, 0);
                tlpOutcomeHeader.Controls.Add(txtOutcomeDesc, 1, 0);
                tlpOutcomeHeader.Controls.Add(btnDeleteOutcome, 2, 0);

                Button btnAddOutput = new Button { Text = "Add Output to this Outcome", Tag = outcome, Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0,5,0,5) };
                btnAddOutput.Click += BtnAddOutputToOutcome_Click;

                Panel pnlOutputsContainer = new Panel
                {
                    Name = $"pnlOutputsContainer_Outcome{outcomeCounter}", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top, Padding = new Padding(20, 5, 0, 5)
                };

                pnlDynamicOutcome.Controls.Add(pnlOutputsContainer);
                pnlDynamicOutcome.Controls.Add(btnAddOutput);
                pnlDynamicOutcome.Controls.Add(tlpOutcomeHeader);

                RenderOutputsForOutcome(outcome, pnlOutputsContainer);

                pnlLogFrameMain.Controls.Add(pnlDynamicOutcome);
                pnlLogFrameMain.Controls.SetChildIndex(pnlDynamicOutcome, 0);
            }
            pnlLogFrameMain.ResumeLayout(true);
        }

        private void BtnDeleteOutcome_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Outcome outcomeToDelete)) return;

            if (MessageBox.Show(
                $"Are you sure you want to delete this outcome and all its associated outputs, indicators, and activities?
Outcome: {outcomeToDelete.OutcomeDescription}",
                "Confirm Delete Outcome", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _currentProject.Outcomes.Remove(outcomeToDelete);
                RenderAllOutcomes();
            }
        }

        private void BtnAddOutputToOutcome_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Outcome outcome)) return;

            Output newOutput = new Output { OutcomeID = outcome.OutcomeID, Outcome = outcome };
            if (outcome.Outputs == null) outcome.Outputs = new HashSet<Output>();
            outcome.Outputs.Add(newOutput);

            Panel outcomePanel = btn.Parent as Panel;
            Panel pnlOutputsContainer = outcomePanel?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlOutputsContainer_Outcome"));
            if (pnlOutputsContainer != null)
            {
                RenderOutputsForOutcome(outcome, pnlOutputsContainer);
            } else {
                RenderAllOutcomes();
            }
        }

        private void RenderOutputsForOutcome(Outcome outcome, Panel parentOutputPanel)
        {
            parentOutputPanel.SuspendLayout();
            parentOutputPanel.Controls.Clear();

            if (outcome.Outputs == null) outcome.Outputs = new HashSet<Output>();
            int outputCounter = 0;

            foreach (var outputInstance in outcome.Outputs.ToList())
            {
                outputCounter++;
                Panel pnlDynamicOutput = new Panel
                {
                    Name = $"pnlOutput_{outcome.OutcomeID}_{outputCounter}", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5), Margin = new Padding(0, 0, 0, 10), Dock = DockStyle.Top
                };

                TableLayoutPanel tlpOutputHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 3, Padding = new Padding(0,0,0,5) };
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                Label lblOutputTitle = new Label { Text = $"Output {outputCounter}:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0,6,3,0) };

                PlaceholderTextBox txtOutputDesc = new PlaceholderTextBox
                {
                    Text = outputInstance.OutputDescription, Multiline = true, ScrollBars = ScrollBars.Vertical,
                    Height = 40, Dock = DockStyle.Fill, Tag = outputInstance, PlaceholderText = "Enter output description"
                };
                txtOutputDesc.TextChanged += (s, ev) => {
                    var tb = s as PlaceholderTextBox; var op = tb.Tag as Output;
                    if (op != null) op.OutputDescription = tb.Text;
                };

                Button btnDeleteOutput = new Button { Text = "Delete Output", Tag = new Tuple<Outcome, Output>(outcome, outputInstance), ForeColor = Color.Red, Width =110, Anchor = AnchorStyles.Right | AnchorStyles.Top, AutoSize=true };
                btnDeleteOutput.Click += BtnDeleteOutput_Click;

                tlpOutputHeader.Controls.Add(lblOutputTitle, 0, 0);
                tlpOutputHeader.Controls.Add(txtOutputDesc, 1, 0);
                tlpOutputHeader.Controls.Add(btnDeleteOutput, 2, 0);

                FlowLayoutPanel pnlButtonsForOutput = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0,5,0,5), WrapContents=false };
                Button btnAddIndicator = new Button { Text = "Add Indicator", Tag = outputInstance, AutoSize=true };
                btnAddIndicator.Click += BtnAddIndicator_Click;
                Button btnAddActivity = new Button { Text = "Add Activity", Tag = outputInstance, AutoSize=true, Margin = new Padding(5,0,0,0) };
                btnAddActivity.Click += BtnAddActivity_Click;

                pnlButtonsForOutput.Controls.Add(btnAddIndicator);
                pnlButtonsForOutput.Controls.Add(btnAddActivity);

                Panel pnlIndicatorsContainer = new Panel { Name = $"pnlIndicatorsContainer_Output{outputCounter}", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Padding = new Padding(15, 5,0,5), MinimumSize = new Size(0,5) };
                Panel pnlActivitiesContainer = new Panel { Name = $"pnlActivitiesContainer_Output{outputCounter}", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Padding = new Padding(15, 5,0,5), MinimumSize = new Size(0,5) };

                pnlDynamicOutput.Controls.Add(pnlActivitiesContainer);
                pnlDynamicOutput.Controls.Add(pnlIndicatorsContainer);
                pnlDynamicOutput.Controls.Add(pnlButtonsForOutput);
                pnlDynamicOutput.Controls.Add(tlpOutputHeader);

                parentOutputPanel.Controls.Add(pnlDynamicOutput);
                parentOutputPanel.Controls.SetChildIndex(pnlDynamicOutput, 0);

                RenderIndicatorsForOutput(outputInstance, pnlIndicatorsContainer);
                RenderActivitiesForOutput(outputInstance, pnlActivitiesContainer);
            }
            parentOutputPanel.ResumeLayout(true);
        }

        private void BtnDeleteOutput_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Tuple<Outcome, Output> tags)) return;
            Outcome parentOutcome = tags.Item1;
            Output outputToDelete = tags.Item2;

            if (MessageBox.Show(
                $"Are you sure you want to delete this output and its related indicators and activities?
Output: {outputToDelete.OutputDescription}",
                "Confirm Delete Output", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                parentOutcome.Outputs.Remove(outputToDelete);
                Control current = btn;
                Panel dynamicOutcomePanel = null;
                while(current != null && current.Parent != null) {
                    if(current.Parent.Name.StartsWith("pnlOutcome_")) {
                        dynamicOutcomePanel = current.Parent as Panel;
                        break;
                    }
                    current = current.Parent;
                }

                if (dynamicOutcomePanel != null) {
                    Panel pnlOutputsContainer = dynamicOutcomePanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlOutputsContainer_Outcome"));
                    if (pnlOutputsContainer != null) RenderOutputsForOutcome(parentOutcome, pnlOutputsContainer);
                    else RenderAllOutcomes();
                } else {
                    RenderAllOutcomes();
                }
            }
        }

        private void BtnAddIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Output output)) return;

            ProjectIndicator newIndicator = new ProjectIndicator {
                ProjectID = _currentProject.ProjectID, Project = _currentProject,
                OutputID = output.OutputID, Output = output,
                OutcomeID = output.Outcome?.OutcomeID, Outcome = output.Outcome
            };
            if (output.ProjectIndicators == null) output.ProjectIndicators = new HashSet<ProjectIndicator>();
            output.ProjectIndicators.Add(newIndicator);

            Panel pnlDynamicOutput = btn.Parent.Parent as Panel;
            Panel pnlIndicatorsContainer = pnlDynamicOutput?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlIndicatorsContainer_Output"));
            if (pnlIndicatorsContainer != null) RenderIndicatorsForOutput(output, pnlIndicatorsContainer);
        }

        private void RenderIndicatorsForOutput(Output output, Panel parentIndicatorPanel)
        {
            parentIndicatorPanel.SuspendLayout();
            parentIndicatorPanel.Controls.Clear();

            if (output.ProjectIndicators == null) output.ProjectIndicators = new HashSet<ProjectIndicator>();
            int indicatorCounter = 0;

            foreach (var indicatorInstance in output.ProjectIndicators.ToList())
            {
                indicatorCounter++;
                Panel pnlDynamicIndicator = new Panel { Name = $"pnlIndicator_{output.OutputID}_{indicatorCounter}", AutoSize = true, AutoSizeMode=AutoSizeMode.GrowAndShrink, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(3), Margin = new Padding(0, 0, 0, 5), Dock = DockStyle.Top };

                TableLayoutPanel tlpIndicatorRoot = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode=AutoSizeMode.GrowAndShrink, ColumnCount = 1};
                tlpIndicatorRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpIndicatorRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpIndicatorRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                tlpIndicatorRoot.Controls.Add(new Label { Text = $"Indicator {indicatorCounter}:", AutoSize=true, Margin = new Padding(0,3,0,0), Font = new Font(this.Font ?? SystemFonts.DefaultFont, FontStyle.Bold)},0,0);

                PlaceholderTextBox txtIndicatorName = new PlaceholderTextBox
                {
                    Text = indicatorInstance.IndicatorName, Dock = DockStyle.Fill, Multiline=true, Height=35,
                    Tag = indicatorInstance, PlaceholderText = "Indicator Description"
                };
                txtIndicatorName.TextChanged += (s, ev) => {
                    var tb = s as PlaceholderTextBox; var pi = tb.Tag as ProjectIndicator;
                    if (pi != null) pi.IndicatorName = tb.Text;
                };
                tlpIndicatorRoot.Controls.Add(txtIndicatorName, 0,1);

                FlowLayoutPanel flpTargetsAndDelete = new FlowLayoutPanel {
                    FlowDirection = FlowDirection.LeftToRight, AutoSize = true, AutoSizeMode=AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top, WrapContents = true, Padding = new Padding(0,3,0,0)
                };

                Label lblTargets = new Label { Text = "Targets:", AutoSize = true, Margin = new Padding(0, 3, 3, 0), Font = new Font(this.Font ?? SystemFonts.DefaultFont, FontStyle.Bold) };
                flpTargetsAndDelete.Controls.Add(lblTargets);

                NumericUpDown nudTargetMen = new NumericUpDown { Value = indicatorInstance.TargetMen, Maximum = 1000000, Width = 55, Tag = indicatorInstance };
                nudTargetMen.ValueChanged += (s,ev) => { var nud = s as NumericUpDown; var pi = nud.Tag as ProjectIndicator; if(pi!=null) pi.TargetMen = (int)nud.Value; };
                flpTargetsAndDelete.Controls.Add(new Label { Text = "M:", AutoSize = true, Margin = new Padding(3,3,0,0) }); flpTargetsAndDelete.Controls.Add(nudTargetMen);

                NumericUpDown nudTargetWomen = new NumericUpDown { Value = indicatorInstance.TargetWomen, Maximum = 1000000, Width = 55, Tag = indicatorInstance };
                nudTargetWomen.ValueChanged += (s,ev) => { var nud = s as NumericUpDown; var pi = nud.Tag as ProjectIndicator; if(pi!=null) pi.TargetWomen = (int)nud.Value; };
                flpTargetsAndDelete.Controls.Add(new Label { Text = "W:", AutoSize = true, Margin = new Padding(3,3,0,0) }); flpTargetsAndDelete.Controls.Add(nudTargetWomen);

                NumericUpDown nudTargetBoys = new NumericUpDown { Value = indicatorInstance.TargetBoys, Maximum = 1000000, Width = 55, Tag = indicatorInstance };
                nudTargetBoys.ValueChanged += (s,ev) => { var nud = s as NumericUpDown; var pi = nud.Tag as ProjectIndicator; if(pi!=null) pi.TargetBoys = (int)nud.Value; };
                flpTargetsAndDelete.Controls.Add(new Label { Text = "B:", AutoSize = true, Margin = new Padding(3,3,0,0) }); flpTargetsAndDelete.Controls.Add(nudTargetBoys);

                NumericUpDown nudTargetGirls = new NumericUpDown { Value = indicatorInstance.TargetGirls, Maximum = 1000000, Width = 55, Tag = indicatorInstance };
                nudTargetGirls.ValueChanged += (s,ev) => { var nud = s as NumericUpDown; var pi = nud.Tag as ProjectIndicator; if(pi!=null) pi.TargetGirls = (int)nud.Value; };
                flpTargetsAndDelete.Controls.Add(new Label { Text = "G:", AutoSize = true, Margin = new Padding(3,3,0,0) }); flpTargetsAndDelete.Controls.Add(nudTargetGirls);

                NumericUpDown nudTargetTotal = new NumericUpDown { Value = indicatorInstance.TargetTotal, Maximum = 4000000, Width = 60, Enabled = true, Tag = indicatorInstance };
                nudTargetTotal.ValueChanged += (s,ev) => { var nud = s as NumericUpDown; var pi = nud.Tag as ProjectIndicator; if(pi!=null) pi.TargetTotal = (int)nud.Value; };
                flpTargetsAndDelete.Controls.Add(new Label { Text = "Tot:", AutoSize = true, Margin = new Padding(3,3,0,0) }); flpTargetsAndDelete.Controls.Add(nudTargetTotal);

                Button btnDeleteIndicator = new Button { Text = "Delete Indicator", Tag = new Tuple<Output, ProjectIndicator>(output, indicatorInstance), ForeColor = Color.DarkRed, AutoSize=true, Margin = new Padding(10,0,0,0) };
                btnDeleteIndicator.Click += BtnDeleteIndicator_Click;
                flpTargetsAndDelete.Controls.Add(btnDeleteIndicator);

                tlpIndicatorRoot.Controls.Add(flpTargetsAndDelete, 0, 2);
                pnlDynamicIndicator.Controls.Add(tlpIndicatorRoot);
                parentIndicatorPanel.Controls.Add(pnlDynamicIndicator);
                parentIndicatorPanel.Controls.SetChildIndex(pnlDynamicIndicator, 0);
            }
            parentIndicatorPanel.ResumeLayout(true);
        }

        private void BtnDeleteIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Tuple<Output, ProjectIndicator> tags)) return;
            Output parentOutput = tags.Item1;
            ProjectIndicator indicatorToDelete = tags.Item2;

            if (MessageBox.Show(
                $"Are you sure you want to delete this indicator?
Indicator: {indicatorToDelete.IndicatorName}",
                "Confirm Delete Indicator", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                parentOutput.ProjectIndicators.Remove(indicatorToDelete);
                Panel pnlIndicator = btn.Parent.Parent.Parent as Panel;
                Panel pnlIndicatorsContainer = pnlIndicator?.Parent as Panel;
                if (pnlIndicatorsContainer != null)
                {
                    RenderIndicatorsForOutput(parentOutput, pnlIndicatorsContainer);
                } else {
                    Control current = btn; Panel dynamicOutputPanel = null;
                    while(current != null && current.Parent != null) {
                       if(current.Parent.Name.StartsWith("pnlOutput_")) { dynamicOutputPanel = current.Parent as Panel; break; }
                       current = current.Parent;
                    }
                    if (dynamicOutputPanel != null) {
                         Panel container = dynamicOutputPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlIndicatorsContainer_"));
                         if (container != null) RenderIndicatorsForOutput(parentOutput, container); else RenderAllOutcomes();
                    } else RenderAllOutcomes();
                }
            }
        }

        private void BtnAddActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Output output)) return;

            Activity newActivity = new Activity {
                OutputID = output.OutputID, Output = output,
                OutcomeID = output.Outcome?.OutcomeID, Outcome = output.Outcome
            };
            if (output.Activities == null) output.Activities = new HashSet<Activity>();
            output.Activities.Add(newActivity);

            Panel pnlDynamicOutput = btn.Parent.Parent as Panel;
            Panel pnlActivitiesContainer = pnlDynamicOutput?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlActivitiesContainer_Output"));
            if (pnlActivitiesContainer != null) RenderActivitiesForOutput(output, pnlActivitiesContainer);
        }

        private void RenderActivitiesForOutput(Output output, Panel parentActivityPanel)
        {
            parentActivityPanel.SuspendLayout();
            parentActivityPanel.Controls.Clear();

            if (output.Activities == null) output.Activities = new HashSet<Activity>();
            int activityCounter = 0;

            foreach (var activityInstance in output.Activities.ToList())
            {
                activityCounter++;
                Panel pnlDynamicActivity = new Panel { Name = $"pnlActivity_{output.OutputID}_{activityCounter}", AutoSize = true, AutoSizeMode=AutoSizeMode.GrowAndShrink, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(3), Margin = new Padding(0,0,0,5), Dock = DockStyle.Top };

                TableLayoutPanel tlpActivity = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode=AutoSizeMode.GrowAndShrink, ColumnCount = 3 };
                tlpActivity.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tlpActivity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tlpActivity.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                PlaceholderTextBox txtActivityDesc = new PlaceholderTextBox
                {
                    Text = activityInstance.ActivityDescription, Dock = DockStyle.Fill, Multiline=true, Height=35,
                    Tag = activityInstance, PlaceholderText = "Activity Description"
                };
                txtActivityDesc.TextChanged += (s, ev) => {
                    var tb = s as PlaceholderTextBox; var act = tb.Tag as Activity;
                    if (act != null) act.ActivityDescription = tb.Text;
                };

                Button btnDeleteActivity = new Button { Text = "Delete Activity", Tag = new Tuple<Output, Activity>(output, activityInstance), ForeColor = Color.DarkRed, AutoSize=true, Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDeleteActivity.Click += BtnDeleteActivity_Click;

                tlpActivity.Controls.Add(new Label { Text = $"Activity {activityCounter}:", AutoSize = true, Margin = new Padding(0,6,3,0), Font = new Font(this.Font ?? SystemFonts.DefaultFont, FontStyle.Bold)}, 0, 0);
                tlpActivity.Controls.Add(txtActivityDesc, 1, 0);
                tlpActivity.Controls.Add(btnDeleteActivity, 2, 0);

                pnlDynamicActivity.Controls.Add(tlpActivity);
                parentActivityPanel.Controls.Add(pnlDynamicActivity);
                parentActivityPanel.Controls.SetChildIndex(pnlDynamicActivity, 0);
            }
            parentActivityPanel.ResumeLayout(true);
        }

        private void BtnDeleteActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Tuple<Output, Activity> tags)) return;
            Output parentOutput = tags.Item1;
            Activity activityToDelete = tags.Item2;

            if (MessageBox.Show(
                $"Are you sure you want to delete this activity?
Activity: {activityToDelete.ActivityDescription}",
                "Confirm Delete Activity", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                parentOutput.Activities.Remove(activityToDelete);
                Panel pnlActivity = btn.Parent.Parent as Panel;
                Panel pnlActivitiesContainer = pnlActivity?.Parent as Panel;
                if (pnlActivitiesContainer != null)
                {
                    RenderActivitiesForOutput(parentOutput, pnlActivitiesContainer);
                } else {
                     Control current = btn; Panel dynamicOutputPanel = null;
                     while(current != null && current.Parent != null) {
                        if(current.Parent.Name.StartsWith("pnlOutput_")) { dynamicOutputPanel = current.Parent as Panel; break; }
                        current = current.Parent;
                     }
                    if (dynamicOutputPanel != null) {
                         Panel container = dynamicOutputPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlActivitiesContainer_"));
                         if (container != null) RenderActivitiesForOutput(parentOutput, container); else RenderAllOutcomes();
                    } else RenderAllOutcomes();
                }
            }
        }

        private void SetAccessibilityProperties()
        {
            if(txtProjectName != null) {
                txtProjectName.AccessibleName = "Project Name";
                txtProjectName.AccessibleDescription = "Enter the full official name of the project. This field is required.";
            }
            if(txtProjectCode != null) {
                txtProjectCode.AccessibleName = "Project Code";
                txtProjectCode.AccessibleDescription = "Enter the unique code for the project (optional).";
            }
            if(dtpStartDate != null) {
                dtpStartDate.AccessibleName = "Project Start Date";
                dtpStartDate.AccessibleDescription = "Select the start date of the project. Check the box to enable date selection.";
            }
            if(dtpEndDate != null) {
                dtpEndDate.AccessibleName = "Project End Date";
                dtpEndDate.AccessibleDescription = "Select the end date of the project. Check the box to enable date selection.";
            }
            if(txtLocation != null) {
                txtLocation.AccessibleName = "Project Location";
                txtLocation.AccessibleDescription = "Enter the geographical location(s) of the project.";
            }
            if(txtOverallObjective != null) {
                txtOverallObjective.AccessibleName = "Overall Objective";
                txtOverallObjective.AccessibleDescription = "Describe the main goal or overall objective of the project.";
            }
            if(txtStatus != null) {
                txtStatus.AccessibleName = "Project Status";
                txtStatus.AccessibleDescription = "Enter the current status of the project (e.g., Planning, Active, Completed).";
            }
            if(txtDonor != null) {
                txtDonor.AccessibleName = "Project Donor";
                txtDonor.AccessibleDescription = "Enter the name of the donor or funding source for the project.";
            }
            if(nudTotalBudget != null) {
                nudTotalBudget.AccessibleName = "Total Project Budget";
                nudTotalBudget.AccessibleDescription = "Enter the total budget amount for the project.";
            }
            if(cmbSection != null) {
                cmbSection.AccessibleName = "Organizational Section";
                cmbSection.AccessibleDescription = "Select the organizational section or department responsible for the project.";
            }
            if(cmbManager != null) {
                cmbManager.AccessibleName = "Project Manager";
                cmbManager.AccessibleDescription = "Select the user who will manage this project.";
            }
            if(btnSave != null) {
                btnSave.AccessibleName = "Save Project Details";
                btnSave.AccessibleDescription = "Saves the current project information to the database.";
            }
            if(btnCancel != null) {
                btnCancel.AccessibleName = "Cancel Editing";
                btnCancel.AccessibleDescription = "Discards any changes and closes the project details form.";
            }
        }

        private async Task LoadComboBoxesAsync()
        {
            if (cmbSection == null || cmbManager == null) return;

            cmbSection.DisplayMember = "Text";
            cmbSection.ValueMember = "Value";
            cmbSection.Items.Clear();
            cmbSection.Items.Add(new ComboboxItem { Text = "(No Section)", Value = 0 });
            try
            {
                List<Section> sections = await _sectionService.GetSectionsAsync();
                foreach (var section in sections)
                {
                    cmbSection.Items.Add(new ComboboxItem { Text = section.SectionName, Value = section.SectionID });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sections: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (_currentProject != null && _currentProject.SectionID.HasValue)
            {
                foreach (ComboboxItem item in cmbSection.Items) { if (item.Value == _currentProject.SectionID.Value) { cmbSection.SelectedItem = item; break; } }
            } else if (!_isEditMode && _initialSectionId.HasValue) {
                 foreach (ComboboxItem item in cmbSection.Items) { if (item.Value == _initialSectionId.Value) { cmbSection.SelectedItem = item; break; } }
            }
            if (cmbSection.SelectedItem == null && cmbSection.Items.Count > 0) cmbSection.SelectedIndex = 0;


            cmbManager.DisplayMember = "Text";
            cmbManager.ValueMember = "Value";
            cmbManager.Items.Clear();
            cmbManager.Items.Add(new ComboboxItem { Text = "(No Manager)", Value = 0 });
            cmbManager.Items.Add(new ComboboxItem { Text = "Default Manager 1 (User ID 1)", Value = 1 });
            cmbManager.Items.Add(new ComboboxItem { Text = "Default Manager 2 (User ID 2)", Value = 2 });

            if (_currentProject != null && _currentProject.ManagerUserID.HasValue)
            {
                 foreach (ComboboxItem item in cmbManager.Items) { if (item.Value == _currentProject.ManagerUserID.Value) { cmbManager.SelectedItem = item; break; } }
            }
             if (cmbManager.SelectedItem == null && cmbManager.Items.Count > 0) cmbManager.SelectedIndex = 0;
        }

        private void PopulateControls()
        {
            if (_currentProject == null) return;
            if(txtProjectName != null) txtProjectName.Text = _currentProject.ProjectName;
            if(txtProjectCode != null) txtProjectCode.Text = _currentProject.ProjectCode;
            if(dtpStartDate != null) {
                if (_currentProject.StartDate.HasValue) { dtpStartDate.Value = _currentProject.StartDate.Value; dtpStartDate.Checked = true; } else { dtpStartDate.Checked = false; }
            }
            if(dtpEndDate != null) {
                if (_currentProject.EndDate.HasValue) { dtpEndDate.Value = _currentProject.EndDate.Value; dtpEndDate.Checked = true; } else { dtpEndDate.Checked = false; }
            }
            if(txtLocation != null) txtLocation.Text = _currentProject.Location;
            if(txtOverallObjective != null) txtOverallObjective.Text = _currentProject.OverallObjective;
            if(txtStatus != null) txtStatus.Text = _currentProject.Status;
            if(txtDonor != null) txtDonor.Text = _currentProject.Donor;
            if(nudTotalBudget != null) nudTotalBudget.Value = _currentProject.TotalBudget ?? 0;
        }

        private bool CollectAndValidateData()
        {
            if (txtProjectName == null || string.IsNullOrWhiteSpace(txtProjectName.Text) || (txtProjectName is PlaceholderTextBox ptb && ptb.IsPlaceholderActive))
            {
                MessageBox.Show("Project Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtProjectName?.Focus();
                return false;
            }
            _currentProject.ProjectName = txtProjectName.Text.Trim();
            _currentProject.ProjectCode = txtProjectCode == null || string.IsNullOrWhiteSpace(txtProjectCode.Text) ? null : txtProjectCode.Text.Trim();
            _currentProject.StartDate = dtpStartDate != null && dtpStartDate.Checked ? (DateTime?)dtpStartDate.Value : null;
            _currentProject.EndDate = dtpEndDate != null && dtpEndDate.Checked ? (DateTime?)dtpEndDate.Value : null;

            if (_currentProject.StartDate.HasValue && _currentProject.EndDate.HasValue && _currentProject.EndDate.Value < _currentProject.StartDate.Value)
            {
                MessageBox.Show("End Date cannot be earlier than Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtpEndDate?.Focus();
                return false;
            }
            _currentProject.Location = (txtLocation is PlaceholderTextBox ptbLoc && ptbLoc.IsPlaceholderActive) ? null : (txtLocation == null || string.IsNullOrWhiteSpace(txtLocation.Text) ? null : txtLocation.Text.Trim());
            _currentProject.OverallObjective = (txtOverallObjective is PlaceholderTextBox ptbObj && ptbObj.IsPlaceholderActive) ? null : (txtOverallObjective == null || string.IsNullOrWhiteSpace(txtOverallObjective.Text) ? null : txtOverallObjective.Text.Trim());
            _currentProject.Status = txtStatus == null || string.IsNullOrWhiteSpace(txtStatus.Text) ? null : txtStatus.Text.Trim();
            _currentProject.Donor = txtDonor == null || string.IsNullOrWhiteSpace(txtDonor.Text) ? null : txtDonor.Text.Trim();
            _currentProject.TotalBudget = nudTotalBudget?.Value ?? 0;

            if (cmbSection != null && cmbSection.SelectedItem != null && ((ComboboxItem)cmbSection.SelectedItem).Value != 0) { _currentProject.SectionID = ((ComboboxItem)cmbSection.SelectedItem).Value; } else { _currentProject.SectionID = null; }
            if (cmbManager != null && cmbManager.SelectedItem != null && ((ComboboxItem)cmbManager.SelectedItem).Value != 0) { _currentProject.ManagerUserID = ((ComboboxItem)cmbManager.SelectedItem).Value; } else { _currentProject.ManagerUserID = null; }

            if (!_isEditMode) { _currentProject.CreatedAt = DateTime.UtcNow; }
            _currentProject.UpdatedAt = DateTime.UtcNow;

            // LogFrame and Budget Validations
            if (_currentProject.Outcomes != null)
            {
                int outcomeCounter = 0;
                foreach (var outcome in _currentProject.Outcomes)
                {
                    outcomeCounter++;
                    if (string.IsNullOrWhiteSpace(outcome.OutcomeDescription))
                    {
                        MessageBox.Show($"Outcome {outcomeCounter} description is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (tabControlMain != null) tabControlMain.SelectedTab = tabPageLogFrame;
                        return false;
                    }
                    if (outcome.Outputs != null)
                    {
                        int outputCounter = 0;
                        foreach (var output in outcome.Outputs)
                        {
                            outputCounter++;
                            if (string.IsNullOrWhiteSpace(output.OutputDescription))
                            {
                                MessageBox.Show($"Output {outputCounter} (under Outcome {outcomeCounter}) description is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                if (tabControlMain != null) tabControlMain.SelectedTab = tabPageLogFrame;
                                return false;
                            }
                            if (output.ProjectIndicators != null)
                            {
                                int indicatorCounter = 0;
                                foreach (var indicator in output.ProjectIndicators)
                                {
                                    indicatorCounter++;
                                    if (string.IsNullOrWhiteSpace(indicator.IndicatorName))
                                    {
                                        MessageBox.Show($"Indicator {indicatorCounter} (Output {outputCounter}, Outcome {outcomeCounter}) description is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        if (tabControlMain != null) tabControlMain.SelectedTab = tabPageLogFrame;
                                        return false;
                                    }
                                }
                            }
                            if (output.Activities != null)
                            {
                                int activityCounter = 0;
                                foreach (var activity in output.Activities)
                                {
                                    activityCounter++;
                                    if (string.IsNullOrWhiteSpace(activity.ActivityDescription))
                                    {
                                        MessageBox.Show($"Activity {activityCounter} (Output {outputCounter}, Outcome {outcomeCounter}) description is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        if (tabControlMain != null) tabControlMain.SelectedTab = tabPageLogFrame;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (_currentProject.DetailedBudgetLines != null)
            {
                 int lineCounter = 0;
                 foreach (var budgetLine in _currentProject.DetailedBudgetLines.OrderBy(bl => bl.Category).ThenBy(bl => bl.Remarks))
                 {
                     lineCounter++;
                     if (string.IsNullOrWhiteSpace(budgetLine.Remarks))
                     {
                         MessageBox.Show($"A budget line in category '{budgetLine.Category.ToString()}' (around item {lineCounter}) has an empty description/remarks.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                         if (tabControlMain != null) tabControlMain.SelectedTab = tabPageBudget;
                         return false;
                     }
                     if (budgetLine.Quantity <= 0)
                     {
                          MessageBox.Show($"Budget line '{budgetLine.Remarks}' (Category: {budgetLine.Category.ToString()}) has an invalid Quantity. Must be > 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                          if (tabControlMain != null) tabControlMain.SelectedTab = tabPageBudget;
                          return false;
                     }
                     if (budgetLine.UnitCost <= 0)
                     {
                          MessageBox.Show($"Budget line '{budgetLine.Remarks}' (Category: {budgetLine.Category.ToString()}) has an invalid Unit Cost. Must be > 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                          if (tabControlMain != null) tabControlMain.SelectedTab = tabPageBudget;
                          return false;
                     }
                 }
            }

            return true;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!CollectAndValidateData()) { return; }
            if(btnSave != null) btnSave.Enabled = false;
            if(btnCancel != null) btnCancel.Enabled = false;
            this.UseWaitCursor = true;
            try
            {
                bool success = await _projectService.SaveProjectAsync(_currentProject);
                if (success)
                {
                    MessageBox.Show("Project saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save project. Check logs for details or ensure related entities (Outcomes, Outputs, etc.) are correctly handled during save.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if(btnSave != null) btnSave.Enabled = true;
                if(btnCancel != null) btnCancel.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    // PlaceholderTextBox class remains unchanged below this line
    public class PlaceholderTextBox : TextBox
    {
        private string _placeholderText = "";
        private bool _isPlaceholderActive = false;
        private Color _originalForeColor;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The placeholder text displayed when the control has no text.")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set {
                _placeholderText = value;
                if (this.IsHandleCreated && (string.IsNullOrEmpty(this.Text) || (_isPlaceholderActive && this.Text != _placeholderText )))
                {
                     SetPlaceholder();
                } else if (!this.IsHandleCreated && string.IsNullOrEmpty(this.Text)) {
                   // If handle not created yet, SetPlaceholder will be called by OnHandleCreated
                }
            }
        }

        [Browsable(false)]
        public bool IsPlaceholderActive { get { return _isPlaceholderActive; } }

        public PlaceholderTextBox() : base()
        {
            _originalForeColor = this.ForeColor;
            this.GotFocus += (sender, e) => { RemovePlaceholder(); };
            this.LostFocus += (sender, e) => { SetPlaceholder(); };
            if (this.IsHandleCreated) SetPlaceholder(); else this.HandleCreated += (s,ev) => SetPlaceholder();
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(_placeholderText))
            {
                this.Text = _placeholderText;
                this.ForeColor = Color.Gray;
                _isPlaceholderActive = true;
            } else if (!string.IsNullOrEmpty(this.Text) && this.Text != _placeholderText) {
                _isPlaceholderActive = false;
                this.ForeColor = _originalForeColor;
            } else if (string.IsNullOrEmpty(this.Text)) {
                 _isPlaceholderActive = false;
                this.ForeColor = _originalForeColor;
                if (!string.IsNullOrEmpty(_placeholderText)) {
                     this.Text = _placeholderText;
                     this.ForeColor = Color.Gray;
                     _isPlaceholderActive = true;
                }
            }
        }

        private void RemovePlaceholder()
        {
            if (_isPlaceholderActive && this.Text == _placeholderText)
            {
                this.Text = "";
                this.ForeColor = _originalForeColor;
                _isPlaceholderActive = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!this.Focused && string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(_placeholderText))
            {
                if (!_isPlaceholderActive) SetPlaceholder();
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (this.Focused && _isPlaceholderActive && this.Text != _placeholderText)
            {
                _isPlaceholderActive = false;
                this.ForeColor = _originalForeColor;
            } else if (!this.Focused && !string.IsNullOrEmpty(this.Text) && this.Text != _placeholderText) {
                 _isPlaceholderActive = false;
                this.ForeColor = _originalForeColor;
            } else if (string.IsNullOrEmpty(this.Text) && !this.Focused && !_isPlaceholderActive && !string.IsNullOrEmpty(_placeholderText)) {
                SetPlaceholder();
            }
        }
    }
}
