using System.Net.Sockets;

namespace DZ2.Servers;

/// <summary>
/// Реализация Http сервера, использующего Pool Thread для обработки запросов.
/// </summary>
public class PoolThreadHttpServer : HttpServer
{
    /// <summary>
    /// Конструктор класса PoolThreadHttpServer.
    /// Определяет максимальное и минимальное количество потоков в пуле потоков.
    /// </summary>
    /// <param name="port">Порт на котором будет работать Http сервер.</param>
    /// <param name="requestHandler">Обработчик запросов.</param>
    public PoolThreadHttpServer(int port, IRequestHandler requestHandler) : base(port, requestHandler)
    {
        var maxThreadsCount = Environment.ProcessorCount * 4;
        // Устанавливает максимальное количество потоков в ThreadPool
        ThreadPool.SetMaxThreads(maxThreadsCount, maxThreadsCount);
        // Устанавливает минимальное количество потоков в ThreadPool
        ThreadPool.SetMinThreads(2, 2);
    }

    /// <summary>
    /// Обработка клиентских подключений и отправка их в пул потоков ThreadPool.
    /// </summary>
    /// <param name="client">Объект TcpClient, представляющий клиентское подключение.</param>
    protected override void ProcessClient(TcpClient client)
    {
        // Помещает метод в очередь на выполнение и указывает объект, успешно используемый этим методом.
        ThreadPool.QueueUserWorkItem(state =>
        {
            if (state is not TcpClient clientState) return;
            using var stream = clientState.GetStream();
            // Обработка запроса при помощи установленного обработчика запросов
            RequestHandler.HandleRequest(stream);
        }, client);
    }
}