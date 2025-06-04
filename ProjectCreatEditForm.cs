using HumanitarianProjectManagement.DataAccessLayer;
using HumanitarianProjectManagement.Models;
using System;
using System.Collections.Generic; // Added for List<T>
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using HumanitarianProjectManagement.UI; // Added
using System.Globalization; // Added for CultureInfo

namespace HumanitarianProjectManagement.Forms
{
    public partial class ProjectCreateEditForm : Form
    {
        private readonly ProjectService _projectService;
        private readonly SectionService _sectionService; // Added SectionService field
        private Project _currentProject;
        private readonly bool _isEditMode;
        private int? _initialSectionId;

        // Simple class for ComboBox items
        private class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }


        // Existing constructor, modified to call the new one
        public ProjectCreateEditForm(Project projectToEdit = null)
            : this(projectToEdit, null)
        {
        }

        // New constructor accepting initialSectionId
        public ProjectCreateEditForm(Project projectToEdit = null, int? initialSectionId = null)
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToForm(this);
            _projectService = new ProjectService();
            _sectionService = new SectionService(); // Instantiate SectionService
            _logFrameService = new LogFrameService(); // Instantiate LogFrameService
            _initialSectionId = initialSectionId;

            _isEditMode = (projectToEdit != null);
            this.WindowState = FormWindowState.Maximized; // Maximize the form

            if (_isEditMode)
            {
                _currentProject = projectToEdit;
                this.Text = $"Edit Project - {_currentProject.ProjectName}";
                PopulateControls();
            }
            else
            {
                _currentProject = new Project();
                this.Text = "Add New Project";
                dtpStartDate.Value = DateTime.Now;
                dtpStartDate.Checked = false;
                dtpEndDate.Checked = false;
            }
            // LoadComboBoxes(); // Will be called from Form_Load
            SetAccessibilityProperties();
            this.Load += new System.EventHandler(this.ProjectCreateEditForm_Load); // Wire up Load event
        }

        private readonly LogFrameService _logFrameService; // Added LogFrameService field

        private async void ProjectCreateEditForm_Load(object sender, EventArgs e)
        {
            await LoadComboBoxesAsync();
            InitializeLogFrameUI(); // New method to setup LogFrame
            InitializeBudgetUITab(); // Setup Budget Tab
            InitializeActivityPlanTab(); // Setup Activity Plan Tab

            // Wire up DGV events for Activity Plan
            // Find dgvActivityPlan control dynamically. This assumes it's somewhere on the form.
            DataGridView dgvActivityPlan = this.Controls.Find("dgvActivityPlan", true).FirstOrDefault() as DataGridView;
            if (dgvActivityPlan != null)
            {
                 // Ensure event handlers are not subscribed multiple times if Load is called again (though unusual for form load)
                 dgvActivityPlan.CellValueChanged -= dgvActivityPlan_CellValueChanged;
                 dgvActivityPlan.CurrentCellDirtyStateChanged -= dgvActivityPlan_CurrentCellDirtyStateChanged;

                 dgvActivityPlan.CellValueChanged += dgvActivityPlan_CellValueChanged;
                 dgvActivityPlan.CurrentCellDirtyStateChanged += dgvActivityPlan_CurrentCellDirtyStateChanged;
            }

            // Wire up project date changes to refresh activity plan
            // Ensure dtpStartDate and dtpEndDate are the correct names from your designer
            // These controls are assumed to be direct members of the form class (this.dtpStartDate)
            if (this.dtpStartDate != null)
            {
                this.dtpStartDate.ValueChanged -= ProjectDatesChanged_RefreshActivityPlan;
                this.dtpStartDate.ValueChanged += ProjectDatesChanged_RefreshActivityPlan;
            }
            if (this.dtpEndDate != null)
            {
                this.dtpEndDate.ValueChanged -= ProjectDatesChanged_RefreshActivityPlan;
                this.dtpEndDate.ValueChanged += ProjectDatesChanged_RefreshActivityPlan;
            }
        }

        private void InitializeLogFrameUI()
        {
            // Wire up the main "Add Outcome" button from the designer
            this.btnAddOutcome.Click += new System.EventHandler(this.btnAddOutcome_Click);

            // Initially, pnlOutcome1 might be hidden or represent a placeholder
            // For this subtask, we'll assume pnlOutcome1 is for the first/active outcome
            // If editing, render all existing outcomes.
            if (_isEditMode && _currentProject.Outcomes != null && _currentProject.Outcomes.Any())
            {
                RenderAllOutcomes();
            }
            else
            {
                // For a new project, pnlLogFrameMain will be empty except for btnAddOutcome
                // Clear any design-time placeholder panels if they existed and were not part of btnAddOutcome's parent
                // For this structure, pnlLogFrameMain's direct children (apart from btnAddOutcome) will be outcome panels
                ClearOutcomePanels();
            }
        }

        private void btnAddOutcome_Click(object sender, EventArgs e)
        {
            Outcome newOutcome = new Outcome { ProjectID = _currentProject.ProjectID };
            if (_currentProject.Outcomes == null) _currentProject.Outcomes = new HashSet<Outcome>();
            _currentProject.Outcomes.Add(newOutcome);
            RenderAllOutcomes();
        }

        private void ClearOutcomePanels()
        {
            // Remove all panels that are outcome representations, leave btnAddOutcome
            var outcomePanels = pnlLogFrameMain.Controls.OfType<Panel>().ToList();
            foreach (var panel in outcomePanels)
            {
                pnlLogFrameMain.Controls.Remove(panel);
                panel.Dispose();
            }
        }

        private void RenderAllOutcomes()
        {
            ClearOutcomePanels();
            pnlLogFrameMain.SuspendLayout(); // Suspend layout for smoother update

            int outcomeCounter = 0;
            foreach (var outcome in _currentProject.Outcomes.ToList()) // ToList for safe iteration if collection modified by delete
            {
                outcomeCounter++;
                string outcomeInitialText = string.IsNullOrWhiteSpace(outcome.OutcomeDescription) ? "Enter outcome description" : outcome.OutcomeDescription;
                GroupBox grpOutcome = new GroupBox
                {
                    Text = $"Outcome {outcomeCounter}: {FirstWords(outcomeInitialText, 7)}",
                    Name = $"grpOutcome_{outcomeCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(10, 8, 10, 10), // Adjusted padding
                    Margin = new Padding(5, 5, 5, 15), // Add some bottom margin
                    Dock = DockStyle.Top // Stack them vertically
                };

                // --- Outcome Header: Description TextBox, Delete Outcome Button ---
                TableLayoutPanel tlpOutcomeHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 5, 0, 5) };
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // TextBox
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Button

                TextBox txtOutcomeDesc = new TextBox
                {
                    Text = outcome.OutcomeDescription,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    MinHeight = 40, // Use MinHeight
                    Height = 40,    // Initial height
                    Dock = DockStyle.Fill,
                    PlaceholderText = "Enter outcome description"
                };
                txtOutcomeDesc.TextChanged += (s, ev) => {
                    outcome.OutcomeDescription = ((TextBox)s).Text;
                    string currentDesc = string.IsNullOrWhiteSpace(outcome.OutcomeDescription) ? "Enter outcome description" : outcome.OutcomeDescription;
                    grpOutcome.Text = $"Outcome {outcomeCounter}: {FirstWords(currentDesc, 7)}";
                };

                Button btnDeleteOutcome = new Button { Text = "Delete Outcome", Tag = outcome, ForeColor = Color.Red, MinimumSize = new Size(120, 0), Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true };
                btnDeleteOutcome.Click += BtnDeleteOutcome_Click;

                tlpOutcomeHeader.Controls.Add(txtOutcomeDesc, 0, 0);
                tlpOutcomeHeader.Controls.Add(btnDeleteOutcome, 1, 0);
                grpOutcome.Controls.Add(tlpOutcomeHeader);

                // --- Panel to Contain Outputs for this Outcome (directly inside GroupBox) ---
                Panel pnlOutputsContainer = new Panel
                {
                    Name = $"pnlOutputsContainer_Outcome{outcomeCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Padding = new Padding(15, 5, 0, 5), // Indent outputs
                    Margin = new Padding(0, 5, 0, 5)
                };
                // This container is added to GroupBox AFTER the header and BEFORE AddOutput button

                // --- Add Output Button for this Outcome ---
                Button btnAddOutput = new Button { Text = "Add Output to this Outcome", Tag = outcome, Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
                btnAddOutput.Click += BtnAddOutputToOutcome_Click;

                // Order of adding to GroupBox's controls matters for DockStyle.Top
                grpOutcome.Controls.Add(pnlOutputsContainer); // Add container first
                RenderOutputsForOutcome(outcome, pnlOutputsContainer); // Then populate it
                grpOutcome.Controls.Add(btnAddOutput); // Then add the button below outputs

                pnlLogFrameMain.Controls.Add(grpOutcome);
                grpOutcome.BringToFront(); // Ensure this outcome group is placed correctly relative to others
            }
            pnlLogFrameMain.ResumeLayout(true); // Perform layout after all changes
        }

        // Helper function to get first few words of a string
        private string FirstWords(string text, int wordCount)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var takenWords = words.Take(wordCount);
            return string.Join(" ", takenWords) + (words.Length > wordCount ? "..." : "");
        }

        private void BtnDeleteOutcome_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Outcome outcomeToDelete)) return;

            if (MessageBox.Show($"Are you sure you want to delete this outcome and all its outputs, activities, and indicators?\n\n{outcomeToDelete.OutcomeDescription}",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _currentProject.Outcomes.Remove(outcomeToDelete);
                RenderAllOutcomes(); // Refresh the entire outcomes UI
            }
        }

        // Renamed from AddOutputToOutcome_Click (if that was specific to pnlOutcome1)
        // This is now a generic handler for any "Add Output" button on an outcome panel
        private void BtnAddOutputToOutcome_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Outcome outcome)) return;

            Output newOutput = new Output { OutcomeID = outcome.OutcomeID }; // Link it
            if (outcome.Outputs == null) outcome.Outputs = new HashSet<Output>();
            outcome.Outputs.Add(newOutput);

            // The button is now a direct child of the GroupBox (grpOutcome)
            // The pnlOutputsContainer is also a direct child of grpOutcome
            GroupBox grpOutcome = btn.Parent as GroupBox;
            Panel pnlOutputsContainer = grpOutcome?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlOutputsContainer_Outcome"));

            if (pnlOutputsContainer != null)
            {
                RenderOutputsForOutcome(outcome, pnlOutputsContainer); // Re-render outputs in their container
            }
            else
            {
                // Fallback or error logging. If the structure is correct, this shouldn't be hit.
                // If hit, it implies pnlOutputsContainer was not found in the GroupBox.
                RenderAllOutcomes(); // As a safeguard, re-render everything.
            }
        }

        private void RenderOutputsForOutcome(Outcome outcome, Panel parentOutputPanel) // parentOutputPanel is pnlOutputsContainer
        {
            parentOutputPanel.SuspendLayout();
            parentOutputPanel.Controls.Clear();
            // parentOutputPanel.Height = 0; // AutoSize should handle this if children are Dock.Top

            int outputCounter = 0;
            foreach (var outputInstance in outcome.Outputs.ToList())
            {
                outputCounter++;
                Panel pnlDynamicOutput = new Panel // This is the main panel for one output
                {
                    Name = $"pnlOutput_{outcome.OutcomeID}_{outputInstance.OutputID}_{outputCounter}", // More specific name
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10),
                    Margin = new Padding(0, 0, 0, 10), // Bottom margin to space out outputs
                    Dock = DockStyle.Top
                };

                // --- Output Header: Label, Description TextBox, Delete Output Button ---
                TableLayoutPanel tlpOutputHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, Margin = new Padding(0,0,0,5)};
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // TextBox
                tlpOutputHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Button

                Label lblOutputTitle = new Label { Text = $"Output {outputCounter}:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0,3,3,0) };
                TextBox txtOutputDesc = new TextBox
                {
                    Text = outputInstance.OutputDescription,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    MinHeight = 40,
                    Height = 40,
                    Dock = DockStyle.Fill,
                    PlaceholderText = "Enter output description"
                };
                txtOutputDesc.TextChanged += (s, ev) => outputInstance.OutputDescription = ((TextBox)s).Text;

                Button btnDeleteOutput = new Button { Text = "Delete Output", Tag = new Tuple<Outcome, Output>(outcome, outputInstance), ForeColor = Color.Red, MinimumSize = new Size(110,0), Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true};
                btnDeleteOutput.Click += BtnDeleteOutput_Click;

                tlpOutputHeader.Controls.Add(lblOutputTitle, 0, 0);
                tlpOutputHeader.Controls.Add(txtOutputDesc, 1, 0);
                tlpOutputHeader.Controls.Add(btnDeleteOutput, 2, 0);
                pnlDynamicOutput.Controls.Add(tlpOutputHeader);

                // --- Buttons Panel for Add Indicator/Activity for this Output ---
                FlowLayoutPanel flpOutputActions = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0,0,0,5)};
                Button btnAddIndicator = new Button { Text = "Add Indicator", Tag = outputInstance, AutoSize=true };
                btnAddIndicator.Click += BtnAddIndicator_Click;
                Button btnAddActivity = new Button { Text = "Add Activity", Tag = outputInstance, AutoSize=true, Margin = new Padding(5,0,0,0) };
                btnAddActivity.Click += BtnAddActivity_Click;

                flpOutputActions.Controls.Add(btnAddIndicator);
                flpOutputActions.Controls.Add(btnAddActivity);
                pnlDynamicOutput.Controls.Add(flpOutputActions);

                // --- Panel to Contain Indicators for this Output ---
                Panel pnlIndicatorsContainer = new Panel
                {
                    Name = $"pnlIndicatorsContainer_Output{outputInstance.OutputID}_{outputCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Padding = new Padding(20, 5, 0, 5), // Indent indicators more
                    Margin = new Padding(0,0,0,5)
                };
                pnlDynamicOutput.Controls.Add(pnlIndicatorsContainer);

                // --- Panel to Contain Activities for this Output ---
                Panel pnlActivitiesContainer = new Panel
                {
                    Name = $"pnlActivitiesContainer_Output{outputInstance.OutputID}_{outputCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Padding = new Padding(20, 5, 0, 5) // Indent activities more
                };
                pnlDynamicOutput.Controls.Add(pnlActivitiesContainer);

                parentOutputPanel.Controls.Add(pnlDynamicOutput);
                pnlDynamicOutput.BringToFront();

                RenderIndicatorsForOutput(outputInstance, pnlIndicatorsContainer);
                RenderActivitiesForOutput(outputInstance, pnlActivitiesContainer);
            }
            parentOutputPanel.ResumeLayout(true);
        }

        private void BtnDeleteOutput_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Tuple<Outcome, Output> data)) return;

            Outcome parentOutcome = data.Item1;
            Output outputToDelete = data.Item2;

            if (parentOutcome == null || outputToDelete == null)
            {
                MessageBox.Show("Error identifying the item to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete this output and all its activities and indicators?\n\nOutput: {outputToDelete.OutputDescription}",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                parentOutcome.Outputs.Remove(outputToDelete);
                // Re-render the outputs for the specific parent outcome.
                // To do this, we need to find the container panel for this outcome's outputs.
                // The GroupBox for the outcome can be found by iterating up from btn.Parent until a GroupBox is found.
                // Then, find the pnlOutputsContainer within that GroupBox.

                Control currentControl = btn;
                GroupBox outcomeGrp = null;
                while (currentControl.Parent != null)
                {
                    if (currentControl.Parent is GroupBox && currentControl.Parent.Name.StartsWith("grpOutcome_"))
                    {
                        outcomeGrp = currentControl.Parent as GroupBox;
                        break;
                    }
                    currentControl = currentControl.Parent;
                }

                if (outcomeGrp != null)
                {
                    Panel pnlOutputsContainer = outcomeGrp.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlOutputsContainer_Outcome"));
                    if (pnlOutputsContainer != null)
                    {
                        RenderOutputsForOutcome(parentOutcome, pnlOutputsContainer);
                    }
                    else
                    {
                        RenderAllOutcomes(); // Fallback if specific container not found
                    }
                }
                else
                {
                    RenderAllOutcomes(); // Fallback if GroupBox not found
                }
            }
        }

        private void BtnAddIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button; // This button is in flpOutputActions
            if (btn == null || !(btn.Tag is Output output)) return;

            ProjectIndicator newIndicator = new ProjectIndicator { ProjectID = _currentProject.ProjectID, OutputID = output.OutputID };
            if (output.ProjectIndicators == null) output.ProjectIndicators = new HashSet<ProjectIndicator>();
            output.ProjectIndicators.Add(newIndicator);

            // btn -> flpOutputActions -> pnlDynamicOutput
            Panel pnlDynamicOutput = btn.Parent.Parent as Panel;
            Panel pnlIndicatorsContainer = pnlDynamicOutput?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name != null && p.Name.StartsWith("pnlIndicatorsContainer_Output"));
            if (pnlIndicatorsContainer != null)
            {
                RenderIndicatorsForOutput(output, pnlIndicatorsContainer);
            }
            else
            {
                RenderAllOutcomes();
            }
        }

        private void RenderIndicatorsForOutput(Output output, Panel parentIndicatorPanel) // parentIndicatorPanel is pnlIndicatorsContainer
        {
            parentIndicatorPanel.SuspendLayout();
            parentIndicatorPanel.Controls.Clear();
            // parentIndicatorPanel.Height = 0; // AutoSize handles this

            int indicatorCounter = 0;
            foreach (var indicatorInstance in output.ProjectIndicators.ToList())
            {
                indicatorCounter++;
                Panel pnlDynamicIndicator = new Panel
                {
                    Name = $"pnlIndicator_{output.OutputID}_{indicatorInstance.ProjectIndicatorID}_{indicatorCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(8), // Increased padding
                    Margin = new Padding(0, 0, 0, 8), // Bottom margin for spacing
                    Dock = DockStyle.Top
                };

                TableLayoutPanel tlpIndicatorMain = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
                tlpIndicatorMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F)); // Labels like "Indicator:"
                tlpIndicatorMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Controls like TextBox, FlowLayoutPanel
                tlpIndicatorMain.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Delete button

                // Row 0: Indicator Description
                Label lblIndicatorName = new Label { Text = "Indicator:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 3, 0, 0) };
                TextBox txtIndicatorName = new TextBox
                {
                    Text = indicatorInstance.IndicatorName,
                    Dock = DockStyle.Fill,
                    PlaceholderText = "Enter indicator description",
                    Multiline = true,
                    MinHeight = 40,
                    Height = 40,
                    ScrollBars = ScrollBars.Vertical
                };
                txtIndicatorName.TextChanged += (s, ev) => indicatorInstance.IndicatorName = ((TextBox)s).Text;

                Button btnDeleteIndicator = new Button { Text = "Delete", Tag = new Tuple<Output, ProjectIndicator>(output, indicatorInstance), ForeColor = Color.Red, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, MinimumSize = new Size(75,0) };
                btnDeleteIndicator.Click += BtnDeleteIndicator_Click;

                tlpIndicatorMain.Controls.Add(lblIndicatorName, 0, 0);
                tlpIndicatorMain.Controls.Add(txtIndicatorName, 1, 0);
                tlpIndicatorMain.Controls.Add(btnDeleteIndicator, 2, 0);

                // Row 1: Targets Label
                Label lblTargets = new Label { Text = "Targets:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 8, 0, 0) }; // Added top margin
                tlpIndicatorMain.Controls.Add(lblTargets, 0, 1);

                // Row 1 (continued): Targets FlowLayoutPanel
                FlowLayoutPanel flpTargets = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    WrapContents = true, // Allow wrapping of target groups
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 5, 0, 0) // Top margin for flpTargets
                };
                tlpIndicatorMain.Controls.Add(flpTargets, 1, 1);
                tlpIndicatorMain.SetColumnSpan(flpTargets, 2); // Span across control and button column

                Action<string, NumericUpDown, Action<int>> addTargetControl = (labelText, nud, setter) => {
                    FlowLayoutPanel flpSingleTargetGroup = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 0, 10, 3) }; // Right/bottom margin for spacing between groups
                    flpSingleTargetGroup.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 3, 3, 0) });
                    nud.Maximum = 1000000; nud.Width = 70; nud.Anchor = AnchorStyles.Left;
                    nud.ValueChanged += (s, ev) => setter((int)((NumericUpDown)s).Value);
                    flpSingleTargetGroup.Controls.Add(nud);
                    flpTargets.Controls.Add(flpSingleTargetGroup);
                };

                addTargetControl("Men:", new NumericUpDown { Value = indicatorInstance.TargetMen }, val => indicatorInstance.TargetMen = val);
                addTargetControl("Women:", new NumericUpDown { Value = indicatorInstance.TargetWomen }, val => indicatorInstance.TargetWomen = val);
                addTargetControl("Boys:", new NumericUpDown { Value = indicatorInstance.TargetBoys }, val => indicatorInstance.TargetBoys = val);
                addTargetControl("Girls:", new NumericUpDown { Value = indicatorInstance.TargetGirls }, val => indicatorInstance.TargetGirls = val);

                FlowLayoutPanel flpTotalTargetGroup = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0,0,10,3) };
                flpTotalTargetGroup.Controls.Add(new Label { Text = "Total:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0,3,3,0) });
                NumericUpDown nudTargetTotal = new NumericUpDown { Value = indicatorInstance.TargetTotal, Maximum = 4000000, Width = 75, Enabled = true, Anchor = AnchorStyles.Left };
                nudTargetTotal.ValueChanged += (s, ev) => indicatorInstance.TargetTotal = (int)((NumericUpDown)s).Value;
                flpTotalTargetGroup.Controls.Add(nudTargetTotal);
                flpTargets.Controls.Add(flpTotalTargetGroup);

                pnlDynamicIndicator.Controls.Add(tlpIndicatorMain);
                parentIndicatorPanel.Controls.Add(pnlDynamicIndicator);
                pnlDynamicIndicator.BringToFront();
            }
            parentIndicatorPanel.ResumeLayout(true);
        }

        private void BtnDeleteIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Tuple<Output, ProjectIndicator> data)) return;

            Output parentOutput = data.Item1;
            ProjectIndicator indicatorToDelete = data.Item2;

            if (MessageBox.Show($"Are you sure you want to delete this indicator?\n\nIndicator: {indicatorToDelete.IndicatorName}",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                parentOutput.ProjectIndicators.Remove(indicatorToDelete);

                Control current = btn;
                Panel indicatorsContainer = null;
                while(current.Parent != null)
                {
                    // Traverse up to find the specific pnlIndicatorsContainer for this output
                    if(current.Parent is Panel && current.Parent.Name != null && current.Parent.Name.StartsWith("pnlIndicatorsContainer_Output"))
                    {
                        indicatorsContainer = current.Parent as Panel;
                        break;
                    }
                    current = current.Parent;
                }

                if (indicatorsContainer != null)
                {
                    RenderIndicatorsForOutput(parentOutput, indicatorsContainer);
                }
                else
                {
                     RenderAllOutcomes(); // Fallback if specific container not found
                }
            }
        }

        private void BtnAddActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button; // This button is in flpOutputActions
            if (btn == null || !(btn.Tag is Output output)) return;

            Activity newActivity = new Activity() { OutputID = output.OutputID }; // Ensure OutputID is linked if applicable
            if (output.Activities == null) output.Activities = new HashSet<Activity>();
            output.Activities.Add(newActivity);

            // btn -> flpOutputActions -> pnlDynamicOutput
            Panel pnlDynamicOutput = btn.Parent.Parent as Panel;
            Panel pnlActivitiesContainer = pnlDynamicOutput?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name != null && p.Name.StartsWith("pnlActivitiesContainer_Output"));
            if (pnlActivitiesContainer != null)
            {
                RenderActivitiesForOutput(output, pnlActivitiesContainer);
            }
            else
            {
                RenderAllOutcomes(); // Fallback
            }
            InitializeActivityPlanTab(); // Refresh activity plan
        }

        private void RenderActivitiesForOutput(Output output, Panel parentActivityPanel) // parentActivityPanel is pnlActivitiesContainer
        {
            parentActivityPanel.SuspendLayout();
            parentActivityPanel.Controls.Clear();
            // parentActivityPanel.Height = 0; // AutoSize handles this

            int activityCounter = 0;
            foreach (var activityInstance in output.Activities.ToList())
            {
                activityCounter++;
                Panel pnlDynamicActivity = new Panel
                {
                    Name = $"pnlActivity_{output.OutputID}_{activityInstance.ActivityID}_{activityCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(8), // Consistent padding
                    Margin = new Padding(0, 0, 0, 8), // Bottom margin for spacing
                    Dock = DockStyle.Top
                };

                TableLayoutPanel tlpActivityMain = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
                tlpActivityMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F)); // Label "Activity:"
                tlpActivityMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // TextBox
                tlpActivityMain.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Delete Button

                Label lblActivityDesc = new Label { Text = "Activity:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 3, 0, 0) };
                TextBox txtActivityDesc = new TextBox
                {
                    Text = activityInstance.ActivityDescription,
                    Dock = DockStyle.Fill,
                    PlaceholderText = "Enter activity description",
                    Multiline = true,
                    MinHeight = 40,
                    Height = 40,
                    ScrollBars = ScrollBars.Vertical
                };
                txtActivityDesc.TextChanged += (s, ev) => {
                    activityInstance.ActivityDescription = ((TextBox)s).Text;
                    InitializeActivityPlanTab(); // Refresh activity plan if description changes
                };

                Button btnDeleteActivity = new Button { Text = "Delete", Tag = new Tuple<Output, Activity>(output, activityInstance), ForeColor = Color.Red, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, MinimumSize = new Size(75,0) };
                btnDeleteActivity.Click += BtnDeleteActivity_Click;

                tlpActivityMain.Controls.Add(lblActivityDesc, 0, 0);
                tlpActivityMain.Controls.Add(txtActivityDesc, 1, 0);
                tlpActivityMain.Controls.Add(btnDeleteActivity, 2, 0);

                pnlDynamicActivity.Controls.Add(tlpActivityMain);
                parentActivityPanel.Controls.Add(pnlDynamicActivity);
                pnlDynamicActivity.BringToFront();
            }
            parentActivityPanel.ResumeLayout(true);
        }

        private void BtnDeleteActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Tuple<Output, Activity> data)) return;

            Output parentOutput = data.Item1;
            Activity activityToDelete = data.Item2;

            if (MessageBox.Show($"Are you sure you want to delete this activity?\n\nActivity: {activityToDelete.ActivityDescription}",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                parentOutput.Activities.Remove(activityToDelete);

                Control current = btn;
                Panel activitiesContainer = null;
                while (current.Parent != null)
                {
                    if (current.Parent is Panel && current.Parent.Name != null && current.Parent.Name.StartsWith("pnlActivitiesContainer_Output"))
                    {
                        activitiesContainer = current.Parent as Panel;
                        break;
                    }
                    current = current.Parent;
                }

                if (activitiesContainer != null)
                {
                    RenderActivitiesForOutput(parentOutput, activitiesContainer);
                }
                else
                {
                    RenderAllOutcomes(); // Fallback
                }
                InitializeActivityPlanTab(); // Refresh activity plan
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


        private async Task LoadComboBoxesAsync() // Changed to async Task
        {
            // Sections
            cmbSection.DisplayMember = "Text";
            cmbSection.ValueMember = "Value";
            cmbSection.Items.Clear(); // Clear existing items
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
                // Keep "(No Section)" as the only option if loading fails
            }

            // Set Selected Section
            if (_isEditMode && _currentProject.SectionID != null)
            {
                foreach (ComboboxItem item in cmbSection.Items)
                {
                    if (item.Value == _currentProject.SectionID.Value)
                    {
                        cmbSection.SelectedItem = item;
                        break;
                    }
                }
                if (cmbSection.SelectedItem == null) cmbSection.SelectedIndex = 0; // Default if not found
            }
            else if (!_isEditMode && _initialSectionId.HasValue)
            {
                bool found = false;
                foreach (ComboboxItem item in cmbSection.Items)
                {
                    if (item.Value == _initialSectionId.Value)
                    {
                        cmbSection.SelectedItem = item;
                        found = true;
                        break;
                    }
                }
                if (!found) cmbSection.SelectedIndex = 0;
            }
            else
            {
                cmbSection.SelectedIndex = 0;
            }


            // Managers (Users)
            cmbManager.DisplayMember = "Text";
            cmbManager.ValueMember = "Value";
            cmbManager.Items.Add(new ComboboxItem { Text = "(No Manager)", Value = 0 }); // For null ManagerUserID
            cmbManager.Items.Add(new ComboboxItem { Text = "Default Manager 1 (User ID 1)", Value = 1 }); // Assuming UserID 1 exists
            cmbManager.Items.Add(new ComboboxItem { Text = "Default Manager 2 (User ID 2)", Value = 2 }); // Assuming UserID 2 exists
                                                                                                          // Select current project's manager or default to (No Manager)
            if (_isEditMode && _currentProject.ManagerUserID != null)
            {
                foreach (ComboboxItem item in cmbManager.Items)
                {
                    if (item.Value == _currentProject.ManagerUserID.Value)
                    {
                        cmbManager.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                cmbManager.SelectedIndex = 0; // Default to (No Manager)
            }
        }

        private void PopulateControls()
        {
            if (_currentProject == null) return;

            txtProjectName.Text = _currentProject.ProjectName;
            txtProjectCode.Text = _currentProject.ProjectCode;

            if (_currentProject.StartDate.HasValue)
            {
                dtpStartDate.Value = _currentProject.StartDate.Value;
                dtpStartDate.Checked = true;
            }
            else
            {
                dtpStartDate.Value = DateTime.Now; // Default value if null, but keep unchecked
                dtpStartDate.Checked = false;
            }

            if (_currentProject.EndDate.HasValue)
            {
                dtpEndDate.Value = _currentProject.EndDate.Value;
                dtpEndDate.Checked = true;
            }
            else
            {
                dtpEndDate.Value = DateTime.Now; // Default value if null, but keep unchecked
                dtpEndDate.Checked = false;
            }

            txtLocation.Text = _currentProject.Location;
            txtOverallObjective.Text = _currentProject.OverallObjective;
            txtStatus.Text = _currentProject.Status;
            txtDonor.Text = _currentProject.Donor;
            nudTotalBudget.Value = _currentProject.TotalBudget ?? 0;

            // ComboBoxes are handled in LoadComboBoxes after _currentProject is set
            if (_isEditMode)
            {
                InitializeBudgetUITab(); // Load existing budget lines
                InitializeActivityPlanTab(); // Load activity plan data
            }
        }

        private bool CollectAndValidateData()
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Project Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtProjectName.Focus();
                return false;
            }

            _currentProject.ProjectName = txtProjectName.Text.Trim();
            _currentProject.ProjectCode = string.IsNullOrWhiteSpace(txtProjectCode.Text) ? null : txtProjectCode.Text.Trim();

            _currentProject.StartDate = dtpStartDate.Checked ? (DateTime?)dtpStartDate.Value : null;
            _currentProject.EndDate = dtpEndDate.Checked ? (DateTime?)dtpEndDate.Value : null;

            if (_currentProject.StartDate.HasValue && _currentProject.EndDate.HasValue && _currentProject.EndDate.Value < _currentProject.StartDate.Value)
            {
                MessageBox.Show("End Date cannot be earlier than Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtpEndDate.Focus();
                return false;
            }

            _currentProject.Location = string.IsNullOrWhiteSpace(txtLocation.Text) ? null : txtLocation.Text.Trim();
            _currentProject.OverallObjective = string.IsNullOrWhiteSpace(txtOverallObjective.Text) ? null : txtOverallObjective.Text.Trim();
            _currentProject.Status = string.IsNullOrWhiteSpace(txtStatus.Text) ? null : txtStatus.Text.Trim();
            _currentProject.Donor = string.IsNullOrWhiteSpace(txtDonor.Text) ? null : txtDonor.Text.Trim();
            _currentProject.TotalBudget = nudTotalBudget.Value;

            // Handle ComboBox selections
            if (cmbSection.SelectedItem != null && ((ComboboxItem)cmbSection.SelectedItem).Value != 0)
            {
                _currentProject.SectionID = ((ComboboxItem)cmbSection.SelectedItem).Value;
            }
            // Removed the else-if block that directly assigned _initialSectionId to _currentProject.SectionID
            // The pre-selection in LoadComboBoxesAsync and this block should suffice.
            else
            {
                _currentProject.SectionID = null;
            }

            if (cmbManager.SelectedItem != null && ((ComboboxItem)cmbManager.SelectedItem).Value != 0)
            {
                _currentProject.ManagerUserID = ((ComboboxItem)cmbManager.SelectedItem).Value;
            }
            else
            {
                _currentProject.ManagerUserID = null;
            }

            if (!_isEditMode) // For new projects
            {
                _currentProject.CreatedAt = DateTime.UtcNow;
            }
            _currentProject.UpdatedAt = DateTime.UtcNow; // Always update this field

            return true;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!CollectAndValidateData())
            {
                return;
            }

            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            // Consider disabling other input controls as well
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
                    MessageBox.Show("Failed to save project. Check logs for details.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the project: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
                btnCancel.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Budget Tab Implementation Start

        private string GetCategoryDisplayName(BudgetCategoriesEnum category)
        {
            string name = category.ToString();
            // Example: A_StaffAndPersonnel -> "A. Staff And Personnel"
            if (name.Length > 2 && name[1] == '_')
            {
                name = name[0] + ". " + name.Substring(2);
            }
            // Add spaces before capital letters (simple version)
            return System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();
        }

        void ClearControlsFromRow(TableLayoutPanel panel, int rowIndex)
        {
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                Control control = panel.GetControlFromPosition(i, rowIndex);
                if (control != null)
                {
                    // TODO: Unsubscribe events if necessary before disposing if they cause issues.
                    // For this specific set of controls, direct removal and dispose is usually fine.
                    panel.Controls.Remove(control);
                    control.Dispose();
                }
            }
        }

        private void InitializeBudgetUITab()
        {
            if (flpBudgetCategories == null) return; // Designer control might not be ready in some call paths initially.
                                                  // This method might be called multiple times (Load, PopulateControls), ensure idempotency.

            // Check if already initialized to prevent duplicate controls and event subscriptions
            if (flpBudgetCategories.Controls.OfType<GroupBox>().Any()) {
                 // If re-populating for existing data, just re-render lines, not structure.
                 // However, the current design rebuilds GroupBoxes too.
                 // For simplicity, clear and rebuild. More advanced would be to update existing.
                foreach (GroupBox gb in flpBudgetCategories.Controls.OfType<GroupBox>().ToList())
                {
                    TableLayoutPanel tlp = gb.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                    if (tlp != null)
                    {
                         BudgetCategoriesEnum catEnum = (BudgetCategoriesEnum)gb.Tag;
                         RenderBudgetLinesForCategory(catEnum, tlp);
                    }
                }
                return; // Already initialized the main structure
            }

            flpBudgetCategories.Controls.Clear(); // Clear previous if any (e.g. design-time placeholders)
            flpBudgetCategories.SuspendLayout();

            foreach (BudgetCategoriesEnum catEnum in Enum.GetValues(typeof(BudgetCategoriesEnum)))
            {
                GroupBox gbCategory = new GroupBox
                {
                    Text = GetCategoryDisplayName(catEnum),
                    Tag = catEnum,
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(10),
                    Margin = new Padding(0, 0, 0, 10),
                    // Set width relative to parent FlowLayoutPanel, accounting for scrollbar and margins
                    Width = flpBudgetCategories.ClientSize.Width - (flpBudgetCategories.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0) - flpBudgetCategories.Padding.Horizontal - 10
                };

                TableLayoutPanel tlpLines = new TableLayoutPanel
                {
                    ColumnCount = 8, // Description, Unit, Qty, UnitCost, Duration, %CBPF, Total, Action
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                    Padding = new Padding(5),
                    Margin = new Padding(0,5,0,5),
                    Width = gbCategory.ClientSize.Width - gbCategory.Padding.Horizontal
                };

                // Define ColumnStyles
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Description
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));  // Unit
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));  // Qty
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // UnitCost
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));  // Duration
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // % CBPF
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F)); // Total Cost
                tlpLines.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));    // Action Button

                // Add Header Row
                string[] headers = { "Description", "Unit", "Qty", "Unit Cost", "Duration", "% CBPF", "Total Cost", "Action" };
                for (int i = 0; i < headers.Length; i++)
                {
                    Label lblHeader = new Label { Text = headers[i], Font = new Font(this.Font, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, AutoSize = true};
                    tlpLines.Controls.Add(lblHeader, i, 0);
                }
                tlpLines.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header row

                gbCategory.Controls.Add(tlpLines);

                Button btnAddLine = new Button
                {
                    Text = "Add New Budget Line",
                    Tag = catEnum,
                    Dock = DockStyle.Top, // Docks below the table inside the groupbox
                    AutoSize = true,
                    Margin = new Padding(0,5,0,0)
                };
                btnAddLine.Click += BtnAddBudgetLine_Click;
                gbCategory.Controls.Add(btnAddLine);
                tlpLines.BringToFront(); // Ensure table is before button if both Dock.Top

                flpBudgetCategories.Controls.Add(gbCategory);
                RenderBudgetLinesForCategory(catEnum, tlpLines); // Populate with existing lines if any
            }
            flpBudgetCategories.ResumeLayout(true);
        }

        private void RenderBudgetLinesForCategory(BudgetCategoriesEnum category, TableLayoutPanel uiTable)
        {
            uiTable.SuspendLayout();
            // Clear existing data rows (keep header row 0)
            while (uiTable.RowCount > 1)
            {
                ClearControlsFromRow(uiTable, uiTable.RowCount - 1);
                uiTable.RowStyles.RemoveAt(uiTable.RowCount -1); // Remove RowStyle too
                uiTable.RowCount--;
            }

            if (_currentProject.DetailedBudgetLines == null) _currentProject.DetailedBudgetLines = new HashSet<DetailedBudgetLine>();
            var linesForCategory = _currentProject.DetailedBudgetLines.Where(b => b.Category == category).ToList();

            if (linesForCategory != null)
            {
                foreach (var line in linesForCategory)
                {
                    int currentRow = uiTable.RowCount;
                    uiTable.RowCount++; // Increment row count
                    uiTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Add a style for the new row

                    TextBox txtDesc = new TextBox { Text = line.Description, Dock = DockStyle.Fill, MaxLength = 1500, Multiline = true, MinimumSize = new Size(150, 40), ScrollBars = ScrollBars.Vertical, Height=40 };
                    txtDesc.TextChanged += (s, e) => line.Description = ((TextBox)s).Text;

                    TextBox txtUnit = new TextBox { Text = line.Unit, Dock = DockStyle.Fill, MaxLength = 50 };
                    txtUnit.TextChanged += (s, e) => line.Unit = ((TextBox)s).Text;

                    NumericUpDown nudQty = new NumericUpDown { Value = line.Quantity, Maximum = 9999999, DecimalPlaces = 3, Dock = DockStyle.Fill, Width = 70 };
                    NumericUpDown nudUnitCost = new NumericUpDown { Value = line.UnitCost, Maximum = 99999999, DecimalPlaces = 2, Dock = DockStyle.Fill, Width = 80 };
                    NumericUpDown nudDuration = new NumericUpDown { Value = line.Duration, Maximum = 9999, DecimalPlaces = 2, Dock = DockStyle.Fill, Width = 70 };
                    NumericUpDown nudPrcCharged = new NumericUpDown { Value = line.PercentageChargedToCBPF, Maximum = 100, Minimum = 0, DecimalPlaces = 2, Dock = DockStyle.Fill, Width = 70 };

                    Label lblTotal = new Label { Text = line.TotalCost.ToString("C", CultureInfo.InvariantCulture), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, AutoSize = true };

                    Button btnDeleteLine = new Button { Text = "Delete", Tag = line, AutoSize = true, ForeColor = Color.Red };
                    btnDeleteLine.Click += (s, e) =>
                    {
                        _currentProject.DetailedBudgetLines.Remove(line);
                        RenderBudgetLinesForCategory(category, uiTable);
                    };

                    EventHandler recalc = (s, e) =>
                    {
                        line.Quantity = nudQty.Value;
                        line.UnitCost = nudUnitCost.Value;
                        line.Duration = nudDuration.Value;
                        line.PercentageChargedToCBPF = nudPrcCharged.Value;
                        line.TotalCost = line.Quantity * line.UnitCost * line.Duration * (line.PercentageChargedToCBPF / 100M);
                        lblTotal.Text = line.TotalCost.ToString("C", CultureInfo.InvariantCulture);
                    };

                    nudQty.ValueChanged += recalc;
                    nudUnitCost.ValueChanged += recalc;
                    nudDuration.ValueChanged += recalc;
                    nudPrcCharged.ValueChanged += recalc;

                    uiTable.Controls.Add(txtDesc, 0, currentRow);
                    uiTable.Controls.Add(txtUnit, 1, currentRow);
                    uiTable.Controls.Add(nudQty, 2, currentRow);
                    uiTable.Controls.Add(nudUnitCost, 3, currentRow);
                    uiTable.Controls.Add(nudDuration, 4, currentRow);
                    uiTable.Controls.Add(nudPrcCharged, 5, currentRow);
                    uiTable.Controls.Add(lblTotal, 6, currentRow);
                    uiTable.Controls.Add(btnDeleteLine, 7, currentRow);
                }
            }
            uiTable.ResumeLayout(true);
        }

        private void BtnAddBudgetLine_Click(object sender, EventArgs e)
        {
            var category = (BudgetCategoriesEnum)((Button)sender).Tag;
            var newLine = new DetailedBudgetLine
            {
                ProjectId = _currentProject.ProjectID,
                Category = category,
                Quantity = 1,
                Duration = 1,
                UnitCost = 0,
                PercentageChargedToCBPF = 100
            };
            newLine.TotalCost = newLine.Quantity * newLine.UnitCost * newLine.Duration * (newLine.PercentageChargedToCBPF / 100M);

            if (_currentProject.DetailedBudgetLines == null) _currentProject.DetailedBudgetLines = new HashSet<DetailedBudgetLine>();
            _currentProject.DetailedBudgetLines.Add(newLine);

            // Find the TableLayoutPanel associated with this category
            GroupBox gbParent = (GroupBox)((Button)sender).Parent;
            TableLayoutPanel tlp = gbParent.Controls.OfType<TableLayoutPanel>().First();
            RenderBudgetLinesForCategory(category, tlp);
        }

        // Budget Tab Implementation End

        // Activity Plan Tab Implementation Start

        private void InitializeActivityPlanTab()
        {
            // Ensure dgvActivityPlan is referenced from this.Controls or the specific TabPage
            DataGridView dgvActivityPlan = this.Controls.Find("dgvActivityPlan", true).FirstOrDefault() as DataGridView;

            if (dgvActivityPlan == null || _currentProject == null)
            {
                return;
            }

            dgvActivityPlan.SuspendLayout();
            dgvActivityPlan.Columns.Clear(); // Clear existing columns first
            dgvActivityPlan.Rows.Clear();    // Clear existing rows

            // Setup Columns
            dgvActivityPlan.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActivityDescription",
                HeaderText = "Activity",
                ReadOnly = true,
                Frozen = true,
                FillWeight = 30
            });

            DateTime startDate = _currentProject.StartDate ?? (_currentProject.CreatedAt != DateTime.MinValue ? _currentProject.CreatedAt : DateTime.Now);
            DateTime endDate = _currentProject.EndDate ?? startDate.AddYears(1);

            if (endDate < startDate) endDate = startDate.AddYears(1);

            DateTime currentMonthIterator = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime endMonthTarget = new DateTime(endDate.Year, endDate.Month, 1);

            while (currentMonthIterator <= endMonthTarget)
            {
                string monthYearKeyStorage = currentMonthIterator.ToString("MMM/yyyy", CultureInfo.InvariantCulture); // For storage "Jan/2023"
                string headerTextDisplay = currentMonthIterator.ToString("MMM/yy", CultureInfo.InvariantCulture);   // For display "Jan/23"

                // Use a more robust name that doesn't rely on special characters like '/'
                string columnName = $"Month_{currentMonthIterator.Year}_{currentMonthIterator.Month:00}";

                dgvActivityPlan.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    Name = columnName,
                    HeaderText = headerTextDisplay, // Display "Jan/23"
                    Tag = monthYearKeyStorage,      // Store "Jan/2023" in Tag for reliable parsing
                    Width = 70
                });
                currentMonthIterator = currentMonthIterator.AddMonths(1);
            }

            // Populate Rows
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
                            int rowIndex = dgvActivityPlan.Rows.Add();
                            DataGridViewRow row = dgvActivityPlan.Rows[rowIndex];
                            row.Cells["ActivityDescription"].Value = activity.ActivityDescription;
                            row.Tag = activity; // Store activity object

                            List<string> plannedMonthsList = string.IsNullOrEmpty(activity.PlannedMonths)
                                ? new List<string>()
                                : activity.PlannedMonths.Split(',').ToList();

                            foreach (DataGridViewColumn col in dgvActivityPlan.Columns)
                            {
                                if (col is DataGridViewCheckBoxColumn && col.Name.StartsWith("Month_"))
                                {
                                    string colStorageKey = col.Tag as string; // "Jan/2023"
                                    if (colStorageKey != null && plannedMonthsList.Contains(colStorageKey))
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
                    }
                }
            }
            dgvActivityPlan.ResumeLayout();
        }

        private void dgvActivityPlan_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            DataGridView dgvActivityPlan = sender as DataGridView;
            if (dgvActivityPlan != null && dgvActivityPlan.IsCurrentCellDirty && dgvActivityPlan.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvActivityPlan.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvActivityPlan_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgvActivityPlan = sender as DataGridView;
            if (dgvActivityPlan == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataGridViewRow row = dgvActivityPlan.Rows[e.RowIndex];
            Activity activity = row.Tag as Activity;
            if (activity == null) return;

            DataGridViewColumn col = dgvActivityPlan.Columns[e.ColumnIndex];
            if (col is DataGridViewCheckBoxColumn && col.Name.StartsWith("Month_"))
            {
                bool isChecked = (bool)(row.Cells[e.ColumnIndex].Value ?? false);
                string monthYearKey = col.Tag as string; // "Jan/2023" from Tag

                if (monthYearKey == null) return; // Should not happen if Tag is set during column creation

                List<string> plannedMonthsList = string.IsNullOrEmpty(activity.PlannedMonths)
                    ? new List<string>()
                    : activity.PlannedMonths.Split(',').ToList();

                if (isChecked)
                {
                    if (!plannedMonthsList.Contains(monthYearKey))
                    {
                        plannedMonthsList.Add(monthYearKey);
                    }
                }
                else
                {
                    plannedMonthsList.Remove(monthYearKey);
                }
                activity.PlannedMonths = string.Join(",", plannedMonthsList);
            }
        }

        private void ProjectDatesChanged_RefreshActivityPlan(object sender, EventArgs e)
        {
            InitializeActivityPlanTab();
        }

        // Activity Plan Tab Implementation End


        private void SetAccessibilityProperties()
