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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataResponse response;
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //await UpdateStatsInfo();
            Iint();
        }

        public async void Iint(string city = null)
        {
            //try
            //{
            //    Days.Items.Clear();
            //    isFromCache = false;

            //    string cityName = string.IsNullOrEmpty(city) ? "Пермь" : city;
            //    response = await GetWeather.GetWeatherData(cityName, userId);

            //    if (response?.forecasts != null)
            //    {
            //        foreach (Forecast forecast in response.forecasts)
            //            Days.Items.Add(forecast.date.ToString("dd.MM.yyyy"));

            //        Create(0);
            //        await UpdateStatsInfo();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка получения данных: {ex.Message}", "Ошибка",
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void SelectDay(object sender, SelectionChangedEventArgs e)
        {

        }

        //public void Create(int idForecast)
        //{
        //    parent.Children.Clear();
        //    if (response?.forecasts != null && idForecast < response.forecasts.Count)
        //    {
        //        foreach (Hour hour in response.forecasts[idForecast].hours)
        //        {
        //            parent.Children.Add(new Elements.Item(hour));
        //        }
        //        if (isFromCache)
        //        {
        //            CacheInfo.Text = " (из кэша)";
        //            CacheInfo.Foreground = System.Windows.Media.Brushes.Green;
        //            CacheInfo.ToolTip = "Данные взяты из локального кэша";
        //        }
        //        else
        //        {
        //            CacheInfo.Text = " (с API)";
        //            CacheInfo.Foreground = System.Windows.Media.Brushes.Blue;
        //            CacheInfo.ToolTip = "Данные получены из API Яндекс.Погоды";
        //        }
        //    }
        //}
    }
}