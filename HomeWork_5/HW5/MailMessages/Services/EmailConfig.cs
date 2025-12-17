using System.Text.Json;

namespace EmailServer.Services;

// Класс-синглтон для хранения настроек SMTP из файла email.json
public sealed class EmailConfig
{
    // Ленивая загрузка: конфиг читается только при первом обращении
    private static readonly Lazy<EmailConfig> _instance = new(() => Load());

    // Единственный экземпляр для всего приложения
    public static EmailConfig Instance => _instance.Value;

    // SMTP-сервер (например, smtp.gmail.com)
    public string SmtpHost { get; init; } = string.Empty;

    // Порт SMTP (587 — стандартный для TLS)
    public int SmtpPort { get; init; }

    // Логин для SMTP (обычно полный email)
    public string SmtpUser { get; init; } = string.Empty;

    // Пароль или App Password
    public string SmtpPass { get; init; } = string.Empty;

    // Адрес отправителя (от кого приходят письма)
    public string FromAddr { get; init; } = string.Empty;

    // Имя отправителя, которое увидит получатель
    public string FromName { get; init; } = "Мой сайт";

    // Приватный метод загрузки конфигурации из файла
    private static EmailConfig Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "email.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Не найден файл email.json по пути: {path}");

        string json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<EmailConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null)
            throw new InvalidOperationException("Файл email.json пустой или повреждён");

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(config.SmtpUser))
            throw new InvalidOperationException("SmtpUser не указан в email.json");
        if (string.IsNullOrWhiteSpace(config.SmtpPass))
            throw new InvalidOperationException("SmtpPass не указан в email.json");
        if (string.IsNullOrWhiteSpace(config.FromAddr))
            throw new InvalidOperationException("FromAddr не указан в email.json");

        return config;
    }
}