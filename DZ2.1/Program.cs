using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DZ2._1;

/// <summary>
/// Структура для представления HTTP-ответа.
/// </summary>
public struct HttpResponse
{
    public int StatusCode;
    public string? StatusDescription;
    public string? ContentType;
    public string Charset;
    public string? Content;

    /// <summary>
    /// Конструктор по умолчанию для HttpResponse.
    /// </summary>
    public HttpResponse()
    {
        StatusCode = 0;
        StatusDescription = null;
        ContentType = null;
        Charset = "UTF-8";
        Content = null;
    }

    /// <summary>
    /// Форматирует HTTP-ответ в строку для отправки по сети.
    /// </summary>
    public override string ToString()
    {
        return $"HTTP/1.1 {StatusCode} {StatusDescription}\r\n" +
               $"Content-Type: {ContentType}; charset={Charset}\r\n\r\n" +
               Content;
    }
}

/// <summary>
/// Строитель объекта HttpResponse, используемый для настройки и сборки экземпляров HttpResponse. 
/// </summary>
public class HttpResponseBuilder
{
    private HttpResponse _httpResponse;

    public HttpResponseBuilder SetStatusCode(int statusCode)
    {
        _httpResponse.StatusCode = statusCode;
        return this;
    }

    public HttpResponseBuilder SetStatusDescription(string statusDescription)
    {
        _httpResponse.StatusDescription = statusDescription;
        return this;
    }

    public HttpResponseBuilder SetHtmlContent(string htmlContent)
    {
        _httpResponse.Content = htmlContent;
        _httpResponse.ContentType = "text/html";
        return this;
    }

    public HttpResponseBuilder SetCharset(string charset)
    {
        _httpResponse.Charset = charset;
        return this;
    }

    /// <summary>
    /// Построение и возврат настроенного экземпляра HttpResponse.
    /// </summary>
    public HttpResponse Build()
    {
        var result = _httpResponse;
        // сбросить _httpResponse на новый пустой HttpResponse
        _httpResponse = new HttpResponse();
        return result;
    }
}

/// <summary>
/// Класс, содержащий статические методы для создания различных типов HTTP-ответов.
/// </summary>
public static class HttpResponses
{
    /// <summary>
    /// Создает окончательный HTTP-ответ со статус-кодом 200 (OK).
    /// </summary>
    public static string OkResponse(string content)
    {
        return BuildHttpResponse(200, "OK", content);
    }

    /// <summary>
    /// Создает окончательный HTTP-ответ со статус-кодом 404 (Not Found).
    /// </summary>
    public static string NotFoundResponse(string content)
    {
        return BuildHttpResponse(404, "Not Found", content);
    }

    /// <summary>
    /// Вспомогательный метод для создания HTTP-ответов с заданными кодами статуса, описаниями и содержимым.
    /// </summary>
    private static string BuildHttpResponse(int statusCode, string statusDescription, string content)
    {
        return new HttpResponseBuilder()
            .SetStatusCode(statusCode)
            .SetStatusDescription(statusDescription)
            .SetHtmlContent(content)
            .SetCharset("UTF-8")
            .Build()
            .ToString();
    }
}

/// <summary>
/// Интерфейс для обработки HTTP-запросов.
/// </summary>
public interface IRequestHandler
{
    void HandleRequest(NetworkStream stream);
}

/// <summary>
/// Реализация обработчика запросов, служащая статические HTML-файлы.
/// </summary>
public class StaticFileRequestHandler : IRequestHandler
{
    private readonly string? _resourcePath = new ConfigurationBuilder()
        .AddUserSecrets<StaticFileRequestHandler>()
        .Build()["resource_path"];

    /// <summary>
    /// Обрабатывает запрос, принимая NetworkStream и возвращая ему ответ.
    /// </summary>
    public void HandleRequest(NetworkStream stream)
    {
        var request = ReadMessage(stream);
        Console.WriteLine("Received message: " + request);

        // Парсим запрос на файл
        var requestedFile = ParseHttpRequest(request);

        // Создаем HTTP ответ
        var response = ConstructHttpResponse(requestedFile);
        WriteMessage(stream, response);
    }

    /// <summary>
    /// Читает сообщение из потока (stream) и возвращает его в виде строки.
    /// </summary>
    private static string ReadMessage(Stream stream)
    {
        var request = new byte[1024];
        var bytesRead = stream.Read(request, 0, request.Length);
        return Encoding.ASCII.GetString(request, 0, bytesRead);
    }

    /// <summary>
    /// Пишет сообщение в поток (stream).
    /// </summary>
    private static void WriteMessage(Stream stream, string message)
    {
        var buffer = Encoding.ASCII.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    /// <summary>
    /// Создает HTTP-ответ, используя запрошенный файл.
    /// </summary>
    private string ConstructHttpResponse(string requestedFile)
    {
        try {
            var fileContent = File.ReadAllText(_resourcePath + requestedFile);
            return HttpResponses.OkResponse(fileContent);
        } catch (FileNotFoundException ex) {
            Console.WriteLine(ex.Message);
            return HttpResponses.NotFoundResponse($"File \"{requestedFile}\" not found!");
        }
    }

    /// <summary>
    /// Парсит HTTP-запрос и возвращает запрошенный путь файла.
    /// </summary>
    private static string ParseHttpRequest(string message)
    {
        var requestLines = message.Split(' ');
        return requestLines.Length > 1 ? requestLines[1] : "";
    }
}

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
    protected readonly IRequestHandler Handler;

    /// <summary>
    /// Конструктор класса HttpServer.
    /// </summary>
    /// <param name="port">Порт на котором будет работать Http сервер.</param>
    /// <param name="handler">Обработчик запросов.</param>
    protected HttpServer(int port, IRequestHandler handler)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _isRunning = false;
        Handler = handler;
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

/// <summary>
/// Реализация Http сервера, который использует метод Thread per Request.
/// </summary>
public class ThreadPerRequestHttpServer : HttpServer
{
    /// <summary>
    /// Конструктор для создания экземпляра класса ThreadPerRequestHttpServer.
    /// </summary>
    /// <param name="port">Порт, на котором будет работать Http сервер.</param>
    /// <param name="handler">Обработчик запросов.</param>
    public ThreadPerRequestHttpServer(int port, IRequestHandler handler) : base(port, handler) { }

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
            Handler.HandleRequest(stream);
        });
        thread.Start(); // Запуск потока
    }
}

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
    /// <param name="handler">Обработчик запросов.</param>
    public PoolThreadHttpServer(int port, IRequestHandler handler) : base(port, handler)
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
            Handler.HandleRequest(stream);
        }, client);
    }
}

internal static class Program
{
    public static void Main()
    {
        IServer server = new PoolThreadHttpServer(49212, new StaticFileRequestHandler());
        server.Start();
    }
}