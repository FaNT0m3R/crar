namespace client
{
    partial class FrmSettings
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtBoxIP = new System.Windows.Forms.TextBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxTime = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(138, 78);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 24);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Отменить";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(56, 78);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(76, 24);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtBoxIP
            // 
            this.txtBoxIP.Location = new System.Drawing.Point(80, 10);
            this.txtBoxIP.Name = "txtBoxIP";
            this.txtBoxIP.Size = new System.Drawing.Size(133, 20);
            this.txtBoxIP.TabIndex = 2;
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(12, 13);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(62, 13);
            this.lblIP.TabIndex = 4;
            this.lblIP.Text = "IP сервера";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Время ожидания ответа";
            // 
            // txtBoxTime
            // 
            this.txtBoxTime.Location = new System.Drawing.Point(148, 41);
            this.txtBoxTime.Name = "txtBoxTime";
            this.txtBoxTime.Size = new System.Drawing.Size(61, 20);
            this.txtBoxTime.TabIndex = 6;
            // 
            // FrmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(228, 114);
            this.Controls.Add(this.txtBoxTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.txtBoxIP);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "FrmSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки";
            this.Shown += new System.EventHandler(this.FrmSettings_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtBoxIP;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxTime;
    }
}