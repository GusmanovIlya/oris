using System;
using System.Collections.Generic;
using System.Linq;

namespace TEST.Container;

public class SimpleContainer : IDependencyContainer
{
    private readonly Dictionary<Type, Type> _registrations = new();
    private readonly Dictionary<Type, object> _instances = new();

    public void Register<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _registrations[typeof(TService)] = typeof(TImplementation);
    }

    public void RegisterInstance<TService>(TService instance)
        where TService : class
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        _instances[typeof(TService)] = instance;
    }

    public TService Resolve<TService>()
        where TService : class
    {
        return (TService)Resolve(typeof(TService), new Stack<Type>());
    }

    private object Resolve(Type serviceType, Stack<Type> resolvingChain)
    {
        if (_instances.TryGetValue(serviceType, out var existingInstance))
            return existingInstance;

        if (!_registrations.TryGetValue(serviceType, out var implementationType))
            throw new InvalidOperationException($"Сервис {serviceType.Name} не зарегистрирован");

        if (resolvingChain.Contains(serviceType))
            throw new InvalidOperationException($"Обнаружена циклическая зависимость: {serviceType.Name}");

        resolvingChain.Push(serviceType);

        try
        {
            var constructor = implementationType
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
                throw new InvalidOperationException($"У класса {implementationType.Name} нет публичного конструктора");

            var parameters = constructor
                .GetParameters()
                .Select(p => Resolve(p.ParameterType, resolvingChain))
                .ToArray();

            var createdInstance = Activator.CreateInstance(implementationType, parameters);

            if (createdInstance == null)
                throw new InvalidOperationException($"Не удалось создать объект {implementationType.Name}");

            _instances[serviceType] = createdInstance;

            return createdInstance;
        }
        finally
        {
            resolvingChain.Pop();
        }
    }
}