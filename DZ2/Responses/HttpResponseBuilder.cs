namespace DZ2.Responses;

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