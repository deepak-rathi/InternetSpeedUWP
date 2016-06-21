using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using static InternetSpeedUWP.InternetSpeedService.InternetSpeedEnum;

namespace InternetSpeedUWP.Converter
{
    class InternetSpeedToColorConverter : IValueConverter
    {
        //Red Color
        readonly string redColor = "#E45F59";
        //Yellow Color
        readonly string yellowColor = "#DF822D";
        //Green Color
        readonly string greenColor = "#41BB8A";

        enum InternetSpeedColor
        {
            Red,
            Yellow,
            Green 
        }

        private SolidColorBrush GetSolidColorBrush(InternetSpeedColor color)
        {
            switch (color)
            {
                case InternetSpeedColor.Red:
                    return GetColorFromHexa(redColor);
                case InternetSpeedColor.Yellow:
                    return GetColorFromHexa(yellowColor);
                case InternetSpeedColor.Green:
                    return GetColorFromHexa(greenColor);
                default:
                    return new SolidColorBrush(Windows.UI.Colors.Transparent);
            };
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value.GetType() == typeof(InternetSpeed))
            {
                switch ((InternetSpeed)value)
                {
                    case InternetSpeed.NoInternet:
                    case InternetSpeed.VeryPoorInternet:
                        return GetSolidColorBrush(InternetSpeedColor.Red);
                    case InternetSpeed.SlowInternet:
                    case InternetSpeed.AverageInternet:
                        return GetSolidColorBrush(InternetSpeedColor.Yellow);
                    case InternetSpeed.VeryGoodInternet:
                        return GetSolidColorBrush(InternetSpeedColor.Green);
                    default:
                        return new SolidColorBrush(Windows.UI.Colors.Transparent);
                }
            }
            else
            { return null; }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        /// <summary>
        ///     HEX code string to SolidColorBrush
        /// </summary>
        /// <param name="hexaColor"></param>
        /// <returns></returns>
        private SolidColorBrush GetColorFromHexa(string hexaColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(
                    255,
                    System.Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    System.Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    System.Convert.ToByte(hexaColor.Substring(5, 2), 16)
                    )
                );
        }
    }
}
