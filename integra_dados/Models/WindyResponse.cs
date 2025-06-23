using System.Text.Json.Serialization;

namespace integra_dados.Models;

public class WindyResponse
{
    [JsonPropertyName("ts")]
    public List<long>? Ts { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("hclouds-surface")]
    public List<float>? HighCloudsCoverage { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("lclouds-surface")]
    public List<float>? LowCloudsCoverage { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("cape-surface")]
    public List<float>? CapeSurface { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("temp-surface")]
    public List<float>? TempSurface { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("wind_u-surface")]
    public List<float>? WindY { get; set; } // speed of wind from west to east

    // [JsonIgnore]
    [JsonPropertyName("wind_v-surface")]
    public List<float>? WindX { get; set; } // speed of wind from south to north

    // [JsonIgnore]
    [JsonPropertyName("rh-surface")]
    public List<float>? Rh { get; set; } // Relative Humidity in %

    // [JsonIgnore]
    [JsonPropertyName("past3hconvprecip-surface")]
    public List<float>? PrecipSurface { get; set; } // Rainfall index (past 3 hours)
}