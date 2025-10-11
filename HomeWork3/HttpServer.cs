using System.Net;
using System.Text;
using System.Text.Json;

public class HttpServer : IDisposable
{
    private HttpListener _listener;
    private bool _isRunning;
    private readonly ServerSettings _settings;
    private CancellationTokenSource _cancellationTokenSource;

    public HttpServer(ServerSettings settings)
{
    _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    _listener = new HttpListener();
    _cancellationTokenSource = new CancellationTokenSource();
    
    // Используем значения из настроек или значения по умолчанию
    var prefixes = _settings.Prefixes ?? new[] { "http://127.0.0.1:8888/connection/" };
    var staticFilesPath = _settings.StaticFilesPath ?? "static";
    var defaultFile = _settings.DefaultFile ?? "Hello.html";
    
    // Создаем новый объект с гарантированно заполненными значениями
    _settings = new ServerSettings 
    { 
        Prefixes = prefixes,
        StaticFilesPath = staticFilesPath,
        DefaultFile = defaultFile
    };
    
    // Добавляем префиксы из настроек
    foreach (var prefix in _settings.Prefixes)
    {
        _listener.Prefixes.Add(prefix);
    }
}

    public static HttpServer CreateFromSettings(string settingsPath = "settings.json")
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                LogWarning($"Файл настроек {settingsPath} не найден. Используются настройки по умолчанию.");
                return new HttpServer(new ServerSettings());
            }

            string json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<ServerSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settings == null)
            {
                LogWarning("Не удалось загрузить настройки. Используются настройки по умолчанию.");
                return new HttpServer(new ServerSettings());
            }

            LogInfo($"Настройки успешно загружены из {settingsPath}");
            return new HttpServer(settings);
        }
        catch (Exception ex)
        {
            LogError($"Ошибка при загрузке настроек: {ex.Message}");
            LogWarning("Используются настройки по умолчанию.");
            return new HttpServer(new ServerSettings());
        }
    }

    public async Task StartAsync()
    {
        try
        {
            LogInfo("Сервер запускается...");
            
            // Проверяем, есть ли префиксы
            if (_listener.Prefixes.Count == 0)
            {
                throw new InvalidOperationException("Не указаны префиксы для прослушивания. Проверьте файл settings.json");
            }
            
            // Логируем префиксы
            LogInfo($"Прослушиваемые адреса:");
            foreach (var prefix in _listener.Prefixes)
            {
                LogInfo($"  - {prefix}");
            }
            
            _listener.Start();
            _isRunning = true;
            
            LogInfo("Сервер успешно запущен");
            LogInfo("Ожидание входящих подключений...");

            // Запускаем бесконечный цикл обработки запросов
            await ListenAsync();
        }
        catch (Exception ex)
        {
            LogError("Ошибка при запуске сервера", ex);
            throw;
        }
    }

    public void Start()
    {
        // Синхронная версия для обратной совместимости
        _ = StartAsync();
    }

    private async Task ListenAsync()
    {
        var requests = new List<Task>();
        
        while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                // Ожидаем следующий запрос с поддержкой отмены
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                
                // Обрабатываем запрос в отдельной задаче
                var requestTask = ProcessRequestAsync(context);
                requests.Add(requestTask);
                
                // Очищаем завершенные задачи
                if (requests.Count > 10) // Ограничиваем количество отслеживаемых задач
                {
                    requests.RemoveAll(t => t.IsCompleted);
                }
            }
            catch (HttpListenerException) when (!_isRunning)
            {
                LogInfo("Сервер прекратил прослушивание");
                break;
            }
            catch (ObjectDisposedException)
            {
                LogInfo("Listener был disposed");
                break;
            }
            catch (Exception ex)
            {
                LogError("Ошибка в цикле прослушивания", ex);
                await Task.Delay(1000); // Задержка перед повторной попыткой
            }
        }
        
        LogInfo("Цикл прослушивания завершен");
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
{
    var request = context.Request;
    var response = context.Response;

    try
    {
        LogInfo($"=== НОВЫЙ ЗАПРОС ===");
        LogInfo($"Remote: {request.RemoteEndPoint}");
        LogInfo($"URL: {request.Url}");
        LogInfo($"LocalPath: {request.Url.LocalPath}");
        LogInfo($"AbsolutePath: {request.Url.AbsolutePath}");
        
        // Проверяем текущую директорию и существование папки static
        string currentDir = Directory.GetCurrentDirectory();
        LogInfo($"Текущая директория: {currentDir}");
        LogInfo($"Папка static существует: {Directory.Exists("static")}");
        
        if (Directory.Exists("static"))
        {
            LogInfo($"Содержимое папки static:");
            foreach (var file in Directory.GetFiles("static", "*", SearchOption.AllDirectories))
            {
                LogInfo($"  - {file}");
            }
        }

        string filePath = GetFilePath(request.Url.LocalPath);
        
        if (File.Exists(filePath))
        {
            LogInfo($"✅ Файл найден: {filePath}");
            await SendFileResponseAsync(response, filePath);
        }
        else
        {
            LogInfo($"❌ Файл не найден: {filePath}");
            await Send404ResponseAsync(response, request.Url.LocalPath);
        }
    }
    catch (Exception ex)
    {
        LogError("Ошибка при обработке запроса", ex);
        await Send500ResponseAsync(response, ex.Message);
    }
}

    private string GetFilePath(string urlPath)
{
    LogInfo($"Определение пути для: {urlPath}");
    
    // Обрабатываем корневой путь и /connection/
    if (urlPath == "/" || urlPath == "/connection" || urlPath == "/connection/")
    {
        string defaultFilePath = Path.Combine(_settings.StaticFilesPath, _settings.DefaultFile);
        LogInfo($"Пытаемся найти файл по умолчанию: {defaultFilePath}");
        LogInfo($"Файл существует: {File.Exists(defaultFilePath)}");
        
        // Проверяем существование файла по умолчанию
        if (File.Exists(defaultFilePath))
        {
            return defaultFilePath;
        }
        
        // Если файл по умолчанию не найден, пробуем Hello.html
        string helloFilePath = Path.Combine(_settings.StaticFilesPath, "Hello.html");
        LogInfo($"Пытаемся найти Hello.html: {helloFilePath}");
        LogInfo($"Hello.html существует: {File.Exists(helloFilePath)}");
        
        if (File.Exists(helloFilePath))
        {
            return helloFilePath;
        }
        
        return defaultFilePath;
    }
    
    // Убираем начальный слэш
    string localPath = urlPath.TrimStart('/');
    LogInfo($"Локальный путь после TrimStart: {localPath}");
    
    // Убираем "connection/" из пути, если он есть
    if (localPath.StartsWith("connection/"))
    {
        localPath = localPath.Substring("connection/".Length);
        LogInfo($"Локальный путь после удаления 'connection/': {localPath}");
    }
    
    string fullPath = Path.Combine(_settings.StaticFilesPath, localPath);
    LogInfo($"Полный путь к файлу: {fullPath}");
    LogInfo($"Файл существует: {File.Exists(fullPath)}");
    
    return fullPath;
}

    private async Task SendFileResponseAsync(HttpListenerResponse response, string filePath)
    {
        byte[] buffer = File.ReadAllBytes(filePath);
        
        string contentType = GetContentType(filePath);
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;
        response.StatusCode = 200;
        
        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private async Task Send404ResponseAsync(HttpListenerResponse response, string requestedPath)
    {
        string responseText = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>404 - Не найдено</title>
</head>
<body>
    <h1>404 - Страница не найдена</h1>
    <p>Запрошенный ресурс <strong>{requestedPath}</strong> не существует.</p>
    <a href='/'>Вернуться на главную</a>
</body>
</html>";
        
        byte[] buffer = Encoding.UTF8.GetBytes(responseText);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.StatusCode = 404;
        
        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private async Task Send500ResponseAsync(HttpListenerResponse response, string errorMessage)
    {
        string responseText = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>500 - Ошибка сервера</title>
</head>
<body>
    <h1>500 - Внутренняя ошибка сервера</h1>
    <p>Произошла ошибка: <strong>{errorMessage}</strong></p>
    <a href='/'>Вернуться на главную</a>
</body>
</html>";
        
        byte[] buffer = Encoding.UTF8.GetBytes(responseText);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.StatusCode = 500;
        
        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        
        return extension switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".json" => "application/json",
            ".txt" => "text/plain",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }

    public void Stop()
    {
        try
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            LogInfo("Сервер остановлен");
        }
        catch (Exception ex)
        {
            LogError("Ошибка при остановке сервера", ex);
        }
    }

    private static void LogInfo(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}");
    }

    private static void LogError(string message, Exception ex = null)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}");
        if (ex != null)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DETAILS: {ex.Message}");
        }
    }

    private static void LogWarning(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARN: {message}");
    }

    public void Dispose()
    {
        Stop();
        _listener?.Close();
        _cancellationTokenSource?.Dispose();
    }
}