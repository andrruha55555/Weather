using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Weather.Classes;
using Weather.Models;

namespace Weather
{
    public partial class MainWindow : Window
    {
        private DataResponse response;
        private readonly string userId = "user_" + Guid.NewGuid().ToString().Substring(0, 8);
        private bool isFromCache = false;
        private Hour currentHourData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            Initialize("Пермь");
        }

        public async void Initialize(string city = null)
        {
            try
            {
                Days.Items.Clear();
                isFromCache = false;
                UpdateStatus("Загрузка...", Brushes.Orange);

                string cityName = string.IsNullOrEmpty(city) ? "Пермь" : city;
                CityTextBox.Text = cityName;

                response = await GetWeather.GetWeatherData(cityName, userId);

                if (response?.forecasts != null && response.forecasts.Count > 0)
                {
                    foreach (Forecast forecast in response.forecasts)
                        Days.Items.Add(forecast.date.ToString("dd.MM.yyyy"));

                    Days.SelectedIndex = 0;

                    await UpdateMainWeatherInfo(0);

                    UpdateHourlyForecast(0);

                    UpdateStatus("Данные загружены", Brushes.Green);
                }
                else
                {
                    UpdateStatus("Нет данных", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Ошибка: {ex.Message}", Brushes.Red);
            }
        }

        private async Task UpdateMainWeatherInfo(int dayIndex)
        {
            if (response?.forecasts != null && dayIndex < response.forecasts.Count)
            {
                var forecast = response.forecasts[dayIndex];

                // Получаем текущий час
                int currentHour = DateTime.Now.Hour;
                currentHourData = forecast.hours.FirstOrDefault(h =>
                    int.Parse(h.hour) == currentHour) ?? forecast.hours.FirstOrDefault();

                if (currentHourData != null)
                {
                    CurrentTemperature.Text = $"{currentHourData.temp}°";
                    FeelsLike.Text = $"Ощущается как {currentHourData.temp}°";

                    string condition = currentHourData.ToCondition();
                    ConditionValue.Text = $"🌤 {condition}";

                    SetWeatherIcon(condition);

                    HumidityValue.Text = $"💧 {currentHourData.humidity}%";

                    string precType = currentHourData.ToPrecType();
                    PrecipitationValue.Text = $"☔ {precType}";

                    CurrentCity.Text = $"📍 {CityTextBox.Text}";
                    UpdateTime.Text = $"🕐 {DateTime.Now:HH:mm}";

                    SetTemperatureColor(currentHourData.temp);
                }
            }
        }

        private void UpdateHourlyForecast(int dayIndex)
        {
            HourlyPanel.Children.Clear();

            if (response?.forecasts != null && dayIndex < response.forecasts.Count)
            {
                var forecast = response.forecasts[dayIndex];

                foreach (var hour in forecast.hours)
                {
                    var card = CreateHourlyCard(hour);
                    HourlyPanel.Children.Add(card);
                }
            }
        }

        private Border CreateHourlyCard(Hour hour)
        {
            var border = new Border
            {
                Style = (Style)FindResource("HourlyCard"),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var timeText = new TextBlock
            {
                Text = $"{hour.hour}:00",
                Style = (Style)FindResource("TimeSlot")
            };

            var iconText = new TextBlock
            {
                Style = (Style)FindResource("WeatherIcon"),
                FontSize = 20,
                Margin = new Thickness(0, 5, 0, 5)
            };
            SetHourlyIcon(iconText, hour.ToCondition());

            var tempText = new TextBlock
            {
                Text = $"{hour.temp}°",
                Style = (Style)FindResource("MetricValue"),
                FontSize = 20
            };
            SetHourlyTemperatureColor(tempText, hour.temp);

            var conditionText = new TextBlock
            {
                Text = hour.ToCondition().Split(' ').First(),
                Style = (Style)FindResource("MetricLabel"),
                MaxWidth = 100,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(timeText);
            stackPanel.Children.Add(iconText);
            stackPanel.Children.Add(tempText);
            stackPanel.Children.Add(conditionText);

            border.Child = stackPanel;
            return border;
        }

        private void SetWeatherIcon(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                WeatherIconLarge.Text = "⛅";
                return;
            }

            string lowerCondition = condition.ToLower();

            if (lowerCondition.Contains("ясно") || lowerCondition.Contains("clear"))
                WeatherIconLarge.Text = "☀";
            else if (lowerCondition.Contains("малооблачно") || lowerCondition.Contains("partly"))
                WeatherIconLarge.Text = "⛅";
            else if (lowerCondition.Contains("облачно") || lowerCondition.Contains("пасмурно") ||
                     lowerCondition.Contains("cloudy") || lowerCondition.Contains("overcast"))
                WeatherIconLarge.Text = "☁";
            else if (lowerCondition.Contains("дождь") || lowerCondition.Contains("rain"))
                WeatherIconLarge.Text = "🌧";
            else if (lowerCondition.Contains("гроза") || lowerCondition.Contains("thunderstorm"))
                WeatherIconLarge.Text = "⛈";
            else if (lowerCondition.Contains("снег") || lowerCondition.Contains("snow"))
                WeatherIconLarge.Text = "❄";
            else if (lowerCondition.Contains("град") || lowerCondition.Contains("hail"))
                WeatherIconLarge.Text = "°";
            else if (lowerCondition.Contains("туман") || lowerCondition.Contains("fog") ||
                     lowerCondition.Contains("mist"))
                WeatherIconLarge.Text = "🌫";
            else
                WeatherIconLarge.Text = "🌤";
        }

        private void SetHourlyIcon(TextBlock iconText, string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                iconText.Text = "🌤";
                return;
            }

            string lowerCondition = condition.ToLower();

            if (lowerCondition.Contains("ясно") || lowerCondition.Contains("clear"))
                iconText.Text = "☀";
            else if (lowerCondition.Contains("малооблачно") || lowerCondition.Contains("partly"))
                iconText.Text = "⛅";
            else if (lowerCondition.Contains("облачно") || lowerCondition.Contains("пасмурно") ||
                     lowerCondition.Contains("cloudy") || lowerCondition.Contains("overcast"))
                iconText.Text = "☁";
            else if (lowerCondition.Contains("дождь") || lowerCondition.Contains("rain"))
                iconText.Text = "🌧";
            else if (lowerCondition.Contains("гроза") || lowerCondition.Contains("thunderstorm"))
                iconText.Text = "⛈";
            else if (lowerCondition.Contains("снег") || lowerCondition.Contains("snow"))
                iconText.Text = "❄";
            else
                iconText.Text = "🌤";
        }

        private void SetTemperatureColor(int temperature)
        {
            var color = GetTemperatureColor(temperature);
            CurrentTemperature.Foreground = new SolidColorBrush(color);
        }

        private void SetHourlyTemperatureColor(TextBlock textBlock, int temperature)
        {
            var color = GetTemperatureColor(temperature);
            textBlock.Foreground = new SolidColorBrush(color);
        }

        private Color GetTemperatureColor(int temperature)
        {
            if (temperature >= 30) return Color.FromRgb(220, 53, 69); 
            else if (temperature >= 25) return Color.FromRgb(253, 126, 20); 
            else if (temperature >= 20) return Color.FromRgb(255, 193, 7); 
            else if (temperature >= 15) return Color.FromRgb(40, 167, 69); 
            else if (temperature >= 10) return Color.FromRgb(0, 123, 255); 
            else if (temperature >= 0) return Color.FromRgb(23, 162, 184); 
            else return Color.FromRgb(111, 66, 193); 
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text?.Trim();

            if (!string.IsNullOrEmpty(city))
            {
                Initialize(city);
            }
            else
            {
                MessageBox.Show("Введите название города", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void SelectDay(object sender, SelectionChangedEventArgs e)
        {
            if (Days.SelectedIndex >= 0)
            {
                await UpdateMainWeatherInfo(Days.SelectedIndex);
                UpdateHourlyForecast(Days.SelectedIndex);
            }
        }

       

        private void UpdateStatus(string message, Brush color)
        {
        }

        private void CityTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Update(sender, e);
            }
        }
    }
}