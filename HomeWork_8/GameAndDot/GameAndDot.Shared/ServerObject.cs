using GameAndDot.Shared.Models;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

namespace GameAndDot.Shared
{
    public class ServerObject
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888); // сервер для прослушивания
        public List<ClientObject> Clients { get; private set; } = new(); // все подключения

        public List<EventMessage> GameHistory { get; private set; } = new(); // всех точек, которые ставили игроки

        public void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject? client = Clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null) Clients.Remove(client);
            client?.Close();
        }

        // прослушивание входящих подключений
        public async Task ListenAsync()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Clients.Add(clientObject);

                    Task.Run(clientObject.ProcessAsync);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        public async Task BroadcastMessageAsync(string message, string id)
        {
            foreach (var client in Clients)
            {
                try
                {
                    await client.Writer.WriteLineAsync(message); //передача данных
                    await client.Writer.FlushAsync();
                }
                catch
                {

                }
            }
        }

        public async Task BroadcastMessageAllAsync(string message)
        {
            foreach (var client in Clients)
            {
                await client.Writer.WriteLineAsync(message); //передача данных
                await client.Writer.FlushAsync();
            }
        }


        // отключение всех клиентов
        protected internal void Disconnect()
        {
            foreach (var client in Clients)
            {
                client.Close(); //отключение клиента
            }
            tcpListener.Stop(); //остановка сервера
        }
    }
}
