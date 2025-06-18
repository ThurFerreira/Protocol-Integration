using integra_dados.Models;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace integra_dados.Services;

public class WindyApiService(HttpService httpService)
{
    public object? GetWindyForecast(double lat, double lng, string varType)
    {
        string token = "kjUV4msAS33SRffOHjdzLp0ygCxu1Fmr";
        WindyApiRequestModel body = new WindyApiRequestModel(lat, lng, new []{varType}, token);
        
        string postResponse = httpService.Post("https://api.windy.com/api/point-forecast/v2", body, token).Result;
        
        var responseObject = JsonConvert.DeserializeObject(postResponse, typeof(WindyResponse));
        return responseObject;
    }
}