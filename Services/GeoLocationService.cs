using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class GeoLocationService
{
    private readonly HttpClient _httpClient;

    public GeoLocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetCityFromIP(string ip)
    {
        var url = $"http://ip-api.com/json/{ip}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        if (json.RootElement.TryGetProperty("city", out var cityProp))
        {
            return cityProp.GetString();
        }

        return null;
    }
}