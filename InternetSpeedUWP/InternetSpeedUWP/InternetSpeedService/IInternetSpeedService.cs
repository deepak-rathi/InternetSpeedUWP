using System;
using System.Threading.Tasks;

namespace InternetSpeedUWP.InternetSpeedService
{
    interface IInternetSpeedService
    {
        Task<bool> IsInternetAvailable();
        Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedSignalAsync();
        Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedSocketAsync();
        void RegisterActionAvailabiltyChanged(Action<bool> availabiltyChanged);
        void RegisterActionNetworkStrengthChanged(Action<InternetSpeedEnum.InternetSpeed> networkStrengthChanged);
    }
}
