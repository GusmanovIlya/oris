namespace EmailServer;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new HttpServer("static");


        await server.StartAsync();


        while (true)
        {
            Console.WriteLine("Введите 'stop' для завершения работы:");
            string? command = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (command == "stop")
            {
                server.Stop();
                break;
            }
        }
    }
}