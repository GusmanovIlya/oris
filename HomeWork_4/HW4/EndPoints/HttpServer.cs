using System.Net; 
using System.Text;      

public class HttpServer
{
    private readonly HttpListener _listener = new();

    private readonly string _staticPath;

    // Флаг - запущен ли сервер сейчас
    private bool _isRunning = false;

    private readonly CancellationTokenSource _cts = new();

    // Конструктор: принимает относительный или абсолютный путь к папке static
    public HttpServer(string staticPath)
    {
        // Проверка на пустой путь защита от ошибок
        if (string.IsNullOrWhiteSpace(staticPath))
            throw new ArgumentException("Путь к папке static не может быть пустым");

        // Преобразуем в абсолютный путь
        _staticPath = Path.GetFullPath(staticPath);

        
        _listener.Prefixes.Add("http://127.0.0.1:8888/");
    }

    public Task StartAsync()
    {
        // Если сервер уже запущен — ничего не делаем
        if (_isRunning) return Task.CompletedTask;

        // Запускаем прослушивание порта
        _listener.Start();
        _isRunning = true;

        Console.WriteLine("Сервер запущен на http://127.0.0.1:8888/");
        Console.WriteLine("Доступны страницы: /page1 (GET), /page2 (GET и POST)");
        Console.WriteLine("Для остановки введите 'stop'");

        // Запускаем фоновый цикл обработки запросов в отдельном потоке
        _ = Task.Run(() => ProcessRequestsLoopAsync(_cts.Token));

        return Task.CompletedTask;
    }

    // Основной цикл: бесконечно принимает входящие запросы
    private async Task ProcessRequestsLoopAsync(CancellationToken token)
    {
        while (_isRunning && !token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                // Обрабатываем каждый запрос в отдельном потоке (чтобы не блокировать цикл)
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (ObjectDisposedException)
            {
                // Происходит при остановке listener — нормально, выходим из цикла
                break;
            }
            catch (Exception ex)
            {
                // Любые другие ошибки при приёме запроса логируем
                Console.WriteLine($"Ошибка приёма запроса: {ex.Message}");
            }
        }
    }

    // Основной метод обработки одного HTTP-запроса
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;     // Входящий запрос
        var response = context.Response;   // Объект для ответа
        string path = request.Url!.AbsolutePath.ToLowerInvariant(); // Путь без учёта регистра

        try
        {
            // 1. GET /page1 — отдаём статический файл page1.html ===
            if (request.HttpMethod == "GET" && path == "/page1")
            {
                await ServeStaticFile(response, "page1.html");
                return;
            }

            // 2. GET /page2 — отдаём форму page2.html ===
            if (request.HttpMethod == "GET" && path == "/page2")
            {
                await ServeStaticFile(response, "page2.html");
                return;
            }

            // 3. POST /page2 — обрабатываем отправленную форму ===
            if (request.HttpMethod == "POST" && path == "/page2")
            {
                // Читаем тело POST-запроса (обычно в формате x-www-form-urlencoded)
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();

                // Разбираем строку вида "name=Иван&message=Привет" в словарь
                var data = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(body))
                {
                    foreach (var pair in body.Split('&'))
                    {
                        var parts = pair.Split('=', 2); // Разделяем максимум на 2 части
                        if (parts.Length == 2)
                        {
                            string key = Uri.UnescapeDataString(parts[0]);
                            string value = Uri.UnescapeDataString(parts[1]);
                            data[key] = value;
                        }
                    }
                }

                // Извлекаем значения полей (с значениями по умолчанию)
                string name = data.TryGetValue("name", out var n) ? n : "не указано";
                string message = data.TryGetValue("message", out var m) ? m : "";

                // Формируем JSON-ответ клиенту
                var result = new
                {
                    status = "success",
                    name,
                    message,
                    received_at = DateTime.Now.ToString("HH:mm:ss")
                };

                await SendJsonResponse(response, result);
                return;
            }

            // Попытка отдать любой другой статический файл из папки static ===
            string relativePath = request.Url.LocalPath.TrimStart('/');
            string filePath = Path.Combine(_staticPath, relativePath);

            // Если файл существует — отдаём его
            if (File.Exists(filePath))
            {
                await ServeStaticFile(response, relativePath);
                return;
            }

            // Если ничего не подошло — 404
            SendTextResponse(response, 404, "Not Found");
        }
        catch (Exception ex)
        {
            // Логируем ошибку и возвращаем 500
            Console.WriteLine($"Ошибка обработки {request.HttpMethod} {path}: {ex.Message}");
            SendTextResponse(response, 500, "Internal Server Error");
        }
        finally
        {
            // Всегда закрываем ответ, чтобы освободить соединение
            response.Close();
        }
    }

    private async Task ServeStaticFile(HttpListenerResponse response, string relativePath)
    {
        string filePath = Path.Combine(_staticPath, relativePath);

        // Определяем правильный Content-Type по расширению
        string contentType = GetContentType(Path.GetExtension(filePath));

        // Читаем весь файл в память (для небольших файлов — нормально)
        byte[] buffer = await File.ReadAllBytesAsync(filePath);

        // Заполняем заголовки ответа
        response.ContentType = contentType;
        response.StatusCode = 200;
        response.ContentLength64 = buffer.Length;

        // Отправляем содержимое файла клиенту
        await response.OutputStream.WriteAsync(buffer);
        await response.OutputStream.FlushAsync();
    }

    // Отправка JSON-ответа (используется для POST /page2)
    private static async Task SendJsonResponse(HttpListenerResponse response, object data)
    {
        // Сериализуем объект в красивый JSON
        string json = System.Text.Json.JsonSerializer.Serialize(
            data,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        byte[] buffer = Encoding.UTF8.GetBytes(json);

        response.ContentType = "application/json; charset=utf-8";
        response.StatusCode = 200;
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer);
        await response.OutputStream.FlushAsync();
    }

    // Простая отправка текстового ответа (для ошибок)
    private static void SendTextResponse(HttpListenerResponse response, int statusCode, string message)
    {
        response.StatusCode = statusCode;
        response.ContentType = "text/plain; charset=utf-8";
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream" 
        };
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cts.Cancel(); 

        try
        {
            _listener.Stop();
            _listener.Close();
        }
        catch {  }

        Console.WriteLine("Сервер остановлен.");
    }
}