using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Weather.Models;

namespace Weather.Elements
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        public Item(Hour hour)
        {
            InitializeComponent();
            lHour.Content = hour.hour + ":00";
            string condition = hour.ToCondition();
            lCondition.Content = condition;
            lHumidity.Content = hour.humidity + "%";
            string precType = hour.ToPrecType();
            lPrecType.Content = precType;
            lTemp.Content = hour.temp + "°";
            SetConditionIcon(condition);
            SetPrecipitationIcon(precType);
            SetTemperatureColor(hour.temp);
        }

        public void SetConditionIcon(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                iconCondition.Text = "";
                return;
            }
            string lowerCondition = condition.ToLower();

            if (lowerCondition.Contains("ясно") || lowerCondition.Contains("clear"))
                iconCondition.Text = "☀";
            else if (lowerCondition.Contains("малооблачно") || lowerCondition.Contains("partly"))
                iconCondition.Text = "⛅";
            else if (lowerCondition.Contains("облачно") || lowerCondition.Contains("пасмурно") ||
                     lowerCondition.Contains("cloudy") || lowerCondition.Contains("overcast"))
                iconCondition.Text = "☁";
            else if (lowerCondition.Contains("дождь") || lowerCondition.Contains("rain"))
                iconCondition.Text = "🌧";
            else if (lowerCondition.Contains("гроза") || lowerCondition.Contains("thunderstorm"))
                iconCondition.Text = "⛈";
            else if (lowerCondition.Contains("снег") || lowerCondition.Contains("snow"))
                iconCondition.Text = "❄";
            else if (lowerCondition.Contains("град") || lowerCondition.Contains("hail"))
                iconCondition.Text = "°";
            else if (lowerCondition.Contains("туман") || lowerCondition.Contains("fog") ||
                     lowerCondition.Contains("mist"))
                iconCondition.Text = "🌫";
            else
                iconCondition.Text = "🌤";
        }

        public void SetPrecipitationIcon(string precipitationType)
        {
            if (string.IsNullOrEmpty(precipitationType))
            {
                iconPrecipitation.Text = "";
                return;
            }

            string lowerType = precipitationType.ToLower();

            if (lowerType.Contains("дождь") || lowerType.Contains("rain"))
                iconPrecipitation.Text = "🌧";
            else if (lowerType.Contains("снег") || lowerType.Contains("snow"))
                iconPrecipitation.Text = "❄";
            else if (lowerType.Contains("град") || lowerType.Contains("hail"))
                iconPrecipitation.Text = "°";
            else if (lowerType.Contains("дождь со снегом") || lowerType.Contains("wet-snow"))
                iconPrecipitation.Text = "❄🌧";
            else if (lowerType.Contains("без осадков") || lowerType.Contains("нет") ||
                     lowerType == "0" || lowerType.Contains("no"))
                iconPrecipitation.Text = "";
            else
                iconPrecipitation.Text = "💦";
        }

        public void SetTemperatureColor(int temperature)
        {
            if (temperature >= 30)
                lTemp.Foreground = Brushes.DarkRed;
            else if (temperature >= 25)
                lTemp.Foreground = Brushes.OrangeRed;
            else if (temperature >= 20)
                lTemp.Foreground = Brushes.Orange;
            else if (temperature >= 15)
                lTemp.Foreground = Brushes.Gold;
            else if (temperature >= 10)
                lTemp.Foreground = Brushes.YellowGreen;
            else if (temperature >= 5)
                lTemp.Foreground = Brushes.LightSkyBlue;
            else if (temperature >= 0)
                lTemp.Foreground = Brushes.DodgerBlue;
            else
                lTemp.Foreground = Brushes.Blue;
        }

        public event RoutedEventHandler ActionClicked;

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionClicked?.Invoke(this, e);
        }
        public string Hour
        {
            get => lHour.Content?.ToString();
            set => lHour.Content = value;
        }

        public string Condition
        {
            get => lCondition.Content?.ToString();
            set
            {
                lCondition.Content = value;
                SetConditionIcon(value);
            }
        }

        public string Humidity
        {
            get => lHumidity.Content?.ToString();
            set => lHumidity.Content = value;
        }

        public string Precipitation
        {
            get => lPrecType.Content?.ToString();
            set
            {
                lPrecType.Content = value;
                SetPrecipitationIcon(value);
            }
        }

        public string Temperature
        {
            get => lTemp.Content?.ToString();
            set
            {
                lTemp.Content = value;
                if (int.TryParse(value.Replace("°", "").Trim(), out int temp))
                {
                    SetTemperatureColor(temp);
                }
            }
        }
    }
}
