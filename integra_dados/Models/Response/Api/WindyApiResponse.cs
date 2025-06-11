// using System.Text.Json.Serialization;
// using Newtonsoft.Json;
//
// namespace integra_dados.Models.Response.Api;
//
// public class WindyApiResponse
// {
//     public static List<ApiResponse> ToApiResponseForPrecipitation(string response)
//     {
//         var rainFallIndices = JsonConvert.DeserializeObject<RainFallIndexResponseWindy>(response);
//         return rainFallIndices.Ts.Select((timestamp, index) =>
//         {
//             var precSurface = (float)rainFallIndices.PrecSurface[index] * 1000;
//             var localDate = DateConverter.ConvertLongToDateTime(timestamp);
//             return new WindyApiResponse(localDate, precSurface);
//         }).Cast<ApiResponse>().ToList();
//     }
//
//     public static List<ApiResponse> ToApiResponseForTemperature(string response)
//     {
//         var temperatures = JsonConvert.DeserializeObject<TemperatureResponseWindy>(response);
//         return temperatures.Ts.Select((timestamp, index) =>
//         {
//             var surfaceTemp = (float)temperatures.TempSurface[index];
//             var localDate = DateConverter.ConvertLongToDateTime(timestamp);
//             var celsiusSurfaceTemp = TemperatureConverter.ConvertTemperatureToCelsius(surfaceTemp);
//             return new WindyApiResponse(localDate, celsiusSurfaceTemp);
//         }).Cast<ApiResponse>().ToList();
//     }
//
//     public static List<ApiResponse> ToApiResponseForWind(string postResponse)
//     {
//         var forecast = JsonConvert.DeserializeObject<WindResponseWindy>(postResponse);
//         return forecast.Ts.Select((timestamp, index) =>
//         {
//             var localDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
//             var windInYVetor = (float)forecast.Wind_Y[index];
//             var windInXVector = (float)forecast.Wind_X[index];
//             return new WindyApiResponse(localDate, new float[] { windInXVector, windInYVetor });
//         }).Cast<ApiResponse>().ToList();
//     }
//
//     public static List<ApiResponse> ToApiResponseForRelativeHumidity(string postResponse)
//     {
//         var forecast = JsonConvert.DeserializeObject<RelativeHumidityResponseWindy>(postResponse);
//         return forecast.Ts.Select((timestamp, index) =>
//         {
//             var localDate = DateConverter.ConvertLongToDateTime(timestamp);
//             var rhValue = (float)forecast.Rh[index];
//             return new WindyApiResponse(localDate, rhValue);
//         }).Cast<ApiResponse>().ToList();
//     }
//
//     public static List<ApiResponse> ToApiResponseForAtmosphericStability(string postResponse)
//     {
//         var forecast = JsonConvert.DeserializeObject<AtmosphericStabilityResponseWindy>(postResponse);
//         return forecast.Ts.Select((timestamp, index) =>
//         {
//             var localDate = DateConverter.ConvertLongToDateTime(timestamp);
//             var capeSurface = (float)forecast.CapeSurface[index];
//             return new WindyApiResponse(localDate, capeSurface);
//         }).Cast<ApiResponse>().ToList();
//     }
//
//     public static List<ApiResponse> ToApiResponseForLowCloudsCoverage(string postResponse)
//     {
//         var forecast = JsonConvert.DeserializeObject<LowCloudsCoverageResponseWindy>(postResponse);
//         return forecast.Ts.Select((timestamp, index) =>
//         {
//             var localDate = DateConverter.ConvertLongToDateTime(timestamp);
//             var cloudCoverage = (float)forecast.LowCloudsCoverage[index];
//             return new WindyApiResponse(localDate, cloudCoverage);
//         }).Cast<ApiResponse>().ToList();
//     }
//
//     public static List<ApiResponse> ToApiResponseForHighCloudsCoverage(string postResponse)
//     {
//         var forecast = JsonConvert.DeserializeObject<HighCloudsCoverageResponseWindy>(postResponse);
//         return forecast.Ts.Select((timestamp, index) =>
//         {
//             var localDate = DateConverter.ConvertLongToDateTime(timestamp);
//             var cloudCoverage = (float)forecast.HighCloudsCoverage[index];
//             return new WindyApiResponse(localDate, cloudCoverage);
//         }).Cast<ApiResponse>().ToList();
//     }
// }
//
//
//     public class ResponseWindy
//     {
//         public List<long> Ts { get; set; }
//     }
//     
//     public class TemperatureResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("temp-surface")]
//         public List<double> TempSurface { get; set; }
//     }
//     
//     public class HighCloudsCoverageResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("hclouds-surface")]
//         public List<double> HighCloudsCoverage { get; set; }
//     }
//
//     public class LowCloudsCoverageResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("lclouds-surface")]
//         public List<double> LowCloudsCoverage { get; set; }
//     }
//
//     public class AtmosphericStabilityResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("cape-surface")]
//         public List<double> CapeSurface { get; set; }
//     }
//
//     public class WindResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("wind_u-surface")]
//         public List<double> Wind_Y { get; set; }
//
//         [JsonPropertyName("wind_v-surface")]
//         public List<double> Wind_X { get; set; }
//     }
//
//     public class RelativeHumidityResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("rh-surface")]
//         public List<double> Rh { get; set; }
//     }
//
//     public class RainFallIndexResponseWindy : ResponseWindy
//     {
//         [JsonPropertyName("past3hconvprecip-surface")]
//         public List<decimal> PrecSurface { get; set; }
//     }
// }