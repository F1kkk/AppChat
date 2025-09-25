using System;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== CLIENT START ===");

            // Koneksi ke server (sesuai IP dan port server)
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            Console.WriteLine("Connected to server. Ketik pesan, lalu tekan Enter. (Ketik 'exit' untuk keluar)");

            while (true)
            {
                Console.Write("You: ");
                string message = Console.ReadLine();

                if (string.IsNullOrEmpty(message)) continue;

                // Jika user mengetik "exit", keluar dari client
                if (message.ToLower() == "exit")
                    break;

                // Kirim pesan ke server
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // Terima balasan dari server
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Server: " + response);
            }

            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
