using Npgsql;

namespace StudentApi.Data;

public static class DbInitializer
{
    // Создаёт таблицу в БД, если её ещё нет
    public static async Task EnsureCreatedAsync(string connectionString)
    {
        // NpgsqlConnection — соединение с PostgreSQL
        await using var conn = new NpgsqlConnection(connectionString);

        // Открываем соединение
        await conn.OpenAsync();

        // SQL-команда создания таблицы.
        // IF NOT EXISTS — чтобы не падало, если таблица уже есть.
        var sql = """
        CREATE TABLE IF NOT EXISTS students (
            id BIGSERIAL PRIMARY KEY,
            name TEXT NOT NULL,
            surname TEXT NOT NULL,
            age INT NOT NULL,
            nationality TEXT NOT NULL,
            profession TEXT NOT NULL
        );
        """;

        // NpgsqlCommand — SQL-команда, которую мы отправим в PostgreSQL
        await using var cmd = new NpgsqlCommand(sql, conn);

        // Выполнить команду (без возврата строк)
        await cmd.ExecuteNonQueryAsync();
    }
}
