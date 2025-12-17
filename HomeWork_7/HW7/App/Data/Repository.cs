using Npgsql;
using StudentApi.Models;

namespace StudentApi.Data;

// Репозиторий — слой доступа к данным.
// Его задача: выполнять SQL и возвращать/принимать объекты Student.
// Важно: репозиторий НЕ должен знать про HttpListener и HTTP.
public sealed class StudentRepository
{
    private readonly string _cs;

    public StudentRepository(string connectionString) => _cs = connectionString;

    // -----------------------
    // READ: получить всех
    // -----------------------
    public async Task<List<Student>> GetAllAsync()
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();

        // Выбираем все строки, сортируем по id
        const string sql = """
            SELECT id, name, surname, age, nationality, profession
            FROM students
            ORDER BY id DESC;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<Student>();

        // reader.ReadAsync() двигает курсор на следующую строку
        while (await reader.ReadAsync())
        {
            // Вручную маппим колонки в объект Student.
            // Индексы (0..5) соответствуют SELECT id, name, ...
            list.Add(new Student
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Surname = reader.GetString(2),
                Age = reader.GetInt32(3),
                Nationality = reader.GetString(4),
                Profession = reader.GetString(5)
            });
        }

        return list;
    }

    // -----------------------
    // READ: получить по id
    // -----------------------
    public async Task<Student?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = """
            SELECT id, name, surname, age, nationality, profession
            FROM students
            WHERE id = @id;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);

        // Параметр @id защищает от SQL-инъекций
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();

        // Если строк нет — возвращаем null
        if (!await reader.ReadAsync()) return null;

        return new Student
        {
            Id = reader.GetInt64(0),
            Name = reader.GetString(1),
            Surname = reader.GetString(2),
            Age = reader.GetInt32(3),
            Nationality = reader.GetString(4),
            Profession = reader.GetString(5)
        };
    }

    // -----------------------
    // CREATE: добавить студента
    // -----------------------
    public async Task<long> AddAsync(Student s)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();

        // RETURNING id — Postgres вернёт id вставленной строки
        const string sql = """
            INSERT INTO students (name, surname, age, nationality, profession)
            VALUES (@name, @surname, @age, @nationality, @profession)
            RETURNING id;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", s.Name);
        cmd.Parameters.AddWithValue("surname", s.Surname);
        cmd.Parameters.AddWithValue("age", s.Age);
        cmd.Parameters.AddWithValue("nationality", s.Nationality);
        cmd.Parameters.AddWithValue("profession", s.Profession);

        // ExecuteScalarAsync возвращает первый столбец первой строки результата
        return Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    // -----------------------
    // UPDATE: обновить по id
    // -----------------------
    public async Task<bool> UpdateAsync(long id, Student s)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = """
            UPDATE students
            SET name=@name, surname=@surname, age=@age,
                nationality=@nationality, profession=@profession
            WHERE id=@id;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("name", s.Name);
        cmd.Parameters.AddWithValue("surname", s.Surname);
        cmd.Parameters.AddWithValue("age", s.Age);
        cmd.Parameters.AddWithValue("nationality", s.Nationality);
        cmd.Parameters.AddWithValue("profession", s.Profession);

        // ExecuteNonQueryAsync возвращает количество затронутых строк
        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    // -----------------------
    // DELETE: удалить по id
    // -----------------------
    public async Task<bool> DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM students WHERE id=@id;", conn);
        cmd.Parameters.AddWithValue("id", id);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
}
