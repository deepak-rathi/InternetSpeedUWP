using Windows.UI;
using Windows.UI.Xaml.Media;

namespace InternetSpeedUWP.Util
{
    public static class InternetSpeedColorUtil
    {
        public enum InternetSpeedColor
        {
            Red,
            Yellow,
            Green,
            Transparent
        }
        
        //Red Color
        private const string RedColor = "#E45F59";
        //Yellow Color
        private const string YellowColor = "#DF822D";
        //Green Color
        private const string GreenColor = "#41BB8A";

        /// <summary>
        ///     HEX code string to SolidColorBrush
        /// </summary>
        /// <param name="hexaColor"></param>
        /// <returns></returns>
        private static SolidColorBrush GetColorFromHexa(string hexaColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(255,
                System.Convert.ToByte(hexaColor.Substring(1, 2), 16),
                System.Convert.ToByte(hexaColor.Substring(3, 2), 16),
                System.Convert.ToByte(hexaColor.Substring(5, 2), 16)));
        }

        /// <summary>
        ///     Get SolidColor Brush from InternetSpeedColor enum
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static SolidColorBrush GetSolidColorBrush(InternetSpeedColor color)
        {
            switch (color)
            {
                case InternetSpeedColor.Red:
                    return GetColorFromHexa(RedColor);

                case InternetSpeedColor.Yellow:
                    return GetColorFromHexa(YellowColor);

                case InternetSpeedColor.Green:
                    return GetColorFromHexa(GreenColor);

                case InternetSpeedColor.Transparent:
                    return new SolidColorBrush(Colors.Transparent);

                default:
                    return new SolidColorBrush(Colors.Transparent);
            };
        }
    }
}
