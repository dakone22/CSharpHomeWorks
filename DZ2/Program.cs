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

public class RequestHandlerManager : IRequestHandler
{
    private readonly IEnumerable<IRequestProcessor> _requestHandlers; 
    public RequestHandlerManager(IEnumerable<IRequestProcessor> requestHandlers)
    {
        _requestHandlers = new List<IRequestProcessor>(requestHandlers);
    }
    
    public void HandleRequest(NetworkStream stream)
    {
        var request = ReadMessage(stream);
        
        foreach (var handler in _requestHandlers) {
            if (!handler.ShouldHandleRequest(request)) continue;
            
            var response = handler.ProcessRequest(request);
            WriteMessage(stream, response);
            return;
        }
        
        WriteMessage(stream, HttpResponses.InternalServerErrorResponse("Unknown request"));
    }

    /// <summary>
    /// Читает сообщение из потока (stream) и возвращает его в виде строки.
    /// </summary>
    private static string ReadMessage(Stream stream)
    {
        var request = new byte[1024];
        var bytesRead = stream.Read(request, 0, request.Length);
        return Encoding.UTF8.GetString(request, 0, bytesRead);
    }

    /// <summary>
    /// Пишет сообщение в поток (stream).
    /// </summary>
    private static void WriteMessage(Stream stream, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }
}

public interface IRequestProcessor
{
    bool ShouldHandleRequest(string request);
    string ProcessRequest(string request);
}

public class StaticFileRequestProcessor : IRequestProcessor
{
    private readonly string? _resourcePath = new ConfigurationBuilder()
        .AddUserSecrets<StaticFileRequestProcessor>()
        .Build()["resource_path"];

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

    private static string ParseHttpRequest(string message)
    {
        var requestLines = message.Split(' ');
        return requestLines.Length > 1 ? requestLines[1] : "";
    }
}

public class AjaxRequestProcessor : IRequestProcessor
{
    private readonly ICalculator _calculator = new BzaCalculator();
    
    public bool ShouldHandleRequest(string request) => request.Split(" ")[1].Contains("calculate");

    public string ProcessRequest(string request)
    {
        Console.WriteLine("Received AJAX request: " + request);

        var requestPayload = ParseHttpRequestPayload(request);
        var response = ProcessRequestPayload(requestPayload);

        return response;
    }

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