namespace SDRSharp.GpredictConnector
{
    partial class Controlpanel
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxEnable = new System.Windows.Forms.CheckBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelDescserverStat = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // checkBoxEnable
            // 
            this.checkBoxEnable.AutoSize = true;
            this.checkBoxEnable.Location = new System.Drawing.Point(3, 3);
            this.checkBoxEnable.Name = "checkBoxEnable";
            this.checkBoxEnable.Size = new System.Drawing.Size(58, 17);
            this.checkBoxEnable.TabIndex = 1;
            this.checkBoxEnable.Text = "enable";
            this.checkBoxEnable.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(83, 23);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(44, 13);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "standby";
            // 
            // labelDescserverStat
            // 
            this.labelDescserverStat.AutoSize = true;
            this.labelDescserverStat.Location = new System.Drawing.Point(0, 23);
            this.labelDescserverStat.Name = "labelDescserverStat";
            this.labelDescserverStat.Size = new System.Drawing.Size(77, 13);
            this.labelDescserverStat.TabIndex = 5;
            this.labelDescserverStat.Text = "Server Status :";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(83, 3);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(28, 13);
            this.labelVersion.TabIndex = 8;
            this.labelVersion.TabStop = true;
            this.labelVersion.Text = "v0.0";
            // 
            // Controlpanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelDescserverStat);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.checkBoxEnable);
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MinimumSize = new System.Drawing.Size(185, 40);
            this.Name = "Controlpanel";
            this.Size = new System.Drawing.Size(185, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelDescserverStat;
        public System.Windows.Forms.CheckBox checkBoxEnable;
        public System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.LinkLabel labelVersion;
    }
}
