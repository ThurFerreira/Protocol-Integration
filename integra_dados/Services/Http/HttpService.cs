using System.Text;
using integra_dados.Models.Response.Api;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace integra_dados.Services;

public class HttpService(HttpClient httpClient)
{
    public async Task<HttpResponseMessage> Post(string url, object body, string token)
    { 
        string json = JsonConvert.SerializeObject(body);
        // Console.WriteLine(json);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", "Bearer " + token);
        
        try
        {
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (HttpRequestException exception)
        {
            return null;
        }
    }
}