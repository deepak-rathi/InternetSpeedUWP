using System;
using Windows.UI.Xaml.Data;
using static InternetSpeedUWP.InternetSpeedService.InternetSpeedEnum;
using static InternetSpeedUWP.Util.InternetSpeedColorUtil;

namespace InternetSpeedUWP.Converter
{
    partial class InternetSpeedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value.GetType() == typeof(InternetSpeed))
            {
                switch ((InternetSpeed)value)
                {
                    //No Internet connection OR Poor Internet connection
                    case InternetSpeed.NoInternet:
                    case InternetSpeed.VeryPoorInternet:
                        return GetSolidColorBrush(InternetSpeedColor.Red);

                    //Slow Or AverageInternet connection
                    case InternetSpeed.SlowInternet:
                    case InternetSpeed.AverageInternet:
                        return GetSolidColorBrush(InternetSpeedColor.Yellow);

                    //Good Internet connection    
                    case InternetSpeed.VeryGoodInternet:
                        return GetSolidColorBrush(InternetSpeedColor.Green);

                    default:
                        return GetSolidColorBrush(InternetSpeedColor.Transparent);
                }
            }
            else
            { return null; }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
