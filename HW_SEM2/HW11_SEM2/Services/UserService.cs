namespace TEST.Services;

public class UserService : IUserService
{
    private readonly IRepository _repository;
    private readonly ILogger _logger;

    public UserService(IRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public void CreateUser(string name)
    {
        _logger.Log($"Создание пользователя: {name}");
        _repository.Save(name);
    }
}