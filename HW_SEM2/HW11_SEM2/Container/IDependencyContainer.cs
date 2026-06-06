namespace TEST.Container;

public interface IDependencyContainer
{
    void Register<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    void RegisterInstance<TService>(TService instance)
        where TService : class;

    TService Resolve<TService>()
        where TService : class;
}