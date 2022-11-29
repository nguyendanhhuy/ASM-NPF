using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Exercise3 // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Connect to server
            TcpClient client = new TcpClient();
            while(true)
            {
                try
                {
                    Console.WriteLine("==============================================================");
                    Console.WriteLine("                        MONITOR CLIENT                        ");
                    Console.WriteLine("==============================================================\n");
                    Console.Write("Input IP Address Server: ");
                    string input = Console.ReadLine() ?? "Error";
                    IPAddress ip = IPAddress.Parse(input);
                    Console.Write("Input Port: ");
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
            Console.WriteLine("\n===========================CHAT LOG===========================\n");
            NetworkStream ns = client.GetStream();

            Thread thread = new Thread(o => ReceiveMessages((TcpClient)o!));

            string s = "Monitor";
            byte[] buffer = Encoding.ASCII.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
            thread.Start(client);

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
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }
    }
}