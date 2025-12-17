using System.Net;
using System.Text;
using EmailServer.Services;

namespace EmailServer;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _staticPath;
    private bool _isRunning = false;
    private readonly CancellationTokenSource _cts = new();

    public HttpServer(string staticPath)
    {
        _staticPath = Path.GetFullPath(staticPath);
        if (!Directory.Exists(_staticPath))
            Directory.CreateDirectory(_staticPath);

        _listener.Prefixes.Add("http://127.0.0.1:8888/");
    }

    public async Task StartAsync()
    {
        if (_isRunning) return;

        _listener.Start();
        _isRunning = true;

        Console.WriteLine("Сервер запущен: http://127.0.0.1:8888/");
        Console.WriteLine("Форма доступна по корневому адресу.");
        Console.WriteLine("Для остановки введите 'stop'");

        _ = Task.Run(() => ProcessRequestsLoopAsync(_cts.Token));

        await Task.CompletedTask;
    }

    private async Task ProcessRequestsLoopAsync(CancellationToken token)
    {
        while (_isRunning && !token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (Exception ex) when (!token.IsCancellationRequested)
            {
                Console.WriteLine($"Ошибка приёма запроса: {ex.Message}");
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        string path = request.Url!.AbsolutePath.ToLowerInvariant();

        try
        {
            // GET / — отдаём форму
            if (request.HttpMethod == "GET" && (path == "/" || path == "/index.html"))
            {
                await ServeStaticFile(response, "index.html");
                return;
            }

            // POST / — обработка отправленной формы
            if (request.HttpMethod == "POST" && path == "/")
            {
                // Читаем тело POST-запроса
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();

                // Парсим данные формы (format: email=...&message=...)
                var formData = new Dictionary<string, string>();
                foreach (var pair in body.Split('&'))
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = Uri.UnescapeDataString(parts[0]);
                        string value = Uri.UnescapeDataString(parts[1]);
                        formData[key] = value;
                    }
                }

                // Получаем значения из формы
                string userEmail = formData.GetValueOrDefault("email", "").Trim();
                string userMessage = formData.GetValueOrDefault("message", "").Trim();

                // Простая валидация
                if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userMessage))
                {
                    SendTextResponse(response, 400, "Пожалуйста, заполните оба поля: email и сообщение.");
                    return;
                }

                try
                {
                    await EmailService.SendAsync(
                        toEmail: userEmail, 
                        subject: "Спасибо за ваше сообщение!",
                        htmlBody: $@"
                            <h2>Здравствуйте!</h2>
                            <p>Вы только что отправили сообщение через мой сайт:</p>
                            <div style='background:#f4f4f4; padding:15px; border-left:4px solid #007bff; margin:20px 0;'>
                                {userMessage.Replace("\n", "<br>")}
                            </div>
                            <p>Я получил его и скоро отвечу вам на этот адрес: <strong>{userEmail}</strong>.</p>
                            <p>С уважением,<br><em>Ваше имя или название сайта</em></p>
                        "
                    );

                    SendTextResponse(response, 200, "Сообщение успешно отправлено! Проверьте свою почту.");
                }
                catch (Exception ex)
                {
                    // Если отправка не удалась — логируем и сообщаем пользователю
                    Console.WriteLine($"Ошибка отправки письма на {userEmail}: {ex.Message}");
                    SendTextResponse(response, 500, "Не удалось отправить сообщение. Попробуйте позже.");
                }

                return;
            }

            // 404 для всех остальных путей
            SendTextResponse(response, 404, "Страница не найдена");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            SendTextResponse(response, 500, "Внутренняя ошибка сервера");
        }
    }

    // Отдача статического файла html
    private async Task ServeStaticFile(HttpListenerResponse response, string fileName)
    {
        string filePath = Path.Combine(_staticPath, fileName);

        if (!File.Exists(filePath))
        {
            SendTextResponse(response, 404, "Файл формы не найден");
            return;
        }

        byte[] buffer = await File.ReadAllBytesAsync(filePath);

        response.ContentType = "text/html; charset=utf-8";
        response.StatusCode = 200;
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        await response.OutputStream.FlushAsync();
    }

    // Простой текстовый ответ клиенту
    private static void SendTextResponse(HttpListenerResponse response, int statusCode, string message)
    {
        response.StatusCode = statusCode;
        response.ContentType = "text/plain; charset=utf-8";
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    // Остановка сервера
    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cts.Cancel();
        try { _listener.Stop(); _listener.Close(); }
        catch { }

        Console.WriteLine("Сервер остановлен.");
    }
}