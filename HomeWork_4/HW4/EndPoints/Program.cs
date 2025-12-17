
class Program
{
    static async Task Main(string[] args)
    {
        var server = new HttpServer("static");

        await server.StartAsync();

        while (true)
        {
            Console.WriteLine("Введите 'stop' для выхода:");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (input == "stop")
            {
                server.Stop();
                break;
            }
        }
    }
}