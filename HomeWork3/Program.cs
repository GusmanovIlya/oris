class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Загружаем настройки и создаем сервер
            var server = HttpServer.CreateFromSettings("settings.json");
            
            using (server)
            {
                // Запускаем сервер
                server.Start();
                
                // Ожидаем команду пользователя для остановки
                await WaitForStopCommandAsync();
                
                Console.WriteLine("Остановка сервера...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Критическая ошибка: {ex.Message}");
        }
        
        Console.WriteLine("Приложение завершено. Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    private static async Task WaitForStopCommandAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Сервер запущен. Доступные команды:");
        Console.WriteLine("  'q' - остановить сервер и выйти");
        Console.WriteLine("  'r' - перезапустить сервер");
        Console.WriteLine("  's' - показать текущие настройки");
        Console.WriteLine();

        while (true)
        {
            var input = Console.ReadLine()?.ToLower().Trim();
            
            switch (input)
            {
                case "q":
                    Console.WriteLine("Остановка сервера...");
                    return;
                    
                case "r":
                    Console.WriteLine("Для перезапуска необходимо перезапустить приложение");
                    break;
                    
                case "s":
                    Console.WriteLine("Текущие настройки загружены из settings.json");
                    break;
                    
                default:
                    Console.WriteLine("Неизвестная команда. Используйте 'q' для выхода, 'r' для перезапуска, 's' для показа настроек");
                    break;
            }
        }
    }
}