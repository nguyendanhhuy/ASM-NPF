using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using EncryptionDecryptionUsingSymmetricKey;

namespace Exercise3 // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916"; 
            // Connect to server
            TcpClient client = new TcpClient();
            while(true)
            {
                try
                {
                    Console.WriteLine("==============================================================");
                    Console.WriteLine("                       VTCA CHAT SERVICE                      ");
                    Console.WriteLine("==============================================================\n");
                    Console.Write(" Input IP Address Server: ");
                    string input = Console.ReadLine() ?? "Error";
                    IPAddress ip = IPAddress.Parse(input);
                    Console.Write(" Input Port: ");
                    input = Console.ReadLine() ?? "Error";
                    int port = Convert.ToInt32(input);
                    client.Connect(ip, port);
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("\n==============================================================");
                    Console.WriteLine("Cannot connect to the server.\nWrong Ip address/port number or faulty connection.\nTry again.");
                    Console.WriteLine("==============================================================\n");
                }
            }
            Console.WriteLine("\n==============================================================");
            Console.WriteLine(" You has connected to the chat room!");
            Console.WriteLine("==============================================================\n");

            string s = "Client";
            NetworkStream ns = client.GetStream();
            byte[] buffer = Encoding.ASCII.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);

            Thread thread = new Thread(o => ReceiveMessages((TcpClient)o!));

            // Infinite loop to send message to server. Send an empty message to stop.
            
            string s2;
            string name;
            do
            {
                Console.Write("Input Your Name: ");
                name = Console.ReadLine()!;
                if (!string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("\n=========================START TYPING=========================\n");
                    break;
                }
                Console.WriteLine("\n==============================================================");
                Console.WriteLine("Failed input or empty name. Please try again.");
                Console.WriteLine("==============================================================\n");
            } while (true);

            thread.Start(client);

            while (true)
            {
                if (!string.IsNullOrEmpty((s = Console.ReadLine()!)))
                {
                    s2 = string.Concat(name, ": ", s);
                    var encryptedString = AesOperation.EncryptString(key, s2);
                    buffer = Encoding.ASCII.GetBytes(encryptedString);
                    ns.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    break;
                }
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("\n==============================================================");
            Console.WriteLine(" You has disconnected from the server!");
            Console.WriteLine("==============================================================\n");
            Console.ReadKey();
        }

        // Print out messages received from server
        static void ReceiveMessages(TcpClient client)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(AesOperation.DecryptString(key, Encoding.ASCII.GetString(receivedBytes, 0, byte_count)));
            }
        }
    }
}