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
using HumanitarianProjectManagement;
using System.Diagnostics;

namespace HumanitarianProjectManagement.Forms
{
    public partial class ProjectCreateEditForm : Form
    {
        private readonly ProjectService _projectService;
        private readonly SectionService _sectionService;
        private Project _currentProject;
        private bool _isEditMode;
        private int? _initialSectionId;
        private readonly LogFrameService _logFrameService;
        private Panel sidebarPanel;
        private BudgetTabUserControl _budgetTabControlInstance;
        private bool _formInitialLoadComplete = false; // Flag to manage initial load logic

        // Declare SplitContainer controls as class members
        private SplitContainer mainVerticalSplit;
        private SplitContainer rightHorizontalSplit;

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
                // Initialize nested lists if they are null when loading an existing project
                foreach (var outcome in _currentProject.Outcomes)
                {
                    if (outcome.Outputs == null) outcome.Outputs = new List<Output>();
                    foreach (var output in outcome.Outputs)
                    {
                        if (output.Activities == null) output.Activities = new List<ProjectActivity>();
                        if (output.ProjectIndicators == null) output.ProjectIndicators = new List<ProjectIndicator>();
                    }
                }
                if (_currentProject.DetailedBudgetLines == null) _currentProject.DetailedBudgetLines = new BindingList<DetailedBudgetLine>();
                PopulateControls();
            }
            else
            {
                _currentProject = new Project
                {
                    Outcomes = new List<Outcome>(),
                    DetailedBudgetLines = new BindingList<DetailedBudgetLine>()
                };
                this.Text = "Add New Project";
                dtpStartDate.Value = DateTime.Now;
                dtpStartDate.Checked = false;
                dtpEndDate.Checked = false;
            }

            this.Load += new System.EventHandler(this.ProjectCreateEditForm_Load);
            this.Shown += new System.EventHandler(this.ProjectCreateEditForm_Shown); // Subscribe to Form_Shown event
        }

        private void ProjectCreateEditForm_Shown(object sender, EventArgs e)
        {
            Debug.WriteLine($"Form_Shown START: mainVerticalSplit is {(this.mainVerticalSplit == null ? "null" : "not null")}");
            Debug.WriteLine($"Form_Shown: tabControlProjectDetails.Visible = {tabControlProjectDetails.Visible}");
            Debug.WriteLine($"Form_Shown: tabControlProjectDetails contains tabPageLogFrame by key? {tabControlProjectDetails.TabPages.ContainsKey("tabPageLogFrame")}");
            Debug.WriteLine($"Form_Shown: Initially SelectedTab = {(tabControlProjectDetails.SelectedTab?.Name ?? "null")}");

            // Ensure the LogFrame tab is selected
            TabPage logFrameTab = null;
            if (tabControlProjectDetails.TabPages.ContainsKey("tabPageLogFrame"))
            {
                logFrameTab = tabControlProjectDetails.TabPages["tabPageLogFrame"];
                tabControlProjectDetails.SelectedTab = logFrameTab;
                Debug.WriteLine($"Form_Shown: tabPageLogFrame selected. Visible: {logFrameTab.Visible}");
            }
            else
            {
                // Attempt to find by iterating if key lookup fails (e.g. if Name property wasn't set as Key)
                foreach (TabPage tp in tabControlProjectDetails.TabPages)
                {
                    if (tp.Name == "tabPageLogFrame")
                    {
                        logFrameTab = tp;
                        tabControlProjectDetails.SelectedTab = logFrameTab;
                        Debug.WriteLine($"Form_Shown: tabPageLogFrame found by iteration and selected. Visible: {logFrameTab.Visible}");
                        break;
                    }
                }
                if (logFrameTab == null)
                {
                    Debug.WriteLine("Form_Shown: tabPageLogFrame NOT FOUND in tabControlProjectDetails by key or iteration.");
                }
            }
            Debug.WriteLine($"Form_Shown: SelectedTab after attempting to set = {(tabControlProjectDetails.SelectedTab?.Name ?? "null")}");

            // Force layout updates
            tabControlProjectDetails.PerformLayout();
            if (logFrameTab != null) // logFrameTab is the TabPage instance found above
            {
                logFrameTab.PerformLayout();
                logFrameTab.Refresh();
                Debug.WriteLine($"Form_Shown: Performed Layout and Refresh on logFrameTab '{logFrameTab.Name}'. Visible: {logFrameTab.Visible}");
            }
            this.PerformLayout();
            Application.DoEvents(); // Process pending UI messages
            Debug.WriteLine("Form_Shown: Performed PerformLayout on Form and Application.DoEvents()");

            AdjustSplitterDistances();
        }

        private void InitializeBudgetUITab()
        {
            Debug.WriteLine("InitializeBudgetUITab: Started.");
            try
            {
                if (this.tabControlProjectDetails == null)
                {
                    Debug.WriteLine("InitializeBudgetUITab: tabControlProjectDetails is null. Cannot initialize Budget tab.");
                    return;
                }

                if (_budgetTabControlInstance == null)
                {
                    _budgetTabControlInstance = new BudgetTabUserControl();
                    _budgetTabControlInstance.Dock = DockStyle.Fill;
                    Debug.WriteLine("InitializeBudgetUITab: _budgetTabControlInstance created.");
                }

                TabPage tabPageBudget = tabControlProjectDetails.TabPages["tabPageBudget"];
                if (tabPageBudget == null)
                {
                    tabPageBudget = new TabPage { Name = "tabPageBudget", Text = "Budget" };
                    tabControlProjectDetails.TabPages.Add(tabPageBudget);
                    Debug.WriteLine($"InitializeBudgetUITab: tabPageBudget created and added.");
                }

                if (!tabPageBudget.Controls.Contains(_budgetTabControlInstance))
                {
                    tabPageBudget.Controls.Clear();
                    tabPageBudget.Controls.Add(_budgetTabControlInstance);
                    Debug.WriteLine($"InitializeBudgetUITab: _budgetTabControlInstance added to tabPageBudget.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeBudgetUITab: EXCEPTION: {ex.ToString()}");
            }
        }

        private async Task HandleBudgetTabVisibilityAndStateAsync()
        {
            Debug.WriteLine($"HandleBudgetTabVisibilityAndStateAsync: Started. _isEditMode: {_isEditMode}, _currentProject.ProjectID: {_currentProject?.ProjectID ?? -1}");
            InitializeBudgetUITab();
            TabPage tabPageBudget = tabControlProjectDetails.TabPages["tabPageBudget"];

            if (tabPageBudget == null || _budgetTabControlInstance == null)
            {
                Debug.WriteLine("HandleBudgetTabVisibilityAndStateAsync: Budget tab or control instance is null. Aborting.");
                return;
            }

            bool userHasBudgetAccess = false;
            try
            {
                User currentUser = ApplicationState.CurrentUser;
                if (currentUser != null)
                {
                    UserService userService = new UserService();
                    List<int> roleIds = await userService.GetRoleIdsForUserAsync(currentUser.UserID);
                    List<Role> allRoles = await userService.GetAllRolesAsync();
                    List<string> userRoleNames = roleIds.Select(id => allRoles.FirstOrDefault(r => r.RoleID == id)?.RoleName).Where(name => name != null).ToList();
                    List<string> allowedRoleNames = new List<string> { "Administrator", "Project Manager", "Finance" };
                    userHasBudgetAccess = userRoleNames.Any(userRole => allowedRoleNames.Contains(userRole));
                }

                if (!userHasBudgetAccess)
                {
                    if (tabControlProjectDetails.TabPages.Contains(tabPageBudget))
                    {
                        tabControlProjectDetails.TabPages.Remove(tabPageBudget);
                        Debug.WriteLine("HandleBudgetTabVisibilityAndStateAsync: User lacks permission, tabPageBudget removed.");
                    }
                }
                else
                {
                    if (!tabControlProjectDetails.TabPages.Contains(tabPageBudget))
                    {
                        tabControlProjectDetails.TabPages.Add(tabPageBudget);
                        if (!tabPageBudget.Controls.Contains(_budgetTabControlInstance))
                        {
                            tabPageBudget.Controls.Clear();
                            tabPageBudget.Controls.Add(_budgetTabControlInstance);
                        }
                        Debug.WriteLine("HandleBudgetTabVisibilityAndStateAsync: Budget tab (re-)added for permitted user.");
                    }
                    _budgetTabControlInstance.LoadProject(_currentProject);
                    Debug.WriteLine($"HandleBudgetTabVisibilityAndStateAsync: Called LoadProject on _budgetTabControlInstance with ProjectID: {_currentProject?.ProjectID ?? -1}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleBudgetTabVisibilityAndStateAsync: EXCEPTION: {ex.ToString()}");
                if (tabControlProjectDetails.TabPages.Contains(tabPageBudget))
                {
                    tabControlProjectDetails.TabPages.Remove(tabPageBudget);
                }
                MessageBox.Show("Error determining user permissions for the budget tab. The tab will be hidden.", "Permissions Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Debug.WriteLine($"HandleBudgetTabVisibilityAndStateAsync: tabPageBudget is {(tabControlProjectDetails.TabPages.ContainsKey("tabPageBudget") ? "visible/present" : "hidden/removed")}.");
        }

        private async void ProjectCreateEditForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine($"Form_Load START: mainVerticalSplit is {(this.mainVerticalSplit == null ? "null" : "not null")}"); // Added debug line
            Debug.WriteLine($"ProjectCreateEditForm_Load: Started. _isEditMode: {_isEditMode}, _currentProject.ProjectID: {_currentProject?.ProjectID ?? -1}, _formInitialLoadComplete: {_formInitialLoadComplete}");

            if (!_formInitialLoadComplete)
            {
                // Initialize critical UI components synchronously first
                InitializeLogFrameUI();
                InitializeActivityPlanTab();
                // It's generally safer to set _formInitialLoadComplete after all *synchronous* parts of the initial load are done.
                // If LoadComboBoxesAsync or HandleBudgetTabVisibilityAndStateAsync were critical for the "initial load" state machine,
                // this flag might need to be set after them, but that could re-introduce the async/UI timing issue for Form_Shown.
                // For now, assume critical UI for Form_Shown is what matters most for this flag's timing.
                _formInitialLoadComplete = true;

                try
                {
                    await LoadComboBoxesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ProjectCreateEditForm_Load: EXCEPTION during LoadComboBoxesAsync: {ex.ToString()}");
                    MessageBox.Show("Error loading sections/managers: " + ex.Message, "Load Data Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Depending on severity, you might return or allow the form to continue with partial data
                }
            }

            // HandleBudgetTabVisibilityAndStateAsync can run after the initial synchronous UI setup
            // and potentially after _formInitialLoadComplete is set, if its operations are not
            // strictly tied to the _formInitialLoadComplete logic for other parts of the load.
            await HandleBudgetTabVisibilityAndStateAsync();

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
            Debug.WriteLine($"ProjectCreateEditForm_Load: Finished. _isEditMode: {_isEditMode}, _currentProject.ProjectID: {_currentProject?.ProjectID ?? -1}");
        }

        // THIS IS THE ORIGINAL InitializeLogFrameUI - IT NOW CALLS THE NEW METHOD
        private void InitializeLogFrameUI()
        {
            // This will call the new UI setup. If the old logic from previous versions is needed, 
            // this is the point to switch or conditionally call an old InitializeLogFrameUI_Old().
            InitializeLogFrameUI_New();
        }

        // Original CreateSidebarButtons from your provided code
        private void CreateSidebarButtons()
        {
            // This method should target the class member 'sidebarPanel' which is initialized by InitializeLogFrameUI_New
            if (sidebarPanel == null)
            {
                MessageBox.Show("Sidebar panel is not initialized for LogFrame. Cannot create buttons.", "UI Error");
                return;
            }

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

            // This "Add Outcome" button in the sidebar will trigger the top-level outcome entry form
            Button btnAddOutcomeSidebar = CreateSidebarButtonOriginal("Add Outcome", Color.FromArgb(0, 122, 204));
            btnAddOutcomeSidebar.Click += btnAddOutcome_Global_Click; // Ensure this event handler is assigned
            addControlToTable(btnAddOutcomeSidebar, currentRow++);

            Button btnAddOutput = CreateSidebarButtonOriginal("Add Output (Old)", Color.Gray); // Marked as old
            // btnAddOutput.Click += (s, e) => AddElementToLogFrame<Outcome>(null, BtnAddOutputToOutcome_Click_Original); 
            addControlToTable(btnAddOutput, currentRow++);

            Button btnAddIndicator = CreateSidebarButtonOriginal("Add Indicator (Old)", Color.Gray);
            // btnAddIndicator.Click += (s, e) => AddElementToLogFrame<Output>(null, BtnAddIndicator_Click_Original);
            addControlToTable(btnAddIndicator, currentRow++);

            Button btnAddActivity = CreateSidebarButtonOriginal("Add Activity (Old)", Color.Gray);
            // btnAddActivity.Click += (s, e) => AddElementToLogFrame<Output>(null, BtnAddActivity_Click_Original);
            addControlToTable(btnAddActivity, currentRow++);


            addControlToTable(new Label { Text = "NAVIGATION", Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 20, 0, 5) }, currentRow++);
            Button btnSaveLogframe = CreateSidebarButtonOriginal("Save Project", Color.FromArgb(0, 153, 51));
            btnSaveLogframe.Click += btnSave_Click;
            addControlToTable(btnSaveLogframe, currentRow++);

            Button btnCancelLogframe = CreateSidebarButtonOriginal("Close Form", Color.FromArgb(204, 0, 0));
            btnCancelLogframe.Click += btnCancel_Click;
            addControlToTable(btnCancelLogframe, currentRow++);

            fixedButtonsPanel.Controls.Add(buttonsTable);
            sidebarPanel.Controls.Add(fixedButtonsPanel);
        }

        // Renamed original CreateSidebarButton to avoid conflict
        private Button CreateSidebarButtonOriginal(string text, Color backColor)
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
                // Allow adding a new outcome even if none exist for BtnAddOutcome_Click_Original
                // if (firstOutcome == null && specificAddHandler != BtnAddOutcome_Click_Original)  // This specific handler check might be problematic if it's not defined yet
                // { 
                //     MessageBox.Show("Please add an outcome first, or select an outcome to add elements to.", "No Outcomes / No Selection"); return; 
                // }
                specificAddHandler(new Button { Tag = firstOutcome }, EventArgs.Empty);
            }
            else if (typeof(TElementType) == typeof(Output))
            {
                Output firstOutput = parentElementTag as Output ?? _currentProject.Outcomes
                    .SelectMany(o => o.Outputs ?? new List<Output>())
                    .FirstOrDefault();
                if (firstOutput == null) { MessageBox.Show("Please add an outcome and an output first, or select an output.", "No Outputs / No Selection"); return; }
                specificAddHandler(new Button { Tag = firstOutput }, EventArgs.Empty);
            }
        }

        private void RenderAllOutcomes()
        {
            if (pnlLogFrameMain == logFrameMiddleDisplayPanel && logFrameMiddleDisplayPanel != null)
            {
                RenderLogFrameHierarchy();
                return;
            }
            if (pnlLogFrameMain == null) { Debug.WriteLine("RenderAllOutcomes (Old): pnlLogFrameMain is null."); return; }
            pnlLogFrameMain.SuspendLayout(); pnlLogFrameMain.Controls.Clear();
            if (_currentProject?.Outcomes == null) { pnlLogFrameMain.ResumeLayout(true); return; }
            // ... (rest of original RenderAllOutcomes - can be removed if sure it's not called)
            pnlLogFrameMain.ResumeLayout(true);
        }

        private void RenderOutputsForOutcome(Outcome outcome, Panel parentOutputPanel, string outcomeNumberString)
        { /* Old logic - superseded by CreateOutputDisplayCard */ }

        private Panel CreateIndicatorPanel(Output outputInstance, ProjectIndicator indicator, string baseNumberString, int indicatorIndex)
        { /* Old logic - superseded by CreateItemDisplayPanel and contextual entry forms */ return new Panel(); }

        private Panel CreateActivityPanel(Output outputInstance, ProjectActivity activity, string baseNumberString, int activityIndex)
        { /* Old logic - superseded by CreateItemDisplayPanel and contextual entry forms */ return new Panel(); }

        private TableLayoutPanel CreateTargetsPanel(ProjectIndicator indicator)
        {
            TableLayoutPanel tlpTargets = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 5, RowCount = 2, Margin = new Padding(0, 5, 0, 0) };
            for (int i = 0; i < 5; i++) tlpTargets.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpTargets.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpTargets.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            string[] labels = { "Men:", "Women:", "Boys:", "Girls:", "Total:" };
            for (int i = 0; i < labels.Length; i++) { tlpTargets.Controls.Add(new Label { Text = labels[i], AutoSize = true, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Anchor = AnchorStyles.Left }, i, 0); }
            NumericUpDown nudMen = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill }; nudMen.Maximum = 1000000; nudMen.Value = indicator.TargetMen; nudMen.ValueChanged += (s, ev) => indicator.TargetMen = (int)((NumericUpDown)s).Value;
            NumericUpDown nudWomen = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill }; nudWomen.Maximum = 1000000; nudWomen.Value = indicator.TargetWomen; nudWomen.ValueChanged += (s, ev) => indicator.TargetWomen = (int)((NumericUpDown)s).Value;
            NumericUpDown nudBoys = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill }; nudBoys.Maximum = 1000000; nudBoys.Value = indicator.TargetBoys; nudBoys.ValueChanged += (s, ev) => indicator.TargetBoys = (int)((NumericUpDown)s).Value;
            NumericUpDown nudGirls = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill }; nudGirls.Maximum = 1000000; nudGirls.Value = indicator.TargetGirls; nudGirls.ValueChanged += (s, ev) => indicator.TargetGirls = (int)((NumericUpDown)s).Value;
            NumericUpDown nudTotal = new NumericUpDown { Width = 70, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Dock = DockStyle.Fill }; nudTotal.Maximum = 4000000; nudTotal.Value = indicator.TargetTotal; nudTotal.ValueChanged += (s, ev) => indicator.TargetTotal = (int)((NumericUpDown)s).Value;
            tlpTargets.Controls.Add(nudMen, 0, 1); tlpTargets.Controls.Add(nudWomen, 1, 1); tlpTargets.Controls.Add(nudBoys, 2, 1); tlpTargets.Controls.Add(nudGirls, 3, 1); tlpTargets.Controls.Add(nudTotal, 4, 1);
            return tlpTargets;
        }

        private void btnAddOutcome_Click_Original(object sender, EventArgs e)
        {
            if (_currentProject == null) { MessageBox.Show("Project data is not initialized.", "Error"); return; }
            _currentProject.Outcomes = _currentProject.Outcomes ?? new List<Outcome>();
            Outcome newOutcome = new Outcome { ProjectID = _currentProject.ProjectID, Outputs = new List<Output>() };
            _currentProject.Outcomes.Add(newOutcome);
            RenderAllOutcomes();
        }

        private void BtnDeleteOutcome_Click_Original(object sender, EventArgs e)
        {
            if ((sender as Button)?.Tag is Outcome outcomeToDelete &&
                MessageBox.Show($"Delete outcome '{outcomeToDelete.OutcomeDescription}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _currentProject.Outcomes.Remove(outcomeToDelete);
                RenderAllOutcomes();
            }
        }

        private void BtnAddOutputToOutcome_Click_Original(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Outcome parentOutcome)) return;
            parentOutcome.Outputs = parentOutcome.Outputs ?? new List<Output>();
            Output newOutput = new Output { OutcomeID = parentOutcome.OutcomeID, ProjectIndicators = new List<ProjectIndicator>(), Activities = new List<ProjectActivity>() };
            parentOutcome.Outputs.Add(newOutput);
            RenderAllOutcomes();
        }

        private void BtnDeleteOutput_Click_Original(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Tuple<Outcome, Output> data)) return;
            if (MessageBox.Show($"Delete output '{data.Item2.OutputDescription}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                data.Item1.Outputs.Remove(data.Item2);
                RenderAllOutcomes();
            }
        }

        private void BtnAddIndicator_Click_Original(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Output parentOutput)) return;
            parentOutput.ProjectIndicators = parentOutput.ProjectIndicators ?? new List<ProjectIndicator>();
            ProjectIndicator newIndicator = new ProjectIndicator { OutputID = parentOutput.OutputID, ProjectID = _currentProject.ProjectID };
            parentOutput.ProjectIndicators.Add(newIndicator);
            RenderAllOutcomes();
        }

        private void BtnDeleteIndicator_Click_Original(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Tuple<Output, ProjectIndicator> data)) return;
            if (MessageBox.Show($"Delete indicator '{data.Item2.IndicatorName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                data.Item1.ProjectIndicators.Remove(data.Item2);
                RenderAllOutcomes();
            }
        }

        private void BtnAddActivity_Click_Original(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Output parentOutput)) return;
            parentOutput.Activities = parentOutput.Activities ?? new List<ProjectActivity>();
            ProjectActivity newActivity = new ProjectActivity { OutputID = parentOutput.OutputID };
            parentOutput.Activities.Add(newActivity);
            RenderAllOutcomes();
            if (IsHandleCreated) InitializeActivityPlanTab();
        }

        private void BtnDeleteActivity_Click_Original(object sender, EventArgs e)
        {
            if (!((sender as Button)?.Tag is Tuple<Output, ProjectActivity> data)) return;
            if (MessageBox.Show($"Delete activity '{data.Item2.ActivityDescription}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                data.Item1.Activities.Remove(data.Item2);
                RenderAllOutcomes();
                if (IsHandleCreated) InitializeActivityPlanTab();
            }
        }

        // --- BEGIN NEW LOGFRAME UI METHODS ---
        // --- These methods are part of the NEW UI you provided earlier ---
        // --- Make sure they are correctly integrated or defined if missing ---

        private Panel logFrameTopEntryPanel;
        private Panel logFrameMiddleDisplayPanel;

        // Helper method to apply consistent styling to buttons (Moved here for better organization with other new UI methods)
        private void ApplyButtonStyle(Button button, Color backColor, Color foreColor, Font font = null)
        {
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = font ?? new Font("Segoe UI", 9F, FontStyle.Regular);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
        }

        // Creates a styled button for general use, e.g., in cards (often for delete actions)
        private Button CreateStyledSmallButton(string text, Color backColor, Color foreColor, string size = "sm", string tooltip = null)
        {
            Button btn = new Button { Text = text, BackColor = backColor, ForeColor = foreColor, FlatStyle = FlatStyle.Flat, AutoSize = false, Margin = new Padding(2), Padding = new Padding(0) };
            btn.FlatAppearance.BorderSize = 0;
            if (size == "xs") { btn.Size = new Size(26, 26); btn.Font = new Font("Segoe UI Emoji", 8F); }
            else { btn.Size = new Size(30, 30); btn.Font = new Font("Segoe UI Emoji", 9F); } // Default "sm"
            if (!string.IsNullOrEmpty(tooltip)) { ToolTip tt = new ToolTip(); tt.SetToolTip(btn, tooltip); }
            return btn;
        }

        private void CreateTopEntryAreaForOutcomes()
        {
            Debug.WriteLine("CreateTopEntryAreaForOutcomes called."); // Added debug line
            if (logFrameTopEntryPanel == null)
            {
                Debug.WriteLine("CreateTopEntryAreaForOutcomes: logFrameTopEntryPanel is null. Cannot proceed.");
                return;
            }

            logFrameTopEntryPanel.Controls.Clear();

            Label entrySectionLabel = new Label
            {
                Text = "Define New Outcome",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 10)
            };
            logFrameTopEntryPanel.Controls.Add(entrySectionLabel);
            entrySectionLabel.Visible = true; // Explicitly set visible

            Panel outcomeFormContainer = new Panel { Dock = DockStyle.Fill };
            logFrameTopEntryPanel.Controls.Add(outcomeFormContainer);
            outcomeFormContainer.Visible = true; // Explicitly set visible

            CreateOutcomeEntryFormFields(outcomeFormContainer);

            Debug.WriteLine($"CreateTopEntryAreaForOutcomes END: logFrameTopEntryPanel.Size = {logFrameTopEntryPanel.Size}, Visible = {logFrameTopEntryPanel.Visible}, Controls.Count = {logFrameTopEntryPanel.Controls.Count}");
            if (logFrameTopEntryPanel.Controls.Count > 0)
            {
                Debug.WriteLine($"CreateTopEntryAreaForOutcomes: First child of logFrameTopEntryPanel is {logFrameTopEntryPanel.Controls[0].Name}, Visible: {logFrameTopEntryPanel.Controls[0].Visible}, Size: {logFrameTopEntryPanel.Controls[0].Size}. Second child (if exists): {(logFrameTopEntryPanel.Controls.Count > 1 ? logFrameTopEntryPanel.Controls[1].Name : "N/A")}");
            }
            logFrameTopEntryPanel.BringToFront();
            if (logFrameTopEntryPanel.Parent != null)
            {
                logFrameTopEntryPanel.Parent.Refresh(); // Refresh the parent (rightHorizontalSplit.Panel1)
            }
        }

        private void CreateOutcomeEntryFormFields(Panel container)
        {
            Debug.WriteLine("CreateOutcomeEntryFormFields called."); // Added debug line
            container.Controls.Clear();

            TableLayoutPanel formTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Padding = new Padding(0, 5, 0, 5)
            };
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));

            Label lblDesc = new Label
            {
                Text = "Description:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(55, 65, 81),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 5, 0)
            };
            TextBox txtDesc = new TextBox
            {
                Name = "txtGlobalOutcomeDescription",
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0),
                Height = 28,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true // Explicitly set visible
            };
            Button btnAdd = new Button
            {
                Text = "Add Outcome",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Height = 30,
                Dock = DockStyle.Fill,
                Visible = true // Explicitly set visible
            };
            ApplyButtonStyle(btnAdd, Color.FromArgb(59, 130, 246), Color.White);

            btnAdd.Click += (s, e) => {
                AddOutcomeFromTopEntryForm(txtDesc.Text);
            };

            lblDesc.Visible = true; // Explicitly set visible
            formTable.Controls.Add(lblDesc, 0, 0);
            formTable.Controls.Add(txtDesc, 1, 0); // txtDesc is already set Visible = true
            formTable.Controls.Add(btnAdd, 2, 0);  // btnAdd is already set Visible = true
            container.Controls.Add(formTable);
            formTable.Visible = true; // Explicitly set formTable visible

            Debug.WriteLine($"CreateOutcomeEntryFormFields: formTable added. Visible: {formTable.Visible}, Size: {formTable.Size}");
            if (container.Parent != null) // container is outcomeFormContainer, its parent is logFrameTopEntryPanel
            {
                Debug.WriteLine($"CreateOutcomeEntryFormFields: container.Parent (logFrameTopEntryPanel) Size: {container.Parent.Size}, Visible: {container.Parent.Visible}");
            }
            container.Refresh(); // Refresh the container (outcomeFormContainer)
        }

        private void AddOutcomeFromTopEntryForm(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Outcome description cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Outcome newOutcome = new Outcome
            {
                ProjectID = _currentProject.ProjectID,
                OutcomeDescription = description.Trim(),
                Outputs = new List<Output>()
            };
            if (_currentProject.Outcomes == null) _currentProject.Outcomes = new List<Outcome>();
            _currentProject.Outcomes.Add(newOutcome);

            TextBox txtGlobalDesc = logFrameTopEntryPanel?.Controls.Find("txtGlobalOutcomeDescription", true)?.FirstOrDefault() as TextBox;
            if (txtGlobalDesc != null) txtGlobalDesc.Clear();

            RenderLogFrameHierarchy();
        }

        // Event handler for the sidebar "Add Outcome" button
        private void btnAddOutcome_Global_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("btnAddOutcome_Global_Click called.");
            Debug.WriteLine($"btnAddOutcome_Global_Click: logFrameTopEntryPanel is {(logFrameTopEntryPanel == null ? "null" : "not null")}");

            if (logFrameTopEntryPanel != null)
            {
                // Ensure parent containers are visible before creating content
                if (this.mainVerticalSplit != null) this.mainVerticalSplit.Visible = true;
                if (this.mainVerticalSplit != null && this.mainVerticalSplit.Panel2 != null) this.mainVerticalSplit.Panel2.Visible = true;
                if (this.rightHorizontalSplit != null) this.rightHorizontalSplit.Visible = true;
                if (this.rightHorizontalSplit != null && this.rightHorizontalSplit.Panel1 != null) this.rightHorizontalSplit.Panel1.Visible = true;

                this.logFrameTopEntryPanel.Visible = true; // Make sure the top panel itself is visible

                CreateTopEntryAreaForOutcomes();
                TextBox txtGlobalDesc = logFrameTopEntryPanel.Controls.Find("txtGlobalOutcomeDescription", true)?.FirstOrDefault() as TextBox;
                txtGlobalDesc?.Focus();

                Debug.WriteLine("btnAddOutcome_Global_Click: Attempting Invalidate/Update on logFrameTopEntryPanel.");
                logFrameTopEntryPanel.Invalidate(true); // Invalidate with children
                logFrameTopEntryPanel.Update();
            }
            else
            {
                MessageBox.Show("LogFrame UI components for outcome entry are not initialized.", "UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Application.DoEvents(); // Process message queue
            Debug.WriteLine("btnAddOutcome_Global_Click: Application.DoEvents() called after Invalidate/Update.");
        }


        private void RenderLogFrameHierarchy()
        {
            if (logFrameMiddleDisplayPanel == null)
            {
                Debug.WriteLine("RenderLogFrameHierarchy: logFrameMiddleDisplayPanel is null!");
                return;
            }

            logFrameMiddleDisplayPanel.SuspendLayout();
            logFrameMiddleDisplayPanel.Controls.Clear();

            if (_currentProject?.Outcomes == null || !_currentProject.Outcomes.Any())
            {
                TableLayoutPanel emptyStatePanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
                emptyStatePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                emptyStatePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
                emptyStatePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                emptyStatePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
                Label noDataLabel = new Label
                {
                    Text = "No outcomes defined. Use 'Add New Outcome' to begin.",
                    Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20)
                };
                emptyStatePanel.Controls.Add(new Panel(), 0, 0);
                emptyStatePanel.Controls.Add(noDataLabel, 0, 1);
                emptyStatePanel.Controls.Add(new Panel(), 0, 2);
                logFrameMiddleDisplayPanel.Controls.Add(emptyStatePanel);
                logFrameMiddleDisplayPanel.ResumeLayout(true);
                return;
            }

            FlowLayoutPanel outcomesContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(5),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            logFrameMiddleDisplayPanel.Controls.Add(outcomesContainer);

            outcomesContainer.SuspendLayout();
            foreach (var outcome in _currentProject.Outcomes.ToList())
            {
                Panel outcomeCard = CreateOutcomeDisplayCard(outcome);
                outcomesContainer.Controls.Add(outcomeCard);
            }
            outcomesContainer.ResumeLayout(false);
            outcomesContainer.PerformLayout();

            EventHandler cardResizeHandler = (s, ev) => {
                var flp = s as FlowLayoutPanel;
                if (flp == null || flp.IsDisposed) return;
                try
                {
                    flp.SuspendLayout();
                    foreach (Control card in flp.Controls.OfType<Panel>())
                    {
                        if (!card.IsDisposed) card.Width = flp.ClientSize.Width - card.Margin.Horizontal;
                    }
                    flp.ResumeLayout(true);
                }
                catch (Exception ex) { Debug.WriteLine($"Error in cardResizeHandler: {ex.Message}"); }
            };
            outcomesContainer.SizeChanged -= cardResizeHandler;
            outcomesContainer.SizeChanged += cardResizeHandler;
            if (outcomesContainer.Controls.Count > 0) cardResizeHandler(outcomesContainer, EventArgs.Empty);

            logFrameMiddleDisplayPanel.ResumeLayout(true);
        }

        private Panel CreateOutcomeDisplayCard(Outcome outcome)
        {
            Panel card = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10),
                Tag = outcome,
                Width = logFrameMiddleDisplayPanel.ClientSize.Width - 25
            };
            card.SuspendLayout();

            Panel header = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.FromArgb(239, 246, 255), Padding = new Padding(8, 0, 5, 0) };
            Label lblDesc = new Label
            {
                Text = outcome.OutcomeDescription,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 64, 175),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Button btnDel = CreateStyledSmallButton("🗑️", Color.Transparent, Color.FromArgb(185, 28, 28), "sm", "Delete Outcome");
            btnDel.Dock = DockStyle.Right; btnDel.Click += (s, e) => DeleteOutcome(outcome);
            header.Controls.Add(lblDesc); header.Controls.Add(btnDel);
            card.Controls.Add(header);

            Panel content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            card.Controls.Add(content); content.BringToFront();

            Panel outputEntry = CreateOutputEntryFormForOutcome(outcome);
            outputEntry.Dock = DockStyle.Top; content.Controls.Add(outputEntry);

            FlowLayoutPanel outputsDisplay = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            content.Controls.Add(outputsDisplay); outputsDisplay.BringToFront();

            if (outcome.Outputs != null && outcome.Outputs.Any())
            {
                outputsDisplay.SuspendLayout();
                foreach (var output in outcome.Outputs.ToList())
                {
                    outputsDisplay.Controls.Add(CreateOutputDisplayCard(output, outcome));
                }
                outputsDisplay.ResumeLayout(false); outputsDisplay.PerformLayout();
            }

            EventHandler innerCardResize = (s, ev) => {
                var flp = s as FlowLayoutPanel; if (flp == null || flp.IsDisposed) return;
                try { flp.SuspendLayout(); foreach (Control c in flp.Controls.OfType<Panel>()) { if (!c.IsDisposed) c.Width = flp.ClientSize.Width - c.Margin.Horizontal; } flp.ResumeLayout(true); }
                catch (Exception ex) { Debug.WriteLine($"Error in innerCardResize: {ex.Message}"); }
            };
            outputsDisplay.SizeChanged -= innerCardResize; outputsDisplay.SizeChanged += innerCardResize;
            if (outputsDisplay.Controls.Count > 0) innerCardResize(outputsDisplay, EventArgs.Empty);


            card.ResumeLayout(false); card.PerformLayout();
            int cardHeight = header.Height + content.Padding.Vertical;
            foreach (Control c in content.Controls) cardHeight += c.Height + c.Margin.Vertical;
            card.Height = cardHeight + card.Padding.Vertical * 2 + 5; // Added some padding
            return card;
        }

        private Panel CreateOutputEntryFormForOutcome(Outcome parentOutcome)
        {
            Panel entry = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 5, 0, 8), BackColor = Color.FromArgb(249, 250, 251), Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 8) };
            TableLayoutPanel tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F)); tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            Label lbl = new Label { Text = "New Output:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F) };
            TextBox txt = new TextBox { Name = $"txtOutputForOutcome_{parentOutcome.GetHashCode()}_{Guid.NewGuid().ToString("N").Substring(0, 6)}", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Height = 28, BorderStyle = BorderStyle.FixedSingle };
            Button btn = new Button { Text = "Add Output", Dock = DockStyle.Fill, Height = 30, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            ApplyButtonStyle(btn, Color.FromArgb(22, 163, 74), Color.White);
            btn.Click += (s, e) => { if (!string.IsNullOrWhiteSpace(txt.Text)) { AddOutputToOutcome(parentOutcome, txt.Text.Trim()); txt.Clear(); } else { MessageBox.Show("Output description empty."); } };
            tbl.Controls.Add(lbl, 0, 0); tbl.Controls.Add(txt, 1, 0); tbl.Controls.Add(btn, 2, 0);
            entry.Controls.Add(tbl);
            return entry;
        }

        private Panel CreateOutputDisplayCard(Output output, Outcome parentOutcome)
        {
            Panel card = new Panel { BackColor = Color.FromArgb(240, 243, 250), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(1), Margin = new Padding(0, 0, 0, 8), Tag = output, Width = logFrameMiddleDisplayPanel.ClientSize.Width - 50 };
            card.SuspendLayout();
            Panel header = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(221, 228, 242), Padding = new Padding(6, 0, 3, 0) };
            Label lblDesc = new Label { Text = output.OutputDescription, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(49, 46, 129), TextAlign = ContentAlignment.MiddleLeft };
            Button btnDel = CreateStyledSmallButton("🗑️", Color.Transparent, Color.FromArgb(185, 28, 28), "xs", "Delete Output");
            btnDel.Dock = DockStyle.Right; btnDel.Click += (s, e) => DeleteOutput(parentOutcome, output);
            header.Controls.Add(lblDesc); header.Controls.Add(btnDel);
            card.Controls.Add(header);

            Panel content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            card.Controls.Add(content); content.BringToFront();

            Panel actEntry = CreateActivityEntryFormForOutput(output); actEntry.Dock = DockStyle.Top; content.Controls.Add(actEntry);
            FlowLayoutPanel actsDisplay = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 3, 0, 2) };
            content.Controls.Add(actsDisplay); actsDisplay.BringToFront();
            if (output.Activities != null && output.Activities.Any())
            {
                Label actLbl = new Label { Text = "Activities:", Dock = DockStyle.Top, AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Italic), ForeColor = Color.FromArgb(80, 80, 80), Padding = new Padding(0, 2, 0, 1) }; actsDisplay.Controls.Add(actLbl);
                actsDisplay.SuspendLayout();
                foreach (var act in output.Activities.ToList()) actsDisplay.Controls.Add(CreateItemDisplayPanel("Activity", act.ActivityDescription, Color.FromArgb(199, 210, 254), () => DeleteActivityFromOutput(output, act)));
                actsDisplay.ResumeLayout(false); actsDisplay.PerformLayout();
            }

            Panel indEntry = CreateIndicatorEntryFormForOutput(output); indEntry.Dock = DockStyle.Top; content.Controls.Add(indEntry); indEntry.BringToFront();
            FlowLayoutPanel indsDisplay = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 3, 0, 2) };
            content.Controls.Add(indsDisplay); indsDisplay.BringToFront();
            if (output.ProjectIndicators != null && output.ProjectIndicators.Any())
            {
                Label indLbl = new Label { Text = "Indicators:", Dock = DockStyle.Top, AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Italic), ForeColor = Color.FromArgb(80, 80, 80), Padding = new Padding(0, 2, 0, 1) }; indsDisplay.Controls.Add(indLbl);
                indsDisplay.SuspendLayout();
                foreach (var ind in output.ProjectIndicators.ToList()) indsDisplay.Controls.Add(CreateItemDisplayPanel("Indicator", ind.IndicatorName, Color.FromArgb(253, 230, 138), () => DeleteIndicatorFromOutput(output, ind)));
                indsDisplay.ResumeLayout(false); indsDisplay.PerformLayout();
            }

            content.Controls.SetChildIndex(actEntry, 0); content.Controls.SetChildIndex(actsDisplay, 1);
            content.Controls.SetChildIndex(indEntry, 2); content.Controls.SetChildIndex(indsDisplay, 3);

            EventHandler itemResize = (s, ev) => {
                var flp = s as FlowLayoutPanel; if (flp == null || flp.IsDisposed) return;
                try { flp.SuspendLayout(); foreach (Control c in flp.Controls.OfType<Panel>()) { if (!c.IsDisposed) c.Width = flp.ClientSize.Width - c.Margin.Horizontal; } flp.ResumeLayout(true); }
                catch (Exception ex) { Debug.WriteLine($"Error in itemResize: {ex.Message}"); }
            };
            actsDisplay.SizeChanged -= itemResize; actsDisplay.SizeChanged += itemResize;
            indsDisplay.SizeChanged -= itemResize; indsDisplay.SizeChanged += itemResize;
            if (actsDisplay.Controls.Count > 1) itemResize(actsDisplay, EventArgs.Empty);
            if (indsDisplay.Controls.Count > 1) itemResize(indsDisplay, EventArgs.Empty);


            card.ResumeLayout(false); card.PerformLayout();
            int cardH = header.Height + content.Padding.Vertical;
            foreach (Control c in content.Controls) cardH += c.Height + c.Margin.Vertical;
            card.Height = cardH + card.Padding.Vertical * 2 + 5; // Added some padding
            return card;
        }

        private Panel CreateActivityEntryFormForOutput(Output parentOutput)
        {
            Panel entry = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 3, 0, 5), BackColor = Color.Transparent, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 3) };
            TableLayoutPanel tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F)); tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            Label lbl = new Label { Text = "New Activity:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8.5F) };
            TextBox txt = new TextBox { Name = $"txtActivityForOutput_{parentOutput.GetHashCode()}_{Guid.NewGuid().ToString("N").Substring(0, 6)}", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5F), Height = 26, BorderStyle = BorderStyle.FixedSingle };
            Button btn = new Button { Text = "Add Activity", Dock = DockStyle.Fill, Height = 28, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            ApplyButtonStyle(btn, Color.FromArgb(221, 214, 254), Color.FromArgb(109, 40, 217)); // Purple
            btn.Click += (s, e) => { if (!string.IsNullOrWhiteSpace(txt.Text)) { AddActivityToOutput(parentOutput, txt.Text.Trim()); txt.Clear(); } else { MessageBox.Show("Activity description empty."); } };
            tbl.Controls.Add(lbl, 0, 0); tbl.Controls.Add(txt, 1, 0); tbl.Controls.Add(btn, 2, 0);
            entry.Controls.Add(tbl);
            return entry;
        }

        private Panel CreateIndicatorEntryFormForOutput(Output parentOutput)
        {
            Panel entry = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 3, 0, 5), BackColor = Color.Transparent, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 3) };
            TableLayoutPanel tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F)); tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            Label lbl = new Label { Text = "New Indicator:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8.5F) };
            TextBox txt = new TextBox { Name = $"txtIndicatorForOutput_{parentOutput.GetHashCode()}_{Guid.NewGuid().ToString("N").Substring(0, 6)}", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5F), Height = 26, BorderStyle = BorderStyle.FixedSingle };
            Button btn = new Button { Text = "Add Indicator", Dock = DockStyle.Fill, Height = 28, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            ApplyButtonStyle(btn, Color.FromArgb(254, 240, 138), Color.FromArgb(180, 83, 9)); // Amber
            btn.Click += (s, e) => { if (!string.IsNullOrWhiteSpace(txt.Text)) { AddIndicatorToOutput(parentOutput, txt.Text.Trim()); txt.Clear(); } else { MessageBox.Show("Indicator description empty."); } };
            tbl.Controls.Add(lbl, 0, 0); tbl.Controls.Add(txt, 1, 0); tbl.Controls.Add(btn, 2, 0);
            entry.Controls.Add(tbl);
            return entry;
        }

        private Panel CreateItemDisplayPanel(string itemType, string description, Color accentColor, Action deleteAction)
        {
            Panel item = new Panel { Height = 28, BackColor = Color.White, Margin = new Padding(0, 0, 0, 3), Dock = DockStyle.Top, Tag = itemType, BorderStyle = BorderStyle.FixedSingle };
            item.SuspendLayout();
            Panel accent = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = accentColor };
            Label lbl = new Label { Text = description, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0), Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(40, 40, 40) };
            Button btnDel = CreateStyledSmallButton("🗑️", Color.Transparent, accentColor, "xs", $"Delete {itemType}");
            btnDel.Dock = DockStyle.Right; btnDel.Click += (s, e) => deleteAction();
            item.Controls.Add(lbl); item.Controls.Add(accent); item.Controls.Add(btnDel);
            item.ResumeLayout(false);
            return item;
        }

        // --- Data Modification Handlers ---
        private void AddOutputToOutcome(Outcome parentOutcome, string description)
        {
            Output newOutput = new Output { OutcomeID = parentOutcome.OutcomeID, OutputDescription = description, Activities = new List<ProjectActivity>(), ProjectIndicators = new List<ProjectIndicator>() };
            if (parentOutcome.Outputs == null) parentOutcome.Outputs = new List<Output>();
            parentOutcome.Outputs.Add(newOutput);
            RenderLogFrameHierarchy();
        }
        private void AddActivityToOutput(Output parentOutput, string description)
        {
            ProjectActivity newActivity = new ProjectActivity { ActivityDescription = description };
            if (parentOutput.Activities == null) parentOutput.Activities = new List<ProjectActivity>();
            parentOutput.Activities.Add(newActivity);
            RenderLogFrameHierarchy();
        }
        private void AddIndicatorToOutput(Output parentOutput, string description)
        {
            ProjectIndicator newIndicator = new ProjectIndicator { IndicatorName = description };
            if (parentOutput.ProjectIndicators == null) parentOutput.ProjectIndicators = new List<ProjectIndicator>();
            parentOutput.ProjectIndicators.Add(newIndicator);
            RenderLogFrameHierarchy();
        }
        private void DeleteOutcome(Outcome outcome)
        {
            if (MessageBox.Show($"Delete Outcome '{outcome.OutcomeDescription}' and ALL its contents?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            { if (_currentProject.Outcomes.Remove(outcome)) RenderLogFrameHierarchy(); }
        }
        private void DeleteOutput(Outcome parentOutcome, Output output)
        {
            if (MessageBox.Show($"Delete Output '{output.OutputDescription}' and ALL its contents?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            { if (parentOutcome.Outputs.Remove(output)) RenderLogFrameHierarchy(); }
        }
        private void DeleteActivityFromOutput(Output parentOutput, ProjectActivity activity)
        {
            if (MessageBox.Show($"Delete Activity '{activity.ActivityDescription}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            { if (parentOutput.Activities.Remove(activity)) RenderLogFrameHierarchy(); }
        }
        private void DeleteIndicatorFromOutput(Output parentOutput, ProjectIndicator indicator)
        {
            if (MessageBox.Show($"Delete Indicator '{indicator.IndicatorName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            { if (parentOutput.ProjectIndicators.Remove(indicator)) RenderLogFrameHierarchy(); }
        }

        private void InitializeLogFrameUI_New()
        {
            TabPage logFrameTabPage = tabControlProjectDetails.TabPages["tabPageLogFrame"];
            if (logFrameTabPage == null)
            {
                logFrameTabPage = this.Controls.OfType<TabControl>()
                                      .FirstOrDefault(tc => tc.Name == "tabControlProjectDetails")?
                                      .TabPages.Cast<TabPage>()
                                      .FirstOrDefault(tp => tp.Name == "tabPageLogFrame");
                if (logFrameTabPage == null)
                {
                    MessageBox.Show("LogFrame TabPage ('tabPageLogFrame') could not be found. UI setup cannot proceed.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            logFrameTabPage.Controls.Clear();
            logFrameTabPage.BackColor = Color.FromArgb(229, 231, 235);

            // Defer SplitterDistance, Panel1MinSize, and Panel2MinSize assignment & use class members
            this.mainVerticalSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, FixedPanel = FixedPanel.Panel1, BackColor = Color.Transparent, BorderStyle = BorderStyle.None };
            logFrameTabPage.Controls.Add(this.mainVerticalSplit);

            Debug.WriteLine($"InitializeLogFrameUI_New: sidebarPanel is {(this.sidebarPanel == null ? "null" : "not null")} BEFORE potential recreation."); // Added debug line
            if (this.sidebarPanel == null)
            {
                this.sidebarPanel = new Panel();
                Debug.WriteLine("Warning: sidebarPanel was null and re-created in InitializeLogFrameUI_New.");
            }
            this.sidebarPanel.Dock = DockStyle.Fill;
            this.sidebarPanel.BackColor = Color.FromArgb(243, 244, 246);
            this.sidebarPanel.Padding = new Padding(8);

            if (this.mainVerticalSplit != null) // Check if mainVerticalSplit is initialized
            {
                this.mainVerticalSplit.Panel1.Controls.Clear();
                this.mainVerticalSplit.Panel1.Controls.Add(this.sidebarPanel);
            }
            else
            {
                Debug.WriteLine("ERROR: mainVerticalSplit is null before adding sidebarPanel. Sidebar will not be visible.");
            }

            this.CreateSidebarButtons();

            // Defer SplitterDistance, Panel1MinSize, and Panel2MinSize assignment & use class members
            this.rightHorizontalSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, FixedPanel = FixedPanel.Panel1, BackColor = Color.Transparent, BorderStyle = BorderStyle.None, SplitterWidth = 6 };
            this.mainVerticalSplit.Panel2.Controls.Add(this.rightHorizontalSplit);

            this.logFrameTopEntryPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(249, 250, 251), Padding = new Padding(10), AutoScroll = true };
            rightHorizontalSplit.Panel1.Controls.Add(this.logFrameTopEntryPanel);
            this.CreateTopEntryAreaForOutcomes();

            this.logFrameMiddleDisplayPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(236, 242, 249), Padding = new Padding(10), AutoScroll = true };
            rightHorizontalSplit.Panel2.Controls.Add(this.logFrameMiddleDisplayPanel);

            if (this.pnlLogFrameMain != null && !object.ReferenceEquals(this.pnlLogFrameMain, this.logFrameMiddleDisplayPanel))
            {
                this.pnlLogFrameMain = this.logFrameMiddleDisplayPanel;
            }
            else if (this.pnlLogFrameMain == null)
            {
                this.pnlLogFrameMain = this.logFrameMiddleDisplayPanel;
            }

            this.RenderLogFrameHierarchy();
            // SplitterDistance logic is now moved to AdjustSplitterDistances() and will be called from Form_Shown
            Debug.WriteLine($"InitializeLogFrameUI_New END: mainVerticalSplit is {(this.mainVerticalSplit == null ? "null" : "not null")}"); // Added debug line
            Debug.WriteLine($"InitializeLogFrameUI_New END: rightHorizontalSplit is {(this.rightHorizontalSplit == null ? "null" : "not null")}"); // Added debug line
        }

        // --- END NEW LOGFRAME UI METHODS ---

        private void AdjustSplitterDistances()
        {
            if (this.mainVerticalSplit == null || this.rightHorizontalSplit == null)
            {
                Debug.WriteLine("AdjustSplitterDistances: SplitContainer controls not initialized.");
                return;
            }

            // Ensure SplitContainer controls themselves are visible
            this.mainVerticalSplit.Visible = true;
            this.rightHorizontalSplit.Visible = true;

            // Ensure their panels are visible
            this.mainVerticalSplit.Panel1.Visible = true;
            this.mainVerticalSplit.Panel2.Visible = true;
            this.rightHorizontalSplit.Panel1.Visible = true;
            this.rightHorizontalSplit.Panel2.Visible = true;

            if (this.logFrameTopEntryPanel != null)
            {
                this.logFrameTopEntryPanel.Visible = true;
            }
            if (this.logFrameMiddleDisplayPanel != null)
            {
                this.logFrameMiddleDisplayPanel.Visible = true;
            }

            // Safely set SplitterDistance for mainVerticalSplit
            if (this.mainVerticalSplit.Parent != null && this.mainVerticalSplit.Parent.ClientSize.Width > 0)
            {
                try
                {
                    mainVerticalSplit.Panel1MinSize = 200;
                    mainVerticalSplit.Panel2MinSize = 450;

                    // Ensure Panel1MinSize and Panel2MinSize are respected, and SplitterWidth is accounted for.
                    int availableWidth = mainVerticalSplit.Parent.ClientSize.Width - mainVerticalSplit.SplitterWidth;
                    int panel1Min = mainVerticalSplit.Panel1MinSize; // Use the just-set value
                    int panel2Min = mainVerticalSplit.Panel2MinSize; // Use the just-set value

                    if (availableWidth >= panel1Min + panel2Min)
                    {
                        int targetDistance = 230;
                        int newDistance = Math.Max(panel1Min, Math.Min(targetDistance, availableWidth - panel2Min));
                        mainVerticalSplit.SplitterDistance = newDistance;
                    }
                    else if (availableWidth >= panel1Min)
                    {
                        mainVerticalSplit.SplitterDistance = panel1Min; // Fallback to Panel1MinSize if not enough space for both minimums
                    }
                    // If availableWidth < panel1Min, it's a problematic state, SplitContainer might take over or error if distance is set.
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting mainVerticalSplit properties: {ex.Message}");
                }
            }

            // Safely set SplitterDistance for rightHorizontalSplit
            if (rightHorizontalSplit.Parent != null && rightHorizontalSplit.Parent.ClientSize.Height > 0)
            {
                try
                {
                    rightHorizontalSplit.Panel1MinSize = 60;
                    rightHorizontalSplit.Panel2MinSize = 250;

                    int availableHeight = rightHorizontalSplit.Parent.ClientSize.Height - rightHorizontalSplit.SplitterWidth;
                    int panel1Min = rightHorizontalSplit.Panel1MinSize; // Use the just-set value
                    int panel2Min = rightHorizontalSplit.Panel2MinSize; // Use the just-set value

                    if (availableHeight >= panel1Min + panel2Min)
                    {
                        int targetDistance = 70;
                        int newDistance = Math.Max(panel1Min, Math.Min(targetDistance, availableHeight - panel2Min));
                        rightHorizontalSplit.SplitterDistance = newDistance;
                    }
                    else if (availableHeight >= panel1Min)
                    {
                        rightHorizontalSplit.SplitterDistance = panel1Min; // Fallback
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting rightHorizontalSplit properties: {ex.Message}");
                }
            }

            // Debug visibility of relevant panels after adjustments
            if (this.mainVerticalSplit != null && this.mainVerticalSplit.Panel2 != null)
            {
                Debug.WriteLine($"AdjustSplitterDistances: mainVerticalSplit.Panel2.Visible = {this.mainVerticalSplit.Panel2.Visible}");
            }
            if (this.rightHorizontalSplit != null && this.rightHorizontalSplit.Panel1 != null)
            {
                Debug.WriteLine($"AdjustSplitterDistances: rightHorizontalSplit.Panel1.Visible = {this.rightHorizontalSplit.Panel1.Visible}");
            }
            if (this.logFrameTopEntryPanel != null)
            {
                Debug.WriteLine($"AdjustSplitterDistances: logFrameTopEntryPanel.Visible = {this.logFrameTopEntryPanel.Visible}, logFrameTopEntryPanel.HasChildren = {this.logFrameTopEntryPanel.HasChildren}");
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
                bool wasNewProject = !_isEditMode && _currentProject.ProjectID == 0;
                bool success = await _projectService.SaveProjectAsync(_currentProject);

                if (success)
                {
                    MessageBox.Show("Project saved successfully.", "Success");
                    this.DialogResult = DialogResult.OK;

                    if (wasNewProject && _currentProject.ProjectID > 0)
                    {
                        _isEditMode = true;
                        this.Text = $"Edit Project - {_currentProject.ProjectName}";
                        await HandleBudgetTabVisibilityAndStateAsync();
                        if (_budgetTabControlInstance != null && tabControlProjectDetails.TabPages.ContainsKey("tabPageBudget"))
                        {
                            _budgetTabControlInstance.LoadProject(_currentProject);
                        }
                    }
                    else if (_isEditMode && !wasNewProject)
                    {
                        this.Close();
                    }
                }
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
        // Note: The SetAccessibilityProperties method might appear duplicated if merging the new UI code.
        // Ensure only one definition remains. The one at the end of the original user-provided code seems more complete.
    }
}
