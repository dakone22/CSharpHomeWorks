using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DZ1;
using DZ2.Responses;
using DZ2.Servers;
using Microsoft.Extensions.Configuration;

namespace DZ2;

public interface IRequestHandler
{
    void HandleRequest(NetworkStream stream);
}

/// <summary>
/// Менеджер обработки запросов, выбирающий, как обрабатывать запрос.
/// </summary>
public class RequestHandlerManager : IRequestHandler
{
    /// Список обработчиков запросов.
    private readonly IEnumerable<IRequestProcessor> _requestHandlers; 

    /// <summary>
    /// Создать экземпляр класса <see cref="RequestHandlerManager"/>.
    /// </summary>
    /// <param name="requestHandlers">Количество обработчиков запросов.</param>
    public RequestHandlerManager(IEnumerable<IRequestProcessor> requestHandlers)
    {
        _requestHandlers = new List<IRequestProcessor>(requestHandlers);
    }

    /// <summary>
    /// Основной метод обработки запросов
    /// </summary>
    /// <param name="stream">Сетевой поток для чтения/записи.</param>
    public void HandleRequest(NetworkStream stream)
    {
        var request = ReadMessage(stream);
        foreach (var handler in _requestHandlers) 
        { 
            if (!handler.ShouldHandleRequest(request)) continue;
            var response = handler.ProcessRequest(request);
            WriteMessage(stream, response);
            return;
        }
        WriteMessage(stream, HttpResponses.InternalServerErrorResponse("Unknown request"));
    }

    /// <summary>
    /// Чтение сообщения из потока.
    /// </summary>
    /// <param name="stream">Сетевой поток для чтения.</param>
    private static string ReadMessage(Stream stream)
    {
        var request = new byte[1024];
        var bytesRead = stream.Read(request, 0, request.Length);
        return Encoding.UTF8.GetString(request, 0, bytesRead);
    }

    /// <summary>
    /// Запись сообщения в поток.
    /// </summary>
    /// <param name="stream">Сетевой поток для записи.</param>
    /// <param name="message">Сообщение для записи.</param>
    private static void WriteMessage(Stream stream, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }
}

/// <summary>
/// Интерфейс для обработчиков запросов.
/// </summary>
public interface IRequestProcessor
{
    /// <summary>
    /// Метод определяет, должен ли обработчик обрабатывать данный запрос или нет.
    /// </summary>
    /// <param name="request">Запрос, который необходимо проверить.</param>
    /// <returns>Возвращает true, если обработчик должен обработать запрос, иначе false.</returns>
    bool ShouldHandleRequest(string request);
    
    /// <summary>
    /// Создает HTTP ответ на основе файла, запрошенного в запросе.
    /// </summary>
    /// <param name="requestedFile">Запрашиваемый файл.</param>
    /// <returns>Возвращает HTTP ответ в виде строки.</returns>
    string ProcessRequest(string request);
}

/// <summary>
/// Обработчик запросов для статических файлов. Производит обработку запросов на получение статических файлов.
/// </summary>
public class StaticFileRequestProcessor : IRequestProcessor
{
    /// Путь к ресурсам на сервере.
    private readonly string? _resourcePath = new ConfigurationBuilder()
        .AddUserSecrets<StaticFileRequestProcessor>()
        .Build()["resource_path"];

    /// Список поддерживаемых расширений файлов. 
    private readonly string[] _extensions = {
        ".html",
        // ".css",
        // ".js",
        // ".jpg",
        // ".png",
        // ".ico",
    };
    
    public bool ShouldHandleRequest(string request) => _extensions.Any(request.Split(" ")[1].Contains);

    public string ProcessRequest(string request)
    {
        Console.WriteLine("Received message: " + request);

        var requestedFile = ParseHttpRequest(request);

        return ConstructHttpResponse(requestedFile);
    }

    /// <summary>
    /// Создает HTTP ответ на основе файла, запрошенного в запросе.
    /// </summary>
    /// <param name="requestedFile">Запрашиваемый файл.</param>
    /// <returns>Возвращает HTTP ответ в виде строки.</returns>
    private string ConstructHttpResponse(string requestedFile)
    {
        try {
            var fileContent = File.ReadAllText(_resourcePath + requestedFile, Encoding.UTF8);
            return HttpResponses.OkResponse(fileContent);
        } catch (FileNotFoundException ex) {
            Console.WriteLine(ex.Message);
            return HttpResponses.NotFoundResponse($"File \"{requestedFile}\" not found!");
        }
    }

    /// <summary>
    /// Разбор HTTP запроса на составные части.
    /// </summary>
    /// <param name="message">HTTP запрос в виде строки.</param>
    /// <returns>Возвращает имя запрашиваемого файла, если он присутствовал в запросе.</returns>
    private static string ParseHttpRequest(string message)
    {
        var requestLines = message.Split(' ');
        return requestLines.Length > 1 ? requestLines[1] : "";
    }
}

/// <summary>
/// Обработчик AJAX запросов на вычисление выражений.
/// </summary>
public class AjaxRequestProcessor : IRequestProcessor
{
    /// Объект калькулятора, осуществляющего вычисление выражений.
    private readonly ICalculator _calculator = new BzaCalculator();
    
    public bool ShouldHandleRequest(string request) => request.Split(" ")[1].Contains("calculate");

    public string ProcessRequest(string request)
    {
        Console.WriteLine("Received AJAX request: " + request);

        var requestPayload = ParseHttpRequestPayload(request);
        var response = ProcessRequestPayload(requestPayload);

        return response;
    }

    /// <summary>
    /// Обрабатывает полезную нагрузку (payload) AJAX запроса и генерирует HTTP ответ.
    /// </summary>
    /// <param name="requestPayload">Полезная нагрузка запроса.</param>
    /// <returns>Возвращает HTTP ответ в виде строки.</returns>
    private string ProcessRequestPayload(string requestPayload)
    {
        if (string.IsNullOrEmpty(requestPayload))
            return HttpResponses.BadRequestResponse("No expression was provided.");

        var expression = JsonSerializer.Deserialize<Dictionary<string, string>>(requestPayload)?["expression"];
        if (expression == null) return HttpResponses.BadRequestResponse("no \"expression\" key");

        try {
            var result = _calculator.Calculate(expression);
            var jsonResult = JsonSerializer.Serialize(new {result = result});
            return HttpResponses.OkResponse(jsonResult);

        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return HttpResponses.InternalServerErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Разбор HTTP запроса на и извлечение полезной нагрузки.
    /// </summary>
    /// <param name="message">HTTP запрос в виде строки.</param>
    /// <returns>Возвращает строку содержащую полезную нагрузку запроса.</returns>
    private static string ParseHttpRequestPayload(string message)
    {
        return message[(message.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4)..];
    }
}


internal static class Program
{
    public static void Main()
    {
        IServer server = new PoolThreadHttpServer(49212, new RequestHandlerManager(new IRequestProcessor[] {
            new StaticFileRequestProcessor(),
            new AjaxRequestProcessor()
        }));
        server.Start();
    }
}