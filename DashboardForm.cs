using System;
using System.Windows.Forms;
using HumanitarianProjectManagement.UI;
using HumanitarianProjectManagement.Forms; // Ensured this is present

namespace HumanitarianProjectManagement.Forms
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToForm(this);
            // Consider setting IsMdiContainer = true in the designer or here if you want MDI
            // this.IsMdiContainer = true; 

            // Accessibility Enhancements
            mainMenuStrip.AccessibleName = "Main application menu";
            fileToolStripMenuItem.AccessibleName = "File Menu";
            settingsToolStripMenuItem.AccessibleName = "Application Settings";
            exitToolStripMenuItem.AccessibleName = "Exit Application";
            modulesToolStripMenuItem.AccessibleName = "Modules Menu";
            projectsToolStripMenuItem.AccessibleName = "Projects Module";
            monitoringEvaluationToolStripMenuItem.AccessibleName = "Monitoring and Evaluation Module";
            purchasingToolStripMenuItem.AccessibleName = "Purchasing Module";
            beneficiariesToolStripMenuItem.AccessibleName = "Beneficiaries Module";
            stockManagementToolStripMenuItem.AccessibleName = "Stock Management Module";
            reportsToolStripMenuItem.AccessibleName = "Reports Module";
            helpToolStripMenuItem.AccessibleName = "Help Menu";
            aboutToolStripMenuItem.AccessibleName = "About Application";

            lblWelcome.AccessibleName = "Welcome message"; // Though label text is usually sufficient
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Navigating to Settings section...", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void projectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectListForm projectListForm = new ProjectListForm();
            projectListForm.Show(this); // 'this' makes the DashboardForm the owner
        }

        private void monitoringEvaluationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("Navigating to Monitoring & Evaluation section...", "Monitoring & Evaluation", MessageBoxButtons.OK, MessageBoxIcon.Information); // Removed placeholder
            ProjectListForm projectListForm = new ProjectListForm();
            // Consider adding a title or state to projectListForm to indicate M&E focus, if desired later.
            // For now, simply showing the form is sufficient.
            projectListForm.Show(this); // 'this' makes the DashboardForm the owner
        }

        private void purchasingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PurchaseOrderListForm poListForm = new PurchaseOrderListForm();
            poListForm.Show(this); // 'this' makes the DashboardForm the owner
        }

        private void beneficiariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeneficiaryListManagementForm beneficiaryListForm = new BeneficiaryListManagementForm();
            beneficiaryListForm.Show(this); // 'this' makes the DashboardForm the owner
        }

        private void stockManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StockItemListForm stockItemListForm = new StockItemListForm();
            stockItemListForm.Show(this); // 'this' makes the DashboardForm the owner
        }

        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Navigating to Reports section...", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

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
        /*
        private void OpenFormInPanel(Form formToOpen)
        {
            // Close any existing form in the panel
            if (pnlMainContent.Controls.Count > 0)
            {
                Form currentForm = pnlMainContent.Controls[0] as Form;
                if (currentForm != null)
                {
                    currentForm.Close(); // Or Dispose()
                }
                pnlMainContent.Controls.Clear();
            }

            formToOpen.TopLevel = false;
            formToOpen.FormBorderStyle = FormBorderStyle.None;
            formToOpen.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(formToOpen);
            formToOpen.Show();
        }
        */
    }
}
