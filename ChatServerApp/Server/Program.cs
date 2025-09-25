using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServerApp
{
    class Program
    {
        // Socket utama untuk mendengarkan koneksi
        private static Socket serverSocket;
        private static byte[] buffer = new byte[1024];

        static void Main(string[] args)
        {
            Console.WriteLine("=== SERVER START ===");

            // Bind server ke IP lokal dan port
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5000);

            // Buat socket TCP
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind dan listen
            serverSocket.Bind(localEndPoint);
            serverSocket.Listen(10);

            Console.WriteLine("Server listening on port 5000...");

            // Mulai menerima koneksi client secara async
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            // Supaya console tidak langsung close
            Console.ReadLine();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Terima koneksi client
                Socket clientSocket = serverSocket.EndAccept(ar);
                Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint.ToString());

                // Mulai menerima data dari client
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), clientSocket);

                // Siap menerima client lain
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AcceptCallback Error: " + ex.Message);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            int received;

            try
            {
                received = clientSocket.EndReceive(ar);
                if (received == 0)
                {
                    Console.WriteLine("Client disconnected.");
                    clientSocket.Close();
                    return;
                }

                // Ambil data dari buffer
                string text = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine("Received: " + text);

                // Balas pesan ke client
                byte[] response = Encoding.UTF8.GetBytes("Server received: " + text);
                clientSocket.BeginSend(response, 0, response.Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), clientSocket);

                // Lanjut menerima data dari client
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), clientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReceiveCallback Error: " + ex.Message);
                clientSocket.Close();
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket clientSocket = (Socket)ar.AsyncState;

            try
            {
                int sent = clientSocket.EndSend(ar);
                Console.WriteLine("Sent {0} bytes back to client.", sent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendCallback Error: " + ex.Message);
            }
        }
    }
}
