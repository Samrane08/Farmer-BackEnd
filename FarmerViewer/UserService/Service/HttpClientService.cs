using System.Net;
using System.Text.Json;
using UserService.Model.Response;

namespace UserService.Service;
public interface IHttpClientService
{
    Task<T> RequestSend<T>(HttpMethod requestMethod, string requestUri,dynamic? requestBody);
    
}

public class HttpClientService: IHttpClientService
{   
    public async Task<T> RequestSend<T>(HttpMethod requestMethod, string requestUri, dynamic? requestBody)
    {
        HttpStatusCode HttpStatusCode;
        try
        {

            //Uri uri = new Uri(requestUri);
            //var scheme = uri.Scheme;
            //var host = uri.Host;
            //var port = uri.Port;

            //var httpClientHandler = new HttpClientHandler()
            //{
            //    Proxy = new WebProxy($"{scheme}://{host}:{port}"),
            //    UseProxy = true,
            //};

            var request = new HttpRequestMessage(requestMethod, requestUri);
            if (requestBody != null)
            {
                string content = JsonSerializer.Serialize(requestBody);
                request.Content = new StringContent(content, null, "application/json");
            }
            HttpClient httpClient = new HttpClient();
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            var response = await httpClient.SendAsync(request);
            HttpStatusCode = response.StatusCode;
            response.EnsureSuccessStatusCode();
            var result =  await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(result);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Deserialize<T>(string.Empty);
        }
    }

    
}
