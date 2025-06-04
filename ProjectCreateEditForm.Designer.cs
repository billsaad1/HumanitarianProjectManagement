namespace HumanitarianProjectManagement.Forms
{
    partial class ProjectCreateEditForm
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
            this.components = new System.ComponentModel.Container(); // Added for ToolTip
            this.mainToolTip = new System.Windows.Forms.ToolTip(this.components); // Added for ToolTip

            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageProjectDetails = new System.Windows.Forms.TabPage();
            this.tlpProjectDetails = new System.Windows.Forms.TableLayoutPanel();
            this.lblProjectName = new System.Windows.Forms.Label();
            this.txtProjectName = new HumanitarianProjectManagement.Forms.ProjectCreateEditForm.PlaceholderTextBox();
            this.lblProjectCode = new System.Windows.Forms.Label();
            this.txtProjectCode = new HumanitarianProjectManagement.Forms.ProjectCreateEditForm.PlaceholderTextBox();
            this.lblStartDate = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.lblLocation = new System.Windows.Forms.Label();
            this.txtLocation = new HumanitarianProjectManagement.Forms.ProjectCreateEditForm.PlaceholderTextBox();
            this.lblOverallObjective = new System.Windows.Forms.Label();
            this.txtOverallObjective = new HumanitarianProjectManagement.Forms.ProjectCreateEditForm.PlaceholderTextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtStatus = new HumanitarianProjectManagement.Forms.ProjectCreateEditForm.PlaceholderTextBox();
            this.lblDonor = new System.Windows.Forms.Label();
            this.txtDonor = new HumanitarianProjectManagement.Forms.ProjectCreateEditForm.PlaceholderTextBox();
            this.lblTotalBudget = new System.Windows.Forms.Label();
            this.nudTotalBudget = new System.Windows.Forms.NumericUpDown();
            this.lblSection = new System.Windows.Forms.Label();
            this.cmbSection = new System.Windows.Forms.ComboBox();
            this.lblManager = new System.Windows.Forms.Label();
            this.cmbManager = new System.Windows.Forms.ComboBox();

            this.tabPageLogFrame = new System.Windows.Forms.TabPage();
            this.pnlLogFrameBase = new System.Windows.Forms.Panel();
            this.btnAddOutcome = new System.Windows.Forms.Button();
            this.pnlLogFrameMain = new System.Windows.Forms.Panel();

            this.tabPageBudget = new System.Windows.Forms.TabPage();
            // REMOVED: this.pnlBudgetBase = new System.Windows.Forms.Panel();
            // REMOVED: this.lblBudgetPlaceholder = new System.Windows.Forms.Label();
            this.pnlBudgetScrollable = new System.Windows.Forms.Panel();

            this.flpCatAHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatA = new System.Windows.Forms.Button();
            this.lblBudgetCatAHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatAContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatA = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatA = new System.Windows.Forms.Button();

            this.flpCatBHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatB = new System.Windows.Forms.Button();
            this.lblBudgetCatBHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatBContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatB = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatB = new System.Windows.Forms.Button();

            this.flpCatCHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatC = new System.Windows.Forms.Button();
            this.lblBudgetCatCHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatCContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatC = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatC = new System.Windows.Forms.Button();

            this.flpCatDHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatD = new System.Windows.Forms.Button();
            this.lblBudgetCatDHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatDContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatD = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatD = new System.Windows.Forms.Button();

            this.flpCatEHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatE = new System.Windows.Forms.Button();
            this.lblBudgetCatEHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatEContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatE = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatE = new System.Windows.Forms.Button();

            this.flpCatFHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatF = new System.Windows.Forms.Button();
            this.lblBudgetCatFHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatFContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatF = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatF = new System.Windows.Forms.Button();

            this.flpCatGHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleCatG = new System.Windows.Forms.Button();
            this.lblBudgetCatGHeader = new System.Windows.Forms.Label();
            this.pnlBudgetCatGContent = new System.Windows.Forms.Panel();
            this.dgvBudgetCatG = new System.Windows.Forms.DataGridView();
            this.btnAddLineCatG = new System.Windows.Forms.Button();

            this.dgvActivityPlan = new System.Windows.Forms.DataGridView(); // Added for Plan Tab


            this.tabPagePlan = new System.Windows.Forms.TabPage();
            // REMOVED: this.pnlPlanBase = new System.Windows.Forms.Panel();
            // REMOVED: this.lblPlanPlaceholder = new System.Windows.Forms.Label();

            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            this.tabControlMain.SuspendLayout();
            this.tabPageProjectDetails.SuspendLayout();
            this.tlpProjectDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalBudget)).BeginInit();

            this.tabPageLogFrame.SuspendLayout();
            this.pnlLogFrameBase.SuspendLayout();

            this.tabPageBudget.SuspendLayout();
            // REMOVED: this.pnlBudgetBase.SuspendLayout();
            this.pnlBudgetScrollable.SuspendLayout(); // Added
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatG)).BeginInit();

            this.tabPagePlan.SuspendLayout();
            // REMOVED: this.pnlPlanBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActivityPlan)).BeginInit(); // Added

            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            //
            // tabControlMain
            //
            this.tabControlMain.Controls.Add(this.tabPageProjectDetails);
            this.tabControlMain.Controls.Add(this.tabPageLogFrame);
            this.tabControlMain.Controls.Add(this.tabPageBudget);
            this.tabControlMain.Controls.Add(this.tabPagePlan);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1184, 711);
            this.tabControlMain.TabIndex = 0;

            //
            // tabPageProjectDetails
            //
            this.tabPageProjectDetails.Controls.Add(this.tlpProjectDetails);
            this.tabPageProjectDetails.Location = new System.Drawing.Point(4, 22);
            this.tabPageProjectDetails.Name = "tabPageProjectDetails";
            this.tabPageProjectDetails.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageProjectDetails.Size = new System.Drawing.Size(1176, 685);
            this.tabPageProjectDetails.TabIndex = 0;
            this.tabPageProjectDetails.Text = "Project Details";
            this.tabPageProjectDetails.UseVisualStyleBackColor = true;

            //
            // tlpProjectDetails
            //
            this.tlpProjectDetails.ColumnCount = 2;
            this.tlpProjectDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpProjectDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpProjectDetails.Controls.Add(this.lblProjectName, 0, 0);
            this.tlpProjectDetails.Controls.Add(this.txtProjectName, 1, 0);
            this.tlpProjectDetails.Controls.Add(this.lblProjectCode, 0, 1);
            this.tlpProjectDetails.Controls.Add(this.txtProjectCode, 1, 1);
            this.tlpProjectDetails.Controls.Add(this.lblStartDate, 0, 2);
            this.tlpProjectDetails.Controls.Add(this.dtpStartDate, 1, 2);
            this.tlpProjectDetails.Controls.Add(this.lblEndDate, 0, 3);
            this.tlpProjectDetails.Controls.Add(this.dtpEndDate, 1, 3);
            this.tlpProjectDetails.Controls.Add(this.lblLocation, 0, 4);
            this.tlpProjectDetails.Controls.Add(this.txtLocation, 1, 4);
            this.tlpProjectDetails.Controls.Add(this.lblOverallObjective, 0, 5);
            this.tlpProjectDetails.Controls.Add(this.txtOverallObjective, 1, 5);
            this.tlpProjectDetails.Controls.Add(this.lblStatus, 0, 6);
            this.tlpProjectDetails.Controls.Add(this.txtStatus, 1, 6);
            this.tlpProjectDetails.Controls.Add(this.lblDonor, 0, 7);
            this.tlpProjectDetails.Controls.Add(this.txtDonor, 1, 7);
            this.tlpProjectDetails.Controls.Add(this.lblTotalBudget, 0, 8);
            this.tlpProjectDetails.Controls.Add(this.nudTotalBudget, 1, 8);
            this.tlpProjectDetails.Controls.Add(this.lblSection, 0, 9);
            this.tlpProjectDetails.Controls.Add(this.cmbSection, 1, 9);
            this.tlpProjectDetails.Controls.Add(this.lblManager, 0, 10);
            this.tlpProjectDetails.Controls.Add(this.cmbManager, 1, 10);
            this.tlpProjectDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpProjectDetails.Location = new System.Drawing.Point(10, 10);
            this.tlpProjectDetails.Name = "tlpProjectDetails";
            this.tlpProjectDetails.RowCount = 12;
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpProjectDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpProjectDetails.Size = new System.Drawing.Size(1156, 665);
            this.tlpProjectDetails.TabIndex = 0;

            this.lblProjectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProjectName.AutoSize = true;
            this.lblProjectName.Text = "Project Name:";
            this.txtProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectName.Size = new System.Drawing.Size(1030, 20);
            this.txtProjectName.PlaceholderText = "Enter full official project name";
            this.lblProjectCode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProjectCode.AutoSize = true;
            this.lblProjectCode.Text = "Project Code:";
            this.txtProjectCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectCode.Size = new System.Drawing.Size(1030, 20);
            this.txtProjectCode.PlaceholderText = "Enter unique project code (optional)";
            this.lblStartDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStartDate.AutoSize = true;
            this.lblStartDate.Text = "Start Date:";
            this.dtpStartDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dtpStartDate.ShowCheckBox = true; this.dtpStartDate.Checked = false; this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short; this.dtpStartDate.Size = new System.Drawing.Size(130, 20);
            this.lblEndDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Text = "End Date:";
            this.dtpEndDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dtpEndDate.ShowCheckBox = true; this.dtpEndDate.Checked = false; this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short; this.dtpEndDate.Size = new System.Drawing.Size(130, 20);
            this.lblLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
            this.lblLocation.AutoSize = true; this.lblLocation.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.lblLocation.Text = "Location(s):";
            this.txtLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLocation.Multiline = true; this.txtLocation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical; this.txtLocation.Size = new System.Drawing.Size(1030, 59);
            this.txtLocation.PlaceholderText = "Enter geographical location(s) of the project";
            this.lblOverallObjective.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
            this.lblOverallObjective.AutoSize = true; this.lblOverallObjective.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.lblOverallObjective.Text = "Overall Objective:";
            this.txtOverallObjective.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOverallObjective.Multiline = true; this.txtOverallObjective.ScrollBars = System.Windows.Forms.ScrollBars.Vertical; this.txtOverallObjective.Size = new System.Drawing.Size(1030, 59);
            this.txtOverallObjective.PlaceholderText = "Describe the main goal or overall objective";
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Text = "Status:";
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.Size = new System.Drawing.Size(1030, 20);
            this.txtStatus.PlaceholderText = "e.g., Planning, Active, Completed";
            this.lblDonor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDonor.AutoSize = true;
            this.lblDonor.Text = "Donor:";
            this.txtDonor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDonor.Size = new System.Drawing.Size(1030, 20);
            this.txtDonor.PlaceholderText = "Enter the name of the donor or funding source";
            this.lblTotalBudget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTotalBudget.AutoSize = true;
            this.lblTotalBudget.Text = "Total Budget:";
            this.nudTotalBudget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudTotalBudget.DecimalPlaces = 2; this.nudTotalBudget.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 }); this.nudTotalBudget.Size = new System.Drawing.Size(150, 20);
            this.lblSection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSection.AutoSize = true;
            this.lblSection.Text = "Section:";
            this.cmbSection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList; this.cmbSection.FormattingEnabled = true; this.cmbSection.Size = new System.Drawing.Size(1030, 21);
            this.lblManager.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblManager.AutoSize = true;
            this.lblManager.Text = "Manager:";
            this.cmbManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbManager.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList; this.cmbManager.FormattingEnabled = true; this.cmbManager.Size = new System.Drawing.Size(1030, 21);

            //
            // tabPageLogFrame
            //
            this.tabPageLogFrame.Controls.Add(this.pnlLogFrameBase);
            this.tabPageLogFrame.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogFrame.Name = "tabPageLogFrame";
            this.tabPageLogFrame.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogFrame.Size = new System.Drawing.Size(1176, 685);
            this.tabPageLogFrame.TabIndex = 1;
            this.tabPageLogFrame.Text = "Logical Framework";
            this.tabPageLogFrame.UseVisualStyleBackColor = true;
            //
            // pnlLogFrameBase
            //
            this.pnlLogFrameBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogFrameBase.Controls.Add(this.pnlLogFrameMain);
            this.pnlLogFrameBase.Controls.Add(this.btnAddOutcome);
            this.pnlLogFrameBase.Name = "pnlLogFrameBase";
            //
            // btnAddOutcome
            //
            this.btnAddOutcome.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAddOutcome.Location = new System.Drawing.Point(10, 10);
            this.btnAddOutcome.Margin = new System.Windows.Forms.Padding(10);
            this.btnAddOutcome.Name = "btnAddOutcome";
            this.btnAddOutcome.Size = new System.Drawing.Size(1156, 23);
            this.btnAddOutcome.TabIndex = 0;
            this.btnAddOutcome.Text = "Add New Outcome";
            this.btnAddOutcome.UseVisualStyleBackColor = true;
            //
            // pnlLogFrameMain
            //
            this.pnlLogFrameMain.AutoScroll = true;
            this.pnlLogFrameMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogFrameMain.Location = new System.Drawing.Point(10, 43);
            this.pnlLogFrameMain.Name = "pnlLogFrameMain";
            this.pnlLogFrameMain.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlLogFrameMain.Size = new System.Drawing.Size(1156, 632);
            this.pnlLogFrameMain.TabIndex = 1;
            this.pnlLogFrameMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            //
            // tabPageBudget
            //
            // REMOVED: this.tabPageBudget.Controls.Add(this.pnlBudgetBase);
            // REMOVED: this.tabPageBudget.Controls.Remove(this.lblBudgetPlaceholder);
            this.tabPageBudget.Controls.Add(this.pnlBudgetScrollable);
            this.tabPageBudget.Location = new System.Drawing.Point(4, 22);
            this.tabPageBudget.Name = "tabPageBudget";
            this.tabPageBudget.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageBudget.Size = new System.Drawing.Size(1176, 685);
            this.tabPageBudget.TabIndex = 2;
            this.tabPageBudget.Text = "Budget";
            this.tabPageBudget.UseVisualStyleBackColor = true;
            //
            // pnlBudgetScrollable
            //
            this.pnlBudgetScrollable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBudgetScrollable.AutoScroll = true;
            this.pnlBudgetScrollable.Name = "pnlBudgetScrollable";
            this.pnlBudgetScrollable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatGContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatGHeader);
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatFContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatFHeader);
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatEContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatEHeader);
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatDContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatDHeader);
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatCContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatCHeader);
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatBContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatBHeader);
            this.pnlBudgetScrollable.Controls.Add(this.pnlBudgetCatAContent);
            this.pnlBudgetScrollable.Controls.Add(this.flpCatAHeader);

            // flpCatAHeader
            this.flpCatAHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpCatAHeader.AutoSize = true;
            this.flpCatAHeader.WrapContents = false;
            this.flpCatAHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.flpCatAHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatAHeader.Controls.Add(this.btnToggleCatA);
            this.flpCatAHeader.Controls.Add(this.lblBudgetCatAHeader);
            // btnToggleCatA
            this.btnToggleCatA.Text = "-";
            this.btnToggleCatA.Size = new System.Drawing.Size(23, 23);
            this.btnToggleCatA.Name = "btnToggleCatA";
            this.btnToggleCatA.Tag = "A";
            // lblBudgetCatAHeader
            this.lblBudgetCatAHeader.Text = "A. Staff and Other Personnel Costs (Salaries)";
            this.lblBudgetCatAHeader.AutoSize = true;
            this.lblBudgetCatAHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBudgetCatAHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            // pnlBudgetCatAContent
            this.pnlBudgetCatAContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBudgetCatAContent.AutoSize = true;
            this.pnlBudgetCatAContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlBudgetCatAContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10);
            this.pnlBudgetCatAContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatAContent.Controls.Add(this.dgvBudgetCatA);
            this.pnlBudgetCatAContent.Controls.Add(this.btnAddLineCatA);
            // dgvBudgetCatA
            this.dgvBudgetCatA.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBudgetCatA.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvBudgetCatA.Name = "dgvBudgetCatA";
            this.dgvBudgetCatA.Size = new System.Drawing.Size(1100, 120);
            this.dgvBudgetCatA.MinimumSize = new System.Drawing.Size(0, 50);
            this.dgvBudgetCatA.TabIndex = 0;
            this.dgvBudgetCatA.Tag = "A";
            // btnAddLineCatA
            this.btnAddLineCatA.Text = "Add Line Item to Category A";
            this.btnAddLineCatA.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAddLineCatA.AutoSize = true;
            this.btnAddLineCatA.Name = "btnAddLineCatA";
            this.btnAddLineCatA.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btnAddLineCatA.Tag = "A";

            // Category B
            this.flpCatBHeader.Dock = System.Windows.Forms.DockStyle.Top; this.flpCatBHeader.AutoSize = true; this.flpCatBHeader.WrapContents = false; this.flpCatBHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3); this.flpCatBHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatBHeader.Controls.Add(this.btnToggleCatB); this.flpCatBHeader.Controls.Add(this.lblBudgetCatBHeader);
            this.btnToggleCatB.Text = "-"; this.btnToggleCatB.Size = new System.Drawing.Size(23, 23); this.btnToggleCatB.Name = "btnToggleCatB"; this.btnToggleCatB.Tag = "B";
            this.lblBudgetCatBHeader.Text = "B. Supplies, Commodities, Materials"; this.lblBudgetCatBHeader.AutoSize = true; this.lblBudgetCatBHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold); this.lblBudgetCatBHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            this.pnlBudgetCatBContent.Dock = System.Windows.Forms.DockStyle.Top; this.pnlBudgetCatBContent.AutoSize = true; this.pnlBudgetCatBContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink; this.pnlBudgetCatBContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10); this.pnlBudgetCatBContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatBContent.Controls.Add(this.dgvBudgetCatB); this.pnlBudgetCatBContent.Controls.Add(this.btnAddLineCatB);
            this.dgvBudgetCatB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize; this.dgvBudgetCatB.Dock = System.Windows.Forms.DockStyle.Top; this.dgvBudgetCatB.Name = "dgvBudgetCatB"; this.dgvBudgetCatB.Size = new System.Drawing.Size(1100, 120);this.dgvBudgetCatB.MinimumSize = new System.Drawing.Size(0, 50);  this.dgvBudgetCatB.TabIndex = 0; this.dgvBudgetCatB.Tag = "B";
            this.btnAddLineCatB.Text = "Add Line Item to Category B"; this.btnAddLineCatB.Dock = System.Windows.Forms.DockStyle.Top; this.btnAddLineCatB.AutoSize = true; this.btnAddLineCatB.Name = "btnAddLineCatB"; this.btnAddLineCatB.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0); this.btnAddLineCatB.Tag = "B";

            // Category C
            this.flpCatCHeader.Dock = System.Windows.Forms.DockStyle.Top; this.flpCatCHeader.AutoSize = true; this.flpCatCHeader.WrapContents = false; this.flpCatCHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3); this.flpCatCHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatCHeader.Controls.Add(this.btnToggleCatC); this.flpCatCHeader.Controls.Add(this.lblBudgetCatCHeader);
            this.btnToggleCatC.Text = "-"; this.btnToggleCatC.Size = new System.Drawing.Size(23, 23); this.btnToggleCatC.Name = "btnToggleCatC"; this.btnToggleCatC.Tag = "C";
            this.lblBudgetCatCHeader.Text = "C. Equipment"; this.lblBudgetCatCHeader.AutoSize = true; this.lblBudgetCatCHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold); this.lblBudgetCatCHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            this.pnlBudgetCatCContent.Dock = System.Windows.Forms.DockStyle.Top; this.pnlBudgetCatCContent.AutoSize = true; this.pnlBudgetCatCContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink; this.pnlBudgetCatCContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10); this.pnlBudgetCatCContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatCContent.Controls.Add(this.dgvBudgetCatC); this.pnlBudgetCatCContent.Controls.Add(this.btnAddLineCatC);
            this.dgvBudgetCatC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize; this.dgvBudgetCatC.Dock = System.Windows.Forms.DockStyle.Top; this.dgvBudgetCatC.Name = "dgvBudgetCatC"; this.dgvBudgetCatC.Size = new System.Drawing.Size(1100, 120); this.dgvBudgetCatC.MinimumSize = new System.Drawing.Size(0, 50); this.dgvBudgetCatC.TabIndex = 0; this.dgvBudgetCatC.Tag = "C";
            this.btnAddLineCatC.Text = "Add Line Item to Category C"; this.btnAddLineCatC.Dock = System.Windows.Forms.DockStyle.Top; this.btnAddLineCatC.AutoSize = true; this.btnAddLineCatC.Name = "btnAddLineCatC"; this.btnAddLineCatC.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0); this.btnAddLineCatC.Tag = "C";

            // Category D
            this.flpCatDHeader.Dock = System.Windows.Forms.DockStyle.Top; this.flpCatDHeader.AutoSize = true; this.flpCatDHeader.WrapContents = false; this.flpCatDHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3); this.flpCatDHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatDHeader.Controls.Add(this.btnToggleCatD); this.flpCatDHeader.Controls.Add(this.lblBudgetCatDHeader);
            this.btnToggleCatD.Text = "-"; this.btnToggleCatD.Size = new System.Drawing.Size(23, 23); this.btnToggleCatD.Name = "btnToggleCatD"; this.btnToggleCatD.Tag = "D";
            this.lblBudgetCatDHeader.Text = "D. Contractual Services"; this.lblBudgetCatDHeader.AutoSize = true; this.lblBudgetCatDHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold); this.lblBudgetCatDHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            this.pnlBudgetCatDContent.Dock = System.Windows.Forms.DockStyle.Top; this.pnlBudgetCatDContent.AutoSize = true; this.pnlBudgetCatDContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink; this.pnlBudgetCatDContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10); this.pnlBudgetCatDContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatDContent.Controls.Add(this.dgvBudgetCatD); this.pnlBudgetCatDContent.Controls.Add(this.btnAddLineCatD);
            this.dgvBudgetCatD.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize; this.dgvBudgetCatD.Dock = System.Windows.Forms.DockStyle.Top; this.dgvBudgetCatD.Name = "dgvBudgetCatD"; this.dgvBudgetCatD.Size = new System.Drawing.Size(1100, 120); this.dgvBudgetCatD.MinimumSize = new System.Drawing.Size(0, 50); this.dgvBudgetCatD.TabIndex = 0; this.dgvBudgetCatD.Tag = "D";
            this.btnAddLineCatD.Text = "Add Line Item to Category D"; this.btnAddLineCatD.Dock = System.Windows.Forms.DockStyle.Top; this.btnAddLineCatD.AutoSize = true; this.btnAddLineCatD.Name = "btnAddLineCatD"; this.btnAddLineCatD.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0); this.btnAddLineCatD.Tag = "D";

            // Category E
            this.flpCatEHeader.Dock = System.Windows.Forms.DockStyle.Top; this.flpCatEHeader.AutoSize = true; this.flpCatEHeader.WrapContents = false; this.flpCatEHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3); this.flpCatEHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatEHeader.Controls.Add(this.btnToggleCatE); this.flpCatEHeader.Controls.Add(this.lblBudgetCatEHeader);
            this.btnToggleCatE.Text = "-"; this.btnToggleCatE.Size = new System.Drawing.Size(23, 23); this.btnToggleCatE.Name = "btnToggleCatE"; this.btnToggleCatE.Tag = "E";
            this.lblBudgetCatEHeader.Text = "E. Travel"; this.lblBudgetCatEHeader.AutoSize = true; this.lblBudgetCatEHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold); this.lblBudgetCatEHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            this.pnlBudgetCatEContent.Dock = System.Windows.Forms.DockStyle.Top; this.pnlBudgetCatEContent.AutoSize = true; this.pnlBudgetCatEContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink; this.pnlBudgetCatEContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10); this.pnlBudgetCatEContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatEContent.Controls.Add(this.dgvBudgetCatE); this.pnlBudgetCatEContent.Controls.Add(this.btnAddLineCatE);
            this.dgvBudgetCatE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize; this.dgvBudgetCatE.Dock = System.Windows.Forms.DockStyle.Top; this.dgvBudgetCatE.Name = "dgvBudgetCatE"; this.dgvBudgetCatE.Size = new System.Drawing.Size(1100, 120); this.dgvBudgetCatE.MinimumSize = new System.Drawing.Size(0, 50); this.dgvBudgetCatE.TabIndex = 0; this.dgvBudgetCatE.Tag = "E";
            this.btnAddLineCatE.Text = "Add Line Item to Category E"; this.btnAddLineCatE.Dock = System.Windows.Forms.DockStyle.Top; this.btnAddLineCatE.AutoSize = true; this.btnAddLineCatE.Name = "btnAddLineCatE"; this.btnAddLineCatE.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0); this.btnAddLineCatE.Tag = "E";

            // Category F
            this.flpCatFHeader.Dock = System.Windows.Forms.DockStyle.Top; this.flpCatFHeader.AutoSize = true; this.flpCatFHeader.WrapContents = false; this.flpCatFHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3); this.flpCatFHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatFHeader.Controls.Add(this.btnToggleCatF); this.flpCatFHeader.Controls.Add(this.lblBudgetCatFHeader);
            this.btnToggleCatF.Text = "-"; this.btnToggleCatF.Size = new System.Drawing.Size(23, 23); this.btnToggleCatF.Name = "btnToggleCatF"; this.btnToggleCatF.Tag = "F";
            this.lblBudgetCatFHeader.Text = "F. Transfers and Grants to Counterparts"; this.lblBudgetCatFHeader.AutoSize = true; this.lblBudgetCatFHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold); this.lblBudgetCatFHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            this.pnlBudgetCatFContent.Dock = System.Windows.Forms.DockStyle.Top; this.pnlBudgetCatFContent.AutoSize = true; this.pnlBudgetCatFContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink; this.pnlBudgetCatFContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10); this.pnlBudgetCatFContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatFContent.Controls.Add(this.dgvBudgetCatF); this.pnlBudgetCatFContent.Controls.Add(this.btnAddLineCatF);
            this.dgvBudgetCatF.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize; this.dgvBudgetCatF.Dock = System.Windows.Forms.DockStyle.Top; this.dgvBudgetCatF.Name = "dgvBudgetCatF"; this.dgvBudgetCatF.Size = new System.Drawing.Size(1100, 120); this.dgvBudgetCatF.MinimumSize = new System.Drawing.Size(0, 50); this.dgvBudgetCatF.TabIndex = 0; this.dgvBudgetCatF.Tag = "F";
            this.btnAddLineCatF.Text = "Add Line Item to Category F"; this.btnAddLineCatF.Dock = System.Windows.Forms.DockStyle.Top; this.btnAddLineCatF.AutoSize = true; this.btnAddLineCatF.Name = "btnAddLineCatF"; this.btnAddLineCatF.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0); this.btnAddLineCatF.Tag = "F";

            // Category G
            this.flpCatGHeader.Dock = System.Windows.Forms.DockStyle.Top; this.flpCatGHeader.AutoSize = true; this.flpCatGHeader.WrapContents = false; this.flpCatGHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3); this.flpCatGHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpCatGHeader.Controls.Add(this.btnToggleCatG); this.flpCatGHeader.Controls.Add(this.lblBudgetCatGHeader);
            this.btnToggleCatG.Text = "-"; this.btnToggleCatG.Size = new System.Drawing.Size(23, 23); this.btnToggleCatG.Name = "btnToggleCatG"; this.btnToggleCatG.Tag = "G";
            this.lblBudgetCatGHeader.Text = "G. General Operating and Other Direct Costs"; this.lblBudgetCatGHeader.AutoSize = true; this.lblBudgetCatGHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold); this.lblBudgetCatGHeader.Margin = new System.Windows.Forms.Padding(3,3,0,0);
            this.pnlBudgetCatGContent.Dock = System.Windows.Forms.DockStyle.Top; this.pnlBudgetCatGContent.AutoSize = true; this.pnlBudgetCatGContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink; this.pnlBudgetCatGContent.Padding = new System.Windows.Forms.Padding(20, 5, 5, 10); this.pnlBudgetCatGContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBudgetCatGContent.Controls.Add(this.dgvBudgetCatG); this.pnlBudgetCatGContent.Controls.Add(this.btnAddLineCatG);
            this.dgvBudgetCatG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize; this.dgvBudgetCatG.Dock = System.Windows.Forms.DockStyle.Top; this.dgvBudgetCatG.Name = "dgvBudgetCatG"; this.dgvBudgetCatG.Size = new System.Drawing.Size(1100, 120); this.dgvBudgetCatG.MinimumSize = new System.Drawing.Size(0, 50); this.dgvBudgetCatG.TabIndex = 0; this.dgvBudgetCatG.Tag = "G";
            this.btnAddLineCatG.Text = "Add Line Item to Category G"; this.btnAddLineCatG.Dock = System.Windows.Forms.DockStyle.Top; this.btnAddLineCatG.AutoSize = true; this.btnAddLineCatG.Name = "btnAddLineCatG"; this.btnAddLineCatG.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0); this.btnAddLineCatG.Tag = "G";

            //
            // tabPagePlan
            //
            // REMOVED: this.tabPagePlan.Controls.Add(this.pnlPlanBase);
            this.tabPagePlan.Controls.Add(this.dgvActivityPlan); // Added dgvActivityPlan
            this.tabPagePlan.Location = new System.Drawing.Point(4, 22);
            this.tabPagePlan.Name = "tabPagePlan";
            this.tabPagePlan.Padding = new System.Windows.Forms.Padding(10); // Added Padding
            this.tabPagePlan.Size = new System.Drawing.Size(1176, 685);
            this.tabPagePlan.TabIndex = 3;
            this.tabPagePlan.Text = "Activity Plan";
            this.tabPagePlan.UseVisualStyleBackColor = true;
            //
            // dgvActivityPlan
            //
            this.dgvActivityPlan.AllowUserToAddRows = false;
            this.dgvActivityPlan.AllowUserToDeleteRows = false;
            this.dgvActivityPlan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvActivityPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvActivityPlan.Name = "dgvActivityPlan";
            this.dgvActivityPlan.Size = new System.Drawing.Size(1156, 665); // Adjusted to fill padding
            this.dgvActivityPlan.TabIndex = 0;

            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 711);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(1184, 50);
            this.pnlButtons.TabIndex = 1;
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(10,10,10,10);

            //
            // btnSave
            //
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(990, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 25);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(1080, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 25);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // ProjectCreateEditForm
            //
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1184, 761);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.pnlButtons);
            this.MinimumSize = new System.Drawing.Size(1024, 700);
            this.Name = "ProjectCreateEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Project Details";

            this.tabControlMain.ResumeLayout(false);
            this.tabPageProjectDetails.ResumeLayout(false);
            this.tlpProjectDetails.ResumeLayout(false);
            this.tlpProjectDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalBudget)).EndInit();

            this.tabPageLogFrame.ResumeLayout(false);
            this.pnlLogFrameBase.ResumeLayout(false);

            this.tabPageBudget.ResumeLayout(false);
            // REMOVED: this.pnlBudgetBase.PerformLayout();
            this.pnlBudgetScrollable.ResumeLayout(false);
            // this.pnlBudgetScrollable.PerformLayout(); // Not strictly necessary for this layout

            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBudgetCatG)).EndInit();

            this.tabPagePlan.ResumeLayout(false);
            // REMOVED: this.pnlPlanBase.ResumeLayout(false);
            // REMOVED: this.pnlPlanBase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActivityPlan)).EndInit(); // Added

            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageProjectDetails;
        private System.Windows.Forms.TabPage tabPageLogFrame;
        private System.Windows.Forms.TabPage tabPageBudget;
        private System.Windows.Forms.TabPage tabPagePlan;

        private System.Windows.Forms.TableLayoutPanel tlpProjectDetails;
        private System.Windows.Forms.Label lblProjectName;
        private PlaceholderTextBox txtProjectName;
        private System.Windows.Forms.Label lblProjectCode;
        private PlaceholderTextBox txtProjectCode;
        private System.Windows.Forms.Label lblStartDate;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblLocation;
        private PlaceholderTextBox txtLocation;
        private System.Windows.Forms.Label lblOverallObjective;
        private PlaceholderTextBox txtOverallObjective;
        private System.Windows.Forms.Label lblStatus;
        private PlaceholderTextBox txtStatus;
        private System.Windows.Forms.Label lblDonor;
        private PlaceholderTextBox txtDonor;
        private System.Windows.Forms.Label lblTotalBudget;
        private System.Windows.Forms.NumericUpDown nudTotalBudget;
        private System.Windows.Forms.Label lblSection;
        private System.Windows.Forms.ComboBox cmbSection;
        private System.Windows.Forms.Label lblManager;
        private System.Windows.Forms.ComboBox cmbManager;

        private System.Windows.Forms.Panel pnlLogFrameBase;
        private System.Windows.Forms.Button btnAddOutcome;
        private System.Windows.Forms.Panel pnlLogFrameMain;

        // private System.Windows.Forms.Panel pnlBudgetBase; // Removed
        // private System.Windows.Forms.Label lblBudgetPlaceholder; // Removed
        private System.Windows.Forms.Panel pnlBudgetScrollable;

        private System.Windows.Forms.FlowLayoutPanel flpCatAHeader;
        private System.Windows.Forms.Button btnToggleCatA;
        private System.Windows.Forms.Label lblBudgetCatAHeader;
        private System.Windows.Forms.Panel pnlBudgetCatAContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatA;
        private System.Windows.Forms.Button btnAddLineCatA;

        private System.Windows.Forms.FlowLayoutPanel flpCatBHeader;
        private System.Windows.Forms.Button btnToggleCatB;
        private System.Windows.Forms.Label lblBudgetCatBHeader;
        private System.Windows.Forms.Panel pnlBudgetCatBContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatB;
        private System.Windows.Forms.Button btnAddLineCatB;

        private System.Windows.Forms.FlowLayoutPanel flpCatCHeader;
        private System.Windows.Forms.Button btnToggleCatC;
        private System.Windows.Forms.Label lblBudgetCatCHeader;
        private System.Windows.Forms.Panel pnlBudgetCatCContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatC;
        private System.Windows.Forms.Button btnAddLineCatC;

        private System.Windows.Forms.FlowLayoutPanel flpCatDHeader;
        private System.Windows.Forms.Button btnToggleCatD;
        private System.Windows.Forms.Label lblBudgetCatDHeader;
        private System.Windows.Forms.Panel pnlBudgetCatDContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatD;
        private System.Windows.Forms.Button btnAddLineCatD;

        private System.Windows.Forms.FlowLayoutPanel flpCatEHeader;
        private System.Windows.Forms.Button btnToggleCatE;
        private System.Windows.Forms.Label lblBudgetCatEHeader;
        private System.Windows.Forms.Panel pnlBudgetCatEContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatE;
        private System.Windows.Forms.Button btnAddLineCatE;

        private System.Windows.Forms.FlowLayoutPanel flpCatFHeader;
        private System.Windows.Forms.Button btnToggleCatF;
        private System.Windows.Forms.Label lblBudgetCatFHeader;
        private System.Windows.Forms.Panel pnlBudgetCatFContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatF;
        private System.Windows.Forms.Button btnAddLineCatF;

        private System.Windows.Forms.FlowLayoutPanel flpCatGHeader;
        private System.Windows.Forms.Button btnToggleCatG;
        private System.Windows.Forms.Label lblBudgetCatGHeader;
        private System.Windows.Forms.Panel pnlBudgetCatGContent;
        private System.Windows.Forms.DataGridView dgvBudgetCatG;
        private System.Windows.Forms.Button btnAddLineCatG;


        // REMOVED: private System.Windows.Forms.Panel pnlPlanBase;
        // REMOVED: private System.Windows.Forms.Label lblPlanPlaceholder;
        private System.Windows.Forms.DataGridView dgvActivityPlan; // Added

        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip mainToolTip;
    }
}
