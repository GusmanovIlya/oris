using System;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceStatusProcessor
{
    class Program
    {
        private static Config _config;
        private static InvoiceProcessor _processor;
        private static HttpServer _httpServer;
        private static Timer _processingTimer;
        private static ConfigWatcher _configWatcher;

        static async Task Main(string[] args)
        {
            LoadConfigAndInitialize();

            Console.CancelKeyPress += (_, e) =>
            {
                Console.WriteLine("Остановка сервера...");
                e.Cancel = true;
                Shutdown();
            };

            await _httpServer.StartAsync();
            Console.WriteLine("Сервер остановлен");
        }

        private static void LoadConfigAndInitialize()
        {
            _config = Config.Load();
            _processor = new InvoiceProcessor(_config);

            Action reloadAction = () =>
            {
                _config = Config.Load();
                _processor = new InvoiceProcessor(_config);
                _processingTimer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(_config.ProcessingIntervalSeconds));
            };

            _configWatcher = new ConfigWatcher(Directory.GetCurrentDirectory(), reloadAction);

            _httpServer = new HttpServer(_config, _processor, reloadAction);

            _processingTimer = new Timer(_ => _processor.ProcessInvoices(), null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_config.ProcessingIntervalSeconds));
        }

        private static void Shutdown()
        {
            _processingTimer?.Dispose();
            _httpServer?.Stop();
            _configWatcher?.Dispose();
        }
    }
}