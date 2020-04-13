using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.IO.Compression;


namespace server
{
    
    class User
    {
        private UsersRecorder UsrRec;       //общая информация, для чтоб клиенты знали кое что друг о друге
        private TcpClient Client;
        private StreamWriter ConLog;        //сюда пишем лог
        private String Login;
        private FilesUsers Access;          //учёт файлов и их владельцев
        private CryptoStream CryRd, CryWr;  //потоки для шифрованного четнияя и записи
        private int iWr = 0;                //Количество записанных байт. Нужен для выравнивания буфера в границ 0x20.

        public User(TcpClient _Client, StreamWriter _ConLog, FilesUsers _Access, UsersRecorder _UsrRec)
        {
            Client = _Client;
            ConLog = _ConLog;
            Access = _Access;
            UsrRec = _UsrRec;
        }
        public void Start()
        {  
            RijndaelManaged CryRijnd;
            ICryptoTransform Decryptor, Encryptor;
            byte[] CryKey;
            byte[] CryIV;

            CryRijnd = new RijndaelManaged();
            
            Console.WriteLine("Клиент соединен");
            ConLog.WriteLine("Клиент соединен");


            //получаем открытый ключ от клиента.
            //создаём сеансовый открытый и закрытый ключ
            //шифруем их при помощи открытого ключа от клиента
            //отсылаем клиенту
            //для дальнейшей передачи данных используем сеановый ключ

            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    byte[] publicKey = ReadMessageNoCry();   //получили публичный ключ
                    rsa.ImportCspBlob(publicKey);

                    CryRijnd.GenerateKey();       //создали сессионный ключ

                    //этот класс специально предназначен для зашифрованного обмена ключами,
                    //  потому им и воспользуемся
                    RSAOAEPKeyExchangeFormatter keyFormatter = new RSAOAEPKeyExchangeFormatter(rsa);
                    byte[] crySessionKey = keyFormatter.CreateKeyExchange(CryRijnd.Key); //зашифровали ключ
                    byte[] mess = new byte[4 + crySessionKey.Length];
                    ToAByte(crySessionKey.Length).CopyTo(mess, 0);
                    crySessionKey.CopyTo(mess, 4);
                    Client.GetStream().Write(mess, 0, mess.Length);  //отправили клиенту сессионый ключ

                    CryRijnd.GenerateIV();

                    byte[] IV = CryRijnd.IV;
                    mess = new byte[4 + IV.Length];
                    ToAByte(IV.Length).CopyTo(mess, 0);
                    IV.CopyTo(mess, 4);
                    Client.GetStream().Write(mess, 0, mess.Length);     //отправили клиенту вектор инициализации

                    CryKey = CryRijnd.Key;
                    CryIV = CryRijnd.IV;

                    Encryptor = CryRijnd.CreateEncryptor(CryRijnd.Key, CryRijnd.IV);
                    Decryptor = CryRijnd.CreateDecryptor(CryRijnd.Key, CryRijnd.IV);

                    CryWr = new CryptoStream(Client.GetStream(), Encryptor, CryptoStreamMode.Write);
                    CryRd = new CryptoStream(Client.GetStream(), Decryptor, CryptoStreamMode.Read);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                ConLog.WriteLine("Error: {0}", e.Message);
            }


            while (Client.Connected)
            {
                byte[] received = ReadMessage(CryRd);    //ждём сообщения

                if (received == null)           //если вернулся null, то клиент разорвал соединение
                {
                    Console.WriteLine("{0}: Неожиданное завершение работы с клиентом", Login);
                    ConLog.WriteLine("{0}: Неожиданное завершение работы с клиентом", Login);
                    UsrRec.RemoveUser(Login);
                    Client.Close();
                    break;
                }

                int tmpSize = 10 * 2;           //берём первые 10 символов и из них получаем комманду
                if (tmpSize > received.Length) tmpSize = received.Length; //если сообщение короче, чем 10 символов, то берём всё сообщение

                string tmpStr = Encoding.Unicode.GetString(received, 0, tmpSize);
                string command = tmpStr.Split(' ')[0];  //либо команда - это всё сообщение, либо оно
                                                        //отделяется пробелом от данных

                //Console.WriteLine("Received: {0}", command);
                //ConLog.WriteLine("Received: {0}", command);

                if (command == "LOGIN")
                {
                    Login = GetPasString(received, 6 * 2);
                    
                    if (UsrRec.NewUser(Login, CryWr))
                    {
                        Console.WriteLine("Login: {0}", Login);
                        ConLog.WriteLine("Login: {0}", Login);
                        SendResponse(CryWr, "LOG_OK");
                    }
                    else
                    {
                        Console.WriteLine("Ошибка входа с логином {0}. Пользователь уже вошёл", Login);
                        ConLog.WriteLine("Ошибка входа с логином {0}. Пользователь уже вошёл", Login);
                        SendResponse(CryWr, "LOG_ERR");
                        Client.Close();
                        break;
                    }
                }
                else if (command == "LOGOUT")
                {
                    Console.WriteLine("Logout: {0}", Login);
                    ConLog.WriteLine("Logout: {0}", Login);
                    UsrRec.RemoveUser(Login);
                    Client.Close();
                }
                else if (command == "FILE")
                {
                    AddFile(received);
                    received = null;
                }
                else if (command == "GETOTHERF")
                {
                    GetOtherFiles(received);
                }
                else if (command == "GETUSERF")
                {
                    GetUserFiles(received);
                }
                else if (command == "GET")
                {
                    SendFile(received);
                }
                else if (command == "DELETE")
                {
                    DeleteFile(received);
                }
                else if (command == "PSWD")
                {
                    byte[] Key = new byte[0x20];
                    for (int i = 0; i < 0x20; i++)
                        Key[i] = received[5 * 2 + i];
                    UsrRec.SetLastAccessKey(Login, Key);
                }
                else if (command == "ERROR")
                {
                    UsrRec.SetLastAccessDenied(Login);
                }
                
                received = null;
                GC.Collect();
            }
            
        }
        
        
        /////////////////////////////////////////////////////////////////////////////
/////////// Функции обработки сообщений /////////////////////////////////////
        /// <summary>
        /// обработка сообщения FILE
        /// </summary>
        /// <param name="iU"> номер пользователя </param>
        /// <param name="data"> указатель на полученные от клиента данные </param>
        private void AddFile(byte[] data)
        {
            int curPos = 5 * 2;   //'FILE '     - текущая позиция для чтения в полученном сообщении
            string nameFile = GetPasString(data, 5 * 2);    //имя файла
            curPos += nameFile.Length * 2 + 1;
            int sizeFile = ToInt(data, curPos);             //размер файла
            curPos += 4;

            if (Access.AddFile(nameFile, sizeFile, Login))
            {
                try
                {
                    byte[] bfsize = new byte[4];
                    CryRd.Read(bfsize,0,4);
                    uint allFileSize = (uint)ToInt(bfsize);

                    ZipStorer zip;
                          
                    zip = ZipStorer.Create(nameFile + ".zip", "");
                    zip.EncodeUTF8 = true;

                    ZipStorer.ZipFileEntry zfe = zip.AS_Create(ZipStorer.Compression.Deflate, nameFile, DateTime.Now, "");
                    Stream zipstr = zip.AS_GetStream(zfe, 0);
                    uint rds = zip.AS_Write(zfe, zipstr, CryRd, allFileSize);
                    zip.AS_Close(zfe, zipstr, rds);
                    zip.Close();
                }
                catch (Exception e)
                {
                    Access.DeleteFile(nameFile);
                    Console.Write(e.Message);
                    Console.WriteLine("{0}: ошибка добавления файла {1}", Login, nameFile);
                    ConLog.Write(e.Message);
                    ConLog.WriteLine("{0}: ошибка добавления файла {1}", Login, nameFile);
                }

                Console.WriteLine("{0}: добавлен файл {1}", Login, nameFile);
                ConLog.WriteLine("{0}: добавлен файл {1}", Login, nameFile);
                SendResponse(CryWr, "FILE_OK");
            }
            else
            {
                Console.WriteLine("{0}: ошибка добавления файла {1}", Login, nameFile);
                ConLog.WriteLine("{0}: ошибка добавления файла {1}", Login, nameFile);
                SendResponse(CryWr, "FILE_ERR");
            }

        }

        /// <summary>
        /// обработка сообщения GETOTHERF
        /// </summary>
        /// <param name="iU"></param>
        /// <param name="data"></param>
        private void GetOtherFiles(byte[] data)
        {
            byte[] buff;
            int buffSize = 5 * 2 + 1;
            string[] AllFiles = Access.GetOtherFiles(Login);
            for (int i = 0; i < AllFiles.Length; i++)
                buffSize += AllFiles[i].Length * 2 + 1; //строки хранятся в паскаль-стиле, потому 
                                                     //  нужет ещё байт-размер для каждой строки
            buff = new byte[buffSize];

            byte[] bLIST = Encoding.Unicode.GetBytes("LIST ");
            bLIST.CopyTo(buff, 0);
            buff[5 * 2] = (byte)AllFiles.Length;
            int j = 5 * 2 + 1;
            for (int i = 0; i < AllFiles.Length; i++)
            {
                ToPasString(AllFiles[i], buff, j);
                j += AllFiles[i].Length * 2 + 1;
            }

            CryWr.Write(ToAByte(buffSize), 0, 4);
            CryWr.Write(buff, 0, buffSize);
            iWr += 4 + buffSize;
            RoundWrStream();

            Console.WriteLine("{0} получил список файлов не принадлежащих ему", Login);
            ConLog.WriteLine("{0} получил список файлов не принадлежащих ему", Login);
        }

        /// <summary>
        /// обработка сообщения GETUSERF
        /// </summary>
        /// <param name="iU"></param>
        /// <param name="data"></param>
        private void GetUserFiles(byte[] data)
        {
            byte[] buff;
            int buffSize = 5 * 2 + 1;
            string[] UsrFiles = Access.GetUserFiles(Login);
            for (int i = 0; i < UsrFiles.Length; i++)
                buffSize += UsrFiles[i].Length * 2 + 1; //строки хранятся в паскаль-стиле, потому 
                                                         //  нужет ещё байт под размер для каждой строки
            buff = new byte[buffSize];

            byte[] bLIST = Encoding.Unicode.GetBytes("LIST ");
            bLIST.CopyTo(buff, 0);
            buff[5 * 2] = (byte)UsrFiles.Length;
            int j = 5 * 2 + 1;
            for (int i = 0; i < UsrFiles.Length; i++)
            {
                ToPasString(UsrFiles[i], buff, j);
                j += UsrFiles[i].Length * 2 + 1;
            }

            CryWr.Write(ToAByte(buffSize), 0, 4);
            CryWr.Write(buff, 0, buffSize);
            iWr += 4 + buffSize;
            RoundWrStream();

            Console.WriteLine("{0} получил список файлов ему принадлежащих", Login);
            ConLog.WriteLine("{0} получил список файлов ему принадлежащих", Login);
        }

        /// <summary>
        /// обработка сообщения GET
        /// </summary>
        /// <param name="iUser"></param>
        /// <param name="data"></param>
        private void SendFile(byte[] data)
        {
            string name = GetPasString(data, 4 * 2);

            string ThisLogin = Access.GetUser(name);

            int sizeDataFile;

            if (ThisLogin == Login)      // проверкa хозяина файла
            {
                byte[] commFile = Encoding.Unicode.GetBytes("FILE ");

                FileStream inFS = File.OpenRead(name + ".zip"); 
                sizeDataFile = (int)inFS.Length;

                int sizeMess = commFile.Length + 4;;
                int sizeFile = Access.GetFileSize(name);

                //первое сообщение с инфой о файле
                CryWr.Write(ToAByte(sizeMess), 0, 4);               //размер вcего сообщения
                CryWr.Write(commFile, 0, commFile.Length);          //команда

                CryWr.Write(ToAByte(sizeFile), 0, 4);               //длина файла(до шифрования)

                //второе сообщение с файлом
                CryWr.Write(ToAByte(sizeDataFile), 0, 4);               //размер вcего файла
                int rec = 0;
                while (rec < sizeDataFile)
                {
                    int bufSize = Math.Min(1048576, sizeDataFile - rec);
                    byte[] bufFile = new byte[bufSize];

                    inFS.Read(bufFile, 0, bufSize);
                    CryWr.Write(bufFile, 0, bufSize);            //данные файла
                    rec += bufSize;
                }
                inFS.Dispose();

                iWr += 4 + sizeMess + 4 + sizeDataFile;
                RoundWrStream();
            }
            else
            {
                CryptoStream TStrWr = UsrRec.GetStreamWrByLogin(ThisLogin);
                if (TStrWr == null)
                {
                    byte[] bCommand = Encoding.Unicode.GetBytes("ERROR");
                    CryWr.Write(ToAByte(bCommand.Length), 0, 4);    //размер сообщения(без этих 4х байт)
                    CryWr.Write(bCommand, 0, bCommand.Length);      //команда
                    iWr += 4 + bCommand.Length;
                    RoundWrStream();
                }
                else
                {
                    SendResponse(TStrWr, "ACCESS ", Login);
                    
                    byte[] Key;
                    if (!UsrRec.GetKey(ThisLogin, out Key))
                    {
                        Console.WriteLine("{0}: неудачная попытка получить файл {1}", Login, name);
                        ConLog.WriteLine("{0}: неудачная попытка получить файл {1}", Login, name);
                        SendResponse(CryWr, "ERROR");
                        return;
                    }

                    byte[] commFilePswd = Encoding.Unicode.GetBytes("FILEPSWD ");
                        
                    byte[] sizeFile = ToAByte(Access.GetFileSize(name));

                    FileStream inFS = File.OpenRead(name + ".zip");
                    sizeDataFile = (int)inFS.Length;

                    int sizeMess = commFilePswd.Length + 0x20 + 4;

                    //первое сообщение с инфой о файле
                    CryWr.Write(ToAByte(sizeMess), 0, 4);           //размер сообщения(без этих 4х байт)
                    CryWr.Write(commFilePswd, 0, commFilePswd.Length); //команда
                    CryWr.Write(Key,0,0x20);                        //ключ
                    CryWr.Write(sizeFile, 0, 4);                    //размер файла

                    //второе сообщение с файлом
                    CryWr.Write(ToAByte(sizeDataFile), 0, 4);               //размер вcего файла
                    int rec = 0;
                    while (rec < sizeDataFile)                       //данные файла
                    {
                        int bufSize = Math.Min(1048576, sizeDataFile - rec);
                        byte[] bufFile = new byte[bufSize];
                        inFS.Read(bufFile, 0, bufSize);

                        CryWr.Write(bufFile, 0, bufSize);
                        rec += bufSize;
                    }
                    inFS.Dispose();

                    iWr += 4 + sizeMess + 4 + sizeDataFile;
                    RoundWrStream();
                }
            }

            Console.WriteLine("{0}: получил файл {1}", Login, name);
            ConLog.WriteLine("{0}: получил файл {1}", Login, name);
        }

        /// <summary>
        /// обработка сообщения DELETE
        /// </summary>
        /// <param name="iU"></param>
        /// <param name="data"></param>
        private void DeleteFile(byte[] data)
        {
            string name = GetPasString(data, 7 * 2);
            if (Login != Access.GetUser(name))
            {
                Console.WriteLine("{0}: Неудачная попытка удалить файл {1}", Login, name);
                ConLog.WriteLine("{0}: Неудачная попытка удалить {1}", Login, name);
                SendResponse(CryWr, "ERROR");
                return;
            }


            ////////////////////////
            byte[] Key = new byte[0x20];
            byte[] IV = new byte[0x10];

            for (int i = 0; i < 0x20; i++)
                Key[i] = data[i + 7 * 2 + 1 + name.Length * 2];

            for (int i = 0; i < 0x10; i++)
                IV[i] = data[i + 7 * 2 + 1 + name.Length * 2 + 0x20];

            byte[] DataFile = new byte[2*1048576];

            //bool DecryptOk = false;
            //////////////////////////// разархивирование
            try
            {
                using (MemoryStream msResult = new MemoryStream())
                {
                    using (RijndaelManaged rijAlg = new RijndaelManaged())
                    {
                        rijAlg.Key = Key;
                        rijAlg.IV = IV;

                        ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                        ZipStorer zip = ZipStorer.Open(name + ".zip", FileAccess.Read);
                        List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                        Stream zs = zip.ExtractStream(dir[0]);


                        using (CryptoStream csDecrypt = new CryptoStream(zs, decryptor, CryptoStreamMode.Read))
                        {

                            
                            //int iRead;

                            zip.ExtractReadInNull(dir[0], csDecrypt, 0, (int)dir[0].FileSize);

                            /*do
                            {
                                msResult.Seek(0, SeekOrigin.Begin);
                                iRead = zip.ExtractRead(dir[0], csDecrypt, msResult, 0, 2 * 1048576);
                            } while (iRead == 1048576 * 2);
                            */
                            
                        }

                        zs.Dispose();
                        zip.Dispose();
                    }
                    
                    msResult.Close();
                }

            }
            catch (Exception)
            {
                Console.WriteLine("{0}: Неудачная попытка удалить файл {1}", Login, name);
                ConLog.WriteLine("{0}: Неудачная попытка удалить {1}", Login, name);
                SendResponse(CryWr, "ERROR");
                return;
            }
            
            DataFile = null;
            ////////////////////////

            if (Access.DeleteFile(name))
                File.Delete(name + ".zip");
            else
            {
                Console.WriteLine("{0}: Неудачная попытка удалить файл {1}", Login, name);
                ConLog.WriteLine("{0}: Неудачная попытка удалить {1}", Login, name);
                SendResponse(CryWr, "ERROR");
                return;
            }

            Console.WriteLine("{0}: удалён файл {1}", Login, name);
            ConLog.WriteLine("{0}: удалён файл {1}", Login, name);

            SendResponse(CryWr, "OK");
        }
        


/////////////////////////////////////////////////////////////////////////////////
/////////// Функции для отправки/приёма данных //////////////////////////////////
        /// <summary>
        /// читает одно сообщение от клиента. Первые 4 байта с размером остального сообщения пропускается(они только для этой функции)
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private byte[] ReadMessage(CryptoStream streamRd)
        {
            bool nullBuff;
            byte[] buffer;
            byte[] btSize = new byte[4];
            
            do 
            {
                int bufPos = 0;
                nullBuff = false;
                try
                {
                    streamRd.Read(btSize, 0, 4);
                    buffer = new byte[ToInt(btSize)];
                   
                    do
                    {
                        int bufSize = 1048576;
                        if (bufSize > buffer.Length - bufPos)
                            bufSize = buffer.Length - bufPos;

                        bufPos += streamRd.Read(buffer, bufPos, bufSize);
                    } while (buffer.Length - bufPos != 0);

                    if (buffer.Length > 4)
                    {
                        if (buffer[4] == 0)
                            nullBuff = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    ConLog.WriteLine(e);
                    return null;
                }
            } while (nullBuff);

            return buffer;
        }


        private byte[] ReadMessageNoCry()
        {
            byte[] buffer;
            byte[] btSize = new byte[4];
            int totalRead = 0;

            Client.GetStream().Read(btSize, 0, 4);
            buffer = new byte[ToInt(btSize)];

            do
            {
                int read = Client.GetStream().Read(buffer, totalRead,
                            buffer.Length - totalRead);
                totalRead += read;
            } while (buffer.Length - totalRead != 0);

            return buffer;
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
            ToAByte(szB-4).CopyTo(B, 0);
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


        private void SendResponse(CryptoStream streamWr, string command)
        {
            try      //на случай, если сокет разъединился, перехватим ошибку ввода-вывода
            {
                int iWrt = 0;
                byte[] bMess = Encoding.Unicode.GetBytes(command);

                int sizeMess = bMess.Length;

                streamWr.Write(ToAByte(sizeMess), 0, 4);
                streamWr.Write(bMess, 0, bMess.Length);
                iWrt += 4 + sizeMess;

                byte[] B;
                int szB = (0x10 - (iWrt & 0xF)) - 4 + 0x20;
                B = new byte[szB];
                B.Initialize();
                streamWr.Write(ToAByte(szB), 0, 4);
                streamWr.Write(B, 0, szB);
                B = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ConLog.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// отправляет команду и последующую строку
        /// </summary>
        /// <param name="command"></param>
        /// <param name="str"></param>
        private void SendResponse(CryptoStream streamWr ,string command, string str)
        {
            try      //на случай, если сокет разъединился, перехватим ошибку ввода-вывода
            {
                byte[] bMess = Encoding.Unicode.GetBytes(command);
                byte[] bStr = new byte[str.Length * 2 + 1];
                int iWrt = 0;

                ToPasString(str, bStr, 0);

                int sizeMess = bMess.Length + bStr.Length;

                streamWr.Write(ToAByte(sizeMess), 0, 4);
                streamWr.Write(bMess, 0, bMess.Length);
                streamWr.Write(bStr, 0, bStr.Length);
                iWrt += 4 + sizeMess;

                byte[] B;
                int szB = (0x10 - (iWrt & 0xF)) - 4 + 0x20;
                B = new byte[szB];
                B.Initialize();
                streamWr.Write(ToAByte(szB), 0, 4);
                streamWr.Write(B, 0, szB);
            }
            catch (Exception e)
            {
                ConLog.WriteLine(e.Message);
                Console.WriteLine(e.Message);
            }
        }

//////////////////////////////////////////////////////////////////////////////////
////////////// Функции для конвертации данных и их байтового представления ///////
    
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

        /// <summary>
        /// Преобразует строку в массив байтов в стиле паскаль
        /// </summary>
        /// <param name="Str"></param>
        /// <param name="arr">Массив для вывода</param>
        /// <param name="index">Позиция в принимающем массиве, куда писать строку</param>
        static void ToPasString(string Str, byte[] arr, int index)
        {
            arr[index] = (byte)Str.Length;

            byte[] bStr = Encoding.Unicode.GetBytes(Str);
            for (int i = 0; i < Str.Length * 2; i++)
                arr[i + index + 1] = bStr[i];

        }

        /// <summary>
        /// разбивает целое число в массив байтов
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

        /// <summary>
        /// Собирает из первых 4х элементов байтового массива целое число
        /// </summary>
        /// <param name="arr4">Массив, в котором как минимум 4 элемента</param>
        /// <returns></returns>
        private int ToInt(byte[] arr4)
        {
            return (arr4[0] * 0x1000000 + arr4[1] * 0x10000 + arr4[2] * 0x100 + arr4[3]);
        }

        /// <summary>
        /// преобразовывает массив из 4х байт в целое число. 
        /// </summary>
        /// <param name="arr4">массив byte[] с данными минимум с 4-мя элементами</param>
        /// <param name="index">адрес в массиве, с которого начнётся чтение байт</param>
        /// <returns></returns>
        private static int ToInt(byte[] arr4, int index)
        {
            return (arr4[0 + index] * 0x1000000 +
                    arr4[1 + index] * 0x10000 +
                    arr4[2 + index] * 0x100 +
                    arr4[3 + index]);
        }
    }
}
