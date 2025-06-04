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

namespace HumanitarianProjectManagement.Forms
{
    partial class ProjectCreateEditForm : System.Windows.Forms.Form
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
                Panel pnlDynamicOutcome = new Panel
                {
                    Name = $"pnlOutcome_{outcomeCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(5),
                    Margin = new Padding(3, 3, 3, 10), // Add some bottom margin
                    Dock = DockStyle.Top // Stack them vertically
                };

                // --- Outcome Header: Label, Description TextBox, Delete Outcome Button ---
                TableLayoutPanel tlpOutcomeHeader = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3};
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // TextBox
                tlpOutcomeHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Button

                Label lblOutcomeTitle = new Label { Text = $"Outcome {outcomeCounter}:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0,6,0,0) };
                TextBox txtOutcomeDesc = new TextBox
                {
                    Text = outcome.OutcomeDescription,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Height = 40,
                    Dock = DockStyle.Fill,
                    // PlaceholderText = "Enter outcome description"
                };
                txtOutcomeDesc.TextChanged += (s, ev) => outcome.OutcomeDescription = ((TextBox)s).Text;

                Button btnDeleteOutcome = new Button { Text = "Delete Outcome", Tag = outcome, ForeColor = Color.Red, Width = 100, Anchor = AnchorStyles.Right };
                btnDeleteOutcome.Click += BtnDeleteOutcome_Click;

                tlpOutcomeHeader.Controls.Add(lblOutcomeTitle, 0, 0);
                tlpOutcomeHeader.Controls.Add(txtOutcomeDesc, 1, 0);
                tlpOutcomeHeader.Controls.Add(btnDeleteOutcome, 2, 0);
                pnlDynamicOutcome.Controls.Add(tlpOutcomeHeader);

                // --- Add Output Button for this Outcome ---
                Button btnAddOutput = new Button { Text = "Add Output to this Outcome", Tag = outcome, Dock = DockStyle.Top, AutoSize=true };
                btnAddOutput.Click += BtnAddOutputToOutcome_Click; // Generalized handler
                pnlDynamicOutcome.Controls.Add(btnAddOutput);
                btnAddOutput.BringToFront(); // Place it after header

                // --- Panel to Contain Outputs for this Outcome ---
                Panel pnlOutputsContainer = new Panel
                {
                    Name = $"pnlOutputsContainer_Outcome{outcomeCounter}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Padding = new Padding(10, 0, 0, 0) // Indent outputs slightly
                };
                pnlDynamicOutcome.Controls.Add(pnlOutputsContainer);
                pnlOutputsContainer.BringToFront(); // Place it after Add Output button

                RenderOutputsForOutcome(outcome, pnlOutputsContainer); // Existing method, now targetting specific container

                pnlLogFrameMain.Controls.Add(pnlDynamicOutcome);
                pnlDynamicOutcome.BringToFront(); // Ensure outcomes are stacked correctly
            }
            pnlLogFrameMain.ResumeLayout();
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
            if(outcome.Outputs == null) outcome.Outputs = new HashSet<Output>();
            outcome.Outputs.Add(newOutput);

            // Find the pnlOutputsContainer within the specific outcome panel this button belongs to
            Panel outcomePanel = btn.Parent as Panel;
            Panel pnlOutputsContainer = outcomePanel?.Controls.OfType<Panel>().FirstOrDefault(p => p.Name.StartsWith("pnlOutputsContainer_Outcome"));
            if (pnlOutputsContainer != null)
            {
                RenderOutputsForOutcome(outcome, pnlOutputsContainer);
            }
        }

        private void RenderOutputsForOutcome(Outcome outcome, Panel parentOutputPanel) // parentOutputPanel is now specific outputs container
        {
            parentOutputPanel.SuspendLayout(); // Suspend layout
            parentOutputPanel.Controls.Clear();
            parentOutputPanel.Height = 0; // Reset height before adding auto-sized panels

            foreach (var outputInstance in outcome.Outputs.ToList()) // ToList() for safe iteration if collection modified by delete
            {
                Panel pnlDynamicOutput = new Panel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(5),
                    Margin = new Padding(3),
                    Dock = DockStyle.Top // Stack them
                };

                Label lblOutput = new Label { Text = "Output:", AutoSize = true, Location = new Point(5, 5) };
                TextBox txtOutputDesc = new TextBox
                {
                    Text = outputInstance.OutputDescription,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Height = 40,
                    Width = parentOutputPanel.ClientSize.Width - 15, // Adjust for padding
                    Location = new Point(5, 25)
                };
                txtOutputDesc.TextChanged += (s, ev) => outputInstance.OutputDescription = ((TextBox)s).Text;

                Panel pnlButtonsForOutput = new Panel { AutoSize = true, Dock = DockStyle.Top, Location = new Point(5, 70) };
                Button btnAddIndicator = new Button { Text = "Add Indicator", Tag = outputInstance, Location = new Point(0,0) };
                btnAddIndicator.Click += BtnAddIndicator_Click;
                Button btnAddActivity = new Button { Text = "Add Activity", Tag = outputInstance, Location = new Point(btnAddIndicator.Right + 5, 0) };
                btnAddActivity.Click += BtnAddActivity_Click;

                pnlButtonsForOutput.Controls.Add(btnAddIndicator); // Order matters for Flow layout if used, or manual positioning
                pnlButtonsForOutput.Controls.Add(btnAddActivity);


                Panel pnlIndicatorsContainer = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Location = new Point(5, pnlButtonsForOutput.Bottom + 5), Width = pnlDynamicOutput.ClientSize.Width - 10, MinimumSize = new Size(0, 10) };
                Panel pnlActivitiesContainer = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Location = new Point(5, pnlIndicatorsContainer.Bottom + 5), Width = pnlDynamicOutput.ClientSize.Width - 10, MinimumSize = new Size(0,10) };

                pnlDynamicOutput.Controls.Add(lblOutput);
                pnlDynamicOutput.Controls.Add(txtOutputDesc);
                pnlDynamicOutput.Controls.Add(pnlButtonsForOutput);
                pnlDynamicOutput.Controls.Add(pnlIndicatorsContainer); // Add before activities
                pnlDynamicOutput.Controls.Add(pnlActivitiesContainer);

                parentOutputPanel.Controls.Add(pnlDynamicOutput);
                pnlDynamicOutput.BringToFront(); // Ensure it's on top of previous ones if Dock.Top is used

                RenderIndicatorsForOutput(outputInstance, pnlIndicatorsContainer);
                RenderActivitiesForOutput(outputInstance, pnlActivitiesContainer);
            }
            parentOutputPanel.ResumeLayout();
        }

        private void BtnAddIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Output output)) return;

            ProjectIndicator newIndicator = new ProjectIndicator { ProjectID = _currentProject.ProjectID }; // Ensure ProjectID is linked
            output.ProjectIndicators.Add(newIndicator);

            Panel pnlDynamicOutput = btn.Parent.Parent as Panel; // btn -> pnlButtonsForOutput -> pnlDynamicOutput
            Panel pnlIndicatorsContainer = pnlDynamicOutput.Controls.OfType<Panel>().FirstOrDefault(p => p.Name == null && p.Location.Y > btn.Parent.Bottom); // Heuristic to find it
            if (pnlIndicatorsContainer != null) RenderIndicatorsForOutput(output, pnlIndicatorsContainer);
        }

        private void RenderIndicatorsForOutput(Output output, Panel parentIndicatorPanel)
        {
            parentIndicatorPanel.SuspendLayout();
            parentIndicatorPanel.Controls.Clear();
            parentIndicatorPanel.Height = 0;

            foreach (var indicatorInstance in output.ProjectIndicators.ToList())
            {
                Panel pnlDynamicIndicator = new Panel { AutoSize = true, BorderStyle = BorderStyle.None, Padding = new Padding(3), Margin = new Padding(0,0,0,3), Dock = DockStyle.Top };
                TableLayoutPanel tlpIndicator = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
                tlpIndicator.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Labels
                tlpIndicator.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Controls

                TextBox txtIndicatorName = new TextBox { Text = indicatorInstance.IndicatorName, Dock = DockStyle.Fill, /* PlaceholderText = "Indicator Description" */ };
                txtIndicatorName.TextChanged += (s, ev) => indicatorInstance.IndicatorName = ((TextBox)s).Text;

                NumericUpDown nudTargetMen = new NumericUpDown { Value = indicatorInstance.TargetMen, Maximum = 1000000, Width=60 };
                nudTargetMen.ValueChanged += (s, ev) => indicatorInstance.TargetMen = (int)((NumericUpDown)s).Value;
                // ... similar for Women, Boys, Girls, Total
                NumericUpDown nudTargetWomen = new NumericUpDown { Value = indicatorInstance.TargetWomen, Maximum = 1000000, Width=60 };
                nudTargetWomen.ValueChanged += (s, ev) => indicatorInstance.TargetWomen = (int)((NumericUpDown)s).Value;
                 NumericUpDown nudTargetBoys = new NumericUpDown { Value = indicatorInstance.TargetBoys, Maximum = 1000000, Width=60 };
                nudTargetBoys.ValueChanged += (s, ev) => indicatorInstance.TargetBoys = (int)((NumericUpDown)s).Value;
                NumericUpDown nudTargetGirls = new NumericUpDown { Value = indicatorInstance.TargetGirls, Maximum = 1000000, Width=60 };
                nudTargetGirls.ValueChanged += (s, ev) => indicatorInstance.TargetGirls = (int)((NumericUpDown)s).Value;
                NumericUpDown nudTargetTotal = new NumericUpDown { Value = indicatorInstance.TargetTotal, Maximum = 4000000, Width=70, Enabled = false /* often calculated */ }; // Example: Total is calculated
                nudTargetTotal.ValueChanged += (s, ev) => indicatorInstance.TargetTotal = (int)((NumericUpDown)s).Value;


                tlpIndicator.Controls.Add(new Label { Text = "Indicator:", AutoSize=true }, 0, 0); tlpIndicator.Controls.Add(txtIndicatorName, 1, 0);

                FlowLayoutPanel flpTargets = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
                flpTargets.Controls.Add(new Label { Text="Men:", AutoSize=true}); flpTargets.Controls.Add(nudTargetMen);
                flpTargets.Controls.Add(new Label { Text="Women:", AutoSize=true, Margin = new Padding(5,0,0,0) }); flpTargets.Controls.Add(nudTargetWomen);
                flpTargets.Controls.Add(new Label { Text="Boys:", AutoSize=true, Margin = new Padding(5,0,0,0) }); flpTargets.Controls.Add(nudTargetBoys);
                flpTargets.Controls.Add(new Label { Text="Girls:", AutoSize=true, Margin = new Padding(5,0,0,0) }); flpTargets.Controls.Add(nudTargetGirls);
                flpTargets.Controls.Add(new Label { Text="Total:", AutoSize=true, Margin = new Padding(5,0,0,0) }); flpTargets.Controls.Add(nudTargetTotal);

                tlpIndicator.Controls.Add(new Label { Text = "Targets:", AutoSize=true }, 0, 1); tlpIndicator.Controls.Add(flpTargets, 1, 1);

                pnlDynamicIndicator.Controls.Add(tlpIndicator);
                parentIndicatorPanel.Controls.Add(pnlDynamicIndicator);
                pnlDynamicIndicator.BringToFront();
            }
            parentIndicatorPanel.ResumeLayout();
        }

        private void BtnAddActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Output output)) return;

            Activity newActivity = new Activity();
            output.Activities.Add(newActivity);

            Panel pnlDynamicOutput = btn.Parent.Parent as Panel;
            Panel pnlActivitiesContainer = pnlDynamicOutput.Controls.OfType<Panel>().LastOrDefault(p => p.Name == null && p.Location.Y > btn.Parent.Bottom); // Heuristic
            if (pnlActivitiesContainer != null) RenderActivitiesForOutput(output, pnlActivitiesContainer);
        }

        private void RenderActivitiesForOutput(Output output, Panel parentActivityPanel)
        {
            parentActivityPanel.SuspendLayout();
            parentActivityPanel.Controls.Clear();
            parentActivityPanel.Height = 0;

            foreach (var activityInstance in output.Activities.ToList())
            {
                Panel pnlDynamicActivity = new Panel { AutoSize = true, BorderStyle = BorderStyle.None, Padding = new Padding(3), Margin = new Padding(0,0,0,3), Dock = DockStyle.Top };
                TableLayoutPanel tlpActivity = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2};
                tlpActivity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
                tlpActivity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                TextBox txtActivityDesc = new TextBox { Text = activityInstance.ActivityDescription, Dock = DockStyle.Fill, /* PlaceholderText = "Activity Description" */ };
                txtActivityDesc.TextChanged += (s, ev) => activityInstance.ActivityDescription = ((TextBox)s).Text;

                tlpActivity.Controls.Add(new Label { Text = "Activity:", AutoSize=true }, 0, 0); tlpActivity.Controls.Add(txtActivityDesc, 1, 0);

                pnlDynamicActivity.Controls.Add(tlpActivity);
                parentActivityPanel.Controls.Add(pnlDynamicActivity);
                pnlDynamicActivity.BringToFront();
            }
            parentActivityPanel.ResumeLayout();
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
    }
}
