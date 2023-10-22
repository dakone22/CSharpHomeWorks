using System.Net;
using System.Net.Sockets;

namespace DZ2.Servers;

public interface IServer
{
    void Start();
}

/// <summary>
/// Абстрактный класс HttpServer реализует интерфейс IServer, выступая в роли базового сервера.
/// </summary>
public abstract class HttpServer : IServer
{
    private readonly TcpListener _listener;
    private bool _isRunning;

    /// <summary>
    /// Обработчик запросов.
    /// </summary>
    protected readonly IRequestHandler RequestHandler;

    /// <summary>
    /// Конструктор класса HttpServer.
    /// </summary>
    /// <param name="port">Порт на котором будет работать Http сервер.</param>
    /// <param name="requestHandler">Обработчик запросов.</param>
    protected HttpServer(int port, IRequestHandler requestHandler)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _isRunning = false;
        RequestHandler = requestHandler;
    }

    /// <summary>
    /// Запуск сервера.
    /// </summary>
    public void Start()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine("Server started.");
        while (_isRunning) {
            try {
                var client = _listener.AcceptTcpClient();
                ProcessClient(client);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message); // Запись исключения в лог
            }
        }
    }

    /// <summary>
    /// Абстрактный метод для обработки клиентских подключений.
    /// </summary>
    /// <param name="client">Объект TcpClient, представляющий клиентское подключение.</param>
    protected abstract void ProcessClient(TcpClient client);

    /// <summary>
    /// Остановка сервера.
    /// </summary>
    private void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }

    /// <summary>
    /// Деструктор HttpServer, останавливает сервер при уничтожении объекта.
    /// </summary>
    ~HttpServer() => Stop();
}