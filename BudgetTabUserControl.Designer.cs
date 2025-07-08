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
            this.splitVerticalContent = new System.Windows.Forms.SplitContainer();
            // MODIFIED: Changed type from Panel to FlowLayoutPanel
            this.pnlMainBudgetContentArea = new System.Windows.Forms.FlowLayoutPanel();
            // this.btnAddNewSubCategory = new System.Windows.Forms.Button(); // Remains commented out

            ((System.ComponentModel.ISupportInitialize)(this.scMainBudgetLayout)).BeginInit();
            this.scMainBudgetLayout.Panel1.SuspendLayout();
            this.scMainBudgetLayout.Panel2.SuspendLayout();
            this.scMainBudgetLayout.SuspendLayout();
            this.pnlCategorySidebar.SuspendLayout();
            this.pnlBudgetMainArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitVerticalContent)).BeginInit();
            this.splitVerticalContent.Panel1.SuspendLayout();
            this.splitVerticalContent.Panel2.SuspendLayout();
            this.splitVerticalContent.SuspendLayout();
            // If pnlMainBudgetContentArea is an FLP, it might need Suspend/Resume if properties are set after this.
            // However, its properties are set below.
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
            // scMainBudgetLayout.Panel2 (Main Content Hosting another Splitter)
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
            this.pnlBudgetMainArea.Controls.Add(this.splitVerticalContent);
            this.pnlBudgetMainArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBudgetMainArea.Location = new System.Drawing.Point(0, 0);
            this.pnlBudgetMainArea.Name = "pnlBudgetMainArea";
            this.pnlBudgetMainArea.TabIndex = 0;
            this.pnlBudgetMainArea.Padding = new System.Windows.Forms.Padding(0);

            // 
            // splitVerticalContent
            // 
            this.splitVerticalContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitVerticalContent.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitVerticalContent.Name = "splitVerticalContent";
            this.splitVerticalContent.SplitterDistance = 0; // Adjusted
            this.splitVerticalContent.Panel1MinSize = 0;
            this.splitVerticalContent.Panel2MinSize = 0;
            this.splitVerticalContent.TabIndex = 0;
            // 
            // splitVerticalContent.Panel1 (Entry Area Panel)
            // 
            this.splitVerticalContent.Panel1.Padding = new System.Windows.Forms.Padding(0);
            this.splitVerticalContent.Panel1.AutoScroll = true;
            // 
            // splitVerticalContent.Panel2 (Display Area Panel)
            // 
            this.splitVerticalContent.Panel2.Controls.Add(this.pnlMainBudgetContentArea);
            this.splitVerticalContent.Panel2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5); // MODIFIED
            // 
            // pnlMainBudgetContentArea 
            // 
            this.pnlMainBudgetContentArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainBudgetContentArea.Location = new System.Drawing.Point(0, 0);
            this.pnlMainBudgetContentArea.Name = "pnlMainBudgetContentArea";
            this.pnlMainBudgetContentArea.AutoScroll = true;
            // Properties specific to FlowLayoutPanel
            this.pnlMainBudgetContentArea.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlMainBudgetContentArea.WrapContents = false;
            this.pnlMainBudgetContentArea.TabIndex = 0;
            this.pnlMainBudgetContentArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; // Kept from original

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

            this.splitVerticalContent.Panel1.ResumeLayout(false);
            this.splitVerticalContent.Panel2.ResumeLayout(false); // pnlMainBudgetContentArea is child
            ((System.ComponentModel.ISupportInitialize)(this.splitVerticalContent)).EndInit();
            this.splitVerticalContent.ResumeLayout(false);

            this.pnlBudgetMainArea.ResumeLayout(false);

            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.SplitContainer scMainBudgetLayout;
        private System.Windows.Forms.Panel pnlCategorySidebar;
        private System.Windows.Forms.TableLayoutPanel tlpCategoryButtons;
        private System.Windows.Forms.Panel pnlBudgetMainArea;
        // MODIFIED: Changed type from Panel to FlowLayoutPanel
        private System.Windows.Forms.FlowLayoutPanel pnlMainBudgetContentArea;

        private System.Windows.Forms.SplitContainer splitVerticalContent;
        // private System.Windows.Forms.Button btnAddNewSubCategory; // Remains commented
    }
}
