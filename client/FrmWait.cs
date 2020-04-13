using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace client
{
    public partial class FrmWait : Form
    {
        private bool job = true;
        public FrmWait()
        {
            InitializeComponent();
        }
				
				//функция,которая запускает поток с окном. Нужен отдельный 
				//поток, иначе окно нужно или немодальное, или прога будет ждать, пока оно закроется(то есть никогда :) )
        public void Start()
        {
            job = true;
            ShowDialog();
        }
        
        public void End()
        {
            job = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!job)
                Close();
        }
    }
}
