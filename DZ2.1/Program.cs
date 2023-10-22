using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DZ2._1;

public struct HttpResponse
{
    public int StatusCode;
    public string? StatusDescription;
    public string? ContentType;
    public string Charset = "UTF-8";
    public string? Content;

    public HttpResponse()
    {
        StatusCode = 0;
        StatusDescription = null;
        ContentType = null;
        Content = null;
    }

    public override string ToString()
    {
        return $"HTTP/1.1 {StatusCode} {StatusDescription}\r\n" +
               $"Content-Type: {ContentType}; charset={Charset}\r\n\r\n" + 
               Content;
    }
}

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

    public HttpResponse Build()
    {
        var result = _httpResponse;
        _httpResponse = new HttpResponse();
        return result;
    }
}

public static class HttpResponses
{
    public static string OkResponse(string content)
    {
        return BuildHttpResponse(200, "OK", content);
    }
    
    public static string NotFoundResponse(string content)
    {
        return BuildHttpResponse(404, "Not Found", content);
    }

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
/// Реализация по умолчанию для обслуживания статических файлов HTML.
/// </summary>
public class StaticFileRequestHandler : IRequestHandler
{
    private readonly string? _resourcePath = new ConfigurationBuilder()
                                                 .AddUserSecrets<StaticFileRequestHandler>()
                                                 .Build()["resource_path"];
    public void HandleRequest(NetworkStream stream)
    {
        var request = ReadMessage(stream);
        Console.WriteLine("Received message: " + request);

        // Parse request to get file path
        var requestedFile = ParseHttpRequest(request);

        var response = ConstructHttpResponse(requestedFile);
        WriteMessage(stream, response);
    }

    private static string ReadMessage(Stream stream)
    {
        var request = new byte[1024];
        var bytesRead = stream.Read(request, 0, request.Length);
        return Encoding.ASCII.GetString(request, 0, bytesRead);
    }

    private static void WriteMessage(Stream stream, string message)
    {
        var buffer = Encoding.ASCII.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }
    
    private string ConstructHttpResponse(string requestedFile)
    {
        try 
        {
            var fileContent = File.ReadAllText(_resourcePath + requestedFile);
            return HttpResponses.OkResponse(fileContent);
        } 
        catch (FileNotFoundException ex) 
        {
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

public abstract class HttpServer
{
    private readonly TcpListener _listener;
    private bool _isRunning;
    protected readonly IRequestHandler Handler;

    protected HttpServer(int port, IRequestHandler handler)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _isRunning = false;
        Handler = handler;
    }

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
                Console.WriteLine(ex.Message); // Log exception
            }
        }
    }

    protected abstract void ProcessClient(TcpClient client);

    private void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }

    ~HttpServer() => Stop();
}

public class ThreadPerRequestHttpServer : HttpServer
{
    public ThreadPerRequestHttpServer(int port, IRequestHandler handler) : base(port, handler) { }

    protected override void ProcessClient(TcpClient client)
    {
        var thread = new Thread(() =>
        {
            using var stream = client.GetStream();
            Handler.HandleRequest(stream);
        });
        thread.Start();
    }
}

public class PoolThreadHttpServer : HttpServer
{
    public PoolThreadHttpServer(int port, IRequestHandler handler) : base(port, handler)
    {
        var maxThreadsCount = Environment.ProcessorCount * 4;
        ThreadPool.SetMaxThreads(maxThreadsCount, maxThreadsCount);
        ThreadPool.SetMinThreads(2, 2);
    }

    protected override void ProcessClient(TcpClient client)
    {
        ThreadPool.QueueUserWorkItem(state =>
        {
            if (state is not TcpClient clientState) return;
            using var stream = clientState.GetStream();
            Handler.HandleRequest(stream);
        }, client);
    }
}

internal static class Program
{
    public static void Main()
    {
        var server = new PoolThreadHttpServer(49212, new StaticFileRequestHandler());
        server.Start();
    }
}