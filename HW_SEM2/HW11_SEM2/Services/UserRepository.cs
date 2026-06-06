namespace TEST.Services;

public class UserRepository : IRepository
{
    private readonly ILogger _logger;

    public UserRepository(ILogger logger)
    {
        _logger = logger;
    }

    public void Save(string data)
    {
        _logger.Log($"Данные сохранены: {data}");
    }
}