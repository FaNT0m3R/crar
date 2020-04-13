using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Collections.Generic;

namespace client
{
    class Transp
    {
        public int AccTimeOut = 10;

        public string localhost = "127.0.0.1";
        public int port = 1330;

        public String Login = "";
        private TcpClient TPServer;     //соединение с сервером

        private byte[] MyIV = { 0x6C, 0x3D, 0x65, 0xAD, 0x87, 0x1F, 0x93, 0xAC, 
                                0x11, 0xD5, 0x5C, 0x09, 0xC6, 0x8B, 0x6C, 0x6F };
        
        private byte[] PassSalt = {0xA5, 0x78, 0xA2, 0x25, 0x85, 0x15, 0x4E, 0xAC, 
                          0x45, 0x20, 0x2A, 0x60, 0x55, 0x8B, 0x64, 0x77};

        private RijndaelManaged CryRijnd;
        private byte[] MyKey;
        
        private CryptoStream CryWr, CryRd;
        private ICryptoTransform Decryptor, Encryptor;
       
        

        private int iWr = 0;

        //поток ThreadNetMess занимется приёмом сообщений. обрабатывает он только 'ACCESS'
        //остальные просто записывает в следующие переменные, которые потом обработают
        //соответствующие функции. После прочтения они сбрaсываются в null
        private byte[] lastRec = null;      //последнее сообщение от сервера 
        
        public Transp()
        {
            LoadSettings();
        }

        public void SetPassword(string pswd)
        {
            PasswordDeriveBytes PassGen = new PasswordDeriveBytes(pswd, PassSalt);
            MyKey = PassGen.GetBytes(32);
        }

        public void ClearPassword()
        {
            MyKey = null;
        }


    //////////////////////////////////////
    //////////////////////////////////////
    //////////////////////////////////////
    
        private void ThreadNetMess(Object obj)
        {
            while ((TPServer != null) && (TPServer.Connected))
            {
                byte[] received = ReadMessage();    //ждём сообщения

                if (received == null)           //если вернулся null, то клиент разорвал соединение
                {
                    if (TPServer!=null)
                        TPServer.Close();
                    break;
                }

                int tmpSize = 10 * 2;           //берём первые 10 символов и из них получаем комманду
                if (tmpSize > received.Length) tmpSize = received.Length; //если сообщение короче, чем 10 символов, то берём всё сообщение

                string tmpStr = Encoding.Unicode.GetString(received, 0, tmpSize);
                string command = tmpStr.Split(' ')[0];  //либо команда - это всё сообщение, либо оно 
                                                        //отделяется пробелом от данных

                if (command == "ACCESS")
                {
                    string UsAccessLogin = GetPasString(received,7*2);
                    AccessQuestion aq = new AccessQuestion();

                    if (aq.Show(UsAccessLogin,Login,AccTimeOut))
                        SendResponse("PSWD ", MyKey);
                    else
                        SendResponse("ERROR");

                }
                else if (command == "FILE")             //эти if'ы нужны для того, чтобы в ответ не принимался всякий мусор(на всякий случай, мало ли)
                {
                    lastRec = received;
                    while (lastRec != null)
                        Thread.Sleep(50);
                }
                else if (command == "FILEPSWD")
                {
                    lastRec = received;
                    while (lastRec != null)
                        Thread.Sleep(50);
                }
                else if (command == "ERROR")
                {
                    lastRec = received;
                }
                else if (command == "LIST")
                {
                    lastRec = received;
                }
                else if (command == "LOG_OK")
                {
                    lastRec = received;
                }
                else if (command == "LOG_ERR")
                {
                    lastRec = received;
                }
                else if (command == "OK")
                {
                    lastRec = received;
                }
                else if (command == "FILE_OK")
                {
                    lastRec = received;
                }
                else if (command == "FILE_ERR")
                {
                    lastRec = received;
                }


                received = null;
                GC.Collect();
            }
        }
        
    //////////////////////////////////////
    //////////////////////////////////////
    //////////////////////////////////////
        
        public bool SRVLogin()
        {
            
            try
            {
                TPServer = new TcpClient(localhost, port);
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }


            //Фишка алгоритма RSA в том, что открытым ключом можно только зашифровать, расшифровывают 
            // закрытым ключом. Потому с помощью него мы передадим закрытый сессионый ключ,
            //а дальше будем использовать Rijndael-шифрование, потому что им удобнее работать с потоками
            
            //Обычный фокус при шифровании: сначала даём серверу наш открытый ключ
            //сервер им шифрует свой сеансовый открытый и закрытый ключ
            //мы получаем от севера его ключи и расшифровываем(у нас-то закрытый ключ есть)
            //затем передаём данные использую ключи сервера
            //Получаем, что мы ни разу не передали через сеть закрытый ключ в незашифрованном виде
            // и оба оказались с подходящим закрытым и открытым ключом


            CryRijnd = new RijndaelManaged();           //объект класса шифрования


            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                byte[] RSAKeyPublic = RSA.ExportCspBlob(false);     //берём открытый ключ для RSА-шифратора
                byte[] mess = new byte[4 + RSAKeyPublic.Length];
                ToAByte(RSAKeyPublic.Length).CopyTo(mess, 0);
                RSAKeyPublic.CopyTo(mess, 4);
                TPServer.GetStream().Write(mess, 0, mess.Length);       //отправили публичный ключ

                byte[] encSessionKey = ReadMessageNoCry();   //получили сессионый ключ

                //этот класс специально предназначен для зашифрованного обмена ключами,
                //потому им и воспользуемся
                RSAOAEPKeyExchangeDeformatter keyDeformatter = new RSAOAEPKeyExchangeDeformatter(RSA);
                byte[] CryKey = keyDeformatter.DecryptKeyExchange(encSessionKey);
                byte[] CryIV = ReadMessageNoCry();

                CryRijnd.Key = CryKey;              //устанавливаем в шифровщик сессионный ключ
                CryRijnd.IV = CryIV;

                        //шифратор и дешифратор. Нужны для создания потоков шифрования
                Encryptor = CryRijnd.CreateEncryptor();
                Decryptor = CryRijnd.CreateDecryptor();

                             //создаём потоки чтения и записи для шифрования на основе сетевого потока
                CryWr = new CryptoStream(TPServer.GetStream(), Encryptor, CryptoStreamMode.Write);
                CryRd = new CryptoStream(TPServer.GetStream(), Decryptor, CryptoStreamMode.Read);
            }


                            //Запускаем поток ожидания сообщений
            Thread thread = new Thread(new ParameterizedThreadStart(ThreadNetMess));
            thread.Start();


            SendResponse("LOGIN ", Login);

            while (lastRec == null)
                Thread.Sleep(100);

            int tmpSize = 10 * 2;                //берём в строку только первые 10 символов
            if (tmpSize > lastRec.Length) tmpSize = lastRec.Length; //или сколько там этих символов есть в сообщении

            string tmpStr = Encoding.Unicode.GetString(lastRec, 0, tmpSize);
            string command = tmpStr.Split(' ')[0];

            lastRec = null;

            if (command == "LOG_OK")
                return true;

            if (TPServer != null)
                if (TPServer.Connected)
                    TPServer.Close();
            TPServer = null;

            return false;
        }

        public void SRVLogout()
        {
            if (TPServer != null)
                if (TPServer.Connected)
                {
                    SendResponse("LOGOUT");
                    TPServer.Close();
                }
            TPServer = null;
        }

        public bool SRVSendFile(FrmWait frmWait,string path, string name)
        {
            byte[] bufFile;
            int FileSize;

            if (MyKey == null)
            {
                frmWait.End();
                MessageBox.Show("Введите пароль");
                return false;
            }

            //шифруем файл
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = MyKey;
                rijAlg.IV = MyIV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor();

                try
                {
                    using (FileStream inFS = File.OpenRead(path))
                    {
                        using (CryptoStream csRijnd = new CryptoStream(inFS, encryptor, CryptoStreamMode.Read))
                        {
             
                
                            byte[] commFile = Encoding.Unicode.GetBytes("FILE ");
                            byte[] nameFile = Encoding.Unicode.GetBytes(name);
                            byte[] sizeName = { (byte)name.Length };

                            FileSize = (int)inFS.Length;
                            int allBufSize = (int)(inFS.Length & 0xFFFFFFF0) + 0x10;
                            bufFile = new byte[1048576];    //нужно округлять размер до границы в 0x10

                            int sizeMess = commFile.Length + 1 + nameFile.Length + 4;// +allBufSize;
                            try
                            {
                                //первое сообщение с инфой о файле
                                CryWr.Write(ToAByte(sizeMess), 0, 4);               //размер вcего сообщения
                                CryWr.Write(commFile, 0, commFile.Length);          //команда
                
                                CryWr.Write(sizeName, 0, 1);                        //длина имени файла
                                CryWr.Write(nameFile, 0, nameFile.Length);          //имя файла
                                CryWr.Write(ToAByte(FileSize), 0, 4);               //длина файла(до шифрования)

                                //второе сообщение с файлом
                                CryWr.Write(ToAByte(allBufSize), 0, 4);               //размер вcего файла
                                int rec = 0;
                                while (rec < allBufSize)
                                {
                                    int bufSize = Math.Min(1048576, allBufSize - rec);

                                    csRijnd.Read(bufFile, 0, bufSize);
                                    CryWr.Write(bufFile, 0, bufSize);            //данные файла
                                    rec += bufSize;
                                }
                
                                iWr += 4 + sizeMess + 4 + allBufSize;
                                RoundWrStream();
                            }
                            catch (IOException e)
                            {
                                frmWait.End();
                                MessageBox.Show(e.Message);
                            }

                            
                        }
                    }
                }
                catch (IOException e)
                {
                    frmWait.End();
                    MessageBox.Show(e.Message);
                    return false;
                }
            }

            while (lastRec == null)     //ждём ответа от сервера
                Thread.Sleep(100);

            int tmpSize = 8 * 2;                //берём в строку только первые 8 символов
            if (tmpSize > lastRec.Length) tmpSize = lastRec.Length; //или сколько там этих символов есть в сообщении

            string tmpStr = Encoding.Unicode.GetString(lastRec, 0, tmpSize);
            string command = tmpStr.Split(' ')[0];

            lastRec = null;
            frmWait.End();

            if (command == "FILE_OK")
                return true;
            else if (command == "FILE_ERR")
                return false;
            return false;
        }

        public string[] SRVGetOtherFiles()
        {
            SendResponse("GETOTHERF");

            while (lastRec == null)
                Thread.Sleep(100);

            string[] res;

            if (lastRec.Length < 5 * 2 + 1)
            {
                res = new string[0];
            }
            else
            {
                int numFiles = lastRec[5 * 2];
                res = new string[numFiles];

                int j = 5 * 2 + 1;
                for (int i = 0; i < numFiles; i++)
                {
                    res[i] = GetPasString(lastRec, j);
                    j += res[i].Length * 2 + 1;
                }
            }

            lastRec = null;

            return res;
        }

        public string[] SRVGetUserFiles()
        {
            SendResponse("GETUSERF");

            while (lastRec == null)
                Thread.Sleep(100);

            string[] res;

            if (lastRec.Length < 5 * 2 + 1)
            {
                res = new string[0];
            }
            else
            {
                int numFiles = lastRec[5 * 2];
                res = new string[numFiles];

                int j = 5 * 2 + 1;
                for (int i = 0; i < numFiles; i++)
                {
                    res[i] = GetPasString(lastRec, j);
                    j += res[i].Length * 2 + 1;
                }
            }
            lastRec = null;

            return res;
        }

        /// <summary>
        /// Запрашивает архив с сервера
        /// </summary>
        /// <param name="path">путь, куда будет распакован и расшифрован файл</param>
        /// <param name="name">имя файла</param>
        public void SRVGetFile(FrmWait frmWait, string path, string name)
        {
            if (MyKey == null)
            {
                MessageBox.Show("Введите пароль");
                return;
            }


            // сразу проверим, сможем ли мы записать в файл данные, иначе скачивать бессмысленно
            if (File.Exists(path))      
                if ((File.GetAttributes(path) != FileAttributes.Normal) &&
                    (File.GetAttributes(path) != FileAttributes.Archive))
                {
                    MessageBox.Show("Невозможно перезаписать файл");
                    return;
                }
                
            
            SendResponse("GET ", name);

            while (lastRec == null)
                Thread.Sleep(100);

            int tmpSize = 10 * 2;                //берём в строку только первые 10 символов
            if (tmpSize > lastRec.Length) tmpSize = lastRec.Length; //или сколько там этих символов есть в сообщении

            string tmpStr = Encoding.Unicode.GetString(lastRec, 0, tmpSize);
            string command = tmpStr.Split(' ')[0];


            int posData;
            int sizeFile;
            byte[] ThisKey;


            if (command == "FILE")
            {
                ThisKey = MyKey;
                sizeFile = ToInt(lastRec, 5 * 2);  //размер файла
                posData = 5 * 2 + 4;
            }
            else if (command == "FILEPSWD")
            {
                ThisKey = new byte[0x20];
                for (int i = 0; i < 0x20; i++)
                    ThisKey[i] = lastRec[9 * 2 + i];

                sizeFile = ToInt(lastRec, 9 * 2 + 0x20);  //размер файла
                posData = 9 * 2 + 4 + 0x20;

            }
            else
            {
                frmWait.End();
                lastRec = null;
                MessageBox.Show("Не удалось скачать файл");
                return;
            }


            try
            {

                using (FileStream inFS = File.Create(path))
                {
                    using (RijndaelManaged rijAlg = new RijndaelManaged())
                    {
                        rijAlg.Key = ThisKey;
                        rijAlg.IV = MyIV;

                        ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                        using (CryptoStream csDecrypt = new CryptoStream(inFS, decryptor, CryptoStreamMode.Write))
                        {
                            MemoryStream msInp = new MemoryStream(lastRec, posData, lastRec.Length - posData);
                            FileStream fsTemp = File.Create(path + ".tmp");

                            byte[] bfsize = new byte[4];
                            CryRd.Read(bfsize, 0, 4);
                            int allFileSize = ToInt(bfsize);

                            int rec = 0;
                            while (rec < allFileSize)
                            {
                                int bufSize = Math.Min(1048576, allFileSize - rec);
                                byte[] bufFile = new byte[bufSize];

                                CryRd.Read(bufFile, 0, bufSize);
                                fsTemp.Write(bufFile, 0, bufSize);
                                rec += bufSize;
                            }

                            fsTemp.Seek(0, SeekOrigin.Begin);
                            msInp.Seek(0, SeekOrigin.Begin);
                            ZipStorer zip = ZipStorer.Open(fsTemp, FileAccess.Read);
                            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
                            zip.ExtractFile(dir[0], csDecrypt);
                            zip.Close();
                            zip.Dispose();

                            fsTemp.Close();
                            File.Delete(path + ".tmp");
                            msInp.Dispose();
                        }
                    }
                }

            }
            catch (Exception e)
            {
                frmWait.End();
                lastRec = null;
                GC.Collect();
                MessageBox.Show(e.Message);
                return;
            }

            lastRec = null;
            GC.Collect();

            frmWait.End();
            MessageBox.Show("Файл " + name + " успешно скачан");
        }


        public void SRVDelete(string name)
        {
            byte[] mess = new byte[1 + name.Length * 2 + 0x20 + 0x10];
            ToPasString(name, mess, 0);
            MyKey.CopyTo(mess, 1 + name.Length*2);
            MyIV.CopyTo(mess, 1 + name.Length*2 + 0x20);
            SendResponse("DELETE ", mess);

            while (lastRec == null)
                Thread.Sleep(100);

            int tmpSize = 5 * 2;                //берём в строку только первые 10 символов
            if (tmpSize > lastRec.Length) tmpSize = lastRec.Length; //или сколько там этих символов есть в сообщении

            string tmpStr = Encoding.Unicode.GetString(lastRec, 0, tmpSize);
            string command = tmpStr.Split(' ')[0];
            lastRec = null;

            if (command == "ERROR")
                MessageBox.Show("Не удалось удалить файл. Проверьте пароль.");

        }

////////////////////////////////////////////////////////////////////////
/////////// Функции для отправки/приёма данных /////////////////////////

        private void SendResponse(string message)
        {
            try   //на случай, если сокет разъединился, перехватим ошибку ввода-вывода
            {
                byte[] bytes = Encoding.Unicode.GetBytes(message);
                byte[] mess = new byte[4 + bytes.Length];
                ToAByte(mess.Length - 4).CopyTo(mess, 0);
                bytes.CopyTo(mess, 4);
                CryWr.Write(mess, 0, mess.Length);

                byte[] a = new byte[(mess.Length & 0xFFFFFFF0) + 0x20];
                ToAByte(a.Length - 4).CopyTo(a, 0);
                a[4] = 0;
                CryWr.Write(a, 0, a.Length);
            }
            catch (IOException) { }
        }
        
        private void SendResponse(string command, string str)
        {
            try      //на случай, если сокет разъединился, перехватим ошибку ввода-вывода
            {
                byte[] bComm = Encoding.Unicode.GetBytes(command);

                byte[] mess = new byte[((4 + bComm.Length + 1 + str.Length * 2) & 0x0FFFFFFF0) + 0x20];

                ToAByte(mess.Length - 4).CopyTo(mess, 0);
                bComm.CopyTo(mess, 4);
                ToPasString(str, mess, 4 + bComm.Length);
                CryWr.Write(mess, 0, mess.Length);

                byte[] a = new byte[0x20];
                a[4] = 0;
                ToAByte(a.Length - 4).CopyTo(a, 0);
                CryWr.Write(a, 0, a.Length);

            }
            catch (IOException) { }

        }

        private void SendResponse(string command, byte[] data)
        {
            try      //на случай, если сокет разъединился, перехватим ошибку ввода-вывода
            {
                byte[] bComm = Encoding.Unicode.GetBytes(command);

                int messSize = bComm.Length + data.Length;
                messSize = ((int)((messSize) & 0x0FFFFFFF0)) + 0x20;

                byte[] mess = new byte[messSize];

                ToAByte(mess.Length - 4).CopyTo(mess, 0);
                bComm.CopyTo(mess, 4);
                data.CopyTo(mess, 4 + bComm.Length);
                CryWr.Write(mess, 0, messSize);

                byte[] a = new byte[0x20];
                a[4] = 0;
                ToAByte(0x20 - 4).CopyTo(a, 0);
                CryWr.Write(a, 0, 0x20);

            }
            catch (IOException) { }

        }

        /// <summary>
        /// функция записывает пустое сообщение в поток для того, что бы произошла запись из CryptoStream 
        /// в NetworkStream. Иначе поток шифрования будет ждать пока прийдёт очередное сообщение и только 
        /// тогда отправит данные по сети. И Flush() не помогает, нужно или дописывать ещё одно сообщение, 
        /// либо удалять объект класса CryptoStream.
        /// </summary>
        private void RoundWrStream()
        {
            if (CryWr == null)
                return;

            byte[] B;
            int szB = (0x10 - (iWr & 0xF)) + 0x20;
            B = new byte[szB];
            ToAByte(szB - 4).CopyTo(B, 0);
            B[4] = 0;

            try
            {
                CryWr.Write(B, 0, szB);
            }
            catch (Exception)
            {
            }

            iWr = 0;
        }

        /// <summary>
        /// читает данные без шифрования. Нужно только в начала, при передаче ключей
        /// </summary>
        /// <returns></returns>
        private byte[] ReadMessageNoCry()
        {
            byte[] buffer;
            byte[] btSize = new byte[4];
            int totalRead = 0;

            TPServer.GetStream().Read(btSize, 0, 4);
            buffer = new byte[ToInt(btSize)];

            do
            {
                int read = TPServer.GetStream().Read(buffer, totalRead,
                            buffer.Length - totalRead);
                totalRead += read;
            } while (buffer.Length - totalRead != 0);

            return buffer;
        }

        /// <summary>
        /// читает одно сообщение от клиента
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private byte[] ReadMessage()
        {
            byte[] buffer;
            bool nullBuff;

            do
            {
                nullBuff = false;
                byte[] btSize = new byte[4];
                int bufPos = 0;
                try
                {
                    CryRd.Read(btSize, 0, 4);
                    buffer = new byte[ToInt(btSize)];

                    do
                    {
                        int bufSize = 1048576;
                        if (bufSize > buffer.Length - bufPos)
                            bufSize = buffer.Length - bufPos;

                        bufPos += CryRd.Read(buffer, bufPos, bufSize);
                    } while (buffer.Length - bufPos != 0);

                    if (buffer.Length > 4)
                        if (buffer[4] == 0)
                            nullBuff = true;
                }
                catch (SocketException)
                {
                    //MessageBox.Show(e.Message);
                    buffer = null;
                }
                catch (CryptographicException)
                {
                    //MessageBox.Show(e.Message);
                    buffer = null;
                }
                catch (IOException)
                {
                    //MessageBox.Show(e.Message);
                    buffer = null;
                }
            } while (nullBuff);

            return buffer;
        }

///////////////////////////////////////////////////////////////////////
//////// Функции для конвертации данных в байтовое представление //////
        
        /// <summary>
        /// преобразовывает целое число в массив байт
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private byte[] ToAByte(int a)
        {
            byte[] r = new byte[4];
            r[0] = (byte)(a >> 24 & 0xFF);
            r[1] = (byte)(a >> 16 & 0xFF);
            r[2] = (byte)(a >> 8 & 0xFF);
            r[3] = (byte)(a & 0xFF);
            return r;
        }

        // преобразовывает массив из 4х байт в целое число. 
        private static int ToInt(byte[] arr4)
        {
            return (arr4[0] * 0x1000000 + arr4[1] * 0x10000 + arr4[2] * 0x100 + arr4[3]);
        }

        /// <summary>
        /// преобразовывает массив из 4х байт в целое число. 
        /// </summary>
        /// <param name="arr4">массив byte[] с данными минимум с 4-мя элементами</param>
        /// <param name="index">адрес в массиве, с которого начнётся чтение байт</param>
        /// <returns></returns>
        private static int ToInt(byte[] arr4,int index)
        {
            return (arr4[0 + index] * 0x1000000 + 
                    arr4[1 + index] * 0x10000 + 
                    arr4[2 + index] * 0x100 + 
                    arr4[3 + index]);
        }

      

        /// <summary>
        /// Возвращает строку из массива byte
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index">позиция байта с размером строки, после этого байта идёт сама строка</param>
        /// <returns></returns>
        private string GetPasString(byte[] arr, int index)
        {
            int strSize = arr[index] * 2;

            byte[] bStr = new byte[strSize];
            for (int i = 0; i < strSize; i++)
                bStr[i] = arr[i + index + 1];

            return Encoding.Unicode.GetString(bStr);
        }

        private void ToPasString(string Str, byte[] arr, int index)
        {
            arr[index] = (byte)Str.Length;

            byte[] bStr = Encoding.Unicode.GetBytes(Str);
            for (int i = 0; i < Str.Length * 2; i++)
                arr[i + index + 1] = bStr[i];
        }


        /////////////////////////////////////
        // Функции чтения/записи настроек
        /////////////////////////////////////

        private void LoadSettings()
        {
            try
            {
                StreamReader fSetting = File.OpenText("settings.dat");

                string line;
                while (!fSetting.EndOfStream)
                {
                    line = fSetting.ReadLine();
                    string param = line.Split('=')[0].Trim();
                    
                    if (param == "ip")
                        localhost = line.Split('=')[1].Trim();
                    if (param == "timeout")
                        AccTimeOut = Convert.ToInt32(line.Split('=')[1].Trim());
                }

                fSetting.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
              
            }
        }

        public void SaveSettings()
        {
            using (StreamWriter fSetting = File.CreateText("settings.dat"))
            {
                try
                {    
                    fSetting.WriteLine("ip = " + localhost.ToString());
                    fSetting.WriteLine("timeout = " + AccTimeOut.ToString());
                    fSetting.Close();
                }
                catch (Exception)
                {
                    MessageBox.Show("Не удалось сохранить настройки. \n\rПроверьте, открыт ли файл settings.dat в других программах");
                }
            }
        }
    }
}
