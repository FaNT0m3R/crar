using System;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace client
{
    public partial class FrmSettings : Form
    {
        public string localhost = null;
        public int TimeOut = 10;
        public bool result = false;
        public bool Сhange = true;

        public FrmSettings()
        {
            InitializeComponent();
        }

        private void FrmSettings_Shown(object sender, EventArgs e)
        {
            txtBoxIP.Text = localhost.ToString();
            txtBoxTime.Text = TimeOut.ToString();
            txtBoxIP.ReadOnly = !Сhange;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            result = true;
            IPAddress version;
            bool ok = true;

            if (IPAddress.TryParse(txtBoxIP.Text, out version))
                localhost = txtBoxIP.Text;
            else
            {
                MessageBox.Show("Неверный IP-адрес. Пример: 127.0.0.1");
                ok = false;
            }

            try
            {
                TimeOut = Convert.ToInt32(txtBoxTime.Text);
            }
            catch
            {
                ok = false;
            }

            if (ok)
                Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
