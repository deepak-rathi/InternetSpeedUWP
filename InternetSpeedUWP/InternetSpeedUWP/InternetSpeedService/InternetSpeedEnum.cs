namespace InternetSpeedUWP.InternetSpeedService
{
    class InternetSpeedEnum
    {
        public enum WifiSignalStrength
        {
            Weak = 1,
            Average = 2,
            Good = 3,
            VeryGood = 4
        }

        public enum MobileSignalStrength
        {
            VeryWeak = 1,
            Weak = 2,
            Average = 3,
            Good = 4,
            VeryGood = 5
        }

        public enum InternetSpeed
        {
            NoInternet = 0,
            VeryPoorInternet = 1,
            SlowInternet = 2,
            AverageInternet = 3,
            VeryGoodInternet = 4,
            Unknown = 5
        }
    }
}
