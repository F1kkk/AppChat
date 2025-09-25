using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private static List<TcpClient> clients = new();
    private static TcpListener? listener;

    static async Task Main()
    {
        int port = 5000;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Running on port {port}...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            clients.Add(client);
            Console.WriteLine("[Server] New client connected.");

            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            int byteCount;
            try
            {
                byteCount = await stream.ReadAsync(buffer);
            }
            catch
            {
                Console.WriteLine("[Server] Client disconnected (error).");
                clients.Remove(client);
                break;
            }

            if (byteCount == 0)
            {
                Console.WriteLine("[Server] Client disconnected.");
                clients.Remove(client);
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
            Console.WriteLine($"[Client]: {message}");

            // Broadcast ke semua client
            foreach (var c in clients.ToList())
            {
                try
                {
                    var s = c.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await s.WriteAsync(data);
                }
                catch
                {
                    clients.Remove(c);
                }
            }
        }
    }
}
