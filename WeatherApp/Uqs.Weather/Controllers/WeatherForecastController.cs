using Microsoft.AspNetCore.Mvc;
using AdamTibi.OpenWeather;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const int FORECAST_DAYS = 7;
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _config;
    private readonly IClient _client;

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config, IClient client)
    {
        this._logger = logger;
        this._config = config;
        this._client = client;
    }

    [HttpGet("ConvertCToF")]
    public double ConvertCToF(double c)
    {
        double f = c * (9d / 5d) + 32;
        _logger.LogInformation("conversion requested");
        return f;
    }

    [HttpGet("GetRealWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetReal()
    {
        decimal ANTWERPEN_LAT = 51.2194m;
        decimal ANTWERPEN_LON = 4.4025m;



        // DI Container //

        // string apiKey = _config["OpenWeather:Key"];
        // HttpClient httpClient = new HttpClient();
        // OneCall30Client openWeatherClient = new OneCall30Client(apiKey, httpClient);

        OneCallResponse res = await _client.OneCallAsync
            (
                ANTWERPEN_LAT, ANTWERPEN_LON,
                new[] {
                Excludes.Current, Excludes.Minutely,
                Excludes.Hourly, Excludes.Alerts }, Units.Metric
             );

        WeatherForecast[] wfs = new WeatherForecast[FORECAST_DAYS];

        for (int i = 0; i < wfs.Length; i++)
        {
            var wf = wfs[i] = new WeatherForecast();

            wf.Date = res.Daily[i + 1].Dt;
            double forecastedTemp = res.Daily[i + 1].Temp.Day;
            wf.TemperatureC = (int)Math.Round(forecastedTemp);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }
        return wfs;
    }

    [HttpGet("GetRandomWeatherForecast")]
    public IEnumerable<WeatherForecast> GetRandom()
    {
        WeatherForecast[] wfs = new WeatherForecast[FORECAST_DAYS];

        for (int i = 0; i < wfs.Length; i++)
        {
            var wf = wfs[i] = new WeatherForecast();
            wf.Date = DateTime.Now.AddDays(i + 1);
            wf.TemperatureC = Random.Shared.Next(-20, 55);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }

        return wfs;
    }

    private static string MapFeelToTemp(int temperatureC)
    {
        // Anything <= 0 is "Freezing"
        if (temperatureC <= 0)
        {
            return Summaries.First();
        }
        // Dividing the temperature into 5 intervals
        int summariesIndex = (temperatureC / 5) + 1;
        // Anything >= 45 is "Scorching"
        if (summariesIndex >= Summaries.Length)
        {
            return Summaries.Last();
        }
        return Summaries[summariesIndex];
    }
}
