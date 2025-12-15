using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Weather.Classes
{
    public class Geocoder
    {
        public static string GeocodingUrl = "https://geocode-maps.yandex.ru/v1/";
        public static string GeocodingKey = "e1562ea2-8f0a-4cee-b73e-bc04405fa1d7";

        public static async Task<(float lat, float lon)> GetCoordinates(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("Введите название города", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return (0, 0);
            }
            string url = $"{GeocodingUrl}?apikey={GeocodingKey}&geocode={Uri.EscapeDataString(city)}&format=json";

            float latitude = 0;
            float longitude = 0;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(contentResponse);
                        var featureMember = json["response"]?["GeoObjectCollection"]?["featureMember"]?.FirstOrDefault();
                        var pos = featureMember?["GeoObject"]?["Point"]?["pos"]?.ToString();

                        if (!string.IsNullOrEmpty(pos))
                        {
                            var coordParts = pos.Split(' ');
                            if (coordParts.Length >= 2)
                            {
                              
                                if (float.TryParse(coordParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float lon))
                                    longitude = lon;
                                if (float.TryParse(coordParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float lat))
                                    latitude = lat;
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Город '{city}' не найден. Попробуйте уточнить запрос.",
                                "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка от API. Статус: {response.StatusCode}\nПроверьте ключ и сетевое подключение.",
                            "Ошибка запроса", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Ошибка сети: {ex.Message}", "Сетевая ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Исключение",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return (latitude, longitude);
        }
    }
}