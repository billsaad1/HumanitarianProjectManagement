using System;
using System.Windows.Forms;
using HumanitarianProjectManagement.UI;
using HumanitarianProjectManagement.Forms; // Ensured this is present
using HumanitarianProjectManagement.Models; // Added directive
using System.Collections.Generic; // Added for List<T>

using HumanitarianProjectManagement.DataAccessLayer; // For services
using System.Threading.Tasks; // For Task
using Microsoft.VisualBasic; // For InputBox

namespace HumanitarianProjectManagement.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly SectionService _sectionService;
        private readonly ProjectService _projectService;

        public DashboardForm()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToForm(this);
            this.pnlSidebar.BackColor = ThemeManager.PanelBackgroundColor; // Theme for sidebar

            _sectionService = new SectionService();
            _projectService = new ProjectService();

            // Wire up event handlers
            this.Load += DashboardForm_Load; // For loading sections
            // this.btnAddSection.Click += new System.EventHandler(this.btnAddSection_Click); // REMOVE - Moved to Designer
            // this.tvwSections.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwSections_AfterSelect); // REMOVE - Moved to Designer

            // Consider setting IsMdiContainer = true in the designer or here if you want MDI
            // this.IsMdiContainer = true; 

            // Accessibility Enhancements
            mainMenuStrip.AccessibleName = "Main application menu";
            fileToolStripMenuItem.AccessibleName = "File Menu";
            settingsToolStripMenuItem.AccessibleName = "Application Settings";
            exitToolStripMenuItem.AccessibleName = "Exit Application";
            helpToolStripMenuItem.AccessibleName = "Help Menu";
            aboutToolStripMenuItem.AccessibleName = "About Application";

            lblWelcome.AccessibleName = "Welcome message";
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Navigating to Settings section...", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Removed projectsToolStripMenuItem_Click
        // Removed monitoringEvaluationToolStripMenuItem_Click
        // Removed purchasingToolStripMenuItem_Click
        // Removed beneficiariesToolStripMenuItem_Click
        // Removed stockManagementToolStripMenuItem_Click
        // Removed reportsToolStripMenuItem_Click

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Humanitarian Project Management System\nVersion 1.0\n\nDeveloped by [Your Name/Organization]", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If this form is closed, the application should exit.
            // This is important if the LoginForm was hidden and not closed.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Ensure all MDI children are closed or handle them appropriately
                if (this.IsMdiContainer)
                {
                    foreach (Form mdiChild in this.MdiChildren)
                    {
                        mdiChild.Close(); // Or ask user to save, etc.
                    }
                }
                Application.Exit();
            }
        }

        // Optional: Helper method if you want to load forms into pnlMainContent
        // (pnlMainContent should be made public or internal for this to work directly)
        // Implemented OpenFormInPanel
        private void OpenFormInPanel(Form formToOpen)
        {
            // Close any existing form in the panel
            if (pnlMainContent.Controls.Count > 0)
            {
                foreach(Control ctrl in pnlMainContent.Controls)
                {
                    if (ctrl is Form currentForm)
                    {
                         currentForm.Close(); // Or Dispose()
                    }
                }
                pnlMainContent.Controls.Clear(); // Clear all controls
            }

            formToOpen.TopLevel = false;
            formToOpen.FormBorderStyle = FormBorderStyle.None;
            formToOpen.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(formToOpen);
            formToOpen.Tag = "DynamicForm"; // Optional tag to identify it later
            formToOpen.Show();
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            await LoadSectionsTreeViewAsync();
            AddOtherModuleButtons(); // Add buttons for M&E, Purchasing etc.
        }

        private async Task LoadSectionsTreeViewAsync()
        {
            tvwSections.Nodes.Clear();
            TreeNode sectionsRootNode = new TreeNode("Sections");
            tvwSections.Nodes.Add(sectionsRootNode);

            try
            {
                var sections = await _sectionService.GetSectionsAsync();
                if (sections != null)
                {
                    foreach (var section in sections)
                    {
                        TreeNode sectionNode = new TreeNode(section.SectionName);
                        sectionNode.Tag = section.SectionID; // Store SectionID
                        sectionsRootNode.Nodes.Add(sectionNode);
                    }
                }
                sectionsRootNode.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sections: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnAddSection_Click(object sender, EventArgs e)
        {
            string sectionName = Interaction.InputBox("Enter the name for the new section:", "Add New Section", "");
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                string sectionDescription = Interaction.InputBox("Enter the description for the new section (optional):", "Add Section Description", "");

                Section newSection = new Section
                {
                    SectionName = sectionName,
                    Description = sectionDescription
                };

                try
                {
                    int newSectionId = await _sectionService.AddSectionAsync(newSection);
                    if (newSectionId > 0)
                    {
                        MessageBox.Show("Section added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSectionsTreeViewAsync(); // Refresh TreeView
                    }
                    else
                    {
                        MessageBox.Show("Failed to add section.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding section: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tvwSections_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && e.Node.Parent == tvwSections.Nodes[0]) // Check if it's a section node under the root
            {
                if (int.TryParse(e.Node.Tag.ToString(), out int sectionId))
                {
                    // Pass the sectionId to the ProjectListForm constructor
                    ProjectListForm projectListForm = new ProjectListForm(sectionId);
                    OpenFormInPanel(projectListForm);
                }
            }
        }

        private void AddOtherModuleButtons()
        {
            // Clear existing buttons if any (e.g., if called multiple times, though not expected here)
            // flpModuleButtons.Controls.Clear();

            var moduleButtonProperties = new[]
            {
                new { Text = "Projects (All)", FormType = typeof(ProjectListForm) },
                new { Text = "Monitoring & Evaluation", FormType = typeof(ProjectListForm) }, // Assuming ProjectListForm or a specific M&E form
                new { Text = "Purchasing", FormType = typeof(PurchaseOrderListForm) },
                new { Text = "Beneficiaries", FormType = typeof(BeneficiaryListManagementForm) },
                new { Text = "Stock Management", FormType = typeof(StockItemListForm) }
                // Reports can be added if it has a dedicated form. Note: ReportsToolStripMenuItem_Click was simple MessageBox.
            };

            foreach (var props in moduleButtonProperties)
            {
                Button btnModule = new Button();
                btnModule.Text = props.Text;
                btnModule.Height = 30; // Standard height
                btnModule.Width = flpModuleButtons.ClientSize.Width - flpModuleButtons.Padding.Horizontal; // Fill width
                btnModule.Margin = new Padding(3); // Add some margin

                // Apply theme to button
                btnModule.BackColor = ThemeManager.ButtonBackgroundColor;
                btnModule.ForeColor = ThemeManager.ButtonForegroundColor;
                btnModule.FlatStyle = FlatStyle.System; // Or Flat for more custom look

                btnModule.Click += (sender, e) => {
                    Form formToOpen = (Form)Activator.CreateInstance(props.FormType);
                    OpenFormInPanel(formToOpen);
                };
                flpModuleButtons.Controls.Add(btnModule);
            }
        }

    }
}
