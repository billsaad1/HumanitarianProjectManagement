namespace HumanitarianProjectManagement.Forms
{
    partial class DashboardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitoringEvaluationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purchasingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beneficiariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockManagementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.mainMenuStrip.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            //this.modulesToolStripMenuItem, // Removed Modules menu
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(784, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "&Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // modulesToolStripMenuItem
            // 
            //this.modulesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { // Commented out
            //this.projectsToolStripMenuItem, // Commented out
            //this.monitoringEvaluationToolStripMenuItem, // Commented out
            //this.purchasingToolStripMenuItem, // Commented out
            //this.beneficiariesToolStripMenuItem, // Commented out
            //this.stockManagementToolStripMenuItem, // Commented out
            //this.reportsToolStripMenuItem}); // Commented out
            //this.modulesToolStripMenuItem.Name = "modulesToolStripMenuItem"; // Commented out
            //this.modulesToolStripMenuItem.Size = new System.Drawing.Size(65, 20); // Commented out
            //this.modulesToolStripMenuItem.Text = "&Modules"; // Commented out
            // 
            // projectsToolStripMenuItem
            // 
            //this.projectsToolStripMenuItem.Name = "projectsToolStripMenuItem"; // Already Commented out
            //this.projectsToolStripMenuItem.Size = new System.Drawing.Size(201, 22); // Already Commented out
            //this.projectsToolStripMenuItem.Text = "&Projects"; // Already Commented out
            //this.projectsToolStripMenuItem.Click += new System.EventHandler(this.projectsToolStripMenuItem_Click); // Already Commented out
            // 
            // monitoringEvaluationToolStripMenuItem
            // 
            //this.monitoringEvaluationToolStripMenuItem.Name = "monitoringEvaluationToolStripMenuItem"; // Already Commented out
            //this.monitoringEvaluationToolStripMenuItem.Size = new System.Drawing.Size(201, 22); // Already Commented out
            //this.monitoringEvaluationToolStripMenuItem.Text = "&Monitoring & Evaluation"; // Already Commented out
            //this.monitoringEvaluationToolStripMenuItem.Click += new System.EventHandler(this.monitoringEvaluationToolStripMenuItem_Click); // Already Commented out
            // 
            // purchasingToolStripMenuItem
            // 
            //this.purchasingToolStripMenuItem.Name = "purchasingToolStripMenuItem"; // Already Commented out
            //this.purchasingToolStripMenuItem.Size = new System.Drawing.Size(201, 22); // Already Commented out
            //this.purchasingToolStripMenuItem.Text = "P&urchasing"; // Already Commented out
            //this.purchasingToolStripMenuItem.Click += new System.EventHandler(this.purchasingToolStripMenuItem_Click); // Already Commented out
            // 
            // beneficiariesToolStripMenuItem
            // 
            //this.beneficiariesToolStripMenuItem.Name = "beneficiariesToolStripMenuItem"; // Already Commented out
            //this.beneficiariesToolStripMenuItem.Size = new System.Drawing.Size(201, 22); // Already Commented out
            //this.beneficiariesToolStripMenuItem.Text = "&Beneficiaries"; // Already Commented out
            //this.beneficiariesToolStripMenuItem.Click += new System.EventHandler(this.beneficiariesToolStripMenuItem_Click); // Already Commented out
            // 
            // stockManagementToolStripMenuItem
            // 
            //this.stockManagementToolStripMenuItem.Name = "stockManagementToolStripMenuItem"; // Already Commented out
            //this.stockManagementToolStripMenuItem.Size = new System.Drawing.Size(201, 22); // Already Commented out
            //this.stockManagementToolStripMenuItem.Text = "&Stock Management"; // Already Commented out
            //this.stockManagementToolStripMenuItem.Click += new System.EventHandler(this.stockManagementToolStripMenuItem_Click); // Already Commented out
            // 
            // reportsToolStripMenuItem
            // 
            //this.reportsToolStripMenuItem.Name = "reportsToolStripMenuItem"; // Already Commented out
            //this.reportsToolStripMenuItem.Size = new System.Drawing.Size(201, 22); // Already Commented out
            //this.reportsToolStripMenuItem.Text = "&Reports"; // Already Commented out
            //this.reportsToolStripMenuItem.Click += new System.EventHandler(this.reportsToolStripMenuItem_Click); // Already Commented out
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 539);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Size = new System.Drawing.Size(784, 22);
            this.mainStatusStrip.TabIndex = 1;
            this.mainStatusStrip.Text = "statusStrip1";
            // 
            // pnlSidebar
            //
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.tvwSections = new System.Windows.Forms.TreeView();
            this.btnAddSection = new System.Windows.Forms.Button();
            this.pnlSidebar.SuspendLayout();
            //
            // pnlSidebar
            //
            this.flpModuleButtons = new System.Windows.Forms.FlowLayoutPanel(); // Added
            this.pnlSidebar.Controls.Add(this.flpModuleButtons); // Add flp first for fill
            this.pnlSidebar.Controls.Add(this.tvwSections);
            this.pnlSidebar.Controls.Add(this.btnAddSection);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 24); // X, Y (Below menu strip)
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(220, 515); // Width, Height (Full height minus menu & status)
            this.pnlSidebar.TabIndex = 3; // Next available TabIndex
            //
            // tvwSections
            //
            this.tvwSections.Dock = System.Windows.Forms.DockStyle.Top; // Changed from Fill
            this.tvwSections.Location = new System.Drawing.Point(0, 0);
            this.tvwSections.Name = "tvwSections";
            this.tvwSections.Size = new System.Drawing.Size(220, 280); // Explicit height, width matches sidebar
            this.tvwSections.TabIndex = 0;
            //
            // flpModuleButtons
            //
            this.flpModuleButtons.Dock = System.Windows.Forms.DockStyle.Fill; // Fills space between tvwSections and btnAddSection
            this.flpModuleButtons.Location = new System.Drawing.Point(0, 280); // Below tvwSections
            this.flpModuleButtons.Name = "flpModuleButtons";
            this.flpModuleButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpModuleButtons.Size = new System.Drawing.Size(220, 205); // Height = pnlSidebar(515) - tvw(280) - btnAddSection(30) = 205
            this.flpModuleButtons.TabIndex = 1; // After tvwSections
            this.flpModuleButtons.AutoScroll = true; // In case of many buttons
            //
            // btnAddSection
            //
            this.btnAddSection.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnAddSection.Location = new System.Drawing.Point(0, 485);
            this.btnAddSection.Name = "btnAddSection";
            this.btnAddSection.Size = new System.Drawing.Size(220, 30);
            this.btnAddSection.TabIndex = 2; // After flpModuleButtons
            this.btnAddSection.Text = "Add New Section";
            this.btnAddSection.UseVisualStyleBackColor = true;
            //
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.lblWelcome);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(220, 24); // Adjusted X
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(564, 515); // Adjusted Width
            this.pnlMainContent.TabIndex = 2; // TabIndex can remain, order of docking matters more
            // 
            // lblWelcome
            // 
            this.lblWelcome.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWelcome.Location = new System.Drawing.Point(120, 245);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(544, 25);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "Welcome to the Humanitarian Project Management System!";
            this.lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            // Adjusted order and added pnlSidebar
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlMainContent); // pnlMainContent last to correctly fill
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "DashboardForm";
            this.Text = "Dashboard - Humanitarian Project Management System";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DashboardForm_FormClosing);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.pnlMainContent.ResumeLayout(false);
            this.pnlMainContent.PerformLayout();
            this.pnlSidebar.ResumeLayout(false); // Added
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        // private System.Windows.Forms.ToolStripMenuItem modulesToolStripMenuItem; // Fully commented
        // private System.Windows.Forms.ToolStripMenuItem projectsToolStripMenuItem; // Fully commented
        // private System.Windows.Forms.ToolStripMenuItem monitoringEvaluationToolStripMenuItem; // Fully commented
        // private System.Windows.Forms.ToolStripMenuItem purchasingToolStripMenuItem; // Fully commented
        // private System.Windows.Forms.ToolStripMenuItem beneficiariesToolStripMenuItem; // Fully commented
        // private System.Windows.Forms.ToolStripMenuItem stockManagementToolStripMenuItem; // Fully commented
        // private System.Windows.Forms.ToolStripMenuItem reportsToolStripMenuItem; // Fully commented
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.TreeView tvwSections;
        private System.Windows.Forms.Button btnAddSection;
        private System.Windows.Forms.FlowLayoutPanel flpModuleButtons; // Added declaration
    }
}
