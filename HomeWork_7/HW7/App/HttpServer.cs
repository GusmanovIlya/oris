using System.Text;
using System.Text.Json;
using StudentApi.Data;
using StudentApi.Models;
using System.Net;


namespace StudentApi.Http;

public sealed class HttpServer
{
    private readonly HttpListener _listener = new();

    // Репозиторий для БД
    private readonly StudentRepository _repo;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public HttpServer(string baseUrl, StudentRepository repo)
    {
        _repo = repo;

        _listener.Prefixes.Add(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
    }
    public void Start() => _listener.Start();

    public async Task RunForeverAsync()
    {
        while (true)
        {
            var ctx = await _listener.GetContextAsync();

            try
            {
                // Маршрутизируем запрос
                await RouteAsync(ctx);
            }
            catch (Exception ex)
            {
                // Если любая неожиданная ошибка — возвращаем 500
                await WriteErrorAsync(ctx.Response, ex.Message, 500);
            }
            finally
            {
                ctx.Response.Close();
            }
        }
    }

    // ROUTING маршрутизация
    private async Task RouteAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var res = ctx.Response;

        // Путь: "/students", "/students/5" и т.д.
        var path = req.Url?.AbsolutePath ?? "/";

        // Метод: GET/POST/PUT/DELETE
        var method = req.HttpMethod.ToUpperInvariant();

        // 1) Маршрут /students
        // GET  /students  -> список
        // POST /students  -> создать
        if (path == "/students")
        {
            if (method == "GET")
            {
                // Получаем список из БД и отдаём JSON
                var all = await _repo.GetAllAsync();
                await WriteJsonAsync(res, all, 200);
                return;
            }

            if (method == "POST")
            {
                await CreateAsync(req, res);
                return;
            }
        }

        // 2) Маршрут /students/{id}
        // Здесь мы вручную парсим id из URL
        if (path.StartsWith("/students/"))
        {
            // Берём хвост после "/students/"
            var tail = path["/students/".Length..].Trim('/');

            // Если id не число — это не наш маршрут
            if (!long.TryParse(tail, out var id))
            {
                await WriteErrorAsync(res, "Invalid id", 400);
                return;
            }

            // GET /students/{id}
            if (method == "GET")
            {
                await GetByIdAsync(res, id);
                return;
            }

            // PUT /students/{id}
            if (method == "PUT")
            {
                await UpdateAsync(req, res, id);
                return;
            }

            // DELETE /students/{id}
            if (method == "DELETE")
            {
                await DeleteAsync(res, id);
                return;
            }
        }

        // Если не совпало ни с одним маршрутом
        await WriteErrorAsync(res, "Not Found", 404);
    }

    // -------------------------
    // CRUD обработчики
    // -------------------------

    // GET /students/{id}
    private async Task GetByIdAsync(HttpListenerResponse res, long id)
    {
        var s = await _repo.GetByIdAsync(id);

        // Если студент не найден — 404
        if (s is null)
        {
            await WriteErrorAsync(res, "Student not found", 404);
            return;
        }

        // Иначе отдаём объект как JSON
        await WriteJsonAsync(res, s, 200);
    }

    // POST /students
    private async Task CreateAsync(HttpListenerRequest req, HttpListenerResponse res)
    {
        // 1) читаем JSON и десериализуем в Student
        var s = await ReadJsonAsync<Student>(req, res);
        if (s is null) return; // ошибка уже отправлена клиенту

        // 2) валидируем
        if (!Validate(s, out var error))
        {
            await WriteErrorAsync(res, error, 400);
            return;
        }

        // 3) сохраняем в БД
        var id = await _repo.AddAsync(s);

        // 4) возвращаем id созданной записи
        await WriteJsonAsync(res, new { id }, 201);
    }

    // PUT /students/{id}
    private async Task UpdateAsync(HttpListenerRequest req, HttpListenerResponse res, long id)
    {
        var s = await ReadJsonAsync<Student>(req, res);
        if (s is null) return;

        if (!Validate(s, out var error))
        {
            await WriteErrorAsync(res, error, 400);
            return;
        }

        // UpdateAsync вернёт false, если строки с таким id нет
        var ok = await _repo.UpdateAsync(id, s);

        if (!ok)
        {
            await WriteErrorAsync(res, "Student not found", 404);
            return;
        }

        await WriteJsonAsync(res, new { updated = true }, 200);
    }

    // DELETE /students/{id}
    private async Task DeleteAsync(HttpListenerResponse res, long id)
    {
        var ok = await _repo.DeleteAsync(id);

        if (!ok)
        {
            await WriteErrorAsync(res, "Student not found", 404);
            return;
        }

        await WriteJsonAsync(res, new { deleted = true }, 200);
    }

    private static bool Validate(Student s, out string error)
    {
        // Здесь простые правила. Можно расширять.
        if (string.IsNullOrWhiteSpace(s.Name))        { error = "Name is required"; return false; }
        if (string.IsNullOrWhiteSpace(s.Surname))     { error = "Surname is required"; return false; }
        if (s.Age <= 0)                               { error = "Age must be > 0"; return false; }
        if (string.IsNullOrWhiteSpace(s.Nationality)) { error = "Nationality is required"; return false; }
        if (string.IsNullOrWhiteSpace(s.Profession))  { error = "Profession is required"; return false; }

        error = "";
        return true;
    }


    // Читаем тело запроса как строку
    private static async Task<string> ReadBodyAsync(HttpListenerRequest req)
    {
        // InputStream — поток тела запроса
        // ContentEncoding — кодировка, которую указал клиент
        using var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    // Читаем JSON и преобразуем в объект типа T
    // Если JSON неправильный — возвращаем 400
    private static async Task<T?> ReadJsonAsync<T>(HttpListenerRequest req, HttpListenerResponse res)
    {
        var body = await ReadBodyAsync(req);

        if (string.IsNullOrWhiteSpace(body))
        {
            await WriteErrorAsync(res, "Empty body", 400);
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOpts);
        }
        catch
        {
            await WriteErrorAsync(res, "Invalid JSON", 400);
            return default;
        }
    }

    // Пишем JSON-ответ
    private static async Task WriteJsonAsync(HttpListenerResponse res, object data, int status)
    {
        var json = JsonSerializer.Serialize(data, JsonOpts);
        var bytes = Encoding.UTF8.GetBytes(json);

        res.StatusCode = status;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentLength64 = bytes.Length;

        await res.OutputStream.WriteAsync(bytes);
    }

    // Пишем ошибку как JSON: { "error": "..." }
    private static Task WriteErrorAsync(HttpListenerResponse res, string msg, int code)
        => WriteJsonAsync(res, new { error = msg }, code);
}
