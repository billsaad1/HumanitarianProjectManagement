using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HumanitarianProjectManagement.Models;
using HumanitarianProjectManagement.DataAccessLayer;
using System.Data.SqlClient;
using System.Diagnostics;

namespace HumanitarianProjectManagement
{
    public partial class LogFrameTabUserControl : UserControl
    {
        private Project _currentProject;
        private LogFrameService _logFrameService;
        // private Outcome _selectedOutcome; // This field seems unused, consider removing if confirmed.

        private TabControl inputTabControl;
        private TextBox txtOutcomeDescription;
        private Button btnAddOutcome;

        private ComboBox cmbParentOutcomeForOutput;
        private TextBox txtOutputDescription;
        private Button btnAddOutput;

        private ComboBox cmbParentOutcomeForActivity;
        private ComboBox cmbParentOutputForActivity;
        private TextBox txtActivityDescription;
        private TextBox txtActivityPlannedMonths;
        private Button btnAddActivity;

        private ComboBox cmbParentOutcomeForIndicator;
        private ComboBox cmbParentOutputForIndicator;
        private TextBox txtIndicatorName, txtIndicatorDescription, txtIndicatorTargetValue, txtMeansOfVerification;
        private NumericUpDown numTargetMen, numTargetWomen, numTargetBoys, numTargetGirls, numTargetTotal;
        private Button btnAddIndicator;

        private class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }

        public LogFrameTabUserControl()
        {
            InitializeComponent();
            _logFrameService = new LogFrameService();

            if (splitContainerMain != null)
            {
                splitContainerMain.Panel1Collapsed = true;
            }
            if (splitContainerContent != null)
            {
                splitContainerContent.SplitterDistance = 350;
            }

            InitializeInputControls();
            UpdateInputAreaForContext();

            if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
            {
                lblLogFrameDisplayPlaceholder.ForeColor = Color.Gray;
            }
        }

        public async Task LoadProjectAsync(Project project)
        {
            _currentProject = project;
            // _selectedOutcome = null; // Reset selected outcome if any

            ClearLogFrameDisplay();
            UpdateInputAreaForContext();

            if (_currentProject == null)
            {
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    lblLogFrameDisplayPlaceholder.Text = "No project loaded.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                await LoadOutcomesAsync(); // Still load to clear/prepare input combos
                return;
            }

            await LoadOutcomesAsync();

            if (flpLogFrameDisplay.Controls.Count <= 1 && lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed) // Only placeholder might be there
            {
                if (_currentProject.ProjectID > 0)
                {
                    // Text set by LoadOutcomesAsync or RenderOutcomesHierarchically
                }
                else
                {
                    lblLogFrameDisplayPlaceholder.Text = "Please save the main project details first to enable logframe entries.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
            }
        }

        private async Task LoadOutcomesAsync()
        {
            if (_currentProject == null || _currentProject.ProjectID == 0)
            {
                // _selectedOutcome = null; // Reset
                ClearLogFrameDisplay();
                ClearParentComboBoxes();
                UpdateInputAreaForContext();
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    lblLogFrameDisplayPlaceholder.Text = _currentProject == null ? "No project loaded." : "Please save the main project details first to enable logframe entries.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                return;
            }

            List<Outcome> allOutcomes = new List<Outcome>();
            try
            {
                allOutcomes = await _logFrameService.GetOutcomesByProjectIdAsync(_currentProject.ProjectID);
                if (_currentProject.Outcomes == null || !_currentProject.Outcomes.SequenceEqual(allOutcomes))
                { // Sync _currentProject if service layer is source of truth after load
                    _currentProject.Outcomes = allOutcomes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading outcomes for project ID {_currentProject.ProjectID}: {ex.Message}", "Error - LoadOutcomes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error LoadOutcomesAsync: {ex.ToString()}");
                ClearParentComboBoxes();
                UpdateInputAreaForContext();
                return;
            }

            var orderedOutcomes = allOutcomes.OrderBy(o => o.OutcomeID).ToList(); // MODIFIED SORTING
            await PopulateParentOutcomeComboBoxesAsync(orderedOutcomes);

            if (orderedOutcomes.Any())
            {
                await RenderOutcomesHierarchically(orderedOutcomes, flpLogFrameDisplay);
                // UpdateInputAreaForContext(orderedOutcomes.FirstOrDefault()); // This might be too aggressive
                UpdateInputAreaForContext();
            }
            else
            {
                // _selectedOutcome = null; // Reset
                ClearLogFrameDisplay();
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    lblLogFrameDisplayPlaceholder.Text = "No outcomes defined for this project. Add one using the input form.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                UpdateInputAreaForContext();
            }
        }

        private void ClearParentComboBoxes()
        {
            Action<ComboBox> clearAndNullify = (cmb) => {
                if (cmb != null)
                {
                    cmb.DataSource = null;
                    cmb.Items.Clear();
                    cmb.SelectedItem = null;
                    cmb.Text = string.Empty;
                }
            };
            clearAndNullify(cmbParentOutcomeForOutput);
            clearAndNullify(cmbParentOutcomeForIndicator);
            clearAndNullify(cmbParentOutputForIndicator);
            clearAndNullify(cmbParentOutcomeForActivity);
            clearAndNullify(cmbParentOutputForActivity);
        }

        private async Task PopulateParentOutcomeComboBoxesAsync(List<Outcome> outcomes)
        {
            // Sort for dropdown display can remain alphabetical if desired, or change to ID for consistency
            var outcomeItems = outcomes.OrderBy(o => o.OutcomeDescription)
                .Select(o => new ComboboxItem { Text = TruncateText(o.OutcomeDescription, 50), Value = o.OutcomeID })
                .ToList();

            Action<ComboBox, List<ComboboxItem>> populate = (cmb, items) => {
                if (cmb == null) return;
                cmb.SuspendLayout();
                object previouslySelectedValue = cmb.SelectedValue;
                cmb.DataSource = null;
                cmb.Items.Clear();
                if (items.Any())
                {
                    cmb.DisplayMember = "Text";
                    cmb.ValueMember = "Value";
                    cmb.DataSource = new BindingList<ComboboxItem>(items);
                    if (previouslySelectedValue != null && items.Any(i => i.Value.Equals(previouslySelectedValue)))
                    {
                        cmb.SelectedValue = previouslySelectedValue;
                    }
                    else if (items.Count > 0) { cmb.SelectedIndex = 0; }
                    else { cmb.SelectedIndex = -1; }
                }
                else
                {
                    cmb.Items.Add(new ComboboxItem { Text = "-- No outcomes available --", Value = 0 });
                    cmb.SelectedIndex = 0;
                }
                cmb.ResumeLayout();
            };

            populate(cmbParentOutcomeForOutput, outcomeItems);
            populate(cmbParentOutcomeForIndicator, outcomeItems);
            populate(cmbParentOutcomeForActivity, outcomeItems);

            if (cmbParentOutcomeForIndicator.SelectedValue != null && (int)cmbParentOutcomeForIndicator.SelectedValue > 0)
                await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForIndicator, cmbParentOutputForIndicator);
            else ClearOutputCombo(cmbParentOutputForIndicator, "Select Parent Outcome first");

            if (cmbParentOutcomeForActivity.SelectedValue != null && (int)cmbParentOutcomeForActivity.SelectedValue > 0)
                await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForActivity, cmbParentOutputForActivity);
            else ClearOutputCombo(cmbParentOutputForActivity, "Select Parent Outcome first");
        }

        private void ClearOutputCombo(ComboBox cmbOutput, string placeholderText = "Select Parent Outcome first")
        {
            if (cmbOutput != null)
            {
                cmbOutput.DataSource = null;
                cmbOutput.Items.Clear();
                cmbOutput.Items.Add(new ComboboxItem { Text = $"-- {placeholderText} --", Value = 0 });
                cmbOutput.SelectedIndex = 0;
            }
        }

        private async Task PopulateParentOutputComboBoxesAsync(ComboBox cmbOutcomeParent, ComboBox cmbOutputTarget, int? selectedOutcomeIdOverride = null)
        {
            if (cmbOutputTarget == null) return;
            cmbOutputTarget.SuspendLayout();
            object previouslySelectedValue = cmbOutputTarget.SelectedValue;
            cmbOutputTarget.DataSource = null;
            cmbOutputTarget.Items.Clear();

            int parentOutcomeId = selectedOutcomeIdOverride ?? (cmbOutcomeParent?.SelectedValue as int? ?? 0);

            if (parentOutcomeId > 0)
            {
                List<Output> outputs;
                try
                {
                    outputs = await _logFrameService.GetOutputsByOutcomeIdAsync(parentOutcomeId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading outputs for outcome ID {parentOutcomeId}: {ex.Message}", "Error - LoadOutputs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error PopulateParentOutputComboBoxesAsync: {ex.ToString()}");
                    ClearOutputCombo(cmbOutputTarget, "Error loading outputs");
                    cmbOutputTarget.ResumeLayout();
                    return;
                }
                // Sort for dropdown display can remain alphabetical if desired
                var outputItems = outputs.OrderBy(o => o.OutputDescription)
                                         .Select(o => new ComboboxItem { Text = TruncateText(o.OutputDescription, 50), Value = o.OutputID })
                                         .ToList();
                if (outputItems.Any())
                {
                    cmbOutputTarget.DisplayMember = "Text";
                    cmbOutputTarget.ValueMember = "Value";
                    cmbOutputTarget.DataSource = new BindingList<ComboboxItem>(outputItems);
                    if (previouslySelectedValue != null && outputItems.Any(i => i.Value.Equals(previouslySelectedValue)))
                    {
                        cmbOutputTarget.SelectedValue = previouslySelectedValue;
                    }
                    else if (outputItems.Count > 0) { cmbOutputTarget.SelectedIndex = 0; }
                    else { cmbOutputTarget.SelectedIndex = -1; }
                }
                else
                {
                    cmbOutputTarget.Items.Add(new ComboboxItem { Text = "-- No outputs for selected outcome --", Value = 0 });
                    cmbOutputTarget.SelectedIndex = 0;
                }
            }
            else
            {
                ClearOutputCombo(cmbOutputTarget, "Select Parent Outcome first");
            }
            cmbOutputTarget.ResumeLayout();
        }

        private async void CmbParentOutcomeForIndicator_SelectedIndexChanged(object sender, EventArgs e)
        {
            await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForIndicator, cmbParentOutputForIndicator);
        }

        private async void CmbParentOutcomeForActivity_SelectedIndexChanged(object sender, EventArgs e)
        {
            await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForActivity, cmbParentOutputForActivity);
        }

        private void InitializeInputControls()
        {
            if (this.pnlInputArea == null) return;

            pnlInputArea.SuspendLayout();
            pnlInputArea.Controls.Clear();

            inputTabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            inputTabControl.SelectedIndexChanged += InputTabControl_SelectedIndexChanged;

            TabPage outcomeTab = new TabPage("Outcome");
            FlowLayoutPanel flpOutcomeInput = new FlowLayoutPanel { Name = "flpOutcomeInput", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };
            Label lblOutcomeDesc = new Label { Text = "Outcome Description:" }; StyleInputLabel(lblOutcomeDesc);
            flpOutcomeInput.Controls.Add(lblOutcomeDesc);
            txtOutcomeDescription = new TextBox(); StyleInputTextBox(txtOutcomeDescription, multiline: true, height: 80, width: 500);
            flpOutcomeInput.Controls.Add(txtOutcomeDescription);
            btnAddOutcome = new Button { Text = "Add New Outcome" }; StyleAddButton(btnAddOutcome);
            btnAddOutcome.Click += BtnAddOutcome_Click;
            flpOutcomeInput.Controls.Add(btnAddOutcome);
            outcomeTab.Controls.Add(flpOutcomeInput);
            inputTabControl.TabPages.Add(outcomeTab);

            TabPage outputTab = new TabPage("Output");
            FlowLayoutPanel flpOutputInput = new FlowLayoutPanel { Name = "flpOutputInput", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };
            Label lblParentOutcomeForOutput = new Label { Text = "Parent Outcome:" }; StyleInputLabel(lblParentOutcomeForOutput);
            flpOutputInput.Controls.Add(lblParentOutcomeForOutput);
            cmbParentOutcomeForOutput = new ComboBox(); StyleComboBox(cmbParentOutcomeForOutput);
            flpOutputInput.Controls.Add(cmbParentOutcomeForOutput);
            Label lblOutputDesc = new Label { Text = "Output Description:" }; StyleInputLabel(lblOutputDesc);
            flpOutputInput.Controls.Add(lblOutputDesc);
            txtOutputDescription = new TextBox(); StyleInputTextBox(txtOutputDescription, multiline: true, height: 80, width: 500);
            flpOutputInput.Controls.Add(txtOutputDescription);
            btnAddOutput = new Button { Text = "Add New Output" }; StyleAddButton(btnAddOutput);
            btnAddOutput.Click += BtnAddOutput_Click;
            flpOutputInput.Controls.Add(btnAddOutput);
            outputTab.Controls.Add(flpOutputInput);
            inputTabControl.TabPages.Add(outputTab);

            TabPage indicatorTab = new TabPage("Indicator");
            FlowLayoutPanel flpIndicatorInput = new FlowLayoutPanel { Name = "flpIndicatorInput", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };
            GroupBox gbBasicIndicator = new GroupBox { Text = "Basic Details", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(10, 5, 10, 10), Margin = new Padding(0, 0, 0, 10) };
            FlowLayoutPanel flpBasicIndicator = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoSize = true };
            Label lblParentOutcomeForIndicator = new Label { Text = "Parent Outcome:" }; StyleInputLabel(lblParentOutcomeForIndicator);
            flpBasicIndicator.Controls.Add(lblParentOutcomeForIndicator);
            cmbParentOutcomeForIndicator = new ComboBox(); StyleComboBox(cmbParentOutcomeForIndicator);
            cmbParentOutcomeForIndicator.SelectedIndexChanged += CmbParentOutcomeForIndicator_SelectedIndexChanged;
            flpBasicIndicator.Controls.Add(cmbParentOutcomeForIndicator);
            Label lblParentOutputForIndicator = new Label { Text = "Parent Output:" }; StyleInputLabel(lblParentOutputForIndicator);
            flpBasicIndicator.Controls.Add(lblParentOutputForIndicator);
            cmbParentOutputForIndicator = new ComboBox(); StyleComboBox(cmbParentOutputForIndicator);
            flpBasicIndicator.Controls.Add(cmbParentOutputForIndicator);
            Label lblIndicatorName = new Label { Text = "Indicator Name:" }; StyleInputLabel(lblIndicatorName);
            flpBasicIndicator.Controls.Add(lblIndicatorName);
            txtIndicatorName = new TextBox(); StyleInputTextBox(txtIndicatorName, width: 500);
            flpBasicIndicator.Controls.Add(txtIndicatorName);
            Label lblIndicatorDesc = new Label { Text = "Description:" }; StyleInputLabel(lblIndicatorDesc);
            flpBasicIndicator.Controls.Add(lblIndicatorDesc);
            txtIndicatorDescription = new TextBox(); StyleInputTextBox(txtIndicatorDescription, multiline: true, height: 80, width: 500);
            flpBasicIndicator.Controls.Add(txtIndicatorDescription);
            Label lblIndicatorTarget = new Label { Text = "Target Value (textual):" }; StyleInputLabel(lblIndicatorTarget);
            flpBasicIndicator.Controls.Add(lblIndicatorTarget);
            txtIndicatorTargetValue = new TextBox(); StyleInputTextBox(txtIndicatorTargetValue, width: 500);
            flpBasicIndicator.Controls.Add(txtIndicatorTargetValue);
            Label lblMeansOfVerification = new Label { Text = "Means of Verification:" }; StyleInputLabel(lblMeansOfVerification);
            flpBasicIndicator.Controls.Add(lblMeansOfVerification);
            txtMeansOfVerification = new TextBox(); StyleInputTextBox(txtMeansOfVerification, multiline: true, height: 60, width: 500);
            flpBasicIndicator.Controls.Add(txtMeansOfVerification);
            gbBasicIndicator.Controls.Add(flpBasicIndicator);
            flpIndicatorInput.Controls.Add(gbBasicIndicator);
            GroupBox gbDemographics = new GroupBox { Text = "Demographic Targets", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(10, 5, 10, 10), Margin = new Padding(0, 0, 0, 10), MinimumSize = new Size(540, 0) };
            TableLayoutPanel tlpDemographicsInd = new TableLayoutPanel { ColumnCount = 5, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 0, 0, 5), MinimumSize = new Size(530, 0) };
            for (int i = 0; i < 5; i++) tlpDemographicsInd.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Label lblTargetMenInd = new Label { Text = "Men:", Anchor = AnchorStyles.Left }; StyleInputLabel(lblTargetMenInd); tlpDemographicsInd.Controls.Add(lblTargetMenInd, 0, 0);
            numTargetMen = new NumericUpDown { Name = "numTargetMen", Maximum = 1000000, Minimum = 0, Width = 100, Font = new Font("Segoe UI", 9F), Margin = new Padding(3) }; tlpDemographicsInd.Controls.Add(numTargetMen, 0, 1);
            Label lblTargetWomenInd = new Label { Text = "Women:", Anchor = AnchorStyles.Left }; StyleInputLabel(lblTargetWomenInd); tlpDemographicsInd.Controls.Add(lblTargetWomenInd, 1, 0);
            numTargetWomen = new NumericUpDown { Name = "numTargetWomen", Maximum = 1000000, Minimum = 0, Width = 100, Font = new Font("Segoe UI", 9F), Margin = new Padding(3) }; tlpDemographicsInd.Controls.Add(numTargetWomen, 1, 1);
            Label lblTargetBoysInd = new Label { Text = "Boys:", Anchor = AnchorStyles.Left }; StyleInputLabel(lblTargetBoysInd); tlpDemographicsInd.Controls.Add(lblTargetBoysInd, 2, 0);
            numTargetBoys = new NumericUpDown { Name = "numTargetBoys", Maximum = 1000000, Minimum = 0, Width = 100, Font = new Font("Segoe UI", 9F), Margin = new Padding(3) }; tlpDemographicsInd.Controls.Add(numTargetBoys, 2, 1);
            Label lblTargetGirlsInd = new Label { Text = "Girls:", Anchor = AnchorStyles.Left }; StyleInputLabel(lblTargetGirlsInd); tlpDemographicsInd.Controls.Add(lblTargetGirlsInd, 3, 0);
            numTargetGirls = new NumericUpDown { Name = "numTargetGirls", Maximum = 1000000, Minimum = 0, Width = 100, Font = new Font("Segoe UI", 9F), Margin = new Padding(3) }; tlpDemographicsInd.Controls.Add(numTargetGirls, 3, 1);
            Label lblTargetTotalInd = new Label { Text = "Total:", Anchor = AnchorStyles.Left }; StyleInputLabel(lblTargetTotalInd); tlpDemographicsInd.Controls.Add(lblTargetTotalInd, 4, 0);
            numTargetTotal = new NumericUpDown { Name = "numTargetTotal", Maximum = 4000000, Minimum = 0, Width = 100, Font = new Font("Segoe UI", 9F), Margin = new Padding(3) }; tlpDemographicsInd.Controls.Add(numTargetTotal, 4, 1);
            gbDemographics.Controls.Add(tlpDemographicsInd);
            flpIndicatorInput.Controls.Add(gbDemographics);
            btnAddIndicator = new Button { Text = "Add New Indicator" }; StyleAddButton(btnAddIndicator);
            btnAddIndicator.Click += BtnAddIndicator_Click;
            flpIndicatorInput.Controls.Add(btnAddIndicator);
            indicatorTab.Controls.Add(flpIndicatorInput);
            inputTabControl.TabPages.Add(indicatorTab);

            TabPage activityTab = new TabPage("Activity");
            FlowLayoutPanel flpActivityInput = new FlowLayoutPanel { Name = "flpActivityInput", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };
            Label lblParentOutcomeForActivity = new Label { Text = "Parent Outcome:" }; StyleInputLabel(lblParentOutcomeForActivity);
            flpActivityInput.Controls.Add(lblParentOutcomeForActivity);
            cmbParentOutcomeForActivity = new ComboBox(); StyleComboBox(cmbParentOutcomeForActivity);
            cmbParentOutcomeForActivity.SelectedIndexChanged += CmbParentOutcomeForActivity_SelectedIndexChanged;
            flpActivityInput.Controls.Add(cmbParentOutcomeForActivity);
            Label lblParentOutputForActivity = new Label { Text = "Parent Output:" }; StyleInputLabel(lblParentOutputForActivity);
            flpActivityInput.Controls.Add(lblParentOutputForActivity);
            cmbParentOutputForActivity = new ComboBox(); StyleComboBox(cmbParentOutputForActivity);
            flpActivityInput.Controls.Add(cmbParentOutputForActivity);
            Label lblActivityDesc = new Label { Text = "Activity Description:" }; StyleInputLabel(lblActivityDesc);
            flpActivityInput.Controls.Add(lblActivityDesc);
            txtActivityDescription = new TextBox(); StyleInputTextBox(txtActivityDescription, multiline: true, height: 80, width: 500);
            flpActivityInput.Controls.Add(txtActivityDescription);
            Label lblActivityMonths = new Label { Text = "Planned Months (e.g., Jan/23,Feb/23):" }; StyleInputLabel(lblActivityMonths);
            flpActivityInput.Controls.Add(lblActivityMonths);
            txtActivityPlannedMonths = new TextBox(); StyleInputTextBox(txtActivityPlannedMonths, width: 200);
            flpActivityInput.Controls.Add(txtActivityPlannedMonths);
            btnAddActivity = new Button { Text = "Add New Activity" }; StyleAddButton(btnAddActivity);
            btnAddActivity.Click += BtnAddActivity_Click;
            flpActivityInput.Controls.Add(btnAddActivity);
            activityTab.Controls.Add(flpActivityInput);
            inputTabControl.TabPages.Add(activityTab);

            pnlInputArea.Controls.Add(inputTabControl);
            pnlInputArea.ResumeLayout(false);
        }

        private void StyleComboBox(ComboBox cmb)
        {
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.Font = new Font("Segoe UI", 10F);
            cmb.Width = 500;
            cmb.Margin = new Padding(0, 0, 0, 10);
        }

        private void UpdateInputAreaForContext(Outcome outcomeForDisplayContext = null)
        {
            if (inputTabControl == null) InitializeInputControls();

            bool projectIsSavedAndValid = _currentProject != null && _currentProject.ProjectID > 0;

            foreach (TabPage tab in inputTabControl.TabPages)
            {
                tab.Enabled = projectIsSavedAndValid;
            }

            if (!projectIsSavedAndValid)
            {
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    ClearLogFrameDisplay();
                    lblLogFrameDisplayPlaceholder.Text = "Please save the main project details first to enable logframe entries.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                if (inputTabControl.TabPages.Count > 0) inputTabControl.SelectedIndex = 0;
                return;
            }

            bool hasOutcomes = _currentProject.Outcomes != null && _currentProject.Outcomes.Any();

            if (inputTabControl.TabPages.Count > 1) inputTabControl.TabPages[1].Enabled = hasOutcomes;
            if (inputTabControl.TabPages.Count > 2) inputTabControl.TabPages[2].Enabled = hasOutcomes;
            if (inputTabControl.TabPages.Count > 3) inputTabControl.TabPages[3].Enabled = hasOutcomes;

            if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
            {
                if (!hasOutcomes)
                {
                    lblLogFrameDisplayPlaceholder.Text = "No outcomes exist. Please add one using the 'Outcome' tab.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                else if (flpLogFrameDisplay.Controls.Count <= 1)
                {
                    lblLogFrameDisplayPlaceholder.Text = "Logframe items will appear here as they are added.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                else
                {
                    lblLogFrameDisplayPlaceholder.Visible = false;
                }
            }

            if (inputTabControl.SelectedTab != null && !inputTabControl.SelectedTab.Enabled)
            {
                inputTabControl.SelectedIndex = 0;
            }
        }

        private async void BtnAddOutcome_Click(object sender, EventArgs e)
        {
            if (_currentProject == null || _currentProject.ProjectID == 0)
            {
                MessageBox.Show("Please save the project first to obtain a valid Project ID before adding logframe items.", "Save Project Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtOutcomeDescription.Text))
            {
                MessageBox.Show("Outcome description cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOutcomeDescription.Focus();
                return;
            }

            if (btnAddOutcome.Tag is int outcomeIdToUpdate && btnAddOutcome.Text == "Update Outcome")
            {
                Outcome outcomeToUpdate = new Outcome
                {
                    OutcomeID = outcomeIdToUpdate,
                    ProjectID = _currentProject.ProjectID,
                    OutcomeDescription = txtOutcomeDescription.Text.Trim()
                };

                try
                {
                    bool success = await _logFrameService.UpdateOutcomeAsync(outcomeToUpdate);
                    if (success)
                    {
                        MessageBox.Show("Outcome updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var existingOutcome = _currentProject.Outcomes.FirstOrDefault(o => o.OutcomeID == outcomeIdToUpdate);
                        if (existingOutcome != null)
                        {
                            existingOutcome.OutcomeDescription = outcomeToUpdate.OutcomeDescription;
                        }
                        await LoadOutcomesAsync();
                        btnAddOutcome.Text = "Add New Outcome";
                        btnAddOutcome.Tag = null;
                        txtOutcomeDescription.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update outcome.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the outcome: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error BtnAddOutcome_Click (Update): {ex.ToString()}");
                }
            }
            else
            {
                Outcome newOutcome = new Outcome { ProjectID = _currentProject.ProjectID, OutcomeDescription = txtOutcomeDescription.Text.Trim() };
                int newOutcomeId = await _logFrameService.AddOutcomeAsync(newOutcome);
                if (newOutcomeId > 0)
                {
                    newOutcome.OutcomeID = newOutcomeId;
                    if (_currentProject.Outcomes == null) _currentProject.Outcomes = new List<Outcome>();
                    _currentProject.Outcomes.Add(newOutcome);
                    MessageBox.Show("Outcome added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtOutcomeDescription.Clear();
                    await LoadOutcomesAsync();
                    if (inputTabControl.TabPages.Count > 1 && inputTabControl.TabPages[1].Enabled)
                    {
                        inputTabControl.SelectedTab = inputTabControl.TabPages[1];
                        cmbParentOutcomeForOutput?.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to add outcome.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnAddOutput_Click(object sender, EventArgs e)
        {
            if (cmbParentOutcomeForOutput.SelectedItem == null || !(cmbParentOutcomeForOutput.SelectedValue is int parentOutcomeId) || parentOutcomeId == 0)
            {
                MessageBox.Show("Please select a Parent Outcome from the dropdown.", "Parent Outcome Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtOutputDescription.Text))
            {
                MessageBox.Show("Output description cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOutputDescription.Focus();
                return;
            }

            if (btnAddOutput.Tag is int outputIdToUpdate && btnAddOutput.Text == "Update Output")
            {
                Output outputToUpdate = new Output
                {
                    OutputID = outputIdToUpdate,
                    OutcomeID = parentOutcomeId,
                    OutputDescription = txtOutputDescription.Text.Trim()
                };

                try
                {
                    bool success = await _logFrameService.UpdateOutputAsync(outputToUpdate);
                    if (success)
                    {
                        MessageBox.Show("Output updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var pOutcome = _currentProject.Outcomes.FirstOrDefault(o => o.OutcomeID == outputToUpdate.OutcomeID);
                        var existingOutput = pOutcome?.Outputs.FirstOrDefault(op => op.OutputID == outputIdToUpdate);
                        if (existingOutput != null)
                        {
                            existingOutput.OutputDescription = outputToUpdate.OutputDescription;
                        }
                        await LoadOutcomesAsync();
                        btnAddOutput.Text = "Add New Output";
                        btnAddOutput.Tag = null;
                        txtOutputDescription.Clear();
                        cmbParentOutcomeForOutput.SelectedIndex = (cmbParentOutcomeForOutput.Items.Count > 0) ? 0 : -1;
                    }
                    else
                    {
                        MessageBox.Show("Failed to update output.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the output: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error BtnAddOutput_Click (Update): {ex.ToString()}");
                }
            }
            else
            {
                Output newOutput = new Output { OutcomeID = parentOutcomeId, OutputDescription = txtOutputDescription.Text.Trim() };
                int newOutputId = await _logFrameService.AddOutputAsync(newOutput);

                if (newOutputId > 0)
                {
                    newOutput.OutputID = newOutputId;
                    Outcome localParentOutcome = _currentProject.Outcomes.FirstOrDefault(o => o.OutcomeID == parentOutcomeId);
                    if (localParentOutcome != null)
                    {
                        if (localParentOutcome.Outputs == null) localParentOutcome.Outputs = new List<Output>();
                        localParentOutcome.Outputs.Add(newOutput);
                    }
                    MessageBox.Show("Output added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtOutputDescription.Clear();
                    await LoadOutcomesAsync();

                    if (inputTabControl.TabPages.Count > 2 && inputTabControl.TabPages[2].Enabled)
                    {
                        inputTabControl.SelectedTab = inputTabControl.TabPages[2];
                        cmbParentOutcomeForIndicator.SelectedValue = parentOutcomeId;
                        await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForIndicator, cmbParentOutputForIndicator);
                        cmbParentOutputForIndicator.SelectedValue = newOutputId;
                        cmbParentOutputForIndicator.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to add output.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnAddActivity_Click(object sender, EventArgs e)
        {
            if (cmbParentOutputForActivity.SelectedItem == null || !(cmbParentOutputForActivity.SelectedValue is int parentOutputId) || parentOutputId == 0)
            {
                MessageBox.Show("Please select a Parent Outcome and then a Parent Output from the dropdowns.", "Parent Output Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtActivityDescription.Text))
            {
                MessageBox.Show("Activity description cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtActivityDescription.Focus();
                return;
            }

            HumanitarianProjectManagement.Models.Activity activityData = new HumanitarianProjectManagement.Models.Activity
            {
                OutputID = parentOutputId,
                ActivityDescription = txtActivityDescription.Text.Trim(),
                PlannedMonths = txtActivityPlannedMonths.Text.Trim()
            };

            if (btnAddActivity.Tag is int activityIdToUpdate && btnAddActivity.Text == "Update Activity")
            {
                activityData.ActivityID = activityIdToUpdate;
                try
                {
                    bool success = await _logFrameService.UpdateActivityAsync(activityData);
                    if (success)
                    {
                        MessageBox.Show("Activity updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadOutcomesAsync();
                        btnAddActivity.Text = "Add New Activity";
                        btnAddActivity.Tag = null;
                        txtActivityDescription.Clear();
                        txtActivityPlannedMonths.Clear();
                        cmbParentOutcomeForActivity.SelectedIndex = (cmbParentOutcomeForActivity.Items.Count > 0) ? 0 : -1;
                        ClearOutputCombo(cmbParentOutputForActivity);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update activity.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the activity: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error BtnAddActivity_Click (Update): {ex.ToString()}");
                }
            }
            else
            {
                int newActivityId = await _logFrameService.AddActivityAsync(activityData);
                if (newActivityId > 0)
                {
                    activityData.ActivityID = newActivityId;
                    foreach (var outcome in _currentProject.Outcomes)
                    {
                        var pOutput = outcome.Outputs?.FirstOrDefault(o => o.OutputID == activityData.OutputID);
                        if (pOutput != null)
                        {
                            if (pOutput.Activities == null) pOutput.Activities = new List<Models.Activity>();
                            pOutput.Activities.Add(activityData);
                            break;
                        }
                    }
                    MessageBox.Show("Activity added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtActivityDescription.Clear();
                    txtActivityPlannedMonths.Clear();
                    await LoadOutcomesAsync();
                    cmbParentOutputForActivity?.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to add activity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnAddIndicator_Click(object sender, EventArgs e)
        {
            if (cmbParentOutputForIndicator.SelectedItem == null || !(cmbParentOutputForIndicator.SelectedValue is int parentOutputId) || parentOutputId == 0)
            {
                MessageBox.Show("Please select a Parent Outcome and then a Parent Output from the dropdowns.", "Parent Output Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_currentProject == null || _currentProject.ProjectID == 0)
            {
                MessageBox.Show("Current project is not loaded or not saved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtIndicatorName.Text))
            {
                MessageBox.Show("Indicator name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtIndicatorName.Focus();
                return;
            }

            ProjectIndicator indicatorData = new ProjectIndicator
            {
                ProjectID = _currentProject.ProjectID,
                OutputID = parentOutputId,
                IndicatorName = txtIndicatorName.Text.Trim(),
                Description = txtIndicatorDescription.Text.Trim(),
                TargetValue = txtIndicatorTargetValue.Text.Trim(),
                MeansOfVerification = txtMeansOfVerification.Text.Trim(),
                IsKeyIndicator = false,
                TargetMen = (int)numTargetMen.Value,
                TargetWomen = (int)numTargetWomen.Value,
                TargetBoys = (int)numTargetBoys.Value,
                TargetGirls = (int)numTargetGirls.Value,
                TargetTotal = (int)numTargetTotal.Value
            };

            if (btnAddIndicator.Tag is int indicatorIdToUpdate && btnAddIndicator.Text == "Update Indicator")
            {
                indicatorData.ProjectIndicatorID = indicatorIdToUpdate;
                try
                {
                    bool success = await _logFrameService.UpdateProjectIndicatorAsync(indicatorData);
                    if (success)
                    {
                        MessageBox.Show("Indicator updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadOutcomesAsync();
                        btnAddIndicator.Text = "Add New Indicator";
                        btnAddIndicator.Tag = null;
                        txtIndicatorName.Clear();
                        txtIndicatorDescription.Clear();
                        txtIndicatorTargetValue.Clear();
                        txtMeansOfVerification.Clear();
                        numTargetMen.Value = 0; numTargetWomen.Value = 0; numTargetBoys.Value = 0; numTargetGirls.Value = 0; numTargetTotal.Value = 0;
                        cmbParentOutcomeForIndicator.SelectedIndex = (cmbParentOutcomeForIndicator.Items.Count > 0) ? 0 : -1;
                        ClearOutputCombo(cmbParentOutputForIndicator);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update indicator.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the indicator: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error BtnAddIndicator_Click (Update): {ex.ToString()}");
                }
            }
            else
            {
                int newIndicatorId = await _logFrameService.AddProjectIndicatorToOutputAsync(indicatorData);
                if (newIndicatorId > 0)
                {
                    indicatorData.ProjectIndicatorID = newIndicatorId;
                    if (indicatorData.OutputID.HasValue)
                    {
                        foreach (var outcome in _currentProject.Outcomes)
                        {
                            var pOutput = outcome.Outputs?.FirstOrDefault(o => o.OutputID == indicatorData.OutputID.Value);
                            if (pOutput != null)
                            {
                                if (pOutput.ProjectIndicators == null) pOutput.ProjectIndicators = new List<ProjectIndicator>();
                                pOutput.ProjectIndicators.Add(indicatorData);
                                break;
                            }
                        }
                    }
                    MessageBox.Show("Indicator added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtIndicatorName.Clear();
                    txtIndicatorDescription.Clear();
                    txtIndicatorTargetValue.Clear();
                    txtMeansOfVerification.Clear();
                    numTargetMen.Value = 0; numTargetWomen.Value = 0; numTargetBoys.Value = 0; numTargetGirls.Value = 0; numTargetTotal.Value = 0;
                    await LoadOutcomesAsync();
                    cmbParentOutputForIndicator?.Focus();
                }
                else
                {
                    MessageBox.Show("Failed to add indicator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task RenderOutcomesHierarchically(List<Outcome> outcomes, FlowLayoutPanel displayPanel)
        {
            ClearLogFrameDisplay();
            if (outcomes == null || !outcomes.Any())
            {
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    if (_currentProject != null && _currentProject.ProjectID > 0)
                        lblLogFrameDisplayPlaceholder.Text = "No outcomes defined. Add one to begin.";
                    else if (_currentProject != null && _currentProject.ProjectID == 0)
                        lblLogFrameDisplayPlaceholder.Text = "Please save the main project details first to enable logframe entries.";
                    else
                        lblLogFrameDisplayPlaceholder.Text = "No project loaded.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                return;
            }
            if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed) lblLogFrameDisplayPlaceholder.Visible = false;

            displayPanel.SuspendLayout();
            int outcomeCounter = 1;
            foreach (var outcome in outcomes) // outcomes are now ordered by OutcomeID from LoadOutcomesAsync
            {
                await RenderSingleOutcomeWithChildrenAsync(outcome, displayPanel, $"{outcomeCounter}.");
                outcomeCounter++;
            }
            displayPanel.ResumeLayout(true);
            displayPanel.PerformLayout();
        }

        private async Task RenderSingleOutcomeWithChildrenAsync(Outcome outcome, FlowLayoutPanel displayPanel, string outcomeNumberPrefix)
        {
            if (outcome == null) return;

            AddDisplayControl(displayPanel, "Outcome", outcome.OutcomeDescription, 0, FontStyle.Bold, 12.5F, Color.DarkBlue, outcome, true, outcomeNumberPrefix);
            var outputs = await _logFrameService.GetOutputsByOutcomeIdAsync(outcome.OutcomeID);
            var orderedOutputs = outputs.OrderBy(o => o.OutputID).ToList(); // MODIFIED SORTING

            int outputCounter = 1;
            foreach (var output in orderedOutputs)
            {
                string outputNumber = $"{outcomeNumberPrefix}{outputCounter}.";
                AddDisplayControl(displayPanel, "Output", output.OutputDescription, 1, FontStyle.Bold, 11F, Color.DarkGreen, output, true, outputNumber);
                var indicators = await _logFrameService.GetProjectIndicatorsByOutputIdAsync(output.OutputID);
                var orderedIndicators = indicators.OrderBy(i => i.ProjectIndicatorID).ToList(); // MODIFIED SORTING

                int indicatorCounter = 1;
                if (orderedIndicators.Any())
                {
                    foreach (var indicator in orderedIndicators)
                    {
                        string indicatorNumber = $"{outputNumber}{indicatorCounter}.";
                        AddDisplayControl(displayPanel, "Indicator", indicator.IndicatorName, 2, FontStyle.Regular, 10F, Color.Black, indicator, true, indicatorNumber);
                        string details = $"Target: {indicator.TargetValue ?? "N/A"}, MoV: {indicator.MeansOfVerification ?? "N/A"}";
                        if (indicator.TargetMen > 0 || indicator.TargetWomen > 0 || indicator.TargetBoys > 0 || indicator.TargetGirls > 0 || indicator.TargetTotal > 0)
                        {
                            details += $", Demo: M({indicator.TargetMen}), W({indicator.TargetWomen}), B({indicator.TargetBoys}), G({indicator.TargetGirls}), T({indicator.TargetTotal})";
                        }
                        AddDisplayControl(displayPanel, "", details, 3, FontStyle.Italic, 8.5F, Color.DarkSlateGray, null, false, "");
                        indicatorCounter++;
                    }
                }
                else
                {
                    AddDisplayControl(displayPanel, "", "No indicators for this output.", 2, FontStyle.Italic, 9F, Color.Gray, null, false, $"{outputNumber}  ");
                }
                var activities = await _logFrameService.GetActivitiesByOutputIdAsync(output.OutputID);
                var orderedActivities = activities.OrderBy(a => a.ActivityID).ToList(); // MODIFIED SORTING

                int activityCounter = 1;
                if (orderedActivities.Any())
                {
                    foreach (var activity in orderedActivities)
                    {
                        string activityNumber = $"{outputNumber}{((char)('a' + activityCounter - 1))}.";
                        AddDisplayControl(displayPanel, "Activity", activity.ActivityDescription, 2, FontStyle.Regular, 10F, Color.Black, activity, true, activityNumber);
                        if (!string.IsNullOrWhiteSpace(activity.PlannedMonths))
                        {
                            AddDisplayControl(displayPanel, "", $"Planned Months: {activity.PlannedMonths}", 3, FontStyle.Italic, 8.5F, Color.DarkSlateGray, null, false, "");
                        }
                        activityCounter++;
                    }
                }
                else
                {
                    AddDisplayControl(displayPanel, "", "No activities for this output.", 2, FontStyle.Italic, 9F, Color.Gray, null, false, $"{outputNumber}  ");
                }
                outputCounter++;
            }
        }

        private void AddDisplayControl(FlowLayoutPanel panel, string itemTypeTitle, string text, int indentLevel, FontStyle fontStyle, float fontSize, Color foreColor, object itemData, bool isHeader = true, string itemNumberPrefix = "")
        {
            TableLayoutPanel itemRowPanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(20 * indentLevel + 5, isHeader ? 4 : 1, 0, isHeader ? 1 : 1),
            };
            itemRowPanel.ColumnStyles.Clear();
            itemRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 13F));
            itemRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            itemRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = $"{itemNumberPrefix}{(string.IsNullOrEmpty(itemTypeTitle) ? "" : " " + itemTypeTitle + ": ")}{text}",
                Font = new Font("Segoe UI", fontSize, fontStyle),
                ForeColor = foreColor,
                AutoSize = true, // MODIFIED
                AutoEllipsis = false, // MODIFIED
                Padding = new Padding(3, 0, 3, 0),
                Margin = new Padding(0),
                Dock = DockStyle.Fill
            };
            itemRowPanel.Controls.Add(label, 0, 0);

            itemRowPanel.Controls.Add(new Panel { Size = new Size(0, 0), Margin = Padding.Empty }, 1, 0);

            if (itemData != null && isHeader)
            {
                FlowLayoutPanel actionsPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    WrapContents = false,
                    Dock = DockStyle.Right
                };

                Button btnEdit = new Button
                {
                    Text = "Edit",
                    Tag = itemData,
                    Size = new Size(45, 22),
                    Font = new Font("Segoe UI", 7.5F),
                    Margin = new Padding(1, 0, 1, 0),
                    FlatStyle = FlatStyle.System
                };

                Button btnDelete = new Button
                {
                    Text = "Del",
                    Tag = itemData,
                    Size = new Size(40, 22),
                    Font = new Font("Segoe UI", 7.5F),
                    Margin = new Padding(1, 0, 1, 0),
                    FlatStyle = FlatStyle.System,
                    ForeColor = Color.DarkRed
                };

                if (itemData is Outcome)
                {
                    btnEdit.Click += EditOutcome_Click;
                    btnDelete.Click += DeleteOutcome_Click;
                }
                else if (itemData is Output)
                {
                    btnEdit.Click += EditOutput_Click;
                    btnDelete.Click += DeleteOutput_Click;
                }
                else if (itemData is Models.Activity)
                {
                    btnEdit.Click += EditActivity_Click;
                    btnDelete.Click += DeleteActivity_Click;
                }
                else if (itemData is ProjectIndicator)
                {
                    btnEdit.Click += EditIndicator_Click;
                    btnDelete.Click += DeleteIndicator_Click;
                }
                actionsPanel.Controls.Add(btnEdit);
                actionsPanel.Controls.Add(btnDelete);
                itemRowPanel.Controls.Add(actionsPanel, 2, 0);
            }

            panel.Controls.Add(itemRowPanel);

            if (isHeader && indentLevel < 2)
            {
                var separator = new Panel
                {
                    Height = 1,
                    Dock = DockStyle.Top,
                    BackColor = Color.LightGray,
                    Margin = new Padding(20 * indentLevel + 5, 3, 0, (indentLevel == 0 ? 6 : 3))
                };
                panel.Controls.Add(separator);
            }
        }


        private void StyleInputLabel(Label label)
        {
            label.AutoSize = true;
            label.Margin = new Padding(0, 8, 0, 2);
            label.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private void StyleInputTextBox(TextBox textBox, bool multiline = false, int width = 450, int height = 60)
        {
            textBox.Dock = DockStyle.Top;
            textBox.Width = width;
            if (multiline)
            {
                textBox.Multiline = true;
                textBox.ScrollBars = ScrollBars.Vertical;
                textBox.Height = height;
            }
            else
            {
                textBox.Height = (int)(new Font("Segoe UI", 10F).Height * 1.5 + 5);
            }
            textBox.Margin = new Padding(0, 0, 0, 12);
            textBox.Font = new Font("Segoe UI", 10F);
        }

        private void StyleAddButton(Button button)
        {
            button.AutoSize = true;
            button.FlatStyle = FlatStyle.System;
            button.Margin = new Padding(0, 5, 0, 10);
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Padding = new Padding(10, 5, 10, 5);
        }

        private void ClearLogFrameDisplay()
        {
            if (flpLogFrameDisplay == null) return;
            flpLogFrameDisplay.SuspendLayout();
            var controlsToRemove = flpLogFrameDisplay.Controls.OfType<Control>().Where(c => c != lblLogFrameDisplayPlaceholder).ToList();
            foreach (var control in controlsToRemove)
            {
                flpLogFrameDisplay.Controls.Remove(control);
                control.Dispose();
            }
            if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
            {
                lblLogFrameDisplayPlaceholder.Text = "LogFrame Display Area";
                lblLogFrameDisplayPlaceholder.Visible = true;
                if (!flpLogFrameDisplay.Controls.Contains(lblLogFrameDisplayPlaceholder))
                {
                    flpLogFrameDisplay.Controls.Add(lblLogFrameDisplayPlaceholder);
                }
                lblLogFrameDisplayPlaceholder.BringToFront();
            }
            flpLogFrameDisplay.ResumeLayout(true);
            flpLogFrameDisplay.PerformLayout();
        }

        private void ClearOutcomesSidebar()
        {
            if (flpOutcomesSidebar == null) return;
            flpOutcomesSidebar.SuspendLayout();
            foreach (Control ctrl in flpOutcomesSidebar.Controls) ctrl.Dispose();
            flpOutcomesSidebar.Controls.Clear();
            flpOutcomesSidebar.ResumeLayout(true);
            flpOutcomesSidebar.PerformLayout();
        }


        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private async void DeleteOutcome_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Outcome outcomeToDelete))
            {
                MessageBox.Show("Could not determine the outcome to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to delete outcome: '{TruncateText(outcomeToDelete.OutcomeDescription, 50)}'? This will also delete all associated outputs, activities, and indicators.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    bool success = await _logFrameService.DeleteOutcomeAsync(outcomeToDelete.OutcomeID);
                    if (success)
                    {
                        MessageBox.Show("Outcome deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _currentProject?.Outcomes?.Remove(outcomeToDelete);
                        await LoadOutcomesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete outcome. It might be in use or no longer exist.", "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the outcome: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error DeleteOutcome_Click: {ex.ToString()}");
                }
            }
        }

        private void EditOutcome_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Outcome outcomeToEdit))
            {
                MessageBox.Show("Could not determine the outcome to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (inputTabControl.TabPages.Count > 0)
            {
                inputTabControl.SelectedTab = inputTabControl.TabPages[0];
            }

            txtOutcomeDescription.Text = outcomeToEdit.OutcomeDescription;
            btnAddOutcome.Text = "Update Outcome";
            btnAddOutcome.Tag = outcomeToEdit.OutcomeID;
            txtOutcomeDescription.Focus();
        }

        private async void DeleteOutput_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Output outputToDelete))
            {
                MessageBox.Show("Could not determine the output to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to delete output: '{TruncateText(outputToDelete.OutputDescription, 50)}'? This will also delete all associated activities and indicators.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    bool success = await _logFrameService.DeleteOutputAsync(outputToDelete.OutputID);
                    if (success)
                    {
                        MessageBox.Show("Output deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var parentOutcome = _currentProject?.Outcomes?.FirstOrDefault(o => o.OutcomeID == outputToDelete.OutcomeID);
                        parentOutcome?.Outputs?.Remove(outputToDelete);
                        await LoadOutcomesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete output. It might be in use or no longer exist.", "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the output: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error DeleteOutput_Click: {ex.ToString()}");
                }
            }
        }

        private void EditOutput_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Output outputToEdit))
            {
                MessageBox.Show("Could not determine the output to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (inputTabControl.TabPages.Count > 1)
            {
                inputTabControl.SelectedTab = inputTabControl.TabPages[1];
            }

            cmbParentOutcomeForOutput.SelectedValue = outputToEdit.OutcomeID;
            txtOutputDescription.Text = outputToEdit.OutputDescription;
            btnAddOutput.Text = "Update Output";
            btnAddOutput.Tag = outputToEdit.OutputID;
            txtOutputDescription.Focus();
        }

        private async void DeleteActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Models.Activity activityToDelete))
            {
                MessageBox.Show("Could not determine the activity to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to delete activity: '{TruncateText(activityToDelete.ActivityDescription, 50)}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    bool success = await _logFrameService.DeleteActivityAsync(activityToDelete.ActivityID);
                    if (success)
                    {
                        MessageBox.Show("Activity deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        foreach (var outcome in _currentProject.Outcomes)
                        {
                            var parentOutput = outcome.Outputs?.FirstOrDefault(o => o.OutputID == activityToDelete.OutputID);
                            parentOutput?.Activities?.Remove(activityToDelete);
                        }
                        await LoadOutcomesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete activity.", "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the activity: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error DeleteActivity_Click: {ex.ToString()}");
                }
            }
        }

        private async void EditActivity_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Models.Activity activityToEdit))
            {
                MessageBox.Show("Could not determine the activity to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (inputTabControl.TabPages.Count > 3)
            {
                inputTabControl.SelectedTab = inputTabControl.TabPages[3];
            }

            int parentOutcomeIdForActivity = 0;
            Output parentOutput = null;
            foreach (var outcome in _currentProject.Outcomes)
            {
                parentOutput = outcome.Outputs?.FirstOrDefault(o => o.OutputID == activityToEdit.OutputID);
                if (parentOutput != null)
                {
                    parentOutcomeIdForActivity = outcome.OutcomeID;
                    break;
                }
            }

            cmbParentOutcomeForActivity.SelectedValue = parentOutcomeIdForActivity;
            if (parentOutcomeIdForActivity > 0)
            {
                await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForActivity, cmbParentOutputForActivity, parentOutcomeIdForActivity);
            }
            cmbParentOutputForActivity.SelectedValue = activityToEdit.OutputID;

            txtActivityDescription.Text = activityToEdit.ActivityDescription;
            txtActivityPlannedMonths.Text = activityToEdit.PlannedMonths;
            btnAddActivity.Text = "Update Activity";
            btnAddActivity.Tag = activityToEdit.ActivityID;
            txtActivityDescription.Focus();
        }

        private async void DeleteIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is ProjectIndicator indicatorToDelete))
            {
                MessageBox.Show("Could not determine the indicator to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to delete indicator: '{TruncateText(indicatorToDelete.IndicatorName, 50)}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    bool success = await _logFrameService.DeleteProjectIndicatorAsync(indicatorToDelete.ProjectIndicatorID);
                    if (success)
                    {
                        MessageBox.Show("Indicator deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (indicatorToDelete.OutputID.HasValue)
                        {
                            foreach (var outcome in _currentProject.Outcomes)
                            {
                                var parentOutput = outcome.Outputs?.FirstOrDefault(o => o.OutputID == indicatorToDelete.OutputID.Value);
                                parentOutput?.ProjectIndicators?.Remove(indicatorToDelete);
                            }
                        }
                        await LoadOutcomesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete indicator.", "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the indicator: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error DeleteIndicator_Click: {ex.ToString()}");
                }
            }
        }

        private async void EditIndicator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is ProjectIndicator indicatorToEdit))
            {
                MessageBox.Show("Could not determine the indicator to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (inputTabControl.TabPages.Count > 2)
            {
                inputTabControl.SelectedTab = inputTabControl.TabPages[2];
            }

            int parentOutcomeIdForIndicator = 0;
            if (indicatorToEdit.OutputID.HasValue)
            {
                foreach (var outcome in _currentProject.Outcomes)
                {
                    var parentOutput = outcome.Outputs?.FirstOrDefault(o => o.OutputID == indicatorToEdit.OutputID.Value);
                    if (parentOutput != null)
                    {
                        parentOutcomeIdForIndicator = outcome.OutcomeID;
                        break;
                    }
                }
            }

            cmbParentOutcomeForIndicator.SelectedValue = parentOutcomeIdForIndicator;
            if (parentOutcomeIdForIndicator > 0 && indicatorToEdit.OutputID.HasValue)
            {
                await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForIndicator, cmbParentOutputForIndicator, parentOutcomeIdForIndicator);
                cmbParentOutputForIndicator.SelectedValue = indicatorToEdit.OutputID.Value;
            }
            else if (indicatorToEdit.OutputID.HasValue)
            {
                cmbParentOutputForIndicator.SelectedValue = indicatorToEdit.OutputID.Value;
            }
            else
            {
                ClearOutputCombo(cmbParentOutputForIndicator);
            }

            txtIndicatorName.Text = indicatorToEdit.IndicatorName;
            txtIndicatorDescription.Text = indicatorToEdit.Description;
            txtIndicatorTargetValue.Text = indicatorToEdit.TargetValue;
            txtMeansOfVerification.Text = indicatorToEdit.MeansOfVerification;

            numTargetMen.Value = indicatorToEdit.TargetMen;
            numTargetWomen.Value = indicatorToEdit.TargetWomen;
            numTargetBoys.Value = indicatorToEdit.TargetBoys;
            numTargetGirls.Value = indicatorToEdit.TargetGirls;
            numTargetTotal.Value = indicatorToEdit.TargetTotal;

            btnAddIndicator.Text = "Update Indicator";
            btnAddIndicator.Tag = indicatorToEdit.ProjectIndicatorID;
            txtIndicatorName.Focus();
        }

        private void InputTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (btnAddOutcome.Text == "Update Outcome") ResetOutcomeInput();
            if (btnAddOutput.Text == "Update Output") ResetOutputInput();
            if (btnAddActivity.Text == "Update Activity") ResetActivityInput();
            if (btnAddIndicator.Text == "Update Indicator") ResetIndicatorInput();
        }

        private void ResetOutcomeInput()
        {
            txtOutcomeDescription.Clear();
            btnAddOutcome.Text = "Add New Outcome";
            btnAddOutcome.Tag = null;
        }

        private void ResetOutputInput()
        {
            cmbParentOutcomeForOutput.SelectedIndex = (cmbParentOutcomeForOutput.Items.Count > 0) ? 0 : -1;
            txtOutputDescription.Clear();
            btnAddOutput.Text = "Add New Output";
            btnAddOutput.Tag = null;
        }

        private void ResetActivityInput()
        {
            cmbParentOutcomeForActivity.SelectedIndex = (cmbParentOutcomeForActivity.Items.Count > 0) ? 0 : -1;
            ClearOutputCombo(cmbParentOutputForActivity);
            txtActivityDescription.Clear();
            txtActivityPlannedMonths.Clear();
            btnAddActivity.Text = "Add New Activity";
            btnAddActivity.Tag = null;
        }

        private void ResetIndicatorInput()
        {
            cmbParentOutcomeForIndicator.SelectedIndex = (cmbParentOutcomeForIndicator.Items.Count > 0) ? 0 : -1;
            ClearOutputCombo(cmbParentOutputForIndicator);
            txtIndicatorName.Clear();
            txtIndicatorDescription.Clear();
            txtIndicatorTargetValue.Clear();
            txtMeansOfVerification.Clear();
            numTargetMen.Value = 0;
            numTargetWomen.Value = 0;
            numTargetBoys.Value = 0;
            numTargetGirls.Value = 0;
            numTargetTotal.Value = 0;
            btnAddIndicator.Text = "Add New Indicator";
            btnAddIndicator.Tag = null;
        }
    }
}


