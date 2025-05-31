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

        private async void ProjectCreateEditForm_Load(object sender, EventArgs e)
        {
            await LoadComboBoxesAsync();
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
