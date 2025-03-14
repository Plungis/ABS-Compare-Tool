namespace ABSProject
{
    partial class FooterControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.LinkLabel linkLabelDeveloper;
        private System.Windows.Forms.Button btnInstructions;

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

        private void InitializeComponent()
        {
            this.linkLabelDeveloper = new System.Windows.Forms.LinkLabel();
            this.btnInstructions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // linkLabelDeveloper
            // 
            this.linkLabelDeveloper.AutoSize = true;
            this.linkLabelDeveloper.Location = new System.Drawing.Point(10, 15);
            this.linkLabelDeveloper.Name = "linkLabelDeveloper";
            this.linkLabelDeveloper.Size = new System.Drawing.Size(130, 13);
            this.linkLabelDeveloper.TabIndex = 0;
            this.linkLabelDeveloper.TabStop = true;
            this.linkLabelDeveloper.Text = "Developed by: Heavy Harlow";
            this.linkLabelDeveloper.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDeveloper_LinkClicked);
            // 
            // btnInstructions
            // 
            this.btnInstructions.Location = new System.Drawing.Point(220, 10);
            this.btnInstructions.Name = "btnInstructions";
            this.btnInstructions.Size = new System.Drawing.Size(90, 30);
            this.btnInstructions.TabIndex = 1;
            this.btnInstructions.Text = "Instructions";
            this.btnInstructions.UseVisualStyleBackColor = true;
            this.btnInstructions.Click += new System.EventHandler(this.btnInstructions_Click);
            // 
            // FooterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkLabelDeveloper);
            this.Controls.Add(this.btnInstructions);
            this.Name = "FooterControl";
            this.Size = new System.Drawing.Size(320, 50);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
