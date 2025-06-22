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
            this.pnlMainBudgetContentArea = new System.Windows.Forms.Panel(); // Renamed from pnlSubCategoryListArea
            this.btnAddNewSubCategory = new System.Windows.Forms.Button();

            // Removed declarations for pnlItemizedDetailsHolder and its contents

            ((System.ComponentModel.ISupportInitialize)(this.scMainBudgetLayout)).BeginInit();
            this.scMainBudgetLayout.Panel1.SuspendLayout();
            this.scMainBudgetLayout.Panel2.SuspendLayout();
            this.scMainBudgetLayout.SuspendLayout();
            this.pnlCategorySidebar.SuspendLayout();
            this.pnlBudgetMainArea.SuspendLayout();
            // Removed SuspendLayout/ResumeLayout for pnlItemizedDetailsHolder and dgvItemizedDetails
            this.SuspendLayout();

            // 
            // scMainBudgetLayout
            // 
            this.scMainBudgetLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMainBudgetLayout.Name = "scMainBudgetLayout";
            this.scMainBudgetLayout.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scMainBudgetLayout.SplitterDistance = 230;
            this.scMainBudgetLayout.TabIndex = 1;
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
            this.tlpCategoryButtons.TabIndex = 0;

            // 
            // pnlBudgetMainArea
            // 
            this.pnlBudgetMainArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBudgetMainArea.Location = new System.Drawing.Point(0, 0);
            this.pnlBudgetMainArea.Name = "pnlBudgetMainArea";
            this.pnlBudgetMainArea.Size = new System.Drawing.Size(463, 494);
            this.pnlBudgetMainArea.TabIndex = 0;
            this.pnlBudgetMainArea.Padding = new System.Windows.Forms.Padding(5);
            // 
            
            // 
            // pnlMainBudgetContentArea (Renamed from pnlSubCategoryListArea)
            // 
            this.pnlMainBudgetContentArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainBudgetContentArea.Location = new System.Drawing.Point(5, 35); // Below btnAddNewSubCategory
            this.pnlMainBudgetContentArea.Name = "pnlMainBudgetContentArea"; // Renamed
            this.pnlMainBudgetContentArea.AutoScroll = true;
            this.pnlMainBudgetContentArea.Size = new System.Drawing.Size(453, 454); // Adjusted size: 494 (parent) - 5 (pad_top) - 30 (btn) - 5 (pad_bottom)
            this.pnlMainBudgetContentArea.TabIndex = 1;
            this.pnlMainBudgetContentArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Removed pnlItemizedDetailsHolder and its contents' initialization

            // Add controls to pnlBudgetMainArea (Order matters for Dock.Fill)
            this.pnlBudgetMainArea.Controls.Add(this.pnlMainBudgetContentArea); // Renamed
            
            // Removed: this.pnlBudgetMainArea.Controls.Add(this.pnlItemizedDetailsHolder);

            // 
            // BudgetTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scMainBudgetLayout);
            this.Name = "BudgetTabUserControl";
            this.Size = new System.Drawing.Size(700, 500);

            this.scMainBudgetLayout.Panel1.ResumeLayout(false);
            this.scMainBudgetLayout.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMainBudgetLayout)).EndInit();
            this.scMainBudgetLayout.ResumeLayout(false);
            this.pnlCategorySidebar.ResumeLayout(false);
            this.pnlBudgetMainArea.ResumeLayout(false);
            // Removed SuspendLayout/ResumeLayout for pnlItemizedDetailsHolder and dgvItemizedDetails
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.SplitContainer scMainBudgetLayout;
        private System.Windows.Forms.Panel pnlCategorySidebar;
        private System.Windows.Forms.TableLayoutPanel tlpCategoryButtons;
        private System.Windows.Forms.Panel pnlBudgetMainArea;
        private System.Windows.Forms.Button btnAddNewSubCategory;
        private System.Windows.Forms.Panel pnlMainBudgetContentArea; // Renamed from pnlSubCategoryListArea
        // Removed declarations for pnlItemizedDetailsHolder, lblItemizedDetailsHeader, btnAddNewItemizedDetail, dgvItemizedDetails, pnlItemizedDetailsControls
    }
}
