using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace server
{
   
    class Program
    {
        static FilesUsers Access = new FilesUsers();
        static StreamWriter ConLog = File.CreateText("cons.log");
        static UsersRecorder UsrRec = new UsersRecorder();
        
        static void Main(string[] args)
        {
            ConLog.AutoFlush = true;

        
            TcpListener listener = new TcpListener(IPAddress.Any, 1330);
            listener.Start();
            while (true)
            {
                Console.WriteLine("Ожидание нового соединения");
                ConLog.WriteLine("Ожидание нового соединения");

                // Объект AcceptTcpClient ждет соединения с клиентом
                TcpClient client = listener.AcceptTcpClient();
                // Создаём класс юзера, конструктор которого запустит поток ожидания сообщений
                
                Thread thread = new Thread(new ParameterizedThreadStart(HandledientThread));
                thread.Start(client);
            }
        }



        static private void HandledientThread(object obj)
        {
            User thisUser = new User(obj as TcpClient, ConLog, Access, UsrRec);
            thisUser.Start();
        }

    }
}






