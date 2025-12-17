using System.Net;
using System.Text;

namespace StudentCardsServer;

public sealed class SimpleServer
{
    private readonly HttpListener _listener = new();
    private readonly Student[] _students;

    // Пути к папкам с HTML и CSS
    private const string TemplatesDir = "Templates";

    public SimpleServer(string prefix, Student[] students)
    {
        _listener.Prefixes.Add(prefix);
        _students = students;
    }

    public async Task RunAsync()
    {
        _listener.Start();
        Console.WriteLine("Server started");

        while (true)
        {
            var ctx = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleAsync(ctx));
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        try
        {
            var path = ctx.Request.Url?.AbsolutePath ?? "/";
            if (path.Length > 1) path = path.TrimEnd('/');

            // Главная страница
            if (path == "/" && ctx.Request.HttpMethod == "GET")
            {
                var html = RenderStudentsPage();
                await WriteAsync(ctx.Response, html, "text/html; charset=utf-8", 200);
                return;
            }

            // Статика
            if (path.StartsWith("/static/"))
            {
                var file = Path.Combine(StaticDir, Path.GetFileName(path));
                await WriteFileAsync(ctx.Response, file, "text/css; charset=utf-8");
                return;
            }

            // 404
            await WriteAsync(ctx.Response, "404", "text/plain", 404);
        }
        finally
        {
            ctx.Response.Close();
        }
    }


    private string RenderStudentsPage()
    {
        var layout = File.ReadAllText(Path.Combine(TemplatesDir, "layout.html"));
        var page = File.ReadAllText(Path.Combine(TemplatesDir, "students.html"));
        var cardTemplate = File.ReadAllText(Path.Combine(TemplatesDir, "student-card.html"));

        var cards = new StringBuilder();

        foreach (var s in _students)
        {
            var card = MiniTemplate.Render(cardTemplate, new Dictionary<string, string>
            {
                ["Id"] = s.Id.ToString(),
                ["FullName"] = MiniTemplate.Escape(s.FullName),
                ["Age"] = s.Age.ToString(),
                ["Group"] = MiniTemplate.Escape(s.Group),
                ["Gpa"] = s.Gpa.ToString("0.0"),
                ["initials"] = GetInitials(s.FullName)
            });

            cards.AppendLine(card);
        }

        page = MiniTemplate.Render(page, new Dictionary<string, string>
        {
            ["cards"] = cards.ToString()
        });

        return MiniTemplate.Render(layout, new Dictionary<string, string>
        {
            ["title"] = "Студенты",
            ["content"] = page
        });
    }

    private static string GetInitials(string name)
    {
        var p = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return p.Length >= 2 ? $"{p[0][0]}{p[1][0]}" : p[0][0].ToString();
    }

    private static async Task WriteAsync(HttpListenerResponse res, string text, string type, int code)
    {
        var data = Encoding.UTF8.GetBytes(text);
        res.StatusCode = code;
        res.ContentType = type;
        res.ContentLength64 = data.Length;
        await res.OutputStream.WriteAsync(data);
    }

    private static async Task WriteFileAsync(HttpListenerResponse res, string file, string type)
    {
        if (!File.Exists(file))
        {
            res.StatusCode = 404;
            return;
        }

        var data = await File.ReadAllBytesAsync(file);
        res.StatusCode = 200;
        res.ContentType = type;
        res.ContentLength64 = data.Length;
        await res.OutputStream.WriteAsync(data);
    }
}
