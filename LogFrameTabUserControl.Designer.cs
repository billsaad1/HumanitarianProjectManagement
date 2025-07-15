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
            this.pnlInputArea = new System.Windows.Forms.Panel();
            this.splitContainerContent = new System.Windows.Forms.SplitContainer();
            this.flpLogFrameDisplay = new System.Windows.Forms.FlowLayoutPanel();
            this.lblLogFrameDisplayPlaceholder = new System.Windows.Forms.Label();
            this.flpOutcomesSidebar = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerContent)).BeginInit();
            this.splitContainerContent.Panel1.SuspendLayout();
            this.splitContainerContent.Panel2.SuspendLayout();
            this.splitContainerContent.SuspendLayout();
            this.flpLogFrameDisplay.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.splitContainerMain.SplitterWidth = 8;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.pnlInputArea);
            this.splitContainerMain.Panel1.BackColor = System.Drawing.Color.White;
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerContent);
            this.splitContainerMain.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.splitContainerMain.Size = new System.Drawing.Size(1200, 700);
            this.splitContainerMain.SplitterDistance = 350;
            this.splitContainerMain.TabIndex = 0;
            // 
            // pnlInputArea
            // 
            this.pnlInputArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInputArea.Location = new System.Drawing.Point(0, 0);
            this.pnlInputArea.Name = "pnlInputArea";
            this.pnlInputArea.Size = new System.Drawing.Size(350, 700);
            this.pnlInputArea.TabIndex = 0;
            this.pnlInputArea.BackColor = System.Drawing.Color.White;
            this.pnlInputArea.Padding = new System.Windows.Forms.Padding(15);
            // 
            // splitContainerContent
            // 
            this.splitContainerContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerContent.Location = new System.Drawing.Point(0, 0);
            this.splitContainerContent.Name = "splitContainerContent";
            this.splitContainerContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.splitContainerContent.SplitterWidth = 8;
            // 
            // splitContainerContent.Panel1
            // 
            this.splitContainerContent.Panel1.Controls.Add(this.flpLogFrameDisplay);
            this.splitContainerContent.Panel1.BackColor = System.Drawing.Color.White;
            // 
            // splitContainerContent.Panel2
            // 
            this.splitContainerContent.Panel2.Controls.Add(this.flpOutcomesSidebar);
            this.splitContainerContent.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.splitContainerContent.Size = new System.Drawing.Size(842, 700);
            this.splitContainerContent.SplitterDistance = 550;
            this.splitContainerContent.TabIndex = 0;
            // 
            // flpLogFrameDisplay
            // 
            this.flpLogFrameDisplay.AutoScroll = true;
            this.flpLogFrameDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpLogFrameDisplay.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpLogFrameDisplay.Location = new System.Drawing.Point(0, 0);
            this.flpLogFrameDisplay.Name = "flpLogFrameDisplay";
            this.flpLogFrameDisplay.Size = new System.Drawing.Size(550, 700);
            this.flpLogFrameDisplay.TabIndex = 0;
            this.flpLogFrameDisplay.WrapContents = false;
            this.flpLogFrameDisplay.BackColor = System.Drawing.Color.White;
            this.flpLogFrameDisplay.Padding = new System.Windows.Forms.Padding(20);
            this.flpLogFrameDisplay.Controls.Add(this.lblLogFrameDisplayPlaceholder);
            // 
            // lblLogFrameDisplayPlaceholder
            // 
            this.lblLogFrameDisplayPlaceholder.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLogFrameDisplayPlaceholder.AutoSize = true;
            this.lblLogFrameDisplayPlaceholder.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogFrameDisplayPlaceholder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.lblLogFrameDisplayPlaceholder.Location = new System.Drawing.Point(150, 340);
            this.lblLogFrameDisplayPlaceholder.Name = "lblLogFrameDisplayPlaceholder";
            this.lblLogFrameDisplayPlaceholder.Size = new System.Drawing.Size(250, 21);
            this.lblLogFrameDisplayPlaceholder.TabIndex = 1;
            this.lblLogFrameDisplayPlaceholder.Text = "📋 LogFrame Display Area";
            this.lblLogFrameDisplayPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLogFrameDisplayPlaceholder.Visible = true;
            // 
            // flpOutcomesSidebar
            // 
            this.flpOutcomesSidebar.AutoScroll = true;
            this.flpOutcomesSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOutcomesSidebar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOutcomesSidebar.Location = new System.Drawing.Point(0, 0);
            this.flpOutcomesSidebar.Name = "flpOutcomesSidebar";
            this.flpOutcomesSidebar.Size = new System.Drawing.Size(284, 700);
            this.flpOutcomesSidebar.TabIndex = 0;
            this.flpOutcomesSidebar.WrapContents = false;
            this.flpOutcomesSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.flpOutcomesSidebar.Padding = new System.Windows.Forms.Padding(15);
            // 
            // LogFrameTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMain);
            this.Name = "LogFrameTabUserControl";
            this.Size = new System.Drawing.Size(1200, 700);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerContent.Panel1.ResumeLayout(false);
            this.splitContainerContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerContent)).EndInit();
            this.splitContainerContent.ResumeLayout(false);
            this.flpLogFrameDisplay.ResumeLayout(false);
            this.flpLogFrameDisplay.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel pnlInputArea;
        private System.Windows.Forms.SplitContainer splitContainerContent;
        private System.Windows.Forms.FlowLayoutPanel flpLogFrameDisplay;
        private System.Windows.Forms.FlowLayoutPanel flpOutcomesSidebar;
        private System.Windows.Forms.Label lblLogFrameDisplayPlaceholder;
    }
}
