using System;
using System.Threading.Tasks;

namespace InternetSpeedUWP.InternetSpeedService
{
    class InternetSpeedService : IInternetSpeedService
    {
        InternetSpeedHelper _internetSpeedHelper = new InternetSpeedHelper();

        public Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedSignalAsync() => _internetSpeedHelper.CheckInternetSpeedSignalAsync();

        public Task<InternetSpeedEnum.InternetSpeed> CheckInternetSpeedSocketAsync() => _internetSpeedHelper.CheckInternetSpeedSocketAsync();
        
        public void RegisterActionAvailabiltyChanged(Action<bool> availabiltyChanged)
        {
            _internetSpeedHelper.AvailabilityChanged = availabiltyChanged;
        }

        public void RegisterActionNetworkStrengthChanged(Action<InternetSpeedEnum.InternetSpeed> networkStrengthChanged)
        {
            _internetSpeedHelper.NetworkStrengthChanged = networkStrengthChanged;
        }

        public Task<bool> IsInternetAvailable() => _internetSpeedHelper.IsInternetAvailable();
    }
}
