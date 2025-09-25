using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClientApp
{
    public partial class MainWindow : Window
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected) return;

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(IpBox.Text, int.Parse(PortBox.Text));
                stream = client.GetStream();
                isConnected = true;

                ChatBox.Items.Add("[System] Connected to server.");

                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                ChatBox.Items.Add("[Error] " + ex.Message);
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (client is { Connected: true })
                {
                    int byteCount = await stream!.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0) break; // server disconnect

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    Dispatcher.Invoke(() =>
                    {
                        ChatBox.Items.Add(message);
                        ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
                    });
                }
            }
            catch
            {
                Dispatcher.Invoke(() => ChatBox.Items.Add("[System] Disconnected."));
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (stream == null || string.IsNullOrWhiteSpace(MessageBox.Text)) return;

            string msg = $"{UserBox.Text}: {MessageBox.Text}";
            byte[] data = Encoding.UTF8.GetBytes(msg);

            try
            {
                await stream.WriteAsync(data, 0, data.Length);
                Dispatcher.Invoke(() =>
                {
                    ChatBox.Items.Add("[You] " + msg);
                    ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
                    MessageBox.Clear();
                });
            }
            catch
            {
                ChatBox.Items.Add("[Error] Failed to send message.");
            }
        }
    }
}
