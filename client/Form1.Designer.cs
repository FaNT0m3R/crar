namespace client
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLogin = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listFileUser = new System.Windows.Forms.ListBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.listFileAll = new System.Windows.Forms.ListBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.saveFileDlg = new System.Windows.Forms.SaveFileDialog();
            this.textBoxPswd = new System.Windows.Forms.TextBox();
            this.btnPswd = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(12, 12);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(109, 25);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Вход";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(127, 15);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(152, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "Введите логин";
            this.textBox1.Enter += new System.EventHandler(this.textBox1_Enter);
            // 
            // listFileUser
            // 
            this.listFileUser.FormattingEnabled = true;
            this.listFileUser.Location = new System.Drawing.Point(12, 99);
            this.listFileUser.Name = "listFileUser";
            this.listFileUser.Size = new System.Drawing.Size(157, 238);
            this.listFileUser.TabIndex = 5;
            this.listFileUser.Enter += new System.EventHandler(this.listFileUser_Enter);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(12, 350);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(134, 24);
            this.btnSend.TabIndex = 7;
            this.btnSend.Text = "Отправить на сервер";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 380);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(134, 24);
            this.btnLoad.TabIndex = 8;
            this.btnLoad.Text = "Скачать с сервера";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(12, 410);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(134, 24);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "Удалить с сервера";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // openFileDlg
            // 
            this.openFileDlg.ReadOnlyChecked = true;
            // 
            // listFileAll
            // 
            this.listFileAll.FormattingEnabled = true;
            this.listFileAll.Location = new System.Drawing.Point(187, 99);
            this.listFileAll.Name = "listFileAll";
            this.listFileAll.Size = new System.Drawing.Size(155, 238);
            this.listFileAll.TabIndex = 6;
            this.listFileAll.Enter += new System.EventHandler(this.listFileAll_Enter);
            // 
            // btnReload
            // 
            this.btnReload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReload.Location = new System.Drawing.Point(208, 350);
            this.btnReload.Name = "btnReload";
            this.btnReload.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnReload.Size = new System.Drawing.Size(134, 24);
            this.btnReload.TabIndex = 10;
            this.btnReload.Text = "Обновить списки";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Файлы пользователя";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(184, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Файлы других пользователей";
            // 
            // textBoxPswd
            // 
            this.textBoxPswd.Location = new System.Drawing.Point(127, 48);
            this.textBoxPswd.Name = "textBoxPswd";
            this.textBoxPswd.Size = new System.Drawing.Size(152, 20);
            this.textBoxPswd.TabIndex = 2;
            this.textBoxPswd.Text = "Введите пароль к файлам";
            this.textBoxPswd.Enter += new System.EventHandler(this.textBoxPswd_Enter);
            // 
            // btnPswd
            // 
            this.btnPswd.Location = new System.Drawing.Point(12, 45);
            this.btnPswd.Name = "btnPswd";
            this.btnPswd.Size = new System.Drawing.Size(109, 25);
            this.btnPswd.TabIndex = 4;
            this.btnPswd.Text = "Изменить пароль";
            this.btnPswd.UseVisualStyleBackColor = true;
            this.btnPswd.Visible = false;
            this.btnPswd.Click += new System.EventHandler(this.btnPswd_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(296, 0);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(60, 21);
            this.btnHelp.TabIndex = 11;
            this.btnHelp.Text = "Справка";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Image = global::client.Properties.Resources.sett;
            this.btnSettings.Location = new System.Drawing.Point(296, 33);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(35, 35);
            this.btnSettings.TabIndex = 0;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 444);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnPswd);
            this.Controls.Add(this.textBoxPswd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.listFileAll);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.listFileUser);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Криптоархиватор (клиент)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListBox listFileUser;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.ListBox listFileAll;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SaveFileDialog saveFileDlg;
        private System.Windows.Forms.TextBox textBoxPswd;
        private System.Windows.Forms.Button btnPswd;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnHelp;
    }
}

