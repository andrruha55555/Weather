using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Weather.Models;

namespace Weather.Classes
{
    public class GetWeather
    {
        public static string WeatherUrl = "https://api.weather.yandex.ru/v2/forecast";
        public static string WeatherKey = "demo_yandex_weather_api_key_ca6d09349ba0";

        public static async Task<DataResponse> GetWeatherData(string cityName, string userId = "default_user")
        {
            var cachedData = await WeatherCache.GetCachedWeather(cityName);
            if (cachedData != null)
            {
                Console.WriteLine("✓ Используются кэшированные данные");
                return cachedData;
            }
            if (!await WeatherCache.CheckAndIncrementLimit(userId))
            {
                throw new Exception($"Достигнут дневной лимит запросов ({WeatherCache.DAILY_LIMIT}). Данные будут взяты из кэша при наличии.");
            }
            var coordinates = await Geocoder.GetCoordinates(cityName);
            if (coordinates.lat == 0 && coordinates.lon == 0)
            {
                throw new Exception("Не удалось определить координаты города");
            }
            DataResponse dataResponse = await GetFromApi(coordinates.lat, coordinates.lon);
            if (dataResponse != null)
            {
                await WeatherCache.SaveToCache(cityName, coordinates.lat, coordinates.lon, dataResponse);
                Console.WriteLine("✓ Данные получены из API и сохранены в кэш");
            }

            return dataResponse;
        }

        private static async Task<DataResponse> GetFromApi(float lat, float lon)
        {
            DataResponse dataResponse = null;
            string url = $"{WeatherUrl}?lat={lat}&lon={lon}".Replace(",", ".");

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("X-Yandex-Weather-Key", WeatherKey);

                    using (var response = await client.SendAsync(request))
                    {
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        dataResponse = JsonConvert.DeserializeObject<DataResponse>(contentResponse);
                    }
                }
            }
            return dataResponse;
        }
    }
}