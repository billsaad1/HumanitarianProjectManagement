// Using statements from the original file - confirmed to be sufficient
using HumanitarianProjectManagement.DataAccessLayer;
using HumanitarianProjectManagement.Models;
using ProjectActivity = HumanitarianProjectManagement.Models.Activity; // Keep this alias
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
using HumanitarianProjectManagement; // Assuming this is where BudgetCategoriesEnum might be if not in Models
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
        private BudgetTabUserControl _budgetTabControlInstance;
        private bool _formInitialLoadComplete = false;

        private FlowLayoutPanel flpLogFrameMainContent;
        // Note: pnlLogFrameMain from the designer is no longer directly used for logframe content;
        // flpLogFrameMainContent is placed inside the tabPageLogFrame.

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
                _currentProject.Outcomes = _currentProject.Outcomes?.ToList() ?? new List<Outcome>();
                foreach(var outcome in _currentProject.Outcomes)
                {
                    outcome.Outputs = outcome.Outputs?.ToList() ?? new List<Output>();
                    foreach(var output in outcome.Outputs)
                    {
                        output.Activities = output.Activities?.ToList() ?? new List<ProjectActivity>();
                        output.ProjectIndicators = output.ProjectIndicators?.ToList() ?? new List<ProjectIndicator>();
                    }
                }
                _currentProject.DetailedBudgetLines = new BindingList<DetailedBudgetLine>(_currentProject.DetailedBudgetLines?.ToList() ?? new List<DetailedBudgetLine>());
                PopulateControls();
            }
            else
            {
                _currentProject = new Project(); 
                this.Text = "Add New Project";
                _currentProject.Outcomes = new List<Outcome>();
                _currentProject.DetailedBudgetLines = new BindingList<DetailedBudgetLine>();
                dtpStartDate.Value = DateTime.Now;
                dtpStartDate.Checked = false;
                dtpEndDate.Checked = false;
            }

            this.Load += new System.EventHandler(this.ProjectCreateEditForm_Load);
        }

        #region Budget Tab and General Form Load
        private void InitializeBudgetUITab()
        {
            Debug.WriteLine("InitializeBudgetUITab: Started.");
            try
            {
                if (this.tabControlProjectDetails == null) { Debug.WriteLine("InitializeBudgetUITab: tabControlProjectDetails is null."); return; }
                if (_budgetTabControlInstance == null)
                {
                    _budgetTabControlInstance = new BudgetTabUserControl { Dock = DockStyle.Fill };
                    Debug.WriteLine("InitializeBudgetUITab: _budgetTabControlInstance created.");
                }
                TabPage tabPageBudget = tabControlProjectDetails.TabPages["tabPageBudget"];
                if (tabPageBudget == null)
                {
                    tabPageBudget = new TabPage { Name = "tabPageBudget", Text = "Budget" };
                    tabControlProjectDetails.TabPages.Add(tabPageBudget);
                }
                if (!tabPageBudget.Controls.Contains(_budgetTabControlInstance))
                {
                    tabPageBudget.Controls.Clear();
                    tabPageBudget.Controls.Add(_budgetTabControlInstance);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"InitializeBudgetUITab: EXCEPTION: {ex.ToString()}"); }
        }

        private async Task HandleBudgetTabVisibilityAndStateAsync()
        {
            // ... (Implementation as before) ...
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
                    if (tabControlProjectDetails.TabPages.Contains(tabPageBudget)) tabControlProjectDetails.TabPages.Remove(tabPageBudget);
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
                    }
                    _budgetTabControlInstance.LoadProject(_currentProject);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleBudgetTabVisibilityAndStateAsync: EXCEPTION: {ex.ToString()}");
                if (tabControlProjectDetails.TabPages.Contains(tabPageBudget)) tabControlProjectDetails.TabPages.Remove(tabPageBudget);
                MessageBox.Show("Error determining user permissions for the budget tab.", "Permissions Error");
            }
        }


        private async void ProjectCreateEditForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine($"ProjectCreateEditForm_Load: Started. _isEditMode: {_isEditMode}, _currentProject.ProjectID: {_currentProject?.ProjectID ?? -1}, _formInitialLoadComplete: {_formInitialLoadComplete}");
            if (!_formInitialLoadComplete)
            {
                try { await LoadComboBoxesAsync(); }
                catch (Exception ex) { MessageBox.Show("Error loading sections/managers: " + ex.Message, "Load Data Exception"); return; }
                
                InitializeLogFrameUI(); 
                InitializeActivityPlanTab();
                _formInitialLoadComplete = true;
            }
            await HandleBudgetTabVisibilityAndStateAsync();
            // Event handler attachments for dgvActivityPlan and date pickers
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
        #endregion

        #region LogFrame UI Generation
        private void InitializeLogFrameUI()
        {
            Control logFrameTabContainer = tabControlProjectDetails.TabPages["tabPageLogFrame"] ?? 
                                           this.Controls.Find("tabPageLogFrame", true).FirstOrDefault();

            if (logFrameTabContainer == null) 
            { 
                logFrameTabContainer = new TabPage { Name = "tabPageLogFrame", Text = "LogFrame" };
                tabControlProjectDetails.TabPages.Add(logFrameTabContainer);
            }
            
            logFrameTabContainer.Controls.Clear(); 

            flpLogFrameMainContent = new FlowLayoutPanel
            {
                Name = "flpLogFrameMainContent", Dock = DockStyle.Fill, AutoScroll = true,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                Padding = new Padding(10), BackColor = Color.White
            };
            logFrameTabContainer.Controls.Add(flpLogFrameMainContent);
            RefreshLogFrameView();
        }
        
        private void RefreshLogFrameView()
        {
            if (flpLogFrameMainContent == null) return;
            flpLogFrameMainContent.SuspendLayout();
            flpLogFrameMainContent.Controls.Clear();

            flpLogFrameMainContent.Controls.Add(CreateGlobalAddOutcomePanel());
            if (_currentProject?.Outcomes != null)
            {
                for (int i = 0; i < _currentProject.Outcomes.Count; i++)
                {
                    flpLogFrameMainContent.Controls.Add(CreateOutcomeBlock(_currentProject.Outcomes[i], i + 1));
                }
            }
            flpLogFrameMainContent.ResumeLayout(true);
            flpLogFrameMainContent.PerformLayout();
        }

        private Panel CreatePaddedPanel(bool isTopLevel = false)
        {
            return new Panel {
                AutoSize = true,
                Width = flpLogFrameMainContent.Width - flpLogFrameMainContent.Padding.Horizontal - (isTopLevel ? 5 : 25),
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10),
                BorderStyle = BorderStyle.FixedSingle
            };
        }
        
        private Panel CreateInputRowPanel(Control inputControl, Button addButton, string labelText = null)
        {
            Panel pnlInputRow = new Panel { AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 5, 0, 5) };
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = labelText != null ? 3 : 2};
            
            if(labelText != null) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            int colIndex = 0;
            if(labelText != null) tlp.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0,3,3,0)}, colIndex++, 0);
            
            inputControl.Dock = DockStyle.Fill;
            tlp.Controls.Add(inputControl, colIndex++, 0);
            
            addButton.Anchor = AnchorStyles.Right;
            addButton.Height = inputControl is TextBox tb && tb.Multiline ? tb.Height : 30;
            tlp.Controls.Add(addButton, colIndex++, 0);
            
            pnlInputRow.Controls.Add(tlp);
            return pnlInputRow;
        }


        // --- Outcome Methods ---
        private Panel CreateGlobalAddOutcomePanel()
        {
            Panel pnl = CreatePaddedPanel(true);
            pnl.BackColor = Color.FromArgb(230, 240, 255);
            pnl.Controls.Add(new Label { Text = "Define a New Project Outcome:", AutoSize = true, Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), Dock = DockStyle.Top, Margin = new Padding(0,0,0,5) });
            
            TextBox txtDesc = new TextBox { Name = "txtGlobalNewOutcomeDescription", Multiline = true, Height = 40, Font = new Font(this.Font.FontFamily, 9), ScrollBars = ScrollBars.Vertical };
            Button btnAdd = new Button { Name = "btnGlobalAddOutcome", Text = "Add Outcome", Font = new Font(this.Font.FontFamily, 9), FlatStyle = FlatStyle.System, Padding = new Padding(5) };
            btnAdd.Click += BtnGlobalAddOutcome_Click;
            pnl.Controls.Add(CreateInputRowPanel(txtDesc, btnAdd));
            return pnl;
        }

        private void BtnGlobalAddOutcome_Click(object sender, EventArgs e)
        {
            Panel globalAddPanel = (sender as Button)?.Parent?.Parent?.Parent as Panel; // Button -> TLP -> Panel (InputRow) -> Panel (GlobalAdd)
            TextBox txtDesc = globalAddPanel?.Controls.OfType<Panel>().FirstOrDefault()?.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == "txtGlobalNewOutcomeDescription");

            if (txtDesc == null || string.IsNullOrWhiteSpace(txtDesc.Text)) { MessageBox.Show("Outcome description empty.", "Validation Error"); txtDesc?.Focus(); return; }
            Outcome newOutcome = new Outcome { OutcomeDescription = txtDesc.Text.Trim(), ProjectID = _currentProject.ProjectID, Outputs = new List<Output>() };
            _currentProject.Outcomes.Add(newOutcome);
            txtDesc.Clear();
            RefreshLogFrameView();
        }

        private Panel CreateOutcomeBlock(Outcome outcome, int counter)
        {
            Panel pnlOutcome = CreatePaddedPanel(true);
            pnlOutcome.Name = $"pnlOutcome_{outcome.GetHashCode()}";
            pnlOutcome.BackColor = Color.FromArgb(245, 249, 252);

            var headerTlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Width = pnlOutcome.Width - pnlOutcome.Padding.Horizontal };
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            headerTlp.Controls.Add(new Label { Text = $"Outcome {counter}:", Font = new Font(this.Font.FontFamily, 11, FontStyle.Bold), ForeColor = Color.FromArgb(20, 80, 140), AutoSize = true }, 0, 0);
            Button btnDelete = new Button { Text = "Delete Outcome", Tag = outcome, ForeColor = Color.White, BackColor = Color.FromArgb(220, 53, 69), FlatStyle = FlatStyle.Flat, AutoSize = true, Font = new Font(this.Font.FontFamily, 8) };
            btnDelete.FlatAppearance.BorderSize = 0; btnDelete.Click += BtnDeleteLogFrameItem_Click;
            headerTlp.Controls.Add(btnDelete, 1, 0);
            pnlOutcome.Controls.Add(headerTlp);
            
            TextBox txtDesc = new TextBox { Text = outcome.OutcomeDescription, Multiline = true, ScrollBars = ScrollBars.Vertical, MinimumHeight = 50, Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 9), Tag = outcome, Margin = new Padding(0, 5, 0, 10) };
            txtDesc.TextChanged += (s, ev) => { if (txtDesc.Tag is Outcome ot) ot.OutcomeDescription = txtDesc.Text; };
            pnlOutcome.Controls.Add(txtDesc); 

            FlowLayoutPanel flpOutputs = new FlowLayoutPanel { Name = $"flpOutputsForOutcome_{outcome.GetHashCode()}", Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(15, 5, 0, 5), Width = pnlOutcome.Width - pnlOutcome.Padding.Horizontal - 15 };
            pnlOutcome.Controls.Add(flpOutputs);
            if(outcome.Outputs != null) {
                for(int i=0; i < outcome.Outputs.Count; i++) flpOutputs.Controls.Add(CreateOutputBlock(outcome.Outputs[i], $"{counter}.{i+1}"));
            }
            pnlOutcome.Controls.Add(CreateAddOutputPanel(outcome, counter));
            return pnlOutcome;
        }

        // --- Output Methods ---
        private Panel CreateAddOutputPanel(Outcome parentOutcome, int parentCounter)
        {
            Panel pnl = new Panel { Name = $"pnlAddOutputToOutcome_{parentOutcome.GetHashCode()}", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(15, 8, 5, 8), Margin = new Padding(0, 10, 0, 5), BackColor = Color.FromArgb(230, 245, 230) };
            TextBox txtDesc = new TextBox { Name = $"txtNewOutputDescForOutcome_{parentOutcome.GetHashCode()}", Multiline = true, Height = 35, Font = new Font(this.Font.FontFamily, 9), ScrollBars = ScrollBars.Vertical };
            Button btnAdd = new Button { Name = $"btnAddOutputToOutcome_{parentOutcome.GetHashCode()}", Text = "Add Output", Font = new Font(this.Font.FontFamily, 9), FlatStyle = FlatStyle.System, Tag = parentOutcome, Padding = new Padding(5) };
            btnAdd.Click += BtnAddOutputToParentOutcome_Click;
            pnl.Controls.Add(CreateInputRowPanel(txtDesc, btnAdd, $"New Output for Outcome {parentCounter}:"));
            return pnl;
        }

        private void BtnAddOutputToParentOutcome_Click(object sender, EventArgs e)
        {
            Button btnSender = sender as Button;
            Outcome parentOutcome = btnSender?.Tag as Outcome;
            if (parentOutcome == null) return;

            Panel addOutputPanel = btnSender.FindForm().Controls.Find($"pnlAddOutputToOutcome_{parentOutcome.GetHashCode()}", true).FirstOrDefault() as Panel;
            TextBox txtDesc = addOutputPanel?.Controls.OfType<Panel>().FirstOrDefault()?.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == $"txtNewOutputDescForOutcome_{parentOutcome.GetHashCode()}");

            if (txtDesc == null || string.IsNullOrWhiteSpace(txtDesc.Text)) { MessageBox.Show("Output description empty.", "Validation Error"); txtDesc?.Focus(); return; }
            Output newOutput = new Output { OutputDescription = txtDesc.Text.Trim(), OutcomeID = parentOutcome.OutcomeID, ProjectID = _currentProject.ProjectID, ProjectIndicators = new List<ProjectIndicator>(), Activities = new List<ProjectActivity>() };
            parentOutcome.Outputs.Add(newOutput);
            txtDesc.Clear();

            FlowLayoutPanel flpOutputs = addOutputPanel.Parent.Controls.OfType<FlowLayoutPanel>().FirstOrDefault(f => f.Name == $"flpOutputsForOutcome_{parentOutcome.GetHashCode()}");
            if (flpOutputs != null) {
                int outcomeIdx = _currentProject.Outcomes.IndexOf(parentOutcome);
                flpOutputs.Controls.Add(CreateOutputBlock(newOutput, $"{(outcomeIdx >=0 ? outcomeIdx+1 : "?")}.{parentOutcome.Outputs.Count}"));
                flpOutputs.PerformLayout();
            } else { RefreshLogFrameView(); } // Fallback
        }
        
        private Panel CreateOutputBlock(Output output, string numberString)
        {
            Panel pnlOutput = CreatePaddedPanel();
            pnlOutput.Name = $"pnlOutput_{output.GetHashCode()}";
            pnlOutput.BackColor = Color.FromArgb(252, 248, 235); // Light yellow

            var headerTlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Width = pnlOutput.Width - pnlOutput.Padding.Horizontal };
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            headerTlp.Controls.Add(new Label { Text = $"Output {numberString}:", Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold), ForeColor = Color.FromArgb(100, 80, 20), AutoSize = true }, 0, 0);
            Button btnDelete = new Button { Text = "Delete Output", Tag = output, ForeColor = Color.White, BackColor = Color.FromArgb(200, 50, 60), FlatStyle = FlatStyle.Flat, AutoSize = true, Font = new Font(this.Font.FontFamily, 8) };
            btnDelete.FlatAppearance.BorderSize = 0; btnDelete.Click += BtnDeleteLogFrameItem_Click;
            headerTlp.Controls.Add(btnDelete, 1, 0);
            pnlOutput.Controls.Add(headerTlp);

            TextBox txtDesc = new TextBox { Text = output.OutputDescription, Multiline = true, ScrollBars = ScrollBars.Vertical, MinimumHeight = 40, Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 9), Tag = output, Margin = new Padding(0, 5, 0, 10) };
            txtDesc.TextChanged += (s, ev) => { if (txtDesc.Tag is Output op) op.OutputDescription = txtDesc.Text; };
            pnlOutput.Controls.Add(txtDesc);

            FlowLayoutPanel flpActivities = new FlowLayoutPanel { Name = $"flpActivitiesForOutput_{output.GetHashCode()}", Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10, 5, 0, 5), Width = pnlOutput.Width - pnlOutput.Padding.Horizontal - 10 };
            pnlOutput.Controls.Add(flpActivities);
            if(output.Activities != null) {
                 for(int i=0; i < output.Activities.Count; i++) flpActivities.Controls.Add(CreateActivityBlock(output.Activities[i], $"{numberString}.{i+1}"));
            }
            pnlOutput.Controls.Add(CreateAddActivityPanel(output, numberString));

            FlowLayoutPanel flpIndicators = new FlowLayoutPanel { Name = $"flpIndicatorsForOutput_{output.GetHashCode()}", Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10, 5, 0, 5), Width = pnlOutput.Width - pnlOutput.Padding.Horizontal - 10 };
            pnlOutput.Controls.Add(flpIndicators);
             if(output.ProjectIndicators != null) {
                 for(int i=0; i < output.ProjectIndicators.Count; i++) flpIndicators.Controls.Add(CreateIndicatorBlock(output.ProjectIndicators[i], $"{numberString}.{i+1}"));
            }
            pnlOutput.Controls.Add(CreateAddIndicatorPanel(output, numberString));
            return pnlOutput;
        }

        // --- Activity Methods ---
        private Panel CreateAddActivityPanel(Output parentOutput, string parentNumberString)
        {
            Panel pnl = new Panel { Name = $"pnlAddActivityToOutput_{parentOutput.GetHashCode()}", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(10, 5, 0, 5), Margin = new Padding(0, 8, 0, 3), BackColor = Color.FromArgb(225, 240, 225) }; // Pale green
            TextBox txtDesc = new TextBox { Name = $"txtNewActivityDescForOutput_{parentOutput.GetHashCode()}", Multiline = true, Height = 30, Font = new Font(this.Font.FontFamily, 8), ScrollBars = ScrollBars.Vertical };
            Button btnAdd = new Button { Name = $"btnAddActivityToOutput_{parentOutput.GetHashCode()}", Text = "Add Activity", Font = new Font(this.Font.FontFamily, 8), FlatStyle = FlatStyle.System, Tag = parentOutput, Padding = new Padding(4) };
            btnAdd.Click += BtnAddActivityToParentOutput_Click;
            pnl.Controls.Add(CreateInputRowPanel(txtDesc, btnAdd, $"New Activity for Output {parentNumberString}:"));
            return pnl;
        }

        private void BtnAddActivityToParentOutput_Click(object sender, EventArgs e)
        {
            Button btnSender = sender as Button;
            Output parentOutput = btnSender?.Tag as Output;
            if (parentOutput == null) return;

            Panel addPanel = btnSender.FindForm().Controls.Find($"pnlAddActivityToOutput_{parentOutput.GetHashCode()}", true).FirstOrDefault() as Panel;
            TextBox txtDesc = addPanel?.Controls.OfType<Panel>().FirstOrDefault()?.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == $"txtNewActivityDescForOutput_{parentOutput.GetHashCode()}");
            
            if (txtDesc == null || string.IsNullOrWhiteSpace(txtDesc.Text)) { MessageBox.Show("Activity description empty.", "Validation Error"); txtDesc?.Focus(); return; }
            ProjectActivity newActivity = new ProjectActivity { ActivityDescription = txtDesc.Text.Trim(), OutputID = parentOutput.OutputID };
            parentOutput.Activities.Add(newActivity);
            txtDesc.Clear();

            FlowLayoutPanel flpActivities = addPanel.Parent.Controls.OfType<FlowLayoutPanel>().FirstOrDefault(f => f.Name == $"flpActivitiesForOutput_{parentOutput.GetHashCode()}");
            if (flpActivities != null) {
                string outputNumber = (addPanel.Parent.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls.OfType<Label>().FirstOrDefault()?.Text ?? "Output ?:") .Split(' ')[1].TrimEnd(':');
                flpActivities.Controls.Add(CreateActivityBlock(newActivity, $"{outputNumber}.{parentOutput.Activities.Count}"));
                flpActivities.PerformLayout();
            } else { RefreshLogFrameView(); }
        }

        private Panel CreateActivityBlock(ProjectActivity activity, string numberString)
        {
            Panel pnlItem = CreatePaddedPanel();
            pnlItem.Name = $"pnlActivity_{activity.GetHashCode()}";
            pnlItem.BackColor = Color.FromArgb(255, 255, 240); // Lighter Yellow
            pnlItem.Padding = new Padding(5); pnlItem.Margin = new Padding(0,0,0,5);


            var headerTlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Width = pnlItem.Width - pnlItem.Padding.Horizontal };
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            headerTlp.Controls.Add(new Label { Text = $"Activity {numberString}:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold), AutoSize = true }, 0, 0);
            Button btnDelete = new Button { Text = "Del", Tag = activity, ForeColor = Color.DimGray, BackColor = Color.FromArgb(230,230,230), FlatStyle = FlatStyle.Flat, AutoSize = true, Font = new Font(this.Font.FontFamily, 7)};
            btnDelete.FlatAppearance.BorderSize = 0; btnDelete.Click += BtnDeleteLogFrameItem_Click;
            headerTlp.Controls.Add(btnDelete, 1, 0);
            pnlItem.Controls.Add(headerTlp);

            TextBox txtDesc = new TextBox { Text = activity.ActivityDescription, Multiline = true, ScrollBars = ScrollBars.Vertical, MinimumHeight = 30, Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 8), Tag = activity, Margin = new Padding(0,3,0,0) };
            txtDesc.TextChanged += (s, ev) => { if (txtDesc.Tag is ProjectActivity act) act.ActivityDescription = txtDesc.Text; InitializeActivityPlanTab(); }; // Refresh activity plan
            pnlItem.Controls.Add(txtDesc);
            return pnlItem;
        }

        // --- Indicator Methods ---
        private Panel CreateAddIndicatorPanel(Output parentOutput, string parentNumberString)
        {
            Panel pnl = new Panel { Name = $"pnlAddIndicatorToOutput_{parentOutput.GetHashCode()}", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(10, 5, 0, 5), Margin = new Padding(0, 8, 0, 3), BackColor = Color.FromArgb(225, 235, 240) }; // Pale blue/grey
            
            TableLayoutPanel mainInputLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1};
            mainInputLayout.Controls.Add(new Label { Text = $"New Indicator for Output {parentNumberString}:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold), AutoSize = true, Margin = new Padding(0,0,0,5) }, 0, 0);

            TextBox txtName = new TextBox { Name = $"txtNewIndicatorNameForOutput_{parentOutput.GetHashCode()}", Multiline = false, Height = 25, Font = new Font(this.Font.FontFamily, 8), Dock = DockStyle.Fill, PlaceholderText = "Indicator Name/Description" };
            mainInputLayout.Controls.Add(txtName, 0, 1);
            
            TextBox txtMoV = new TextBox { Name = $"txtNewIndicatorMoVForOutput_{parentOutput.GetHashCode()}", Multiline = true, Height = 35, Font = new Font(this.Font.FontFamily, 8), ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, PlaceholderText = "Means of Verification", Margin = new Padding(0,3,0,3) };
            mainInputLayout.Controls.Add(txtMoV, 0, 2);
            
            TableLayoutPanel targetsPanel = CreateTargetsInputPanel($"targetsForNewIndicator_{parentOutput.GetHashCode()}"); // Unique name for target NUDs
            mainInputLayout.Controls.Add(targetsPanel, 0, 3);

            Button btnAdd = new Button { Name = $"btnAddIndicatorToOutput_{parentOutput.GetHashCode()}", Text = "Add Indicator", Font = new Font(this.Font.FontFamily, 8), FlatStyle = FlatStyle.System, Tag = parentOutput, Padding = new Padding(4), Margin = new Padding(0,5,0,0), Anchor = AnchorStyles.Right };
            btnAdd.Click += BtnAddIndicatorToParentOutput_Click;
            mainInputLayout.Controls.Add(btnAdd, 0, 4);
            
            pnl.Controls.Add(mainInputLayout);
            return pnl;
        }

        private void BtnAddIndicatorToParentOutput_Click(object sender, EventArgs e)
        {
            Button btnSender = sender as Button;
            Output parentOutput = btnSender?.Tag as Output;
            if (parentOutput == null) return;

            Panel addPanel = btnSender.FindForm().Controls.Find($"pnlAddIndicatorToOutput_{parentOutput.GetHashCode()}", true).FirstOrDefault() as Panel; // This is the outer panel
            TableLayoutPanel mainInputLayout = addPanel?.Controls.OfType<TableLayoutPanel>().FirstOrDefault();

            TextBox txtName = mainInputLayout?.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == $"txtNewIndicatorNameForOutput_{parentOutput.GetHashCode()}");
            TextBox txtMoV = mainInputLayout?.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == $"txtNewIndicatorMoVForOutput_{parentOutput.GetHashCode()}");
            TableLayoutPanel targetsPanel = mainInputLayout?.Controls.OfType<TableLayoutPanel>().FirstOrDefault(t => t.Name == $"targetsForNewIndicator_{parentOutput.GetHashCode()}");

            if (txtName == null || string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Indicator name empty.", "Validation Error"); txtName?.Focus(); return; }

            ProjectIndicator newIndicator = new ProjectIndicator {
                IndicatorName = txtName.Text.Trim(),
                MeansOfVerification = txtMoV?.Text.Trim(),
                OutputID = parentOutput.OutputID,
                ProjectID = _currentProject.ProjectID
            };
            if(targetsPanel != null) UpdateIndicatorTargetsFromNUDs(newIndicator, targetsPanel);
            
            parentOutput.ProjectIndicators.Add(newIndicator);
            txtName.Clear();
            txtMoV?.Clear();
            ResetTargetsNUDs(targetsPanel);


            FlowLayoutPanel flpIndicators = addPanel.Parent.Controls.OfType<FlowLayoutPanel>().FirstOrDefault(f => f.Name == $"flpIndicatorsForOutput_{parentOutput.GetHashCode()}");
            if (flpIndicators != null) {
                 string outputNumber = (addPanel.Parent.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls.OfType<Label>().FirstOrDefault()?.Text ?? "Output ?:") .Split(' ')[1].TrimEnd(':');
                flpIndicators.Controls.Add(CreateIndicatorBlock(newIndicator, $"{outputNumber}.{parentOutput.ProjectIndicators.Count}"));
                flpIndicators.PerformLayout();
            } else { RefreshLogFrameView(); }
        }
        
        private Panel CreateIndicatorBlock(ProjectIndicator indicator, string numberString)
        {
            Panel pnlItem = CreatePaddedPanel();
            pnlItem.Name = $"pnlIndicator_{indicator.GetHashCode()}";
            pnlItem.BackColor = Color.FromArgb(240, 248, 255); // Pale Cyan
            pnlItem.Padding = new Padding(5); pnlItem.Margin = new Padding(0,0,0,5);

            var headerTlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Width = pnlItem.Width - pnlItem.Padding.Horizontal };
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            headerTlp.Controls.Add(new Label { Text = $"Indicator {numberString}:", Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold), AutoSize = true }, 0, 0);
            Button btnDelete = new Button { Text = "Del", Tag = indicator, ForeColor = Color.DimGray, BackColor = Color.FromArgb(230,230,230), FlatStyle = FlatStyle.Flat, AutoSize = true, Font = new Font(this.Font.FontFamily, 7)};
            btnDelete.FlatAppearance.BorderSize = 0; btnDelete.Click += BtnDeleteLogFrameItem_Click;
            headerTlp.Controls.Add(btnDelete, 1, 0);
            pnlItem.Controls.Add(headerTlp);

            TextBox txtName = new TextBox { Text = indicator.IndicatorName, Multiline = false, Height = 25, Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 8), Tag = indicator, Margin = new Padding(0,3,0,3) };
            txtName.TextChanged += (s, ev) => { if (txtName.Tag is ProjectIndicator ind) ind.IndicatorName = txtName.Text; };
            pnlItem.Controls.Add(txtName);

            TextBox txtMoV = new TextBox { Text = indicator.MeansOfVerification, Multiline = true, ScrollBars = ScrollBars.Vertical, MinimumHeight = 35, Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle, Font = new Font(this.Font.FontFamily, 8), Tag = indicator, Margin = new Padding(0,0,0,3) };
            txtMoV.TextChanged += (s, ev) => { if (txtMoV.Tag is ProjectIndicator ind) ind.MeansOfVerification = txtMoV.Text; };
            pnlItem.Controls.Add(txtMoV);
            
            TableLayoutPanel targetsDisplayPanel = CreateTargetsInputPanel($"targetsForExistingIndicator_{indicator.GetHashCode()}", indicator); // Unique name
            pnlItem.Controls.Add(targetsDisplayPanel);

            return pnlItem;
        }

        private TableLayoutPanel CreateTargetsInputPanel(string baseName, ProjectIndicator indicatorToBind = null)
        {
            TableLayoutPanel tlpTargets = new TableLayoutPanel { Name = baseName, Dock = DockStyle.Top, AutoSize = true, ColumnCount = 5, RowCount = 2, Margin = new Padding(0, 5, 0, 0) };
            for (int i = 0; i < 5; i++) tlpTargets.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpTargets.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpTargets.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            string[] labels = { "Men:", "Women:", "Boys:", "Girls:", "Total:" };
            for (int i = 0; i < labels.Length; i++) { tlpTargets.Controls.Add(new Label { Text = labels[i], AutoSize = true, Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular), Anchor = AnchorStyles.Left }, i, 0); }
            
            NumericUpDown nudMen = new NumericUpDown { Name = $"{baseName}_Men", Width = 60, Font = new Font(this.Font.FontFamily, 8), Dock = DockStyle.Fill, Maximum = 1000000, Value = indicatorToBind?.TargetMen ?? 0 };
            NumericUpDown nudWomen = new NumericUpDown { Name = $"{baseName}_Women", Width = 60, Font = new Font(this.Font.FontFamily, 8), Dock = DockStyle.Fill, Maximum = 1000000, Value = indicatorToBind?.TargetWomen ?? 0 };
            NumericUpDown nudBoys = new NumericUpDown { Name = $"{baseName}_Boys", Width = 60, Font = new Font(this.Font.FontFamily, 8), Dock = DockStyle.Fill, Maximum = 1000000, Value = indicatorToBind?.TargetBoys ?? 0 };
            NumericUpDown nudGirls = new NumericUpDown { Name = $"{baseName}_Girls", Width = 60, Font = new Font(this.Font.FontFamily, 8), Dock = DockStyle.Fill, Maximum = 1000000, Value = indicatorToBind?.TargetGirls ?? 0 };
            NumericUpDown nudTotal = new NumericUpDown { Name = $"{baseName}_Total", Width = 60, Font = new Font(this.Font.FontFamily, 8), Dock = DockStyle.Fill, Maximum = 4000000, Value = indicatorToBind?.TargetTotal ?? 0 };

            if(indicatorToBind != null) // Bind only if an indicator is provided (for display/edit)
            {
                nudMen.ValueChanged += (s, ev) => indicatorToBind.TargetMen = (int)nudMen.Value;
                nudWomen.ValueChanged += (s, ev) => indicatorToBind.TargetWomen = (int)nudWomen.Value;
                nudBoys.ValueChanged += (s, ev) => indicatorToBind.TargetBoys = (int)nudBoys.Value;
                nudGirls.ValueChanged += (s, ev) => indicatorToBind.TargetGirls = (int)nudGirls.Value;
                nudTotal.ValueChanged += (s, ev) => indicatorToBind.TargetTotal = (int)nudTotal.Value;
            }
            tlpTargets.Controls.Add(nudMen, 0, 1); tlpTargets.Controls.Add(nudWomen, 1, 1); tlpTargets.Controls.Add(nudBoys, 2, 1); tlpTargets.Controls.Add(nudGirls, 3, 1); tlpTargets.Controls.Add(nudTotal, 4, 1);
            return tlpTargets;
        }
        
        private void UpdateIndicatorTargetsFromNUDs(ProjectIndicator indicator, TableLayoutPanel tlpTargets)
        {
            indicator.TargetMen = (int)(tlpTargets.Controls.OfType<NumericUpDown>().FirstOrDefault(n => n.Name.EndsWith("_Men"))?.Value ?? 0);
            indicator.TargetWomen = (int)(tlpTargets.Controls.OfType<NumericUpDown>().FirstOrDefault(n => n.Name.EndsWith("_Women"))?.Value ?? 0);
            indicator.TargetBoys = (int)(tlpTargets.Controls.OfType<NumericUpDown>().FirstOrDefault(n => n.Name.EndsWith("_Boys"))?.Value ?? 0);
            indicator.TargetGirls = (int)(tlpTargets.Controls.OfType<NumericUpDown>().FirstOrDefault(n => n.Name.EndsWith("_Girls"))?.Value ?? 0);
            indicator.TargetTotal = (int)(tlpTargets.Controls.OfType<NumericUpDown>().FirstOrDefault(n => n.Name.EndsWith("_Total"))?.Value ?? 0);
        }

        private void ResetTargetsNUDs(TableLayoutPanel tlpTargets)
        {
            if (tlpTargets == null) return;
            foreach(var nud in tlpTargets.Controls.OfType<NumericUpDown>()) nud.Value = 0;
        }

        // --- Deletion ---
        private void BtnDeleteLogFrameItem_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            bool fullRefresh = false; // Prefer targeted removal
            Control itemPanel = btn.Parent; // tlpHeader usually
            while(itemPanel != null && !itemPanel.Name.StartsWith("pnlOutcome_") && !itemPanel.Name.StartsWith("pnlOutput_") && !itemPanel.Name.StartsWith("pnlActivity_") && !itemPanel.Name.StartsWith("pnlIndicator_"))
            {
                itemPanel = itemPanel.Parent;
            }

            if (btn.Tag is Outcome outcomeToDelete)
            {
                if (MessageBox.Show($"Delete Outcome '{outcomeToDelete.OutcomeDescription}' and ALL its children?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _currentProject.Outcomes.Remove(outcomeToDelete);
                    fullRefresh = true; // Deleting outcome, full refresh is simplest
                }
            }
            else if (btn.Tag is Output outputToDelete)
            {
                if (MessageBox.Show($"Delete Output '{outputToDelete.OutputDescription}' and ALL its children?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Outcome parentOutcome = _currentProject.Outcomes.FirstOrDefault(o => o.Outputs.Contains(outputToDelete));
                    parentOutcome?.Outputs.Remove(outputToDelete);
                    if (itemPanel?.Parent is FlowLayoutPanel container) { container.Controls.Remove(itemPanel); container.PerformLayout(); } else { fullRefresh = true; }
                }
            }
            else if (btn.Tag is ProjectActivity activityToDelete)
            {
                 if (MessageBox.Show($"Delete Activity '{activityToDelete.ActivityDescription}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Output parentOutput = _currentProject.Outcomes.SelectMany(o => o.Outputs).FirstOrDefault(op => op.Activities.Contains(activityToDelete));
                    parentOutput?.Activities.Remove(activityToDelete);
                    if (itemPanel?.Parent is FlowLayoutPanel container) { container.Controls.Remove(itemPanel); container.PerformLayout(); InitializeActivityPlanTab(); } else { fullRefresh = true; }
                }
            }
            else if (btn.Tag is ProjectIndicator indicatorToDelete)
            {
                 if (MessageBox.Show($"Delete Indicator '{indicatorToDelete.IndicatorName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Output parentOutput = _currentProject.Outcomes.SelectMany(o => o.Outputs).FirstOrDefault(op => op.ProjectIndicators.Contains(indicatorToDelete));
                    parentOutput?.ProjectIndicators.Remove(indicatorToDelete);
                     if (itemPanel?.Parent is FlowLayoutPanel container) { container.Controls.Remove(itemPanel); container.PerformLayout(); } else { fullRefresh = true; }
                }
            }

            if (fullRefresh) RefreshLogFrameView();
        }
        #endregion

        #region Standard Form Methods (Populate, Save, Validation, etc.)
        private void SetAccessibilityProperties()
        {
            // ... (Implementation as before) ...
            txtProjectName.AccessibleName = "Project Name";
            txtProjectName.AccessibleDescription = "Enter the full official name of the project. This field is required.";
            txtProjectCode.AccessibleName = "Project Code";
            txtProjectCode.AccessibleDescription = "Enter the unique code for the project (optional).";
        }

        private async Task LoadComboBoxesAsync()
        {
            // ... (Implementation as before) ...
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
            // ... (Implementation as before) ...
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
            // ... (Implementation as before) ...
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
            if (!_isEditMode && _currentProject.ProjectID == 0) { _currentProject.CreatedAt = DateTime.UtcNow; } // Ensure CreatedAt is set only for truly new projects
            _currentProject.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            // ... (Implementation as before, added RefreshLogFrameView on new project save) ...
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
                        RefreshLogFrameView(); 
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
        #endregion

        #region Activity Plan Tab
        private void InitializeActivityPlanTab()
        {
            // ... (Implementation as before) ...
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
            // ... (Implementation as before) ...
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
        private void ProjectDatesChanged_RefreshActivityPlan(object sender, EventArgs e) { if(_formInitialLoadComplete) InitializeActivityPlanTab(); }
        #endregion

        private void ProjectCreateEditForm_Load_1(object sender, EventArgs e)
        {
            // This seems to be a redundant event handler, can be removed if not used by the designer.
        }
    }
}
