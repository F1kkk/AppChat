using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatServerApp
{
    public partial class MainWindow : Window
    {
        private TcpListener? listener;
        private readonly List<TcpClient> clients = new();
        private readonly ObservableCollection<string> clientNames = new();
        private bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            ClientList.ItemsSource = clientNames;
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            isRunning = true;

            Log("[System] Server started on port 5000.");

            while (isRunning)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    lock (clients) clients.Add(client);

                    string clientName = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                    Dispatcher.Invoke(() => clientNames.Add(clientName));
                    Log($"[Join] {clientName}");

                    _ = HandleClientAsync(client, clientName);
                }
                catch { break; }
            }
        }

        private async Task HandleClientAsync(TcpClient client, string clientName)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Log($"[Msg] {clientName}: {message}");

                    await BroadcastMessageAsync($"{clientName}: {message}", client);
                }
            }
            catch { }
            finally
            {
                lock (clients) clients.Remove(client);
                Dispatcher.Invoke(() => clientNames.Remove(clientName));
                Log($"[Leave] {clientName}");
                client.Close();
            }
        }

        private async Task BroadcastMessageAsync(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            lock (clients)
            {
                foreach (var client in clients)
                {
                    if (client == sender) continue;
                    try
                    {
                        client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                    catch { }
                }
            }
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning) return;

            isRunning = false;
            listener?.Stop();
            foreach (var c in clients) c.Close();
            clients.Clear();
            clientNames.Clear();

            Log("[System] Server stopped.");
        }

        private void Log(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.Items.Add($"{DateTime.Now:T} {msg}");
                LogBox.ScrollIntoView(LogBox.Items[LogBox.Items.Count - 1]);
            });
        }
    }
}
