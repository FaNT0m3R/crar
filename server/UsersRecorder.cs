using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading;

namespace server
{

    /// <summary>
    /// Содержит некоторую инфу о клиентах, доступную для чтения любому из клиентов
    /// </summary>
    class UsersRecorder
    {
        private List<string> Logins = new List<string>();
        private List<CryptoStream> StrWrs = new List<CryptoStream>();
        private List<byte[]>Keys = new List<byte[]>();
        private List<bool> Accesses = new List<bool>();

        public bool NewUser(string Login, CryptoStream StrWr)
        {
            if (Logins.Contains(Login) == true)
                return false;

            Logins.Add(Login);
            StrWrs.Add(StrWr);
            Keys.Add(null);
            Accesses.Add(true);
            return true;
        }

        public bool RemoveUser(string Login)
        {
            if (Logins.Contains(Login) == false)
                return false;

            int num = Logins.IndexOf(Login);
            Logins.RemoveAt(num);
            StrWrs.RemoveAt(num);
            Keys.RemoveAt(num);
            Accesses.RemoveAt(num);
            return true;
        }

        public CryptoStream GetStreamWrByLogin(string Login)
        {
            if (Logins.Contains(Login) == false)
                return null;

            int num = Logins.IndexOf(Login);
            return StrWrs[num];
        }

        public void SetLastAccessKey(string Login,byte[] Key)
        {
            if (Logins.Contains(Login) == false)
                return;

            int num = Logins.IndexOf(Login);
            Keys[num] = Key;
            Accesses[num] = true;
        }

        public void SetLastAccessDenied(string Login)
        {
            if (Logins.Contains(Login) == false)
                return;

            int num = Logins.IndexOf(Login);
            Accesses[num] = false;
        }


        public bool GetKey(string Login, out byte[] bufKey)
        {
            bufKey = null;
            if (Logins.Contains(Login) == false)
                return false;

            int num = Logins.IndexOf(Login);

            //ожидание ответа
            while ((Accesses[num] == true) && (Keys[num] == null))       //если ключ пустой, а доступ разрешён, то
                Thread.Sleep(100);                                  //это значит, что хозяин ещё не ответил

            bool res;
            if (Accesses[num] == false)
                res = false;
            else
            {
                bufKey = Keys[num];
                res = true;
            }

            Keys[num] = null;        //сбрасываем результат
            Accesses[num] = true;
            return res;
        }
    }
}
