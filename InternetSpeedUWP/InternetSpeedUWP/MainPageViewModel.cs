using InternetSpeedUWP.InternetSpeedService;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static InternetSpeedUWP.InternetSpeedService.InternetSpeedEnum;

namespace InternetSpeedUWP
{
    class MainPageViewModel : INotifyPropertyChanged
    { 
        bool isTaskRunning = false;
        IInternetSpeedService internetSpeedService = new InternetSpeedService.InternetSpeedService();

        #region Constructor
        /// <summary>
        /// Constructor for View Model
        /// </summary>
        public MainPageViewModel()
        {
            RegisterNetworkHandler();
        }
        #endregion

        #region PropertyChanged & OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region RegisterNetworkHandler
        /// <summary>
        /// Register for network strength change action
        /// </summary>
        public async void RegisterNetworkHandler()
        {
            internetSpeedService.RegisterActionNetworkStrengthChanged(AvailabilityChanged);
            //Initially if internet is not found show red banner
            if (!await internetSpeedService.IsInternetAvailable())
            {
                InternetSpeedDetected = InternetSpeed.NoInternet;
            }
        }
        #endregion
        
        #region InternetConnectivityColor
        private InternetSpeed _internetSpeed = InternetSpeed.Unknown;

        public InternetSpeed InternetSpeedDetected
        {
            get { return _internetSpeed; }
            set
            {
                _internetSpeed = value;
                switch (value)
                {
                    case InternetSpeed.NoInternet:
                        InternetConnectivityText = "No Internet Connection";
                        break;
                    case InternetSpeed.VeryPoorInternet:
                        InternetConnectivityText = "Poor Internet connection";
                    break;
                    case InternetSpeed.SlowInternet:
                    case InternetSpeed.AverageInternet:
                        InternetConnectivityText = "Weak Internet Connection";
                        break;
                    case InternetSpeed.VeryGoodInternet:
                        InternetConnectivityText = "Good Internet Connection";
                        break;
                    default:
                        InternetConnectivityText = string.Empty;
                        break;
                };
                OnPropertyChanged();
                InternetConnectivityMessageVisible = true;
            }
        }
        #endregion

        #region InternetConnectivityText
        private string _internetConnectivityText = string.Empty;

        public string InternetConnectivityText
        {
            get { return _internetConnectivityText; }
            set
            {
                _internetConnectivityText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region InternetMessageVisibility
        private bool _internetConnectivityMessageVisible = false;

        public bool InternetConnectivityMessageVisible
        {
            get { return _internetConnectivityMessageVisible; }
            set
            {
                _internetConnectivityMessageVisible = value;
                OnPropertyChanged();
                HideConnectivityTextMessage();
            }
        }

        #endregion

        #region HideConnectivityTextMessage
        /// <summary>
        /// Hide Connectivity text message after 4 sec
        /// </summary>
        private async void HideConnectivityTextMessage()
        {
            if (InternetSpeedDetected == InternetSpeed.NoInternet || InternetSpeedDetected == InternetSpeed.VeryPoorInternet)
                return;

            if (!isTaskRunning)
            {
                isTaskRunning = true;
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
                catch { }
                //Make sure UI thread is causing property changed event
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow
                                .Dispatcher
                                .RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    InternetConnectivityMessageVisible = false;
                                });
                isTaskRunning = false;
            }
        }
        #endregion

        #region AvailabilityChanged
        /// <summary>
        /// Triggered when Internet availabilrt is changed or change in internet strength
        /// </summary>
        /// <param name="internetSpeed"></param>
        private async void AvailabilityChanged(InternetSpeed internetSpeed)
        {
            //ToDo:find alternative for InternetSpeed.Unknown
            if (internetSpeed == InternetSpeed.Unknown)
                return;

            if (isTaskRunning && InternetSpeedDetected != internetSpeed && internetSpeed != InternetSpeed.NoInternet)
            {
                //Do not hide message if No Internet is the status
                //Make sure UI thread is causing property changed event
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow
                            .Dispatcher
                            .RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                InternetConnectivityMessageVisible = false;
                            });
                isTaskRunning = false;
            }
            else if (isTaskRunning || InternetSpeedDetected == internetSpeed && InternetSpeedDetected == InternetSpeed.NoInternet)
            {
                //Make sure UI thread is causing property changed event
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow
                            .Dispatcher
                            .RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                InternetConnectivityMessageVisible = true;
                            });
                return;
            }

            //Make sure UI thread is causing property changed event
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow
                            .Dispatcher
                            .RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                switch (internetSpeed)
                                {
                                    case InternetSpeed.NoInternet:
                                        InternetSpeedDetected = InternetSpeed.NoInternet;
                                        break;
                                    case InternetSpeed.VeryPoorInternet:
                                        InternetSpeedDetected = InternetSpeed.VeryPoorInternet;
                                        break;
                                    case InternetSpeed.SlowInternet:
                                        InternetSpeedDetected = InternetSpeed.SlowInternet;
                                        break;
                                    case InternetSpeed.AverageInternet:
                                        InternetSpeedDetected = InternetSpeed.AverageInternet;
                                        break;
                                    case InternetSpeed.VeryGoodInternet:
                                        InternetSpeedDetected = InternetSpeed.VeryGoodInternet;
                                        break;
                                    default:
                                        InternetSpeedDetected = InternetSpeed.Unknown;
                                        break;
                                };
                            });

        }
        #endregion

    }
}
