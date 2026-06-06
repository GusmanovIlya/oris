using System;
using TEST.Container;
using TEST.Services;

namespace TEST;

public class Program
{
    public static void Main()
    {
        IDependencyContainer container = new SimpleContainer();

        container.Register<ILogger, ConsoleLogger>();
        container.Register<IRepository, UserRepository>();
        container.Register<IUserService, UserService>();

        IUserService userService = container.Resolve<IUserService>();

        userService.CreateUser("Искандер");

        Console.WriteLine("Программа завершена.");
    }
}