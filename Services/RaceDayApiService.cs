using System.Text;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services;

public class RaceDayApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RaceDayApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    // Create an HTTP client for communicating with the API.
    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("RaceDayAPI");
    }

    // Send requests while forwarding the API session cookie.
    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request)
    {
        var client = CreateClient();

        var apiCookie = _httpContextAccessor.HttpContext?
            .Session.GetString("ApiCookie");

        if (!string.IsNullOrEmpty(apiCookie))
        {
            request.Headers.Add("Cookie", apiCookie);
        }

        var response = await client.SendAsync(request);

        if (response.Headers.TryGetValues(
            "Set-Cookie", out var cookies))
        {
            var sessionCookie = cookies
                .FirstOrDefault(c => c.Contains(".AspNetCore.Session"));

            if (sessionCookie != null)
            {
                var cookieValue = sessionCookie.Split(';')[0];

                _httpContextAccessor.HttpContext?
                    .Session.SetString("ApiCookie", cookieValue);
            }
        }

        return response;
    }

    // Retrieve information from the API.
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        var response = await SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    // Send POST requests and retrieve the API response.
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, TRequest data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                json, Encoding.UTF8, "application/json")
        };

        var response = await SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    // Send PUT requests.
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint, TRequest data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);

        var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = new StringContent(
                json, Encoding.UTF8, "application/json")
        };

        var response = await SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    // Send DELETE requests.
    public async Task<bool> DeleteAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        var response = await SendAsync(request);

        return response.IsSuccessStatusCode;
    }

    // Send POST requests and return both successful and error responses.
    public async Task<string?> PostForStringAsync<TRequest>(
        string endpoint, TRequest data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                json, Encoding.UTF8, "application/json")
        };

        var response = await SendAsync(request);

        // Read the API response, including any error details.
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return $"API ERROR {(int)response.StatusCode} " +
                   $"({response.ReasonPhrase}): {responseBody}";
        }

        return responseBody;
    }

    // Upload a file to the API using multipart form data.
    public async Task<HttpResponseMessage> PostFileAsync(
        string endpoint,
        IFormFile file)
    {
        using var content = new MultipartFormDataContent();

        await using var stream = file.OpenReadStream();

        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType);

        content.Add(fileContent, "file", file.FileName);

        var request = new HttpRequestMessage(
            HttpMethod.Post, endpoint)
        {
            Content = content
        };

        return await SendAsync(request);
    }

    // Retrieve an image from the API.
    public async Task<(byte[] Data, string ContentType)?> GetFileAsync(
        string endpoint)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get, endpoint);

        var response = await SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        byte[] data =
            await response.Content.ReadAsByteArrayAsync();

        string contentType =
            response.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        return (data, contentType);
    }
}

