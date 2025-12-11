using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System;
using Weather.Models;

namespace Weather.Classes
{
    public class GetWeather
    {
        public static string Url = "https://api.weather.yandex.ru/v2/forecast";
        public static string Key = "demo_yandex_weather_api_key_ca6d09349ba0";
        private static async Task<DataResponse> GetFromApi(float lat, float lon)
        {
            DataResponse dataResponse = null;
            string url = $"{Url}?lat={lat}&lon={lon}".Replace(",", ".");

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
