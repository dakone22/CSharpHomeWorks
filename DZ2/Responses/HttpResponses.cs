namespace DZ2.Responses;

/// <summary>
/// Класс, содержащий статические методы для создания различных типов HTTP-ответов.
/// </summary>
public static class HttpResponses
{
    /// <summary>
    /// Создает окончательный HTTP-ответ со статус-кодом 200 (OK).
    /// </summary>
    public static string OkResponse(string content) => BuildHttpResponse(200, "OK", content);

    /// <summary>
    /// Создает окончательный HTTP-ответ со статус-кодом 404 (Not Found).
    /// </summary>
    public static string NotFoundResponse(string content) => BuildHttpResponse(404, "Not Found", content);

    /// <summary>
    /// Создает окончательный HTTP-ответ со статус-кодом 400 (Bad Request).
    /// </summary>
    public static string BadRequestResponse(string content) => BuildHttpResponse(400, "Bad Request", content);

    /// <summary>
    /// Создает окончательный HTTP-ответ со статус-кодом 500 (Internal Server Error).
    /// </summary>
    public static string InternalServerErrorResponse(string content) =>
        BuildHttpResponse(500, "Internal Server Error", content);

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