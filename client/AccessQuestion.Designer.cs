namespace client
{
    partial class AccessQuestion
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
            this.components = new System.ComponentModel.Container();
            this.QuestText = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnDenied = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // QuestText
            // 
            this.QuestText.AutoSize = true;
            this.QuestText.Location = new System.Drawing.Point(12, 9);
            this.QuestText.Name = "QuestText";
            this.QuestText.Size = new System.Drawing.Size(47, 13);
            this.QuestText.TabIndex = 0;
            this.QuestText.Text = "WinText";
            this.QuestText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(70, 37);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(84, 24);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Разрешить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnDenied
            // 
            this.btnDenied.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnDenied.Location = new System.Drawing.Point(160, 37);
            this.btnDenied.Name = "btnDenied";
            this.btnDenied.Size = new System.Drawing.Size(84, 24);
            this.btnDenied.TabIndex = 2;
            this.btnDenied.Text = "Запретить";
            this.btnDenied.UseVisualStyleBackColor = true;
            this.btnDenied.Click += new System.EventHandler(this.btnDenied_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(12, 66);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(294, 13);
            this.lblTime.TabIndex = 3;
            this.lblTime.Text = "Доступ будет автоматически запрещён через 10 секунд";
            // 
            // AccessQuestion
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnDenied;
            this.ClientSize = new System.Drawing.Size(317, 83);
            this.ControlBox = false;
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.btnDenied);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.QuestText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AccessQuestion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Запрос на доступ к файлу";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label QuestText;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnDenied;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblTime;
    }
}