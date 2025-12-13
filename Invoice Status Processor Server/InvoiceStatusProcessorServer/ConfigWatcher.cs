using System.IO;
using System.Threading;

public class ConfigWatcher
{
    private readonly FileSystemWatcher _watcher;
    private readonly Action _reloadAction;

    public ConfigWatcher(string path, Action reloadAction)
    {
        _reloadAction = reloadAction;

        _watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), "config.json")
        {
            NotifyFilter = NotifyFilters.LastWrite
        };

        _watcher.Changed += OnConfigChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("Файл config.json изменён — перезагружаю настройки...");
        Thread.Sleep(100);
        _reloadAction();
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
}