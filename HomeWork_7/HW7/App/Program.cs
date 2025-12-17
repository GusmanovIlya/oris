using StudentApi.Data;
using StudentApi.Http;

var connectionString =
    "Host=localhost;Port=5432;Database=students_db;Username=postgres;Password=postgres";

await DbInitializer.EnsureCreatedAsync(connectionString);

var repo = new StudentRepository(connectionString);

var server = new HttpServer("http://localhost:8080/", repo);

server.Start();

Console.WriteLine("Server started:");
Console.WriteLine("GET    /students");
Console.WriteLine("GET    /students/{id}");
Console.WriteLine("POST   /students");
Console.WriteLine("PUT    /students/{id}");
Console.WriteLine("DELETE /students/{id}");

await server.RunForeverAsync();
