namespace OpenHardwareMonitor.UI
{
    sealed partial class AboutBox
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
        private void InitializeComponent() {
            this.okButton = new System.Windows.Forms.Button();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.projectLinkLabel = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize) (this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(269, 79);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // picLogo
            this.picLogo.Location = new System.Drawing.Point(10, 11);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(48, 48);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picLogo.TabIndex = 1;
            this.picLogo.TabStop = false;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Location = new System.Drawing.Point(74, 12);
            this.lblAppTitle.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(117, 13);
            this.lblAppTitle.TabIndex = 2;
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Location = new System.Drawing.Point(74, 46);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(250, 13);
            this.lblCopyright.TabIndex = 3;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(74, 29);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(127, 13);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version 1.2.34567.123";
            // 
            // projectLinkLabel
            // 
            this.projectLinkLabel.AutoSize = true;
            this.projectLinkLabel.Location = new System.Drawing.Point(74, 80);
            this.projectLinkLabel.Margin = new System.Windows.Forms.Padding(0);
            this.projectLinkLabel.Name = "projectLinkLabel";
            this.projectLinkLabel.Size = new System.Drawing.Size(82, 13);
            this.projectLinkLabel.TabIndex = 6;
            this.projectLinkLabel.TabStop = true;
            this.projectLinkLabel.Text = "Project Website";
            this.projectLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 100);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // AboutBox
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(359, 115);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.projectLinkLabel);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.lblAppTitle);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize) (this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.LinkLabel projectLinkLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
