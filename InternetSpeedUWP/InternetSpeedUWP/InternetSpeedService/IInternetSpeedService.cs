using System;
using System.Threading.Tasks;

namespace InternetSpeedUWP.InternetSpeedService
{
    interface IInternetSpeedService
    {
        Task<bool> IsInternetAvailable();
        Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedBySignalAsync();
        Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedBySocketAsync();
        void RegisterAvailabiltyChanged(Action<bool> availabiltyChanged);
        void RegisterNetworkStrengthChanged(Action<InternetSpeedEnum.InternetSpeed> networkStrengthChanged);
    }
}
