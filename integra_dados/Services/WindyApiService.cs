using integra_dados.Models;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace integra_dados.Services;

public class WindyApiService(HttpService httpService)
{
    public async Task<WindyResponse> GetWindyForecast(Location location, string varType)
    {
        string token = "kjUV4msAS33SRffOHjdzLp0ygCxu1Fmr";
        WindyApiRequestModel body = new WindyApiRequestModel(location.Lat, location.Lng, new []{varType}, token);
        
        HttpResponseMessage response = await httpService.Post("https://api.windy.com/api/point-forecast/v2", body, token);
        string responseBody = response.Content.ReadAsStringAsync().Result;
        
        WindyResponse responseObject = JsonSerializer.Deserialize<WindyResponse>(responseBody)!;
        return responseObject;
    }
}