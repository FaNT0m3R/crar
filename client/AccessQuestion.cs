using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace client
{
    public partial class AccessQuestion : Form
    {
        private bool reslt = false;
        private int RecTime = 10;

        public AccessQuestion()
        {
            InitializeComponent();
        }

        public bool Show(string LoginFrom, string LoginTo, int TimeOut)
        {
            RecTime = TimeOut;   
            QuestText.Text = "Разрешить доступ к файлам пользователю " + LoginFrom + "?";
            Text = LoginTo + ": Запрос на доступ к файлам";
            ShowDialog();
            return reslt;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            reslt = true;
            Close();
        }

        private void btnDenied_Click(object sender, EventArgs e)
        {
            reslt = false;
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (--RecTime == 0)
                Close();
            lblTime.Text = "Доступ будет автоматически запрещён через " + RecTime.ToString() +" секунд";
        }
    }
}
