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
        DataResponse response;
        private string userId = "user_" + Guid.NewGuid().ToString().Substring(0, 8);
        private bool isFromCache = false;

        public MainWindow()
        {
            InitializeComponent();

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateStatsInfo();
            Iint();
        }

        public async void Iint(string city = null)
        {
            try
            {
                Days.Items.Clear();
                isFromCache = false;

                string cityName = string.IsNullOrEmpty(city) ? "Пермь" : city;
                response = await GetWeather.GetWeatherData(cityName, userId);

                if (response?.forecasts != null)
                {
                    foreach (Forecast forecast in response.forecasts)
                        Days.Items.Add(forecast.date.ToString("dd.MM.yyyy"));

                    Create(0);
                    await UpdateStatsInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Create(int idForecast)
        {
            parent.Children.Clear();
            if (response?.forecasts != null && idForecast < response.forecasts.Count)
            {
                foreach (Hour hour in response.forecasts[idForecast].hours)
                {
                    parent.Children.Add(new Elements.Item(hour));
                }
                if (isFromCache)
                {
                    CacheInfo.Text = " (из кэша)";
                    CacheInfo.Foreground = System.Windows.Media.Brushes.Green;
                    CacheInfo.ToolTip = "Данные взяты из локального кэша";
                }
                else
                {
                    CacheInfo.Text = " (с API)";
                    CacheInfo.Foreground = System.Windows.Media.Brushes.Blue;
                    CacheInfo.ToolTip = "Данные получены из API Яндекс.Погоды";
                }
            }
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text?.Trim();

            if (!string.IsNullOrEmpty(city))
            {
                Iint(city);
            }
            else
            {
                MessageBox.Show("Введите название города", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectDay(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Days.SelectedIndex >= 0)
                Create(Days.SelectedIndex);
        }

        private async Task UpdateStatsInfo()
        {
            try
            {
                var stats = await WeatherCache.GetStats(userId);

                Dispatcher.Invoke(() =>
                {
                    LimitInfo.Text = $"Запросов: {stats.todayCount}/{WeatherCache.DAILY_LIMIT}";
                    CacheInfo.Text = $" (Кэш: {stats.cachedCities})";
                    if (stats.todayCount >= WeatherCache.DAILY_LIMIT)
                    {
                        LimitInfo.Foreground = System.Windows.Media.Brushes.Red;
                        LimitInfo.ToolTip = "Дневной лимит исчерпан! Используйте кэшированные данные.";
                    }
                    else if (stats.todayCount >= WeatherCache.DAILY_LIMIT * 0.8)
                    {
                        LimitInfo.Foreground = System.Windows.Media.Brushes.Orange;
                        LimitInfo.ToolTip = "Лимит почти исчерпан";
                    }
                    else
                    {
                        LimitInfo.Foreground = System.Windows.Media.Brushes.Green;
                        LimitInfo.ToolTip = $"Осталось запросов: {stats.remaining}";
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении статистики: {ex.Message}");
            }
        }
    }
}