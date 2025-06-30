// Using statements from the original file - confirmed to be sufficient
using HumanitarianProjectManagement.DataAccessLayer;
using HumanitarianProjectManagement.Models;
using ProjectActivity = HumanitarianProjectManagement.Models.Activity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using HumanitarianProjectManagement.UI;
using System.Globalization;
using HumanitarianProjectManagement; // Added for ApplicationState if not implicitly available
using System.Diagnostics; // Added for Debug.WriteLine

// Provided by user in feedback 2024-05-16
namespace HumanitarianProjectManagement.Forms
{
    public partial class ProjectCreateEditForm : Form
    {
        private readonly ProjectService _projectService;
        private readonly SectionService _sectionService;
        private Project _currentProject;
        private readonly bool _isEditMode;
        private int? _initialSectionId;
        private readonly LogFrameService _logFrameService;
        private Panel sidebarPanel; // For LogFrame

        private BudgetTabUserControl _budgetTabControlInstance; // Instance of the new UserControl


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
            _logFrameService = new LogFrameService();
            _initialSectionId = initialSectionId;

            _isEditMode = (projectToEdit != null);
            this.WindowState = FormWindowState.Maximized;

            if (_isEditMode)
            {
                _currentProject = projectToEdit;
                this.Text = $"Edit Project - {_currentProject.ProjectName}";
                if (_currentProject.Outcomes == null) _currentProject.Outcomes = new List<Outcome>();
                PopulateControls();
            }
            else
            {
                _currentProject = new Project();
                this.Text = "Add New Project";
                _currentProject.Outcomes = new List<Outcome>();
                dtpStartDate.Value = DateTime.Now;
                dtpStartDate.Checked = false;
                dtpEndDate.Checked = false;
            }

            this.Load += new System.EventHandler(this.ProjectCreateEditForm_Load);
        }

        private void InitializeBudgetUITab()
        {
            System.Diagnostics.Debug.WriteLine("InitializeBudgetUITab: Started.");
            try
            {
                if (this.tabControlProjectDetails == null)
                {
                    System.Diagnostics.Debug.WriteLine("InitializeBudgetUITab: tabControlProjectDetails is null. Cannot initialize Budget tab.");
                    // Console.WriteLine("Error: tabControlProjectDetails is null. Cannot initialize Budget tab."); // Old logging
                    return;
                }

                _budgetTabControlInstance = new BudgetTabUserControl();
                _budgetTabControlInstance.Dock = DockStyle.Fill;

                TabPage tabPageBudget = new TabPage();
                tabPageBudget.Name = "tabPageBudget";
                tabPageBudget.Text = "Budget";
                tabPageBudget.Controls.Add(_budgetTabControlInstance);

                if (this.tabControlProjectDetails.TabPages.ContainsKey("tabPageBudget"))
                {
                    System.Diagnostics.Debug.WriteLine("InitializeBudgetUITab: tabPageBudget already exists, removing to avoid duplication.");
                    this.tabControlProjectDetails.TabPages.RemoveByKey("tabPageBudget");
                }
                this.tabControlProjectDetails.TabPages.Add(tabPageBudget);
                System.Diagnostics.Debug.WriteLine($"InitializeBudgetUITab: tabPageBudget {(tabControlProjectDetails.TabPages.Contains(tabPageBudget) ? "successfully added" : "NOT found after adding")}. Name: {tabPageBudget.Name}");

                System.Diagnostics.Debug.WriteLine($"InitializeBudgetUITab: _currentProject is {(_currentProject == null ? "null" : "not null, ProjectID: " + _currentProject.ProjectID)}");
                if (_budgetTabControlInstance != null && _currentProject != null)
                {
                    _budgetTabControlInstance.LoadProject(_currentProject);
                }
                else if (_budgetTabControlInstance == null)
                {
                    System.Diagnostics.Debug.WriteLine("InitializeBudgetUITab: _budgetTabControlInstance is null. Cannot load project data.");
                    // Console.WriteLine("Error: _budgetTabControlInstance is null. Cannot load project data."); // Old logging
                }
                else if (_currentProject == null)
                {
                    System.Diagnostics.Debug.WriteLine("InitializeBudgetUITab: _currentProject is null. Budget tab will be initialized without project data.");
                    // Console.WriteLine("Info: _currentProject is null. Budget tab will be initialized without project data."); // Old logging
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeBudgetUITab: EXCEPTION: {ex.ToString()}");
            }
        }

        private async void ProjectCreateEditForm_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ProjectCreateEditForm_Load: Started.");
            System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: tabControlProjectDetails is {(this.tabControlProjectDetails == null ? "null" : "found")}");

            try { await LoadComboBoxesAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: EXCEPTION during LoadComboBoxesAsync: {ex.ToString()}");
                MessageBox.Show("Error loading sections/managers: " + ex.ToString(), "Load Data Exception");
                return;
            }

            InitializeLogFrameUI();
            InitializeActivityPlanTab();
            InitializeBudgetUITab();

            System.Diagnostics.Debug.WriteLine("ProjectCreateEditForm_Load: Attempting to apply role-based visibility for budget tab.");
            TabPage tabPageBudget = null; // Initialize to null
            try
            {
                tabPageBudget = tabControlProjectDetails.TabPages["tabPageBudget"];
                if (tabPageBudget == null)
                {
                    System.Diagnostics.Debug.WriteLine("ProjectCreateEditForm_Load: Budget tab ('tabPageBudget') not found after initialization.");
                }
                else
                {
                    User currentUser = ApplicationState.CurrentUser;
                    System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: ApplicationState.CurrentUser is {(currentUser == null ? "null" : currentUser.Username)}");

                    if (currentUser == null)
                    {
                        System.Diagnostics.Debug.WriteLine("ProjectCreateEditForm_Load: CurrentUser is null, preparing to hide budget tab.");
                        System.Diagnostics.Debug.WriteLine("ProjectCreateEditForm_Load: Condition not met or user lacks permission, attempting to remove tabPageBudget.");
                        tabControlProjectDetails.TabPages.Remove(tabPageBudget);
                    }
                    else
                    {
                        UserService userService = new UserService();
                        List<int> roleIds = await userService.GetRoleIdsForUserAsync(currentUser.UserID);
                        System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: Fetched role IDs for user {currentUser.Username}: {(roleIds == null ? "null" : string.Join(", ", roleIds))}");

                        List<Role> allRoles = await userService.GetAllRolesAsync();
                        System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: Fetched all roles: {(allRoles == null ? "null" : string.Join(", ", allRoles.Select(r => r.RoleName)))}");

                        List<string> userRoleNames = roleIds
                            .Select(id => allRoles.FirstOrDefault(r => r.RoleID == id)?.RoleName)
                            .Where(name => name != null)
                            .ToList();

                        List<string> allowedRoleNames = new List<string> { "Administrator", "Project Manager", "Finance" };
                        System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: Allowed role names for budget tab: {string.Join(", ", allowedRoleNames)}");

                        bool userHasAllowedRole = userRoleNames.Any(userRole => allowedRoleNames.Contains(userRole));
                        System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: User {currentUser.Username} {(userHasAllowedRole ? "HAS" : "DOES NOT HAVE")} an allowed role.");

                        if (!userHasAllowedRole)
                        {
                            System.Diagnostics.Debug.WriteLine("ProjectCreateEditForm_Load: Condition not met or user lacks permission, attempting to remove tabPageBudget.");
                            tabControlProjectDetails.TabPages.Remove(tabPageBudget);
                        }
                    }
                }
                // Final check on tab visibility
                if (tabControlProjectDetails.TabPages.ContainsKey("tabPageBudget")) // Check by key if tabPageBudget variable might be stale after removal
                {
                    System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: tabPageBudget is visible/present.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: tabPageBudget is hidden/removed.");
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: EXCEPTION during role-based visibility: {ex.ToString()}");
                if (tabPageBudget != null && tabControlProjectDetails.TabPages.Contains(tabPageBudget)) // Attempt to remove if error occurred and tab still exists
                {
                    tabControlProjectDetails.TabPages.Remove(tabPageBudget);
                    System.Diagnostics.Debug.WriteLine($"ProjectCreateEditForm_Load: tabPageBudget removed due to exception in role check.");
                }
                MessageBox.Show("Error determining user permissions for the budget tab. The tab will be hidden as a precaution.", "Permissions Error");
            }


            DataGridView dgvActivityPlan = this.Controls.Find("dgvActivityPlan", true).FirstOrDefault() as DataGridView;
            if (dgvActivityPlan != null)
            {
                dgvActivityPlan.CellValueChanged -= dgvActivityPlan_CellValueChanged;
                dgvActivityPlan.CurrentCellDirtyStateChanged -= dgvActivityPlan_CurrentCellDirtyStateChanged;
                dgvActivityPlan.CellValueChanged += dgvActivityPlan_CellValueChanged;
                dgvActivityPlan.CurrentCellDirtyStateChanged += dgvActivityPlan_CurrentCellDirtyStateChanged;
            }

            if (this.dtpStartDate != null) { this.dtpStartDate.ValueChanged -= ProjectDatesChanged_RefreshActivityPlan; this.dtpStartDate.ValueChanged += ProjectDatesChanged_RefreshActivityPlan; }
            if (this.dtpEndDate != null) { this.dtpEndDate.ValueChanged -= ProjectDatesChanged_RefreshActivityPlan; this.dtpEndDate.ValueChanged += ProjectDatesChanged_RefreshActivityPlan; }
        }

        private void InitializeLogFrameUI()
        {
            SplitContainer splitLogframe = new SplitContainer { Dock = DockStyle.Fill, Panel1MinSize = 200, SplitterDistance = 220, FixedPanel = FixedPanel.Panel1, BorderStyle = BorderStyle.None };
            Control logFrameTabContainer = this.Controls.OfType<TabControl>().FirstOrDefault(c => c.Name == "tabControlProjectDetails")?.TabPages.Cast<TabPage>().FirstOrDefault(tp => tp.Name == "tabPageLogFrame") ?? this.Controls.Find("tabPageLogFrame", true).FirstOrDefault();

            if (logFrameTabContainer == null) { System.Windows.Forms.MessageBox.Show("LogFrame TabPage container not found. UI setup cannot proceed.", "Error"); return; }

            logFrameTabContainer.Controls.Clear(); logFrameTabContainer.Controls.Add(splitLogframe);

            sidebarPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = false, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10), BackColor = Color.FromArgb(240, 240, 240) };
            splitLogframe.Panel1.Controls.Add(sidebarPanel);

            Panel mainContentPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BorderStyle = BorderStyle.None, Padding = new Padding(15), BackColor = Color.White };
            splitLogframe.Panel2.Controls.Add(mainContentPanel);
            pnlLogFrameMain = mainContentPanel;

            CreateSidebarButtons();
            if (_isEditMode && _currentProject?.Outcomes?.Any() == true) { RenderAllOutcomes(); }
        }

        private void CreateSidebarButtons()
        {
            sidebarPanel.Controls.Clear();
            Panel fixedButtonsPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 10) };
            TableLayoutPanel buttonsTable = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };

            buttonsTable.RowStyles.Clear();
            Action<Control, int> addControlToTable = (control, rowIndex) => {
                buttonsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                buttonsTable.Controls.Add(control, 0, rowIndex);
            };

            int currentRow = 0;
            addControlToTable(new Label { Text = "ACTIONS", Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 10, 0, 5) }, currentRow++);

            Button btnAddOutcome = new Button();
            btnAddOutcome.Text = "Add Outcome";
            btnAddOutcome.Dock = DockStyle.Fill;
            btnAddOutcome.Height = 40;
            btnAddOutcome.Margin = new Padding(0, 5, 0, 5);
            btnAddOutcome.BackColor = Color.FromArgb(0, 122, 204);
            btnAddOutcome.ForeColor = Color.White;
            btnAddOutcome.FlatStyle = FlatStyle.Flat;
            btnAddOutcome.Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular);
            btnAddOutcome.TextAlign = ContentAlignment.MiddleLeft;
            btnAddOutcome.Padding = new Padding(10, 0, 0, 0);
            btnAddOutcome.Cursor = Cursors.Hand;
            btnAddOutcome.FlatAppearance.BorderSize = 0;
            btnAddOutcome.Click += btnAddOutcome_Click;
            addControlToTable(btnAddOutcome, currentRow++);

            Button btnAddOutput = CreateSidebarButton("Add Output", Color.FromArgb(0, 122, 204)); btnAddOutput.Click += (s, e) => AddElementToLogFrame<Outcome>(null, BtnAddOutputToOutcome_Click); addControlToTable(btnAddOutput, currentRow++);
            Button btnAddIndicator = CreateSidebarButton("Add Indicator", Color.FromArgb(0, 122, 204)); btnAddIndicator.Click += (s, e) => AddElementToLogFrame<Output>(null, BtnAddIndicator_Click); addControlToTable(btnAddIndicator, currentRow++);
            Button btnAddActivity = CreateSidebarButton("Add Activity", Color.FromArgb(0, 122, 204)); btnAddActivity.Click += (s, e) => AddElementToLogFrame<Output>(null, BtnAddActivity_Click); addControlToTable(btnAddActivity, currentRow++);

            addControlToTable(new Label { Text = "NAVIGATION", Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 20, 0, 5) }, currentRow++);
            Button btnSaveLogframe = CreateSidebarButton("Save Logframe", Color.FromArgb(0, 153, 51)); btnSaveLogframe.Click += btnSave_Click; addControlToTable(btnSaveLogframe, currentRow++);
            Button btnCancelLogframe = CreateSidebarButton("Cancel", Color.FromArgb(204, 0, 0)); btnCancelLogframe.Click += btnCancel_Click; addControlToTable(btnCancelLogframe, currentRow++);

            fixedButtonsPanel.Controls.Add(buttonsTable);
            sidebarPanel.Controls.Add(fixedButtonsPanel);
        }

        private Button CreateSidebarButton(string text, Color backColor)
        {
            Button button = new Button { Text = text, Dock = DockStyle.Fill, Height = 40, Margin = new Padding(0, 5, 0, 5), BackColor = backColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void AddElementToLogFrame<TElementType>(object parentElementTag, EventHandler specificAddHandler) where TElementType : class
        {
            if (_currentProject == null || _currentProject.Outcomes == null) { MessageBox.Show("Project or Outcomes not initialized.", "Error"); return; }

            if (typeof(TElementType) == typeof(Outcome))
            {
                Outcome firstOutcome = parentElementTag as Outcome ?? _currentProject.Outcomes.FirstOrDefault();
                if (firstOutcome == null) { MessageBox.Show("Please add an outcome first.", "No Outcomes"); return; }
                specificAddHandler(new Button { Tag = firstOutcome }, EventArgs.Empty);
            }
            else if (typeof(TElementType) == typeof(Output))
            {
                Output firstOutput = parentElementTag as Output ?? _currentProject.Outcomes.SelectMany(o => o.Outputs ?? new List<Output>()).FirstOrDefault();
                if (firstOutput == null) { MessageBox.Show("Please add an outcome and an output first.", "No Outputs"); return; }
                specificAddHandler(new Button { Tag = firstOutput }, EventArgs.Empty);
            }
        }


        private void RenderAllOutcomes()
        {
            if (pnlLogFrameMain == null) { MessageBox.Show("LogFrame main panel is null.", "Error"); return; }
            pnlLogFrameMain.SuspendLayout(); pnlLogFrameMain.Controls.Clear();
            if (_currentProject?.Outcomes == null) { pnlLogFrameMain.ResumeLayout(true); return; }

            for (int i = _currentProject.Outcomes.Count - 1; i >= 0; i--)
            {
                var outcome = _currentProject.Outcomes.ElementAt(i);
                int outcomeDisplayCounter = i + 1;
                Panel pnlOutcome = new Panel { Name = $"pnlOutcome_{outcomeDisplayCounter}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 20), Padding = new Padding(15), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(245, 245, 245) };

                TableLayoutPanel tlpOutcomeHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 2, Margin = new Padding(0, 0, 0, 10) };
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85F)); tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
                tlpOutcomeHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpOutcomeHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                Label lblOutcomeTitle = new Label { Text = $"Outcome {outcomeDisplayCounter}", Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold), ForeColor = Color.FromArgb(0, 102, 204), Dock = DockStyle.Fill, AutoSize = true };
                Button btnDeleteOutcome = new Button { Text = "Delete Outcome", Tag = outcome, ForeColor = Color.White, BackColor = Color.FromArgb(204, 0, 0), FlatStyle = FlatStyle.Flat, Dock = DockStyle.Right, AutoSize = true };
                btnDeleteOutcome.FlatAppearance.BorderSize = 0; btnDeleteOutcome.Click += BtnDeleteOutcome_Click;
                TextBox txtOutcomeDesc = new TextBox { Text = outcome.OutcomeDescription, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 60, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 10, FontStyle.Regular) };
                txtOutcomeDesc.TextChanged += (s, ev) => outcome.OutcomeDescription = ((TextBox)s).Text;

                tlpOutcomeHeader.Controls.Add(lblOutcomeTitle, 0, 0); tlpOutcomeHeader.Controls.Add(btnDeleteOutcome, 1, 0);
                tlpOutcomeHeader.Controls.Add(txtOutcomeDesc, 0, 1); tlpOutcomeHeader.SetColumnSpan(txtOutcomeDesc, 2);

                Button btnAddOutputToThisOutcome = new Button { Text = "Add Output to this Outcome", Tag = outcome, Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 10, 0, 0), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Padding = new Padding(10, 5, 10, 5) };
                btnAddOutputToThisOutcome.FlatAppearance.BorderSize = 0; btnAddOutputToThisOutcome.Click += BtnAddOutputToOutcome_Click;

                Panel pnlOutputsContainer = new Panel { Name = $"pnlOutputsContainer_Outcome{outcomeDisplayCounter}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 10, 0, 0), Padding = new Padding(15, 0, 0, 0) };

                pnlOutcome.Controls.Add(tlpOutcomeHeader);
                pnlOutcome.Controls.Add(btnAddOutputToThisOutcome);
                pnlOutcome.Controls.Add(pnlOutputsContainer);

                RenderOutputsForOutcome(outcome, pnlOutputsContainer, outcomeDisplayCounter.ToString());
                pnlLogFrameMain.Controls.Add(pnlOutcome);
            }
            pnlLogFrameMain.ResumeLayout(true);
        }

        private void RenderOutputsForOutcome(Outcome outcome, Panel parentOutputPanel, string outcomeNumberString)
        {
            if (parentOutputPanel == null || outcome?.Outputs == null) return;
            parentOutputPanel.SuspendLayout(); parentOutputPanel.Controls.Clear();

            int outputCounter = 0;
            foreach (var outputInstance in outcome.Outputs.ToList())
            {
                outputCounter++;

                Panel pnlLogicalUnit = new Panel { Name = $"pnlLogicalUnit_{outcome.OutcomeID}_{outputInstance.OutputID}_{outputCounter}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 15), Padding = new Padding(15), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(250, 250, 250) };

                TableLayoutPanel tlpOutputHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 0, 0, 10) };
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                Label lblOutputTitle = new Label { Text = $"Output {outcomeNumberString}.{outputCounter}:", Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), ForeColor = Color.FromArgb(0, 102, 153), AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top };
                TextBox txtOutputDesc = new TextBox { Text = outputInstance.OutputDescription, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 50, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular) };
                txtOutputDesc.TextChanged += (s, ev) => outputInstance.OutputDescription = ((TextBox)s).Text;
                Button btnDeleteOutput = new Button { Text = "Delete Output", Tag = new Tuple<Outcome, Output>(outcome, outputInstance), ForeColor = Color.White, BackColor = Color.FromArgb(204, 0, 0), FlatStyle = FlatStyle.Flat, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Padding = new Padding(5, 2, 5, 2) };
                btnDeleteOutput.FlatAppearance.BorderSize = 0; btnDeleteOutput.Click += BtnDeleteOutput_Click;
                tlpOutputHeader.Controls.Add(lblOutputTitle, 0, 0); tlpOutputHeader.Controls.Add(txtOutputDesc, 1, 0); tlpOutputHeader.Controls.Add(btnDeleteOutput, 2, 0);

                FlowLayoutPanel pnlOutputActionButtons = new FlowLayoutPanel { Name = $"pnlOutputActionButtons_{outputInstance.OutputID}_{outputCounter}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 5, 0, 5) };
                Button btnAddIndicatorMoved = new Button { Text = "Add Indicator", Tag = outputInstance, AutoSize = true, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Padding = new Padding(10, 5, 10, 5) };
                btnAddIndicatorMoved.FlatAppearance.BorderSize = 0; btnAddIndicatorMoved.Click += BtnAddIndicator_Click;
                Button btnAddActivityMoved = new Button { Text = "Add Activity", Tag = outputInstance, AutoSize = true, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Padding = new Padding(10, 5, 10, 5) };
                btnAddActivityMoved.FlatAppearance.BorderSize = 0; btnAddActivityMoved.Click += BtnAddActivity_Click;
                pnlOutputActionButtons.Controls.Add(btnAddIndicatorMoved); pnlOutputActionButtons.Controls.Add(btnAddActivityMoved);

                Panel pnlIndicators = new Panel { Name = $"pnlIndicators_{outputInstance.OutputID}_{outputCounter}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 10, 0, 0), Padding = new Padding(10, 0, 0, 0) };
                Label lblIndicatorsHeader = new Label { Text = "Indicators:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold), ForeColor = Color.FromArgb(102, 102, 102), Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
                pnlIndicators.Controls.Add(lblIndicatorsHeader);
                if (outputInstance.ProjectIndicators != null)
                {
                    int indicatorIndex = 1;
                    foreach (var indicator in outputInstance.ProjectIndicators)
                    {
                        Panel pnlIndicatorEntry = CreateIndicatorPanel(outputInstance, indicator, $"{outcomeNumberString}.{outputCounter}", indicatorIndex++);
                        pnlIndicators.Controls.Add(pnlIndicatorEntry);
                    }
                }

                Panel pnlActivities = new Panel { Name = $"pnlActivities_{outputInstance.OutputID}_{outputCounter}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 10, 0, 0), Padding = new Padding(10, 0, 0, 0) };
                Label lblActivitiesHeader = new Label { Text = "Activities:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold), ForeColor = Color.FromArgb(102, 102, 102), Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
                pnlActivities.Controls.Add(lblActivitiesHeader);
                if (outputInstance.Activities != null)
                {
                    int activityIndex = 1;
                    foreach (var activity in outputInstance.Activities)
                    {
                        Panel pnlActivityEntry = CreateActivityPanel(outputInstance, activity, $"{outcomeNumberString}.{outputCounter}", activityIndex++);
                        pnlActivities.Controls.Add(pnlActivityEntry);
                    }
                }

                pnlLogicalUnit.Controls.Add(tlpOutputHeader);
                pnlLogicalUnit.Controls.Add(pnlOutputActionButtons);
                pnlLogicalUnit.Controls.Add(pnlIndicators);
                pnlLogicalUnit.Controls.Add(pnlActivities);
                parentOutputPanel.Controls.Add(pnlLogicalUnit);
            }
            parentOutputPanel.ResumeLayout(true);
        }

        private Panel CreateIndicatorPanel(Output outputInstance, ProjectIndicator indicator, string baseNumberString, int indicatorIndex)
        {
            Panel pnlIndicator = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 10), Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            TableLayoutPanel tlpIndicatorLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, RowCount = 3, Margin = new Padding(0, 0, 0, 5) };
            tlpIndicatorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); tlpIndicatorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); tlpIndicatorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Label lblIndicatorLabel = new Label { Text = $"Indicator {baseNumberString}.{indicatorIndex}:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular), AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 3, 3, 0) };
            TextBox txtIndicatorDesc = new TextBox { Text = indicator.IndicatorName, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 40, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular) };
            txtIndicatorDesc.TextChanged += (s, ev) => indicator.IndicatorName = ((TextBox)s).Text;
            Button btnDeleteIndicator = new Button { Text = "Delete", Tag = new Tuple<Output, ProjectIndicator>(outputInstance, indicator), ForeColor = Color.White, BackColor = Color.FromArgb(204, 0, 0), FlatStyle = FlatStyle.Flat, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Padding = new Padding(5, 2, 5, 2) };
            btnDeleteIndicator.FlatAppearance.BorderSize = 0; btnDeleteIndicator.Click += BtnDeleteIndicator_Click;
            tlpIndicatorLayout.Controls.Add(lblIndicatorLabel, 0, 0); tlpIndicatorLayout.Controls.Add(txtIndicatorDesc, 1, 0); tlpIndicatorLayout.Controls.Add(btnDeleteIndicator, 2, 0);
            Label lblMoVLabel = new Label { Text = "Means of Verification:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular), AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 8, 3, 0) };
            TextBox txtMoV = new TextBox { Text = indicator.MeansOfVerification, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 40, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular) };
            txtMoV.TextChanged += (s, ev) => indicator.MeansOfVerification = ((TextBox)s).Text;
            tlpIndicatorLayout.Controls.Add(lblMoVLabel, 0, 1); tlpIndicatorLayout.Controls.Add(txtMoV, 1, 1); tlpIndicatorLayout.SetColumnSpan(txtMoV, 2);
            Label lblTargetsLabel = new Label { Text = "Targets:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 8, 3, 0) };
            TableLayoutPanel tlpTargets = CreateTargetsPanel(indicator); tlpTargets.Dock = DockStyle.Fill;
            tlpIndicatorLayout.Controls.Add(lblTargetsLabel, 0, 2); tlpIndicatorLayout.Controls.Add(tlpTargets, 1, 2); tlpIndicatorLayout.SetColumnSpan(tlpTargets, 2);
            pnlIndicator.Controls.Add(tlpIndicatorLayout);
            return pnlIndicator;
        }

        private Panel CreateActivityPanel(Output outputInstance, ProjectActivity activity, string baseNumberString, int activityIndex)
        {
            Panel pnlActivity = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 10), Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            TableLayoutPanel tlpActivityLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, RowCount = 1 };
            tlpActivityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); tlpActivityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); tlpActivityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Label lblActivityLabel = new Label { Text = $"Activity {baseNumberString}.{activityIndex}:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular), AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 3, 3, 0) };
            TextBox txtActivityDesc = new TextBox { Text = activity.ActivityDescription, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 40, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 9, FontStyle.Regular) };
            txtActivityDesc.TextChanged += (s, ev) => { activity.ActivityDescription = ((TextBox)s).Text; if (IsHandleCreated) InitializeActivityPlanTab(); };
            Button btnDeleteActivity = new Button { Text = "Delete", Tag = new Tuple<Output, ProjectActivity>(outputInstance, activity), ForeColor = Color.White, BackColor = Color.FromArgb(204, 0, 0), FlatStyle = FlatStyle.Flat, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Padding = new Padding(5, 2, 5, 2) };
            btnDeleteActivity.FlatAppearance.BorderSize = 0; btnDeleteActivity.Click += BtnDeleteActivity_Click;
            tlpActivityLayout.Controls.Add(lblActivityLabel, 0, 0); tlpActivityLayout.Controls.Add(txtActivityDesc, 1, 0); tlpActivityLayout.Controls.Add(btnDeleteActivity, 2, 0);
            pnlActivity.Controls.Add(tlpActivityLayout);
            return pnlActivity;
        }

        private TableLayoutPanel CreateTargetsPanel(ProjectIndicator indicator)
        {
            TableLayoutPanel tlpTargets = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 5, RowCount = 2, Margin = new Padding(0, 5, 0, 0) };
            for (int i = 0; i < 5; i++) tlpTargets.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpTargets.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpTargets.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            string[] labels = { "Men:", "Women:", "Boys:", "Girls:", "Total:" };
            for (int i = 0; i < labels.Length; i++) { tlpTargets.Controls.Add(new Label { Text = labels[i], AutoSize = true, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Anchor = AnchorStyles.Left }, i, 0); }
            NumericUpDown nudMen = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill };
            nudMen.Maximum = 1000000;
            nudMen.Value = indicator.TargetMen;
            nudMen.ValueChanged += (s, ev) => indicator.TargetMen = (int)((NumericUpDown)s).Value;
            NumericUpDown nudWomen = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill };
            nudWomen.Maximum = 1000000;
            nudWomen.Value = indicator.TargetWomen;
            nudWomen.ValueChanged += (s, ev) => indicator.TargetWomen = (int)((NumericUpDown)s).Value;
            NumericUpDown nudBoys = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill };
            nudBoys.Maximum = 1000000;
            nudBoys.Value = indicator.TargetBoys;
            nudBoys.ValueChanged += (s, ev) => indicator.TargetBoys = (int)((NumericUpDown)s).Value;
            NumericUpDown nudGirls = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill };
            nudGirls.Maximum = 1000000;
            nudGirls.Value = indicator.TargetGirls;
            nudGirls.ValueChanged += (s, ev) => indicator.TargetGirls = (int)((NumericUpDown)s).Value;
            NumericUpDown nudTotal = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill };
            nudTotal.Maximum = 4000000;
            nudTotal.Value = indicator.TargetTotal;
            nudTotal.ValueChanged += (s, ev) => indicator.TargetTotal = (int)((NumericUpDown)s).Value;
            tlpTargets.Controls.Add(nudMen, 0, 1); tlpTargets.Controls.Add(nudWomen, 1, 1); tlpTargets.Controls.Add(nudBoys, 2, 1); tlpTargets.Controls.Add(nudGirls, 3, 1); tlpTargets.Controls.Add(nudTotal, 4, 1);
            return tlpTargets;
        }

        private void btnAddOutcome_Click(object sender, EventArgs e)
        {
            if (_currentProject == null) { MessageBox.Show("Project data is not initialized.", "Error"); return; }
            _currentProject.Outcomes = _currentProject.Outcomes ?? new List<Outcome>();
            Outcome newOutcome = new Outcome { ProjectID = _currentProject.ProjectID, Outputs = new List<Output>() };
            _currentProject.Outcomes.Add(newOutcome);
            RenderAllOutcomes();
        }

        private void BtnDeleteOutcome_Click(object sender, EventArgs e)
        {
            if ((sender as Button)?.Tag is Outcome outcomeToDelete &&
                MessageBox.Show($"Delete outcome '{outcomeToDelete.OutcomeDescription}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _currentProject.Outcomes.Remove(outcomeToDelete);
                RenderAllOutcomes();
            }
        }

        private void BtnAddOutputToOutcome_Click(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Outcome parentOutcome)) return;
            parentOutcome.Outputs = parentOutcome.Outputs ?? new List<Output>();
            Output newOutput = new Output { OutcomeID = parentOutcome.OutcomeID, ProjectIndicators = new List<ProjectIndicator>(), Activities = new List<ProjectActivity>() };
            parentOutcome.Outputs.Add(newOutput);
            RenderAllOutcomes();
        }

        private void BtnDeleteOutput_Click(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Tuple<Outcome, Output> data)) return;
            if (MessageBox.Show($"Delete output '{data.Item2.OutputDescription}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                data.Item1.Outputs.Remove(data.Item2);
                RenderAllOutcomes();
            }
        }

        private void BtnAddIndicator_Click(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Output parentOutput)) return;
            parentOutput.ProjectIndicators = parentOutput.ProjectIndicators ?? new List<ProjectIndicator>();
            ProjectIndicator newIndicator = new ProjectIndicator { OutputID = parentOutput.OutputID, ProjectID = _currentProject.ProjectID };
            parentOutput.ProjectIndicators.Add(newIndicator);
            RenderAllOutcomes();
        }

        private void BtnDeleteIndicator_Click(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Tuple<Output, ProjectIndicator> data)) return;
            if (MessageBox.Show($"Delete indicator '{data.Item2.IndicatorName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                data.Item1.ProjectIndicators.Remove(data.Item2);
                RenderAllOutcomes();
            }
        }

        private void BtnAddActivity_Click(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Output parentOutput)) return;
            parentOutput.Activities = parentOutput.Activities ?? new List<ProjectActivity>();
            ProjectActivity newActivity = new ProjectActivity { OutputID = parentOutput.OutputID };
            parentOutput.Activities.Add(newActivity);
            RenderAllOutcomes();
            if (IsHandleCreated) InitializeActivityPlanTab();
        }

        private void BtnDeleteActivity_Click(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Tuple<Output, ProjectActivity> data)) return;
            if (MessageBox.Show($"Delete activity '{data.Item2.ActivityDescription}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                data.Item1.Activities.Remove(data.Item2);
                RenderAllOutcomes();
                if (IsHandleCreated) InitializeActivityPlanTab();
            }
        }

        private void SetAccessibilityProperties()
        {
            txtProjectName.AccessibleName = "Project Name";
            txtProjectName.AccessibleDescription = "Enter the full official name of the project. This field is required.";
            txtProjectCode.AccessibleName = "Project Code";
            txtProjectCode.AccessibleDescription = "Enter the unique code for the project (optional).";
            dtpStartDate.AccessibleName = "Project Start Date";
            dtpStartDate.AccessibleDescription = "Select the start date of the project. Check the box to enable date selection.";
            dtpEndDate.AccessibleName = "Project End Date";
            dtpEndDate.AccessibleDescription = "Select the end date of the project. Check the box to enable date selection.";
            txtLocation.AccessibleName = "Project Location";
            txtLocation.AccessibleDescription = "Enter the geographical location(s) of the project.";
            txtOverallObjective.AccessibleName = "Overall Objective";
            txtOverallObjective.AccessibleDescription = "Describe the main goal or overall objective of the project.";
            txtStatus.AccessibleName = "Project Status";
            txtStatus.AccessibleDescription = "Enter the current status of the project (e.g., Planning, Active, Completed).";
            txtDonor.AccessibleName = "Project Donor";
            txtDonor.AccessibleDescription = "Enter the name of the donor or funding source for the project.";
            nudTotalBudget.AccessibleName = "Total Project Budget";
            nudTotalBudget.AccessibleDescription = "Enter the total budget amount for the project.";
            cmbSection.AccessibleName = "Organizational Section";
            cmbSection.AccessibleDescription = "Select the organizational section or department responsible for the project.";
            cmbManager.AccessibleName = "Project Manager";
            cmbManager.AccessibleDescription = "Select the user who will manage this project.";
            btnSave.AccessibleName = "Save Project Details";
            btnSave.AccessibleDescription = "Saves the current project information to the database.";
            btnCancel.AccessibleName = "Cancel Editing";
            btnCancel.AccessibleDescription = "Discards any changes and closes the project details form.";
        }

        private async Task LoadComboBoxesAsync()
        {
            cmbSection.DisplayMember = "Text"; cmbSection.ValueMember = "Value";
            cmbSection.Items.Clear(); cmbSection.Items.Add(new ComboboxItem { Text = "(No Section)", Value = 0 });
            try { List<Section> sections = await _sectionService.GetSectionsAsync(); sections.ForEach(s => cmbSection.Items.Add(new ComboboxItem { Text = s.SectionName, Value = s.SectionID })); }
            catch (Exception ex) { MessageBox.Show($"Error loading sections: {ex.Message}", "Error"); }

            if (_isEditMode && _currentProject.SectionID.HasValue) { foreach (ComboboxItem item in cmbSection.Items) if (item.Value == _currentProject.SectionID.Value) { cmbSection.SelectedItem = item; break; } if (cmbSection.SelectedItem == null) cmbSection.SelectedIndex = 0; }
            else if (!_isEditMode && _initialSectionId.HasValue) { bool found = false; foreach (ComboboxItem item in cmbSection.Items) if (item.Value == _initialSectionId.Value) { cmbSection.SelectedItem = item; found = true; break; } if (!found) cmbSection.SelectedIndex = 0; }
            else { cmbSection.SelectedIndex = 0; }

            cmbManager.DisplayMember = "Text"; cmbManager.ValueMember = "Value";
            cmbManager.Items.Add(new ComboboxItem { Text = "(No Manager)", Value = 0 });
            cmbManager.Items.Add(new ComboboxItem { Text = "Default Manager 1 (User ID 1)", Value = 1 });
            cmbManager.Items.Add(new ComboboxItem { Text = "Default Manager 2 (User ID 2)", Value = 2 });
            if (_isEditMode && _currentProject.ManagerUserID.HasValue) { foreach (ComboboxItem item in cmbManager.Items) if (item.Value == _currentProject.ManagerUserID.Value) { cmbManager.SelectedItem = item; break; } }
            else { cmbManager.SelectedIndex = 0; }
        }

        private void PopulateControls()
        {
            if (_currentProject == null) return;
            txtProjectName.Text = _currentProject.ProjectName;
            txtProjectCode.Text = _currentProject.ProjectCode;
            if (_currentProject.StartDate.HasValue) { dtpStartDate.Value = _currentProject.StartDate.Value; dtpStartDate.Checked = true; } else { dtpStartDate.Value = DateTime.Now; dtpStartDate.Checked = false; }
            if (_currentProject.EndDate.HasValue) { dtpEndDate.Value = _currentProject.EndDate.Value; dtpEndDate.Checked = true; } else { dtpEndDate.Value = DateTime.Now; dtpEndDate.Checked = false; }
            txtLocation.Text = _currentProject.Location;
            txtOverallObjective.Text = _currentProject.OverallObjective;
            txtStatus.Text = _currentProject.Status;
            txtDonor.Text = _currentProject.Donor;
            nudTotalBudget.Value = _currentProject.TotalBudget ?? 0;
        }


        private bool CollectAndValidateData()
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text)) { MessageBox.Show("Project Name is required.", "Validation Error"); txtProjectName.Focus(); return false; }
            _currentProject.ProjectName = txtProjectName.Text.Trim();
            _currentProject.ProjectCode = string.IsNullOrWhiteSpace(txtProjectCode.Text) ? null : txtProjectCode.Text.Trim();
            _currentProject.StartDate = dtpStartDate.Checked ? (DateTime?)dtpStartDate.Value : null;
            _currentProject.EndDate = dtpEndDate.Checked ? (DateTime?)dtpEndDate.Value : null;
            if (_currentProject.StartDate.HasValue && _currentProject.EndDate.HasValue && _currentProject.EndDate.Value < _currentProject.StartDate.Value) { MessageBox.Show("End Date cannot be earlier than Start Date.", "Validation Error"); dtpEndDate.Focus(); return false; }
            _currentProject.Location = string.IsNullOrWhiteSpace(txtLocation.Text) ? null : txtLocation.Text.Trim();
            _currentProject.OverallObjective = string.IsNullOrWhiteSpace(txtOverallObjective.Text) ? null : txtOverallObjective.Text.Trim();
            _currentProject.Status = string.IsNullOrWhiteSpace(txtStatus.Text) ? null : txtStatus.Text.Trim();
            _currentProject.Donor = string.IsNullOrWhiteSpace(txtDonor.Text) ? null : txtDonor.Text.Trim();
            _currentProject.TotalBudget = nudTotalBudget.Value;
            _currentProject.SectionID = (cmbSection.SelectedItem as ComboboxItem)?.Value == 0 ? null : (int?)(cmbSection.SelectedItem as ComboboxItem)?.Value;
            _currentProject.ManagerUserID = (cmbManager.SelectedItem as ComboboxItem)?.Value == 0 ? null : (int?)(cmbManager.SelectedItem as ComboboxItem)?.Value;
            if (!_isEditMode) { _currentProject.CreatedAt = DateTime.UtcNow; }
            _currentProject.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!CollectAndValidateData()) { return; }
            btnSave.Enabled = false; btnCancel.Enabled = false; this.UseWaitCursor = true;
            try
            {
                bool success = await _projectService.SaveProjectAsync(_currentProject);
                if (success) { MessageBox.Show("Project saved successfully.", "Success"); this.DialogResult = DialogResult.OK; this.Close(); }
                else { MessageBox.Show("Failed to save project.", "Save Error"); }
            }
            catch (Exception ex) { MessageBox.Show($"An error occurred: {ex.Message}", "Error"); }
            finally { btnSave.Enabled = true; btnCancel.Enabled = true; this.UseWaitCursor = false; }
        }

        private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }

        private string GetCategoryDisplayName(BudgetCategoriesEnum category) { string name = category.ToString(); if (name.Length > 2 && name[1] == '_') { name = name[0] + ". " + name.Substring(2); } return System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim(); }
        void ClearControlsFromRow(TableLayoutPanel panel, int rowIndex) { for (int i = 0; i < panel.ColumnCount; i++) { Control control = panel.GetControlFromPosition(i, rowIndex); if (control != null) { panel.Controls.Remove(control); control.Dispose(); } } }



        private void InitializeActivityPlanTab()
        {
            DataGridView dgvActivityPlan = this.Controls.Find("dgvActivityPlan", true).FirstOrDefault() as DataGridView;
            if (dgvActivityPlan == null || _currentProject == null) { return; }
            dgvActivityPlan.SuspendLayout(); dgvActivityPlan.Columns.Clear(); dgvActivityPlan.Rows.Clear();
            dgvActivityPlan.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActivityDescription", HeaderText = "Activity", ReadOnly = true, Frozen = true, FillWeight = 30 });
            DateTime startDate = _currentProject.StartDate ?? (_currentProject.CreatedAt != DateTime.MinValue ? _currentProject.CreatedAt : DateTime.Now);
            DateTime endDate = _currentProject.EndDate ?? startDate.AddYears(1);
            if (endDate < startDate) endDate = startDate.AddYears(1);
            DateTime currentMonthIterator = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime endMonthTarget = new DateTime(endDate.Year, endDate.Month, 1);
            while (currentMonthIterator <= endMonthTarget)
            {
                string monthYearKeyStorage = currentMonthIterator.ToString("MMM/yyyy", CultureInfo.InvariantCulture);
                string headerTextDisplay = currentMonthIterator.ToString("MMM/yy", CultureInfo.InvariantCulture);
                string columnName = $"Month_{currentMonthIterator.Year}_{currentMonthIterator.Month:00}";
                dgvActivityPlan.Columns.Add(new DataGridViewCheckBoxColumn { Name = columnName, HeaderText = headerTextDisplay, Tag = monthYearKeyStorage, Width = 70 });
                currentMonthIterator = currentMonthIterator.AddMonths(1);
            }
            if (_currentProject.Outcomes != null)
            {
                foreach (var outcome in _currentProject.Outcomes)
                {
                    if (outcome.Outputs == null) continue;
                    foreach (var output in outcome.Outputs)
                    {
                        if (output.Activities == null) continue;
                        foreach (var activity in output.Activities)
                        {
                            int rowIndex = dgvActivityPlan.Rows.Add(); DataGridViewRow row = dgvActivityPlan.Rows[rowIndex];
                            row.Cells["ActivityDescription"].Value = activity.ActivityDescription; row.Tag = activity;
                            List<string> plannedMonthsList = string.IsNullOrEmpty(activity.PlannedMonths) ? new List<string>() : activity.PlannedMonths.Split(',').ToList();
                            foreach (DataGridViewColumn col in dgvActivityPlan.Columns)
                            {
                                if (col is DataGridViewCheckBoxColumn && col.Name.StartsWith("Month_"))
                                {
                                    string colStorageKey = col.Tag as string;
                                    if (colStorageKey != null && plannedMonthsList.Contains(colStorageKey)) { row.Cells[col.Name].Value = true; } else { row.Cells[col.Name].Value = false; }
                                }
                            }
                        }
                    }
                }
            }
            dgvActivityPlan.ResumeLayout();
        }

        private void dgvActivityPlan_CurrentCellDirtyStateChanged(object sender, EventArgs e) { if (sender is DataGridView dgv && dgv.IsCurrentCellDirty && dgv.CurrentCell is DataGridViewCheckBoxCell) { dgv.CommitEdit(DataGridViewDataErrorContexts.Commit); } }
        private void dgvActivityPlan_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!(sender is DataGridView dgv) || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (!(dgv.Rows[e.RowIndex].Tag is ProjectActivity activity)) return;
            DataGridViewColumn col = dgv.Columns[e.ColumnIndex];
            if (col is DataGridViewCheckBoxColumn && col.Name.StartsWith("Month_") && col.Tag is string monthYearKey)
            {
                bool isChecked = (bool)(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value ?? false);
                List<string> plannedMonthsList = string.IsNullOrEmpty(activity.PlannedMonths) ? new List<string>() : activity.PlannedMonths.Split(',').ToList();
                if (isChecked && !plannedMonthsList.Contains(monthYearKey)) { plannedMonthsList.Add(monthYearKey); }
                else if (!isChecked) { plannedMonthsList.Remove(monthYearKey); }
                activity.PlannedMonths = string.Join(",", plannedMonthsList);
            }
        }
        private void ProjectDatesChanged_RefreshActivityPlan(object sender, EventArgs e) { InitializeActivityPlanTab(); }

        private void ProjectCreateEditForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}
