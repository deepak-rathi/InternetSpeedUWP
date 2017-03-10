using System;
using System.Threading.Tasks;

namespace InternetSpeedUWP.InternetSpeedService
{
    class InternetSpeedService : IInternetSpeedService
    {
        InternetSpeedHelper _internetSpeedHelper = new InternetSpeedHelper();

        public Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedBySignalAsync() => _internetSpeedHelper.CheckInternetSpeedSignalAsync();

        public Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedBySocketAsync() => _internetSpeedHelper.CheckInternetSpeedSocketAsync();
        
        public void RegisterAvailabiltyChanged(Action<bool> availabiltyChanged)
        {
            _internetSpeedHelper.AvailabilityChanged = availabiltyChanged;
        }

        public void RegisterNetworkStrengthChanged(Action<InternetSpeedEnum.InternetSpeed> networkStrengthChanged)
        {
            _internetSpeedHelper.NetworkStrengthChanged = networkStrengthChanged;
        }

        public Task<bool> IsInternetAvailable() => _internetSpeedHelper.IsInternetAvailable();
    }
}
