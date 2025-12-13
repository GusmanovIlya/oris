using Microsoft.Extensions.Configuration;
using System;
using System.IO;

public class Config
{
    public string ConnectionString { get; set; } = "";
    public int ProcessingIntervalSeconds { get; set; } = 300;
    public int MaxErrorRetries { get; set; } = 5;

    public static Config Load()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false, reloadOnChange: false);

        var configuration = builder.Build();
        var config = new Config();
        configuration.Bind(config);

        Console.WriteLine($"Конфиг загружен: интервал = {config.ProcessingIntervalSeconds} сек, попыток = {config.MaxErrorRetries}");
        return config;
    }
}