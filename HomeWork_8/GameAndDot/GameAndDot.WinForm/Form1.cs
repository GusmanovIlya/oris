using GameAndDot.Shared.Enums;
using GameAndDot.Shared.Models;
using System.Net.Sockets;
using System.Text.Json;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GameAndDot.WinForm
{
    public partial class Form1 : Form
    {
        private int dotSize = 15;

        private readonly StreamReader? _reader;
        private readonly StreamWriter? _writer;

        private readonly TcpClient _client;

        private readonly Dictionary<string, Color> _playerColors = new();
        private readonly List<(string Username, Point Position)> _allDots = new();

        const string host = "127.0.0.1";
        const int port = 8888;

        private string _playerName = "";

        public Form1()
        {
            InitializeComponent();

            _client = new TcpClient();

            gameField.Paint += GameField_Paint;
            gameField.MouseClick += GameField_MouseClick;
            // отрисовка списка с цветами
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += ListBox1_DrawItem;

            try
            {
                _client.Connect(host, port); //подключение клиента

                var stream = _client.GetStream();

                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            textBox1.Visible = false;
            button1.Visible = false;

            label2.Visible = true;
            label4.Visible = true;
            usernameLbl.Visible = true;
            colorLbl.Visible = true;
            listBox1.Visible = true;
            gameField.Visible = true;

            string name = textBox1.Text.Trim();
            _playerName = name;
            usernameLbl.Text = name;

            // запускаем новый поток для получения данных
            Task.Run(() => ReceiveMessageAsync());

            var message = new EventMessage()
            {
                Type = EventType.PlayerConnected,
                Username = name
            };

            string json = JsonSerializer.Serialize(message);

            // запускаем ввод сообщений
            await SendMessageAsync(json);
        }

        // получение сообщений
        async Task ReceiveMessageAsync()
        {
            while (_client.Connected)
            {
                try
                {
                    // считываем ответ в виде строки
                    string? jsonRequest = await _reader.ReadLineAsync();
                    var messageRequest = JsonSerializer.Deserialize<EventMessage>(jsonRequest);

                    switch (messageRequest.Type)
                    {
                        case EventType.PlayerConnected:

                            Invoke(() =>
                            {
                                listBox1.Items.Clear();
                                _playerColors.Clear();

                                foreach (var name in messageRequest.Players)
                                {
                                    Color color = ColorTranslator.FromHtml(name.ColorHex);
                                    _playerColors[name.Username] = color;
                                    listBox1.Items.Add(name.Username);

                                    if (_playerColors.TryGetValue(_playerName, out var myCol))
                                    {
                                        colorLbl.Text = name.ColorHex;
                                        colorLbl.ForeColor = myCol;
                                    }
                                }
                            });
                            break;

                        case EventType.PlayerPlacedDot:
                            Invoke(() =>
                            {
                                if (messageRequest.DotX.HasValue && messageRequest.DotY.HasValue && messageRequest.Username != null)
                                {
                                    if (messageRequest.Username != _playerName)
                                    {
                                        AddDotLocally(messageRequest.Username, messageRequest.DotX.Value, messageRequest.DotY.Value);
                                    }
                                }
                            });
                            break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        // отправка сообщений
        async Task SendMessageAsync(string message)
        {
            // сначала отправляем имя
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }

        private void ListBox1_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string username = listBox1.Items[e.Index].ToString();

            e.DrawBackground();

            // Получаем цвет игрока
            Color color = _playerColors.ContainsKey(username)
                ? _playerColors[username]
                : Color.Black;

            // Рисуем текст заданным цветом
            using (Brush brush = new SolidBrush(color))
            {
                e.Graphics.DrawString(username, e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        private void GameField_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var (username, pos) in _allDots)
            {
                if (!_playerColors.TryGetValue(username, out Color color)) continue;

                using SolidBrush brush = new SolidBrush(color);
                e.Graphics.FillEllipse(brush, pos.X - dotSize / 2, pos.Y - dotSize / 2, dotSize, dotSize);
            }
        }

        private async void GameField_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            // Получаем координаты относительно gameField
            int x = e.X;
            int y = e.Y;

            // Отправляем серверу
            var moveMessage = new EventMessage
            {
                Type = EventType.PlayerPlacedDot,  // новый тип события
                Username = usernameLbl.Text,
                DotX = x,
                DotY = y
            };

            string json = JsonSerializer.Serialize(moveMessage);
            await SendMessageAsync(json);

            // Сразу рисуем у себя
            AddDotLocally(usernameLbl.Text, x, y);
        }

        private void AddDotLocally(string username, int x, int y)
        {
            _allDots.Add((username, new Point(x, y)));
            gameField.Invalidate(); // перерисовать
        }
    }
}
