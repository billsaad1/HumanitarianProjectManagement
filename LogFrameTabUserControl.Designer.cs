namespace HumanitarianProjectManagement
{
    partial class LogFrameTabUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.flpOutcomesSidebar = new System.Windows.Forms.FlowLayoutPanel();
            this.splitContainerContent = new System.Windows.Forms.SplitContainer();
            this.pnlInputArea = new System.Windows.Forms.Panel();
            this.lblInputAreaPlaceholder = new System.Windows.Forms.Label();
            this.flpLogFrameDisplay = new System.Windows.Forms.FlowLayoutPanel();
            this.lblLogFrameDisplayPlaceholder = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerContent)).BeginInit();
            this.splitContainerContent.Panel1.SuspendLayout();
            this.splitContainerContent.Panel2.SuspendLayout();
            this.splitContainerContent.SuspendLayout();
            this.pnlInputArea.SuspendLayout();
            this.flpLogFrameDisplay.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.flpOutcomesSidebar);
            this.splitContainerMain.Panel1MinSize = 220; // Adjusted
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerContent);
            this.splitContainerMain.Size = new System.Drawing.Size(800, 600);
            this.splitContainerMain.SplitterDistance = 240; // Adjusted
            this.splitContainerMain.TabIndex = 0;
            // 
            // flpOutcomesSidebar
            // 
            this.flpOutcomesSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOutcomesSidebar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOutcomesSidebar.AutoScroll = true;
            this.flpOutcomesSidebar.WrapContents = false;
            this.flpOutcomesSidebar.Padding = new System.Windows.Forms.Padding(5);
            this.flpOutcomesSidebar.Name = "flpOutcomesSidebar";
            this.flpOutcomesSidebar.Location = new System.Drawing.Point(0, 0);
            this.flpOutcomesSidebar.Size = new System.Drawing.Size(200, 600);
            this.flpOutcomesSidebar.TabIndex = 0;
            // 
            // splitContainerContent
            // 
            this.splitContainerContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerContent.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainerContent.Location = new System.Drawing.Point(0, 0);
            this.splitContainerContent.Name = "splitContainerContent";
            // 
            // splitContainerContent.Panel1
            // 
            this.splitContainerContent.Panel1.Controls.Add(this.pnlInputArea);
            this.splitContainerContent.Panel1MinSize = 200; // Adjusted
            // 
            // splitContainerContent.Panel2
            // 
            this.splitContainerContent.Panel2.Controls.Add(this.flpLogFrameDisplay);
            this.splitContainerContent.Size = new System.Drawing.Size(556, 600); // Size will adjust based on parent
            this.splitContainerContent.SplitterDistance = 250; // Adjusted
            this.splitContainerContent.TabIndex = 0;
            // 
            // pnlInputArea
            // 
            this.pnlInputArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInputArea.AutoScroll = true;
            this.pnlInputArea.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlInputArea.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInputArea.Controls.Add(this.lblInputAreaPlaceholder);
            this.pnlInputArea.Name = "pnlInputArea";
            this.pnlInputArea.Location = new System.Drawing.Point(0, 0);
            this.pnlInputArea.Size = new System.Drawing.Size(596, 200);
            this.pnlInputArea.TabIndex = 0;
            // 
            // lblInputAreaPlaceholder
            // 
            this.lblInputAreaPlaceholder.AutoSize = true;
            this.lblInputAreaPlaceholder.Location = new System.Drawing.Point(13, 13);
            this.lblInputAreaPlaceholder.Name = "lblInputAreaPlaceholder";
            this.lblInputAreaPlaceholder.Size = new System.Drawing.Size(112, 13);
            this.lblInputAreaPlaceholder.TabIndex = 0;
            this.lblInputAreaPlaceholder.Text = "Input Area Placeholder";
            // 
            // flpLogFrameDisplay
            // 
            this.flpLogFrameDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpLogFrameDisplay.AutoScroll = true;
            this.flpLogFrameDisplay.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpLogFrameDisplay.WrapContents = false;
            this.flpLogFrameDisplay.BackColor = System.Drawing.SystemColors.Window;
            this.flpLogFrameDisplay.Padding = new System.Windows.Forms.Padding(10);
            this.flpLogFrameDisplay.Controls.Add(this.lblLogFrameDisplayPlaceholder);
            this.flpLogFrameDisplay.Name = "flpLogFrameDisplay";
            this.flpLogFrameDisplay.Location = new System.Drawing.Point(0, 0);
            this.flpLogFrameDisplay.Size = new System.Drawing.Size(596, 396);
            this.flpLogFrameDisplay.TabIndex = 0;
            // 
            // lblLogFrameDisplayPlaceholder
            // 
            this.lblLogFrameDisplayPlaceholder.AutoSize = true;
            this.lblLogFrameDisplayPlaceholder.Location = new System.Drawing.Point(13, 10);
            this.lblLogFrameDisplayPlaceholder.Name = "lblLogFrameDisplayPlaceholder";
            this.lblLogFrameDisplayPlaceholder.Size = new System.Drawing.Size(134, 13);
            this.lblLogFrameDisplayPlaceholder.TabIndex = 0;
            this.lblLogFrameDisplayPlaceholder.Text = "LogFrame Display Area Placeholder";
            // 
            // LogFrameTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMain);
            this.Name = "LogFrameTabUserControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerContent.Panel1.ResumeLayout(false);
            this.splitContainerContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerContent)).EndInit();
            this.splitContainerContent.ResumeLayout(false);
            this.pnlInputArea.ResumeLayout(false);
            this.pnlInputArea.PerformLayout();
            this.flpLogFrameDisplay.ResumeLayout(false);
            this.flpLogFrameDisplay.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.FlowLayoutPanel flpOutcomesSidebar;
        private System.Windows.Forms.SplitContainer splitContainerContent;
        private System.Windows.Forms.Panel pnlInputArea;
        private System.Windows.Forms.FlowLayoutPanel flpLogFrameDisplay;
        private System.Windows.Forms.Label lblInputAreaPlaceholder;
        private System.Windows.Forms.Label lblLogFrameDisplayPlaceholder;
    }
}
