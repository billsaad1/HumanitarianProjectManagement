using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HumanitarianProjectManagement.Models; // Assuming this brings in your Activity model
using HumanitarianProjectManagement.DataAccessLayer;
using System.Data.SqlClient;
using System.Diagnostics;

namespace HumanitarianProjectManagement
{
    public partial class LogFrameTabUserControl : UserControl
    {
        private Project _currentProject;
        private LogFrameService _logFrameService;
        private Outcome _selectedOutcome;

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
            _selectedOutcome = null;

            ClearLogFrameDisplay();
            UpdateInputAreaForContext();

            if (_currentProject == null)
            {
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    lblLogFrameDisplayPlaceholder.Text = "No project loaded.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                await LoadOutcomesAsync();
                return;
            }

            await LoadOutcomesAsync();

            if (_selectedOutcome == null && lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
            {
                if (_currentProject.ProjectID > 0)
                {
                    lblLogFrameDisplayPlaceholder.Text = "Logframe loaded. Add or select items.";
                }
                else
                {
                    lblLogFrameDisplayPlaceholder.Text = "Please save the main project details first to enable logframe entries.";
                }
                lblLogFrameDisplayPlaceholder.Visible = true;
            }
        }

        private async Task LoadOutcomesAsync()
        {
            if (_currentProject == null || _currentProject.ProjectID == 0)
            {
                _selectedOutcome = null;
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
            }
            catch (Exception ex) // General catch for now, can be more specific if needed
            {
                MessageBox.Show($"Error loading outcomes for project ID {_currentProject.ProjectID}: {ex.Message}", "Error - LoadOutcomes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error LoadOutcomesAsync: {ex.ToString()}");
                ClearParentComboBoxes();
                UpdateInputAreaForContext();
                return;
            }

            var orderedOutcomes = allOutcomes.OrderBy(o => o.OutcomeDescription).ToList();

            await PopulateParentOutcomeComboBoxesAsync(orderedOutcomes);

            if (orderedOutcomes.Any())
            {
                await RenderOutcomesHierarchically(orderedOutcomes, flpLogFrameDisplay);
                UpdateInputAreaForContext(orderedOutcomes.FirstOrDefault());
            }
            else
            {
                _selectedOutcome = null;
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
            var outcomeItems = outcomes
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
                    else
                    {
                        cmb.SelectedIndex = -1;
                    }
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

            try
            {
                if (cmbParentOutcomeForIndicator.SelectedValue != null && (int)cmbParentOutcomeForIndicator.SelectedValue > 0)
                    await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForIndicator, cmbParentOutputForIndicator);
                else ClearOutputCombo(cmbParentOutputForIndicator, "Select Parent Outcome first");
            }
            catch (Exception ex) { Debug.WriteLine($"Error in PopulateParentOutcomeComboBoxesAsync (Indicator Branch): {ex.ToString()}"); }


            try
            {
                if (cmbParentOutcomeForActivity.SelectedValue != null && (int)cmbParentOutcomeForActivity.SelectedValue > 0)
                    await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForActivity, cmbParentOutputForActivity);
                else ClearOutputCombo(cmbParentOutputForActivity, "Select Parent Outcome first");
            }
            catch (Exception ex) { Debug.WriteLine($"Error in PopulateParentOutcomeComboBoxesAsync (Activity Branch): {ex.ToString()}"); }
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
                catch (Exception ex) // General catch for now
                {
                    MessageBox.Show($"Error loading outputs for outcome ID {parentOutcomeId}: {ex.Message}", "Error - LoadOutputs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error PopulateParentOutputComboBoxesAsync: {ex.ToString()}");
                    ClearOutputCombo(cmbOutputTarget, "Error loading outputs");
                    cmbOutputTarget.ResumeLayout();
                    return;
                }

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
                    else
                    {
                        cmbOutputTarget.SelectedIndex = -1;
                    }
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

            TabPage outcomeTab = new TabPage("Add Outcome");
            FlowLayoutPanel flpOutcomeInput = new FlowLayoutPanel { Name = "flpOutcomeInput", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };
            Label lblOutcomeDesc = new Label { Text = "New Outcome Description:" }; StyleInputLabel(lblOutcomeDesc);
            flpOutcomeInput.Controls.Add(lblOutcomeDesc);
            txtOutcomeDescription = new TextBox(); StyleInputTextBox(txtOutcomeDescription, multiline: true, height: 80, width: 500);
            flpOutcomeInput.Controls.Add(txtOutcomeDescription);
            btnAddOutcome = new Button { Text = "Add New Outcome" }; StyleAddButton(btnAddOutcome);
            btnAddOutcome.Click += BtnAddOutcome_Click;
            flpOutcomeInput.Controls.Add(btnAddOutcome);
            outcomeTab.Controls.Add(flpOutcomeInput);
            inputTabControl.TabPages.Add(outcomeTab);

            TabPage outputTab = new TabPage("Add Output");
            FlowLayoutPanel flpOutputInput = new FlowLayoutPanel { Name = "flpOutputInput", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };
            Label lblParentOutcomeForOutput = new Label { Text = "Parent Outcome:" }; StyleInputLabel(lblParentOutcomeForOutput);
            flpOutputInput.Controls.Add(lblParentOutcomeForOutput);
            cmbParentOutcomeForOutput = new ComboBox(); StyleComboBox(cmbParentOutcomeForOutput);
            flpOutputInput.Controls.Add(cmbParentOutcomeForOutput);
            Label lblOutputDesc = new Label { Text = "New Output Description:" }; StyleInputLabel(lblOutputDesc);
            flpOutputInput.Controls.Add(lblOutputDesc);
            txtOutputDescription = new TextBox(); StyleInputTextBox(txtOutputDescription, multiline: true, height: 80, width: 500);
            flpOutputInput.Controls.Add(txtOutputDescription);
            btnAddOutput = new Button { Text = "Add New Output" }; StyleAddButton(btnAddOutput);
            btnAddOutput.Click += BtnAddOutput_Click;
            flpOutputInput.Controls.Add(btnAddOutput);
            outputTab.Controls.Add(flpOutputInput);
            inputTabControl.TabPages.Add(outputTab);

            TabPage indicatorTab = new TabPage("Add Indicator");
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

            Label lblIndicatorName = new Label { Text = "New Indicator Name:" }; StyleInputLabel(lblIndicatorName);
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

            GroupBox gbDemographics = new GroupBox
            {
                Text = "Demographic Targets",
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 5, 10, 10),
                Margin = new Padding(0, 0, 0, 10),
                MinimumSize = new Size(540, 0)
            };
            TableLayoutPanel tlpDemographicsInd = new TableLayoutPanel
            {
                ColumnCount = 5,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 5),
                MinimumSize = new Size(530, 0)
            };
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
            btnAddIndicator.Click += BtnAddIndicator_Click; // Crucial: Ensure event handler is wired
            flpIndicatorInput.Controls.Add(btnAddIndicator);
            indicatorTab.Controls.Add(flpIndicatorInput);
            inputTabControl.TabPages.Add(indicatorTab);

            TabPage activityTab = new TabPage("Add Activity");
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

            Label lblActivityDesc = new Label { Text = "New Activity Description:" }; StyleInputLabel(lblActivityDesc);
            flpActivityInput.Controls.Add(lblActivityDesc);
            txtActivityDescription = new TextBox(); StyleInputTextBox(txtActivityDescription, multiline: true, height: 80, width: 500);
            flpActivityInput.Controls.Add(txtActivityDescription);
            Label lblActivityMonths = new Label { Text = "Planned Months (e.g., Jan,Feb,Mar):" }; StyleInputLabel(lblActivityMonths);
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

            if (!projectIsSavedAndValid)
            {
                foreach (TabPage tab in inputTabControl.TabPages) { tab.Enabled = false; }
                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed)
                {
                    ClearLogFrameDisplay();
                    lblLogFrameDisplayPlaceholder.Text = "Please save the main project details first to enable logframe entries.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                if (inputTabControl.TabPages.Count > 0) inputTabControl.SelectedIndex = 0;
            }
            else
            {
                inputTabControl.TabPages[0].Enabled = true; // Add Outcome

                bool hasOutcomes = (cmbParentOutcomeForOutput?.Items.Count > 0 &&
                                    !((cmbParentOutcomeForOutput.Items[0] is ComboboxItem item && item.Value == 0 && cmbParentOutcomeForOutput.Items.Count == 1)));

                inputTabControl.TabPages[1].Enabled = hasOutcomes; // Add Output
                inputTabControl.TabPages[2].Enabled = hasOutcomes; // Add Indicator 
                inputTabControl.TabPages[3].Enabled = hasOutcomes; // Add Activity 

                if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed &&
                    lblLogFrameDisplayPlaceholder.Text == "Please save the main project details first to enable logframe entries.")
                {
                    if (!hasOutcomes)
                    {
                        lblLogFrameDisplayPlaceholder.Text = "No outcomes exist. Please add one.";
                        lblLogFrameDisplayPlaceholder.Visible = true;
                    }
                    else
                    {
                        lblLogFrameDisplayPlaceholder.Text = "Outcomes loaded. Select parent items from dropdowns to add children.";
                        lblLogFrameDisplayPlaceholder.Visible = true;
                    }
                }
                else if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed && !hasOutcomes && projectIsSavedAndValid)
                {
                    lblLogFrameDisplayPlaceholder.Text = "No outcomes exist. Please add one.";
                    lblLogFrameDisplayPlaceholder.Visible = true;
                }
                else if (lblLogFrameDisplayPlaceholder != null && !lblLogFrameDisplayPlaceholder.IsDisposed && hasOutcomes && outcomeForDisplayContext == null)
                {
                    if (flpLogFrameDisplay.Controls.Count <= 1)
                    {
                        lblLogFrameDisplayPlaceholder.Text = "Logframe items will appear here.";
                        lblLogFrameDisplayPlaceholder.Visible = true;
                    }
                    else
                    {
                        lblLogFrameDisplayPlaceholder.Visible = false;
                    }
                }

                if (inputTabControl.TabPages.Count > 0)
                {
                    if (inputTabControl.SelectedTab != null && !inputTabControl.SelectedTab.Enabled)
                    {
                        inputTabControl.SelectedTab = inputTabControl.TabPages[0];
                    }
                    else if (hasOutcomes && inputTabControl.SelectedTab == inputTabControl.TabPages[0])
                    {
                        if (inputTabControl.TabPages.Count > 1 && inputTabControl.TabPages[1].Enabled)
                            inputTabControl.SelectedTab = inputTabControl.TabPages[1];
                    }
                    else if (!hasOutcomes && inputTabControl.SelectedTab != inputTabControl.TabPages[0])
                    {
                        inputTabControl.SelectedTab = inputTabControl.TabPages[0];
                    }
                }
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
            Outcome newOutcome = new Outcome { ProjectID = _currentProject.ProjectID, OutcomeDescription = txtOutcomeDescription.Text.Trim() };
            int newOutcomeId = await _logFrameService.AddOutcomeAsync(newOutcome);
            if (newOutcomeId > 0)
            {
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

            Output newOutput = new Output
            {
                OutcomeID = parentOutcomeId,
                OutputDescription = txtOutputDescription.Text.Trim()
            };

            int newOutputId = await _logFrameService.AddOutputAsync(newOutput);
            if (newOutputId > 0)
            {
                MessageBox.Show("Output added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtOutputDescription.Clear();

                if (cmbParentOutcomeForIndicator.SelectedValue as int? == parentOutcomeId)
                    await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForIndicator, cmbParentOutputForIndicator);
                if (cmbParentOutcomeForActivity.SelectedValue as int? == parentOutcomeId)
                    await PopulateParentOutputComboBoxesAsync(cmbParentOutcomeForActivity, cmbParentOutputForActivity);

                await RenderOutcomesHierarchically(
                    (await _logFrameService.GetOutcomesByProjectIdAsync(_currentProject.ProjectID)).OrderBy(o => o.OutcomeDescription).ToList(),
                    flpLogFrameDisplay);

                if (inputTabControl.TabPages.Count > 2 && inputTabControl.TabPages[2].Enabled)
                {
                    inputTabControl.SelectedTab = inputTabControl.TabPages[2];
                    cmbParentOutcomeForIndicator?.Focus();
                }
            }
            else
            {
                MessageBox.Show("Failed to add output.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Use the alias for your Activity model
            Models.Activity newActivity = new Models.Activity
            {
                OutputID = parentOutputId,
                ActivityDescription = txtActivityDescription.Text.Trim(),
                PlannedMonths = txtActivityPlannedMonths.Text.Trim()
            };

            int newActivityId = await _logFrameService.AddActivityAsync(newActivity);
            if (newActivityId > 0)
            {
                MessageBox.Show("Activity added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtActivityDescription.Clear();
                txtActivityPlannedMonths.Clear();
                await RenderOutcomesHierarchically(
                    (await _logFrameService.GetOutcomesByProjectIdAsync(_currentProject.ProjectID)).OrderBy(o => o.OutcomeDescription).ToList(),
                    flpLogFrameDisplay);
                cmbParentOutputForActivity?.Focus();
            }
            else
            {
                MessageBox.Show("Failed to add activity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            ProjectIndicator newIndicator = new ProjectIndicator
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

            int newIndicatorId = await _logFrameService.AddProjectIndicatorToOutputAsync(newIndicator);

            if (newIndicatorId > 0)
            {
                MessageBox.Show("Indicator added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtIndicatorName.Clear();
                txtIndicatorDescription.Clear();
                txtIndicatorTargetValue.Clear();
                txtMeansOfVerification.Clear();
                numTargetMen.Value = 0; numTargetWomen.Value = 0; numTargetBoys.Value = 0; numTargetGirls.Value = 0; numTargetTotal.Value = 0;
                await RenderOutcomesHierarchically(
                    (await _logFrameService.GetOutcomesByProjectIdAsync(_currentProject.ProjectID)).OrderBy(o => o.OutcomeDescription).ToList(),
                    flpLogFrameDisplay);
                cmbParentOutputForIndicator?.Focus();
            }
            else
            {
                MessageBox.Show("Failed to add indicator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            foreach (var outcome in outcomes)
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

            AddDisplayControl(displayPanel, "Outcome", outcome.OutcomeDescription, 0, FontStyle.Bold, 12.5F, Color.DarkBlue, itemNumberPrefix: outcomeNumberPrefix);

            var outputs = await _logFrameService.GetOutputsByOutcomeIdAsync(outcome.OutcomeID);
            int outputCounter = 1;
            foreach (var output in outputs.OrderBy(o => o.OutputDescription))
            {
                string outputNumber = $"{outcomeNumberPrefix}{outputCounter}.";
                AddDisplayControl(displayPanel, "Output", output.OutputDescription, 1, FontStyle.Bold, 11F, Color.DarkGreen, itemNumberPrefix: outputNumber);

                var indicators = await _logFrameService.GetProjectIndicatorsByOutputIdAsync(output.OutputID);
                int indicatorCounter = 1;
                if (indicators.Any())
                {
                    foreach (var indicator in indicators.OrderBy(i => i.IndicatorName))
                    {
                        string indicatorNumber = $"{outputNumber}{indicatorCounter}.";
                        AddDisplayControl(displayPanel, "Indicator", indicator.IndicatorName, 2, FontStyle.Regular, 10F, Color.Black, itemNumberPrefix: indicatorNumber);
                        string details = $"Target: {indicator.TargetValue ?? "N/A"}, Means of Verification: {indicator.MeansOfVerification ?? "N/A"}";
                        if (indicator.TargetMen > 0 || indicator.TargetWomen > 0 || indicator.TargetBoys > 0 || indicator.TargetGirls > 0 || indicator.TargetTotal > 0)
                        {
                            details += $"\nDemographics: Men({indicator.TargetMen}), Women({indicator.TargetWomen}), Boys({indicator.TargetBoys}), Girls({indicator.TargetGirls}), Total({indicator.TargetTotal})";
                        }
                        AddDisplayControl(displayPanel, "", details, 3, FontStyle.Italic, 8.5F, Color.DarkSlateGray, false);
                        indicatorCounter++;
                    }
                }
                else
                {
                    AddDisplayControl(displayPanel, "", "No indicators for this output.", 2, FontStyle.Italic, 9F, Color.Gray, itemNumberPrefix: $"{outputNumber}  ", isHeader: false);
                }

                var activities = await _logFrameService.GetActivitiesByOutputIdAsync(output.OutputID);
                int activityCounter = 1;
                if (activities.Any())
                {
                    foreach (var activity in activities.OrderBy(a => a.ActivityDescription))
                    {
                        string activityNumber = $"{outputNumber}{((char)('a' + activityCounter - 1))}.";
                        AddDisplayControl(displayPanel, "Activity", activity.ActivityDescription, 2, FontStyle.Regular, 10F, Color.Black, itemNumberPrefix: activityNumber);
                        if (!string.IsNullOrWhiteSpace(activity.PlannedMonths))
                        {
                            AddDisplayControl(displayPanel, "", $"Planned Months: {activity.PlannedMonths}", 3, FontStyle.Italic, 8.5F, Color.DarkSlateGray, false);
                        }
                        activityCounter++;
                    }
                }
                else
                {
                    AddDisplayControl(displayPanel, "", "No activities for this output.", 2, FontStyle.Italic, 9F, Color.Gray, itemNumberPrefix: $"{outputNumber}  ", isHeader: false);
                }
                outputCounter++;
            }
        }

        private void AddDisplayControl(FlowLayoutPanel panel, string itemTypeTitle, string text, int indentLevel, FontStyle fontStyle, float fontSize, Color foreColor, bool isHeader = true, string itemNumberPrefix = "")
        {
            var label = new Label
            {
                Text = $"{itemNumberPrefix}{(string.IsNullOrEmpty(itemTypeTitle) ? "" : " " + itemTypeTitle + ": ")}{text}",
                Font = new Font("Segoe UI", fontSize, fontStyle),
                ForeColor = foreColor,
                AutoSize = true,
                Margin = new Padding(20 * indentLevel + (isHeader ? 5 : 10), isHeader ? 8 : 2, 5, isHeader ? 2 : 2),
                MaximumSize = new Size(panel.ClientSize.Width > 50 ? panel.ClientSize.Width - (20 * indentLevel) - 40 : 300, 0),
                Padding = new Padding(3)
            };
            if (isHeader && indentLevel < 2)
            {
                label.Margin = new Padding(label.Margin.Left, label.Margin.Top, label.Margin.Right, label.Margin.Bottom + (indentLevel == 0 ? 6 : 4));
            }
            panel.Controls.Add(label);

            if (isHeader && indentLevel < 2)
            {
                var separator = new Panel
                {
                    Height = 1,
                    Width = Math.Max(300, panel.ClientSize.Width - (20 * indentLevel) - (panel.Padding.Horizontal + 20)),
                    BackColor = Color.DarkGray,
                    Margin = new Padding(20 * indentLevel + 5, 5, 5, (indentLevel == 0 ? 10 : 5))
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

        private void ClearInputArea() { /* Not actively used */ }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }
    }
}
