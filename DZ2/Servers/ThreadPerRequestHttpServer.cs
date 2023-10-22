using System.Net.Sockets;

namespace DZ2.Servers;

/// <summary>
/// Реализация Http сервера, который использует метод Thread per Request.
/// </summary>
public class ThreadPerRequestHttpServer : HttpServer
{
    /// <summary>
    /// Конструктор для создания экземпляра класса ThreadPerRequestHttpServer.
    /// </summary>
    /// <param name="port">Порт, на котором будет работать Http сервер.</param>
    /// <param name="requestHandler">Обработчик запросов.</param>
    public ThreadPerRequestHttpServer(int port, IRequestHandler requestHandler) : base(port, requestHandler) { }

    /// <summary>
    /// Обработка клиентских подключений и создание нового потока для каждого запроса.
    /// </summary>
    /// <param name="client">Объект TcpClient, представляющий клиентское подключение.</param>
    protected override void ProcessClient(TcpClient client)
    {
        var thread = new Thread(() =>
        {
            using var stream = client.GetStream();
            // Обработка запроса с помощью установленного обработчика
            RequestHandler.HandleRequest(stream);
        });
        thread.Start(); // Запуск потока
    }
}