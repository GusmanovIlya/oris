using StudentCardsServer;

var students = new[]
{
    new Student(1, "Иван Петров", 19, "ИТИС", 4.3),
    new Student(2, "Мария Иванова", 20, "ИТИС", 4.8),
    new Student(3, "Алексей Смирнов", 18, "ИТИС", 3.9),
};

var server = new SimpleServer("http://localhost:8080/", students);

Console.WriteLine("Open http://localhost:8080/");
await server.RunAsync();
