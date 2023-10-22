namespace DZ2.Responses;

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