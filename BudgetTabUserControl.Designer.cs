namespace HumanitarianProjectManagement
{
    partial class BudgetTabUserControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.scMainBudgetLayout = new System.Windows.Forms.SplitContainer();
            this.pnlCategorySidebar = new System.Windows.Forms.Panel();
            this.tlpCategoryButtons = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBudgetMainArea = new System.Windows.Forms.Panel();
            this.pnlSubCategoryListArea = new System.Windows.Forms.Panel();
            this.pnlItemizedDetailsHolder = new System.Windows.Forms.Panel();
            this.dgvItemizedDetails = new System.Windows.Forms.DataGridView();
            this.pnlItemizedDetailsControls = new System.Windows.Forms.Panel();
            this.btnAddNewItemizedDetail = new System.Windows.Forms.Button();
            this.lblItemizedDetailsHeader = new System.Windows.Forms.Label();
            this.btnAddNewSubCategory = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.scMainBudgetLayout)).BeginInit();
            this.scMainBudgetLayout.Panel1.SuspendLayout();
            this.scMainBudgetLayout.Panel2.SuspendLayout();
            this.scMainBudgetLayout.SuspendLayout();
            this.pnlCategorySidebar.SuspendLayout();
            this.pnlBudgetMainArea.SuspendLayout();
            this.pnlItemizedDetailsHolder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItemizedDetails)).BeginInit();
            this.pnlItemizedDetailsControls.SuspendLayout();
            this.SuspendLayout();

            // 
            // scMainBudgetLayout
            // 
            this.scMainBudgetLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMainBudgetLayout.Name = "scMainBudgetLayout";
            this.scMainBudgetLayout.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scMainBudgetLayout.SplitterDistance = 230; 
            this.scMainBudgetLayout.TabIndex = 1; // Main container
            // 
            // scMainBudgetLayout.Panel1 (Category Sidebar)
            // 
            this.scMainBudgetLayout.Panel1.Controls.Add(this.pnlCategorySidebar);
            this.scMainBudgetLayout.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // scMainBudgetLayout.Panel2 (Main Content)
            // 
            this.scMainBudgetLayout.Panel2.Controls.Add(this.pnlBudgetMainArea);
            this.scMainBudgetLayout.Panel2.Padding = new System.Windows.Forms.Padding(3);
            // 
            // pnlCategorySidebar
            // 
            this.pnlCategorySidebar.Controls.Add(this.tlpCategoryButtons);
            this.pnlCategorySidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCategorySidebar.Name = "pnlCategorySidebar";
            this.pnlCategorySidebar.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.pnlCategorySidebar.TabIndex = 0;
            // 
            // tlpCategoryButtons
            // 
            this.tlpCategoryButtons.ColumnCount = 1;
            this.tlpCategoryButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCategoryButtons.Name = "tlpCategoryButtons";
            this.tlpCategoryButtons.AutoScroll = true;
            this.tlpCategoryButtons.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            // RowStyles will be added dynamically in code
            this.tlpCategoryButtons.TabIndex = 0;

            // 
            // pnlBudgetMainArea
            // 
            this.pnlBudgetMainArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBudgetMainArea.Location = new System.Drawing.Point(0, 0); // Relative to Panel2
            this.pnlBudgetMainArea.Name = "pnlBudgetMainArea";
            // Example size, actual size will be determined by SplitContainer's Panel2
            this.pnlBudgetMainArea.Size = new System.Drawing.Size(463, 494); 
            this.pnlBudgetMainArea.TabIndex = 0;
            this.pnlBudgetMainArea.Padding = new System.Windows.Forms.Padding(5);
            // 
            // btnAddNewSubCategory
            // 
            this.btnAddNewSubCategory.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAddNewSubCategory.Location = new System.Drawing.Point(5, 5); 
            this.btnAddNewSubCategory.Name = "btnAddNewSubCategory";
            // Width should be relative to parent pnlBudgetMainArea's client area minus padding
            this.btnAddNewSubCategory.Size = new System.Drawing.Size(453, 30); // Example: 463 (parent width) - 5 - 5 (padding)
            this.btnAddNewSubCategory.TabIndex = 0;
            this.btnAddNewSubCategory.Text = "Add New Subcategory";
            this.btnAddNewSubCategory.UseVisualStyleBackColor = true;
            this.btnAddNewSubCategory.Enabled = false;
            // 
            // pnlSubCategoryListArea
            // 
            this.pnlSubCategoryListArea.Dock = System.Windows.Forms.DockStyle.Fill; 
            this.pnlSubCategoryListArea.Location = new System.Drawing.Point(5, 35); // Below btnAddNewSubCategory
            this.pnlSubCategoryListArea.Name = "pnlSubCategoryListArea";
            this.pnlSubCategoryListArea.AutoScroll = true;
            // Size will be determined by Dock.Fill relative to btnAddNewSubCategory and pnlItemizedDetailsHolder
            this.pnlSubCategoryListArea.Size = new System.Drawing.Size(453, 304); // Example: 494 (parent height) - 5 (pad top) - 30 (btn) - 150 (details) - 5 (pad bottom)
            this.pnlSubCategoryListArea.TabIndex = 1;
            this.pnlSubCategoryListArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // pnlItemizedDetailsHolder
            // 
            this.pnlItemizedDetailsHolder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlItemizedDetailsHolder.Location = new System.Drawing.Point(5, 339); // Example: 35 (btnAddNewSubCategory.Bottom) + 304 (pnlSubCategoryListArea.Height)
            this.pnlItemizedDetailsHolder.Name = "pnlItemizedDetailsHolder";
            this.pnlItemizedDetailsHolder.Size = new System.Drawing.Size(453, 150); 
            this.pnlItemizedDetailsHolder.TabIndex = 2;
            this.pnlItemizedDetailsHolder.Visible = false; 
            this.pnlItemizedDetailsHolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlItemizedDetailsHolder.Padding = new System.Windows.Forms.Padding(3);
            // 
            // pnlItemizedDetailsControls
            // 
            this.pnlItemizedDetailsControls.Controls.Add(this.lblItemizedDetailsHeader);
            this.pnlItemizedDetailsControls.Controls.Add(this.btnAddNewItemizedDetail);
            this.pnlItemizedDetailsControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlItemizedDetailsControls.Location = new System.Drawing.Point(3, 3);
            this.pnlItemizedDetailsControls.Name = "pnlItemizedDetailsControls";
            this.pnlItemizedDetailsControls.Size = new System.Drawing.Size(447, 30); // 453 - 3 - 3
            this.pnlItemizedDetailsControls.TabIndex = 0;
            // 
            // lblItemizedDetailsHeader
            // 
            this.lblItemizedDetailsHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblItemizedDetailsHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblItemizedDetailsHeader.Location = new System.Drawing.Point(0, 0);
            this.lblItemizedDetailsHeader.Name = "lblItemizedDetailsHeader";
            this.lblItemizedDetailsHeader.Size = new System.Drawing.Size(297, 30); // 447 - 150 (button)
            this.lblItemizedDetailsHeader.TabIndex = 0;
            this.lblItemizedDetailsHeader.Text = "Itemized Details for:";
            this.lblItemizedDetailsHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAddNewItemizedDetail
            // 
            this.btnAddNewItemizedDetail.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAddNewItemizedDetail.Location = new System.Drawing.Point(297, 0);
            this.btnAddNewItemizedDetail.Name = "btnAddNewItemizedDetail";
            this.btnAddNewItemizedDetail.Size = new System.Drawing.Size(150, 30);
            this.btnAddNewItemizedDetail.TabIndex = 1;
            this.btnAddNewItemizedDetail.Text = "Add Detail Item";
            this.btnAddNewItemizedDetail.UseVisualStyleBackColor = true;
            // 
            // dgvItemizedDetails
            // 
            this.dgvItemizedDetails.AllowUserToAddRows = false;
            this.dgvItemizedDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItemizedDetails.Dock = System.Windows.Forms.DockStyle.Fill; 
            this.dgvItemizedDetails.Location = new System.Drawing.Point(3, 33); 
            this.dgvItemizedDetails.Name = "dgvItemizedDetails";
            this.dgvItemizedDetails.Size = new System.Drawing.Size(447, 114); // 150 - 30 - 3 - 3
            this.dgvItemizedDetails.TabIndex = 1;
            //
            // Add controls to pnlItemizedDetailsHolder
            //
            this.pnlItemizedDetailsHolder.Controls.Add(this.dgvItemizedDetails); 
            this.pnlItemizedDetailsHolder.Controls.Add(this.pnlItemizedDetailsControls); 
            //
            // Add controls to pnlBudgetMainArea (Order matters for Dock.Fill)
            //
            this.pnlBudgetMainArea.Controls.Add(this.pnlSubCategoryListArea); 
            this.pnlBudgetMainArea.Controls.Add(this.btnAddNewSubCategory); 
            this.pnlBudgetMainArea.Controls.Add(this.pnlItemizedDetailsHolder);
            
            // 
            // BudgetTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scMainBudgetLayout); // Top level control is now the SplitContainer
            this.Name = "BudgetTabUserControl";
            this.Size = new System.Drawing.Size(700, 500); 

            this.scMainBudgetLayout.Panel1.ResumeLayout(false);
            this.scMainBudgetLayout.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainBudgetLayout)).EndInit();
            this.scMainBudgetLayout.ResumeLayout(false);
            this.pnlCategorySidebar.ResumeLayout(false);
            this.pnlBudgetMainArea.ResumeLayout(false);
            this.pnlItemizedDetailsHolder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvItemizedDetails)).EndInit();
            this.pnlItemizedDetailsControls.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.SplitContainer scMainBudgetLayout;
        private System.Windows.Forms.Panel pnlCategorySidebar;
        private System.Windows.Forms.TableLayoutPanel tlpCategoryButtons;
        private System.Windows.Forms.Panel pnlBudgetMainArea;
        private System.Windows.Forms.Button btnAddNewSubCategory;
        private System.Windows.Forms.Panel pnlSubCategoryListArea;
        private System.Windows.Forms.Panel pnlItemizedDetailsHolder;
        private System.Windows.Forms.Label lblItemizedDetailsHeader;
        private System.Windows.Forms.Button btnAddNewItemizedDetail;
        private System.Windows.Forms.DataGridView dgvItemizedDetails;
        private System.Windows.Forms.Panel pnlItemizedDetailsControls;
    }
}
