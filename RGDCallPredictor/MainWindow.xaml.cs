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
using System.Windows.Threading;

namespace RGDCallPredictor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();    
        }

        //List of days considered holidays through 2019
        List<string> holidayList = new List<string>()
        { "11/23/2017", "12/24/2017", "12/25/2017", "01/01/2018",
            "05/28/2018", "07/04/2018", "09/03/2018", "11/22/2018",
            "12/24/2018", "12/25/2018", "01/01/2019", "05/27/2019",
            "07/04/2019", "09/02/2019", "11/28/2019", "12/24/2019",
            "12/25/2019" };

        private async void TurnOn(object sender, EventArgs e)
        {
            RootObject myWeather = await OpenWeatherMapProxy.GetWeather();
            
            //Setting all text blocks in GUI with today's information
            HighTemp.Text = "High: " + ((int)myWeather.main.temp_max).ToString();
            LowTemp.Text = "Low: " + ((int)myWeather.main.temp_min).ToString();
            DateTime dt = DateTime.Today;
            DayOfWeek.Text = dt.DayOfWeek.ToString().ToUpper();
            MonthDate.Text = dt.ToString("MMMM").ToUpper() + " " + dt.Day;

            //Setting string to be replaced with image name corresponding with current weather condition
            string iconName;
            string switchExp = myWeather.weather[0].icon;
            
            switch (switchExp)
            {
                case "01d":
                case "01n":
                    iconName = "sun.png";
                    break;
                case "02d":
                case "02n":
                    iconName = "partlycloudy.png";
                    break;
                case "09d":
                case "09n":
                    iconName = "drizzle.png";
                    break;
                case "10d":
                case "10n":
                    iconName = "rain.png";
                    break;
                case "11d":
                case "11n":
                    iconName = "lightning.png";
                    break;
                case "13d":
                case "13n":
                    iconName = "snow.png";
                    break;
                default:
                    iconName = "cloudy.png";
                    break;
            }
            string icon = String.Format("Images/{0}", iconName);
            ResultImage.Source = new BitmapImage(new Uri(@icon, UriKind.RelativeOrAbsolute));

            //Values for call prediction model based on day of the week
            double weekDayModifier;
            double tomorrowModifier;

            switch (DayOfWeek.Text)
            {
                case "TUESDAY":
                    weekDayModifier = .88257;
                    tomorrowModifier = .8336;
                    break;
                case "WEDNESDAY":
                    weekDayModifier = .8336;
                    tomorrowModifier = .7855;
                    break;
                case "THURSDAY":
                    weekDayModifier = .7855;
                    tomorrowModifier = .77258;
                    break;
                case "FRIDAY":
                    weekDayModifier = .77258;
                    tomorrowModifier = .38961;
                    break;
                case "SATURDAY":
                    weekDayModifier = .38961;
                    tomorrowModifier = .21675;
                    break;
                case "SUNDAY":
                    weekDayModifier = .21675;
                    tomorrowModifier = 1;
                    break;
                default:
                    weekDayModifier = 1;
                    tomorrowModifier = .88257;
                    break;
            }

            //If current date is within holidayList, subtract 80.079 from call prediction.
            double isHoliday = 0;

            if (holidayList.Contains(dt.ToShortDateString()))
            {
                isHoliday = 1;
            }

            //Call prediction formula. Factors: High temp, Low temp, Day of Week, Holiday or not
            int todayPrediction = (int)(-3.0201 + ((((myWeather.main.temp_max) - 97) / 100) * 1.684) +
                                   ((((myWeather.main.temp_min) - 77) / 97) * -13.3671) +
                                   (weekDayModifier * 142.7634) - (isHoliday * 80.079));
            int tomorrowPrediction = (int)(-3.0201 + ((((myWeather.main.temp_max) - 97) / 100) * 1.684) +
                                      ((((myWeather.main.temp_min) - 77) / 97) * -13.3671) +
                                      (tomorrowModifier * 142.7634) - (isHoliday * 80.079));

            //Set minimum call prediction possible to a value of 10
            if (todayPrediction < 10)
                TodayPredicted.Text = "10";
            TodayPredicted.Text = todayPrediction.ToString();

            if (tomorrowPrediction < 10)
                TomorrowPredicted.Text = "10";
            TomorrowPredicted.Text = tomorrowPrediction.ToString();
        }

        //Timer in background to reset GUI at midnight.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.ToString("HH:mm:ss") == "00:00:00")
            {
                MainWindow1.TurnOn(sender, e);
            }
        }
    }
}
