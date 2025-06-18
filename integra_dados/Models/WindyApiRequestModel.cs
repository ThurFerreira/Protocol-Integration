namespace integra_dados.Models;

public class WindyApiRequestModel
{
    public double lat;
    public double lon;
    public string model = "gfs";
    public string[] parameters;
    public string[] levels = {"surface"};
    public string key;
    public WindyApiRequestModel(double _lat, double _lon, string[] _parameters, string _key) {
        lat = _lat;
        lon = _lon;
        parameters = _parameters;
        key = _key;
    }
}