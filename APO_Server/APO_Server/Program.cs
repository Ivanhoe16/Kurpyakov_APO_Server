using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;

namespace APO_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string reply = "";
            string data = null;
            //подключение к бд
            SQLiteConnection connection = new SQLiteConnection("Data Source=Clubs.db");
            connection.Open();
            SQLiteCommand command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = "CREATE TABLE IF NOT EXISTS Clubs("+
                "Name TEXT NOT NULL, League TEXT NOT NULL, City TEXT NOT NULL, Trainer TEXT NOT NULL, Stadium TEXT NOT NULL)";
            command.ExecuteNonQuery();
            //установка локальной конечной точки для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEP = new IPEndPoint(ipAddr, 11000);
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //слушаем входящие сокеты
            sListener.Bind(ipEP);
            sListener.Listen(10);
            while (true)
            {
                //ожидание входящего соединения
                Socket handler = sListener.Accept();
                //клиент найден
                byte[] bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                SQLiteCommand command1 = new SQLiteCommand();
                command1.Connection = connection;
                command1.CommandText = "INSERT INTO Clubs (Name, League, City, Trainer, Stadium) VALUES (" + data + ")";
                command1.ExecuteNonQuery();
                data = "";
                SQLiteCommand command2 = new SQLiteCommand();
                command2.Connection = connection;
                command2.CommandText = "SELECT * FROM Clubs";
                SQLiteDataReader query = command2.ExecuteReader();
                while (query.Read())
                    reply += query["Name"].ToString() + ";" + query["League"].ToString() + ";" + query["City"].ToString() + ";" + query["Trainer"].ToString() + ";" + query["Stadium"].ToString() + ";";
                byte[] msg = Encoding.UTF8.GetBytes(reply);
                reply = "";
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            connection.Close();
        }
    }
}
