using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using EncryptionDecryptionUsingSymmetricKey;

namespace Exercise3 // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        // Create lock object to ensures fields cannot be updated simultaneously by multiple threads
        static readonly object _lock = new object();
        // Create Dictionary to holds a list of client connections
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        static readonly Dictionary<int, string> history = new Dictionary<int, string>();

        static int port;

        static void Main(string[] args)
        {
            // counter to generates id for client connection
            int count = 1;
            do
            {
                Console.WriteLine("====================================");
                Console.WriteLine("            VTCA CHAT SERVER          ");
                Console.WriteLine("====================================\n");
                Console.Write("Input Server Port: ");
                string portStr  = Console.ReadLine() ?? "5000";
                try
                {
                    port = Convert.ToInt32(portStr);
                    if (port < 1024 || port >= 65535 || port == 10000)
                    {
                        Console.WriteLine("\n====================================");
                        Console.WriteLine("Port number must be higher than 1024.\nAnd smaller than 65535.");
                        Console.WriteLine("====================================\n");
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("\n====================================");
                    Console.WriteLine("Wrong input format or input overflowed.");
                    Console.WriteLine("====================================\n");
                }
                
            } while (true);
            Console.WriteLine("\n====================================\n");

            TcpListener ServerSocket = new TcpListener(IPAddress.Any, port);
            ServerSocket.Start();

            while (true)
            {
                // An infinite loop that accepts new connection from client
                // Add new connection to list_clients
                TcpClient client = ServerSocket.AcceptTcpClient();

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);

                // Add client to list_client if isn't a monitor client
                if (data == "Client") lock (_lock) list_clients.Add(count, client);

                Console.WriteLine("\n------------------------------------");
                Console.WriteLine("Someone connected from:\nIp Address: {0}\nPort: {1}", ((IPEndPoint)client.Client.RemoteEndPoint!).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port);
                Console.WriteLine("------------------------------------\n");
                // Create appropriate thread based on type of client
                if (data == "Client")
                {
                    // Create a new thread for client
                    Thread t = new Thread(clientHandler!);
                    t.Start(count);
                    count++;
                }
                else
                {
                    Thread t2 = new Thread(o => sendChatHistory((TcpClient)o!));
                    t2.Start(client);
                }
            }
        }

        public static void clientHandler(object o)
        {
            int id = (int)o;
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                // Infinite loop to receive message from client then send out to other clients
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                
                // Exit handler
                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                var decryptedString = AesOperation.DecryptString(key, data);
                int count = history.Count;
                history.Add(++count, decryptedString);
                sendAll(decryptedString, client);
                Console.WriteLine(decryptedString);
            }

            lock (_lock) list_clients.Remove(id);
            // Disable Socket before Close to ensures data is fully sent or received
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        // Send one client message to all others
        public static void sendAll(string data, TcpClient client)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var encryptedString = AesOperation.EncryptString(key, data + Environment.NewLine);
            byte[] buffer = Encoding.ASCII.GetBytes(encryptedString);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    if (client != c)
                    {
                        NetworkStream stream = c.GetStream();

                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        // Send chat history to all monitors
        public static void sendChatHistory(TcpClient client)
        {
            int id = 1;
            string str;
            NetworkStream stream = client.GetStream();

            while (history.ContainsKey(id))
            {
                str = history[id];
                byte[] buffer = Encoding.ASCII.GetBytes(str + Environment.NewLine);
                stream.Write(buffer, 0, buffer.Length);
                id++;
            }

            // Disable Socket before Close to ensures data is fully sent
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}