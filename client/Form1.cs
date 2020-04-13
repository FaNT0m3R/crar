using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace client
{
    public partial class Form1 : Form
    {
        private FrmSettings WinSett = null;
        private Transp TrData = new Transp();
        private Thread NetThrd = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TrData.Login.Length != 0)
                TrData.SRVLogout();
        }

////////////////////////////////////////////////////////////////////////////////
///////////// поля ввода ///////////////////////////////////////////////////////

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Введите логин")
                textBox1.Text = "";
        }

        private void textBoxPswd_Enter(object sender, EventArgs e)
        {
            if (textBoxPswd.Text == "Введите пароль к файлам")
            {
                textBoxPswd.Text = "";
                textBoxPswd.UseSystemPasswordChar = true;
            }

        }

////////////////////////////////////////////////////////////////////////////////
/////////////// кнопки логин, пароль ///////////////////////////////////////////

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (TrData.Login.Length == 0)
            {
                if ((textBox1.Text != "Введите логин") && 
                    (textBox1.Text != ""))
                {

                    if ((textBoxPswd.Text != "Введите пароль к файлам") &&
                        (textBoxPswd.Text.Length > 5))
                    {
                        textBoxPswd.ReadOnly = true;
                        btnPswd.Text = "Изменить пароль";
                        btnPswd.Visible = true;
                        TrData.SetPassword(textBoxPswd.Text);
                    }
                    else
                    {
                        MessageBox.Show("Пароль должен состоять минимум из 6 символов (любых)");
                        return;
                    }

                    TrData.Login = textBox1.Text;
                    if (TrData.SRVLogin())
                    {
                        textBox1.ReadOnly = true;
                        btnLogin.Text = "Выход";
                        ReloadLists();
                    }
                    else
                    {
                        TrData.Login = "";
                        textBoxPswd.ReadOnly = false;
                        btnPswd.Visible = false;
                        TrData.ClearPassword();
                    }
                }
            }
            else
            {
                TrData.Login = "";
                textBox1.ReadOnly = false;
                textBox1.Text = "Введите логин";
                textBoxPswd.UseSystemPasswordChar = false;
                textBoxPswd.Text = "Введите пароль к файлам";
                textBoxPswd.ReadOnly = false;
                btnPswd.Text = "Изменить пароль";
                btnPswd.Visible = false;
                btnLogin.Text = "Вход";
                listFileUser.Items.Clear();
                listFileAll.Items.Clear();
                TrData.SRVLogout();
                TrData.ClearPassword();
            }
        }

        private void btnPswd_Click(object sender, EventArgs e)
        {
            if ((textBoxPswd.Text != "Введите пароль к файлам") &&
                (textBoxPswd.Text.Length > 5))
            {
                if (btnPswd.Text == "Принять пароль")
                {
                    textBoxPswd.ReadOnly = true;
                    btnPswd.Text = "Изменить пароль";
                    TrData.SetPassword(textBoxPswd.Text);
                }
                else
                {
                    textBoxPswd.Text = "";
                    textBoxPswd.ReadOnly = false;
                    btnPswd.Text = "Принять пароль";
                    TrData.ClearPassword();
                }
            }
            else
            {
                MessageBox.Show("Пароль должен состоять минимум из 6 символов (любых)");
            }
        }

////////////////////////////////////////////////////////////////////////////////
///////////// кнопки управления файлами ////////////////////////////////////////
        
        private void btnSend_Click(object sender, EventArgs e)
        {
            if ((NetThrd != null) &&
                (NetThrd.ThreadState == ThreadState.Stopped))
                NetThrd = null;

            if (NetThrd == null)
            {
                FrmWait frmWait = new FrmWait();

                if (TrData.Login.Length != 0)
                {
                    openFileDlg.ShowDialog();

                    if (openFileDlg.FileName != "")
                    {
                        NetThrd = new Thread(new ParameterizedThreadStart(ThrdSendFile));
                        NetThrd.Start(frmWait);
                        frmWait.Start();
                        
                        ReloadLists();
                    }
                }
                else
                    MessageBox.Show("Введите логин и нажмите кнопку Вход");
            }
        }

        private void ThrdSendFile(object obj)
        {
            FrmWait frmWait = obj as FrmWait;
            TrData.SRVSendFile(frmWait, openFileDlg.FileName, openFileDlg.SafeFileName);
            frmWait.End();
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            if ((NetThrd != null) &&
                (NetThrd.ThreadState == ThreadState.Stopped))
                NetThrd = null;

            if (NetThrd == null)
            {
                FrmWait frmWait = new FrmWait();

                if (TrData.Login.Length != 0)
                {
                    string servname;    //сюда поместим имя файла, как он зовётся на сервере
                    if (listFileUser.SelectedIndex != -1)
                        servname = listFileUser.GetItemText(listFileUser.SelectedItem);
                    else if (listFileAll.SelectedIndex != -1)
                        servname = listFileAll.GetItemText(listFileAll.SelectedItem);
                    else
                    {
                        MessageBox.Show("Выберите файл");
                        return;
                    }

                    saveFileDlg.ShowDialog();

                    if (saveFileDlg.FileName != "")
                    {
                        NetThrd = new Thread(new ParameterizedThreadStart(ThrdLoadFile));
                        object[] parms = new object[3];
                        parms[0] = frmWait;
                        parms[1] = servname;
                        parms[2] = saveFileDlg.FileName;
                        NetThrd.Start(parms);
                        frmWait.Start();
                        saveFileDlg.FileName = "";
                    }
                }
                else
                {
                    MessageBox.Show("Введите логин и нажмите кнопку Вход");
                }
            }
        }

        private void ThrdLoadFile(object obj)
        {
            object[] parms = obj as object[];
            FrmWait frmWait = parms[0] as FrmWait;

            if (TrData.Login.Length != 0)
            {
                string servname = parms[1] as string;    //сюда поместим имя файла, как он зовётся на сервере
                string fname = parms[2] as string;
               
                TrData.SRVGetFile(frmWait, fname, servname);
                frmWait.End();
            }
            else
            {
                frmWait.End();
                MessageBox.Show("Введите логин и нажмите кнопку Вход");
            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if ((NetThrd != null) &&
                (NetThrd.ThreadState == ThreadState.Stopped))
                NetThrd = null;

            if (NetThrd == null)
            {
                FrmWait frmWait = new FrmWait();
                NetThrd = new Thread(new ParameterizedThreadStart(ThrdDelete));
                NetThrd.Start(frmWait);
                frmWait.Start();
                ReloadLists();
            }

        }

        private void ThrdDelete(object obj)
        {
            FrmWait frmWait = obj as FrmWait;
            if (TrData.Login.Length != 0)
            {
                string name;
                if (listFileUser.SelectedIndex != -1)
                    name = listFileUser.GetItemText(listFileUser.SelectedItem);
                else
                {
                    frmWait.End();
                    MessageBox.Show("Выберите файл для удаления(можно удалять только свои файлы)");
                    return;
                }

                TrData.SRVDelete(name);
                frmWait.End();
            }
            else
            {
                frmWait.End();
                MessageBox.Show("Введите логин и нажмите кнопку Вход");
            }
        }


        private void btnReload_Click(object sender, EventArgs e)
        {
            if (TrData.Login.Length != 0)
                ReloadLists();
        }


        private void ReloadLists()
        {
            if ((NetThrd != null) &&
                (NetThrd.ThreadState == ThreadState.Stopped))
                NetThrd = null;

            if (NetThrd == null)
            {
                string[] files;
                listFileUser.Items.Clear();
                listFileAll.Items.Clear();
                files = TrData.SRVGetOtherFiles();
                for (int i = 0; i < files.Length; i++)
                    listFileAll.Items.Add(files[i]);

                files = TrData.SRVGetUserFiles();
                for (int i = 0; i < files.Length; i++)
                    listFileUser.Items.Add(files[i]);
            }
        }

        
////////////////////////////////////////////////////////////////////////////////
///////////// ListBoxes  ///////////////////////////////////////////////////////

        private void listFileAll_Enter(object sender, EventArgs e)
        {
            listFileUser.SelectedIndex = -1;
        }

        private void listFileUser_Enter(object sender, EventArgs e)
        {
            listFileAll.SelectedIndex = -1;
        }

////////////////////////////////////////////////////////////////////////////////
////////////// кнопки настройки и справки //////////////////////////////////////

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (WinSett != null)
                return;
            WinSett = new FrmSettings();

            if (TrData.Login.Length != 0)
            {
                MessageBox.Show("Нужно выйти из аккаунта, иначе не все изменения возможны");
                WinSett.Сhange = false;
            }

            WinSett.localhost = TrData.localhost;
            WinSett.TimeOut = TrData.AccTimeOut;
            WinSett.ShowDialog();
            if (WinSett.result)
            {
                TrData.localhost = WinSett.localhost;
                TrData.AccTimeOut = WinSett.TimeOut;
                TrData.SaveSettings();
            }

            WinSett = null;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "Help.chm",  HelpNavigator.TableOfContents);
        }

        
        
    }
}
