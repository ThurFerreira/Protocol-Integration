using System.Text.Json.Serialization;

namespace integra_dados.Models;

public class WindyResponse
{
    [JsonPropertyName("ts")]
    public List<long>? Ts { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("hclouds-surface")]
    public List<double>? HighCloudsCoverage { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("lclouds-surface")]
    public List<double>? LowCloudsCoverage { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("cape-surface")]
    public List<double>? CapeSurface { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("temp-surface")]
    public List<float>? TempSurface { get; set; }

    // [JsonIgnore]
    [JsonPropertyName("wind_u-surface")]
    public List<double>? WindY { get; set; } // speed of wind from west to east

    // [JsonIgnore]
    [JsonPropertyName("wind_v-surface")]
    public List<double>? WindX { get; set; } // speed of wind from south to north

    // [JsonIgnore]
    [JsonPropertyName("rh-surface")]
    public List<double>? Rh { get; set; } // Relative Humidity in %

    // [JsonIgnore]
    [JsonPropertyName("past3hconvprecip-surface")]
    public List<decimal>? PrecSurface { get; set; } // Rainfall index (past 3 hours)
}