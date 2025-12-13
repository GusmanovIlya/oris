using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class HttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts;
    private readonly Config _config;
    private readonly InvoiceProcessor _processor;
    private readonly Action _reloadConfigAction;

    public HttpServer(Config config, InvoiceProcessor processor, Action reloadConfigAction)
    {
        _config = config;
        _processor = processor;
        _reloadConfigAction = reloadConfigAction;

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8080/");
        _cts = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("Сервер запущен: http://localhost:8080/");

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context), _cts.Token);
            }
            catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Ошибка HTTP: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _listener.Stop();
        _cts.Cancel();
    }

    private void HandleRequest(HttpListenerContext context)
    {
        string responseText = "Not Found";
        int statusCode = 404;
        string contentType = "text/plain";

        try
        {
            string path = context.Request.Url?.AbsolutePath ?? "";
            switch (path)
            {
                case "/health":
                    responseText = "OK";
                    statusCode = 200;
                    break;

                case "/config":
                    var safeConfig = new
                    {
                        ProcessingIntervalSeconds = _config.ProcessingIntervalSeconds,
                        MaxErrorRetries = _config.MaxErrorRetries,
                        ConnectionString = "***** (скрыто)"
                    };
                    responseText = JsonSerializer.Serialize(safeConfig);
                    contentType = "application/json";
                    statusCode = 200;
                    break;

                case "/config/reload":
                    _reloadConfigAction();
                    responseText = "reloaded";
                    statusCode = 200;
                    break;

                case "/stats":
                    var stats = new
                    {
                        LastCycleProcessedPending = _processor.LastProcessedPending,
                        LastCycleSuccess = _processor.LastSuccess,
                        LastCycleError = _processor.LastError
                    };
                    responseText = JsonSerializer.Serialize(stats);
                    contentType = "application/json";
                    statusCode = 200;
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
            responseText = "Internal Server Error";
            statusCode = 500;
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseText);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.Close();
    }

    public void Dispose()
    {
        _listener.Close();
        _cts.Dispose();
    }
}