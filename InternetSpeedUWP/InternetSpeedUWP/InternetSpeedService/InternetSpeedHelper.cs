using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using static InternetSpeedUWP.InternetSpeedService.InternetSpeedEnum;

namespace InternetSpeedUWP.InternetSpeedService
{
    class InternetSpeedHelper
    {
        //Host name to be used to test internet speed using socket connection
        readonly string speedTestHostName = "bing.com";
        //allow already running thread to complete
        bool checkingSocketSpeed = false;
        //default allow periodic thread to run after 30sec
        bool stopPeriodicThread = false;
        //set periodic thead to check internet speed strength at 30 seconds timespan
        double periodicTheadTimeSpanInSeconds = 30.0;
        
        //Store old NetworkStrength
        InternetSpeed previousNetworkStrength = InternetSpeed.Unknown;

        //Action for availabilty changed
        public Action<bool> AvailabilityChanged { get; set; }

        //Action for Network strength changed
        public Action<InternetSpeed> NetworkStrengthChanged { get; set; }

        #region Constructor
        /// <summary>
        /// InternetSpeedHelper
        /// </summary>
        /// <param name="continuousSpeedCheck">Default True, check internet strength after 30sec(default) and notify through NetworkStrengthChanged</param>
        /// <param name="continuousSpeedCheckInSeconds">Default 30.0, check internet strength after 30sec(default) and notify through NetworkStrengthChanged</param>
        public InternetSpeedHelper(bool continuousSpeedCheck = true, double continuousSpeedCheckInSeconds = 30.0)
        {
            NetworkInformation.NetworkStatusChanged += async (s) =>
            {
                //check internt connection availabilty
                var available = await IsInternetAvailable();
                if (continuousSpeedCheck && available)
                {
                    stopPeriodicThread = false;
                    // start periodic thread to check if internet strength changed
                    StartPeriodicThread(skipDelay: true);
                }
                else if (!available)
                {
                    NetworkStrengthChanged?.Invoke(InternetSpeed.NoInternet);
                }

                //check if action is registered for availability changed
                if (AvailabilityChanged != null)
                {
                    //invoke availability changed
                    try { AvailabilityChanged(available); }
                    catch { }
                }
            };
            periodicTheadTimeSpanInSeconds = continuousSpeedCheckInSeconds;
            if (continuousSpeedCheck)
            {
                //start period thread to check if internet strength changed
                StartPeriodicThread();
            }
        }
        #endregion

        #region IsInternetAvailable
        /// <summary>
        /// Checks if internet access is available or not
        /// </summary>
        /// <returns>
        /// True = Internet access is available
        /// False = Internet access is not available
        /// </returns>
        public async Task<bool> IsInternetAvailable()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile == null)
                return false;
            var net = NetworkConnectivityLevel.InternetAccess;
            await Task.CompletedTask;
            return profile.GetNetworkConnectivityLevel().Equals(net);
        }
        #endregion
        
        #region Check internet speed using signal strength
        public async Task<InternetSpeed> CheckInternetSpeedSignalAsync()
        {
            //Initially assuming we have best internet connection
            var internetStability = InternetSpeed.VeryGoodInternet;
            //InternetConnectionProfile
            var InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            //If no internet found return no internet found
            if (InternetConnectionProfile == null)
                return InternetSpeed.NoInternet;

            //Check Connectivity Level
            switch (InternetConnectionProfile.GetNetworkConnectivityLevel())
            {
                case NetworkConnectivityLevel.None:
                    return InternetSpeed.NoInternet;
                case NetworkConnectivityLevel.LocalAccess:
                    return InternetSpeed.NoInternet;
                case NetworkConnectivityLevel.ConstrainedInternetAccess:
                    internetStability = InternetSpeed.AverageInternet;
                    break;
                case NetworkConnectivityLevel.InternetAccess:
                    internetStability = InternetSpeed.VeryGoodInternet;
                    break;
            }

            //Check Domain Connectivity Level
            switch (InternetConnectionProfile.GetDomainConnectivityLevel())
            {
                case DomainConnectivityLevel.None:
                    internetStability = InternetSpeed.VeryGoodInternet;
                    break;
                case DomainConnectivityLevel.Unauthenticated:
                    return InternetSpeed.NoInternet;
                case DomainConnectivityLevel.Authenticated:
                    break;
            }

            //Connection Cost
            var connectionCost = InternetConnectionProfile.GetConnectionCost();
            //Check Metered network
            if (connectionCost.NetworkCostType == NetworkCostType.Unknown
                    || connectionCost.NetworkCostType == NetworkCostType.Unrestricted
                    && (!connectionCost.Roaming))
            {
                //Connection cost is unknown/unrestricted && Not roaming
                //All good Green banner
                internetStability = InternetSpeed.VeryGoodInternet;
            }
            else if (connectionCost.NetworkCostType == NetworkCostType.Variable || connectionCost.NetworkCostType == NetworkCostType.Fixed
                && (!connectionCost.Roaming && !connectionCost.OverDataLimit))
            {
                //Metered Network && Not roaming && Not over data limit
                //Ok but make sure data limit is not reached
                //Yellow banner
                internetStability = InternetSpeed.AverageInternet;
            }
            else if (connectionCost.Roaming || connectionCost.OverDataLimit)
            {
                //roaming or over data limit
                //ToDo://Notify User about roaming internet usage
                //Red banner
                return InternetSpeed.Unknown;
            }

            //Wifi
            bool isWifi = InternetConnectionProfile.IsWlanConnectionProfile;

            //Mobile
            bool isMobile = InternetConnectionProfile.IsWwanConnectionProfile;

            //WiredLan
            bool isWiredLan = false;

            //Check Wired Lan
            if (!isWifi && !isMobile)
                isWiredLan = true;

            //Check Wifi Signal Strength
            var signalBars = InternetConnectionProfile.GetSignalBars();

            //conditional internet strength check
            if (isWiredLan)
            {
                //currently we cannot check if wired lan is having good/ slow/ fast internet speed
                //ToDo://Find a better way
                //may be check both speed by signal strength and then speed by socket connection
                internetStability = InternetSpeed.VeryGoodInternet;
            }
            else if (isWifi)
            {
                if (signalBars >= (int)WifiSignalStrength.VeryGood)
                {
                    //Very Good Wifi Signal
                    //internetStability = InternetStability.VeryGoodInternet;
                }
                else if (signalBars >= (int)WifiSignalStrength.Good)
                {
                    //Average Wifi Signal
                    internetStability = InternetSpeed.AverageInternet;
                }
                else if (signalBars == (int)WifiSignalStrength.Average)
                {
                    //Average Wifi Signal
                    internetStability = InternetSpeed.SlowInternet;
                }
                else
                {
                    //Low Wifi Signal
                    internetStability = InternetSpeed.VeryPoorInternet;
                }

            }
            else if (isMobile)
            {
                if (signalBars >= (int)MobileSignalStrength.VeryGood)
                {
                    //Very Good Mobile Signal
                    //internetStability = InternetStability.VeryGoodInternet;
                }
                else if (signalBars >= (int)MobileSignalStrength.Good)
                {
                    //Average Mobile Signal
                    internetStability = InternetSpeed.VeryGoodInternet;
                }
                else if (signalBars == (int)MobileSignalStrength.Average)
                {
                    //Average Mobile Signal
                    internetStability = InternetSpeed.AverageInternet;
                }
                else if (signalBars == (int)MobileSignalStrength.VeryWeak)
                {
                    internetStability = InternetSpeed.VeryPoorInternet;
                }
                else if (signalBars <= (int)MobileSignalStrength.Weak)
                {
                    //Low Mobile Signal
                    internetStability = InternetSpeed.SlowInternet;
                }

                WwanDataClass connectionClass = InternetConnectionProfile.WwanConnectionProfileDetails.GetCurrentDataClass();
                switch (connectionClass)
                {
                    //2G-equivalent
                    case WwanDataClass.Edge:
                    case WwanDataClass.Gprs:
                        internetStability = InternetSpeed.SlowInternet;
                        break;
                    //3G-equivalent
                    case WwanDataClass.Cdma1xEvdo:
                    case WwanDataClass.Cdma1xEvdoRevA:
                    case WwanDataClass.Cdma1xEvdoRevB:
                    case WwanDataClass.Cdma1xEvdv:
                    case WwanDataClass.Cdma1xRtt:
                    case WwanDataClass.Cdma3xRtt:
                    case WwanDataClass.CdmaUmb:
                    case WwanDataClass.Umts:
                    case WwanDataClass.Hsdpa:
                    case WwanDataClass.Hsupa:
                    //4G-equivalent
                    case WwanDataClass.LteAdvanced:
                        if ((int)internetStability >= (int)InternetSpeed.SlowInternet)
                            internetStability = InternetSpeed.VeryGoodInternet;
                        else
                            internetStability = InternetSpeed.SlowInternet;
                        break;

                    //not connected
                    case WwanDataClass.None:
                        internetStability = InternetSpeed.NoInternet;
                        break;

                    //unknown
                    case WwanDataClass.Custom:
                    default:
                        break;
                }
            }

            await Task.CompletedTask;
            return internetStability;
        }
        #endregion

        #region Check Internet Speed using Socket
        public async Task<InternetSpeed> CheckInternetSpeedSocketAsync()
        {
            if (!checkingSocketSpeed)
            {
                checkingSocketSpeed = true;
                int retries = 4;
                int taskTimeoutms = 1000;
                double currentSpeed = 0.0;
                //InternetConnectionProfile
                var InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

                //Wifi
                bool isWifi = InternetConnectionProfile.IsWlanConnectionProfile;

                //Mobile
                bool isMobile = InternetConnectionProfile.IsWwanConnectionProfile;

                //WiredLan
                bool isWiredLan = false;

                //Check Wired Lan
                if (!isWifi && !isMobile)
                    isWiredLan = true;

                if (isWiredLan)
                {
                    taskTimeoutms = 1000;
                }

                if (isMobile || isWifi)
                {
                    retries = 2;
                }
                for (int i = 0; i < retries; ++i)
                {
                    var serverHost = new HostName(speedTestHostName);

                    StreamSocket clientSocket = new StreamSocket();
                    clientSocket.Control.NoDelay = true;
                    clientSocket.Control.QualityOfService = SocketQualityOfService.LowLatency;
                    clientSocket.Control.KeepAlive = false;

                    //tasks must complete in a fixed amount of time, cancel otherwise..
                    var taskCancellationTokenSource = new CancellationTokenSource();
                    taskCancellationTokenSource.CancelAfter(taskTimeoutms);
                    try
                    {
                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                await clientSocket.ConnectAsync(serverHost, "80", SocketProtectionLevel.PlainSocket);
                            }
                            catch (TaskCanceledException){}
                        }, taskCancellationTokenSource.Token);
                        await task;
                        await task.ContinueWith(antecedent =>
                        {
                            currentSpeed += clientSocket.Information.RoundTripTimeStatistics.Min / 1000000.0;
                        });
                    }
                    catch (COMException)
                    {
                        currentSpeed = 0.0;
                        retries--;
                    }
                    catch (OperationCanceledException)
                    {
                        currentSpeed = 0.0;
                        retries--;
                    }
                    catch (Exception)
                    {
                        retries--;
                    }
                    clientSocket.Dispose();
                }
                checkingSocketSpeed = false;
                //compute internet speed
                if (currentSpeed == 0.0)
                {
                    return InternetSpeed.NoInternet;
                }
                else
                {
                    double rawSpeed = currentSpeed / retries;
                    return GetConnectionSpeed(rawSpeed);
                }
            }
            else
                return InternetSpeed.Unknown;
        }

        protected InternetSpeed GetConnectionSpeed(double roundtriptime)
        {
            //change these constant values if you feel the output result is not accurate
            if (!(roundtriptime > 0.0))
                return InternetSpeed.NoInternet;

            if (roundtriptime <= 0.0014)
                return InternetSpeed.VeryGoodInternet;

            if (roundtriptime > 0.0014 && roundtriptime < 0.14)
                return InternetSpeed.VeryGoodInternet;

            if (roundtriptime > 0.14 && roundtriptime < 0.90)
                return InternetSpeed.AverageInternet;

            if (roundtriptime > 0.90 && roundtriptime <= 1.5)
                return InternetSpeed.SlowInternet;

            return InternetSpeed.VeryPoorInternet;
        }

        #endregion
        
        #region StartPeriodicThread
        /// <summary>
        /// Asyc Thread to check internet speed strength periodically
        /// </summary>
        /// <param name="skipDelay"></param>
        private async void StartPeriodicThread(bool skipDelay = false)
        {
            if (stopPeriodicThread)
                return;

            if (!skipDelay)
                await Task.Delay(TimeSpan.FromSeconds(periodicTheadTimeSpanInSeconds));

            CheckInternetStregthPeriodically();
            if (!stopPeriodicThread)
                StartPeriodicThread();
        }
        #endregion

        #region CheckInternetStrengthPeriodically
        /// <summary>
        /// Async Thread to check internet speed and notify user based on internet speed
        /// </summary>
        private async void CheckInternetStregthPeriodically()
        {
            try
            {
                if (await IsInternetAvailable() || NetworkStrengthChanged != null)
                {
                    var currentInternetStrength = InternetSpeed.Unknown;
                    //check internet speed
                    var internetSpeedbySocket = await CheckInternetSpeedSocketAsync();
                    var internetSpeedbasedOnSignal = await CheckInternetSpeedSignalAsync();
                    switch (internetSpeedbasedOnSignal)
                    {
                        case InternetSpeed.VeryGoodInternet:
                            if (internetSpeedbySocket == InternetSpeed.VeryGoodInternet || internetSpeedbySocket == InternetSpeed.AverageInternet)
                                currentInternetStrength = InternetSpeed.VeryGoodInternet;
                            else if (internetSpeedbySocket == InternetSpeed.SlowInternet || internetSpeedbySocket == InternetSpeed.VeryPoorInternet)
                                currentInternetStrength = InternetSpeed.SlowInternet;
                            else if (internetSpeedbySocket == InternetSpeed.NoInternet)
                                currentInternetStrength = InternetSpeed.VeryPoorInternet;

                            break;
                        case InternetSpeed.AverageInternet:
                            if (internetSpeedbySocket == InternetSpeed.VeryGoodInternet || internetSpeedbySocket == InternetSpeed.AverageInternet)
                                currentInternetStrength = InternetSpeed.AverageInternet;
                            else if (internetSpeedbySocket == InternetSpeed.SlowInternet)
                                currentInternetStrength = InternetSpeed.SlowInternet;
                            else if (internetSpeedbySocket == InternetSpeed.VeryPoorInternet || internetSpeedbySocket == InternetSpeed.NoInternet)
                                currentInternetStrength = InternetSpeed.VeryPoorInternet;

                            break;
                        case InternetSpeed.SlowInternet:
                            if (internetSpeedbySocket == InternetSpeed.VeryGoodInternet || internetSpeedbySocket == InternetSpeed.AverageInternet)
                                currentInternetStrength = InternetSpeed.AverageInternet;
                            else if (internetSpeedbySocket == InternetSpeed.SlowInternet)
                                currentInternetStrength = InternetSpeed.SlowInternet;
                            else if (internetSpeedbySocket == InternetSpeed.VeryPoorInternet || internetSpeedbySocket == InternetSpeed.NoInternet)
                                currentInternetStrength = InternetSpeed.VeryPoorInternet;
                            break;
                        case InternetSpeed.VeryPoorInternet:
                            if (internetSpeedbySocket == InternetSpeed.VeryGoodInternet || internetSpeedbySocket == InternetSpeed.AverageInternet || internetSpeedbySocket == InternetSpeed.SlowInternet)
                                currentInternetStrength = InternetSpeed.SlowInternet;
                            else if (internetSpeedbySocket == InternetSpeed.VeryPoorInternet || internetSpeedbySocket == InternetSpeed.NoInternet)
                                currentInternetStrength = InternetSpeed.VeryPoorInternet;

                            break;
                        case InternetSpeed.NoInternet:
                            if (internetSpeedbySocket == InternetSpeed.VeryGoodInternet || internetSpeedbySocket == InternetSpeed.AverageInternet || internetSpeedbySocket == InternetSpeed.SlowInternet)
                                currentInternetStrength = InternetSpeed.VeryPoorInternet;
                            else if (internetSpeedbySocket == InternetSpeed.VeryPoorInternet || internetSpeedbySocket == InternetSpeed.NoInternet)
                                currentInternetStrength = InternetSpeed.NoInternet;

                            break;
                    };
                    if (previousNetworkStrength != currentInternetStrength)
                    {
                        try {
                            NetworkStrengthChanged?.Invoke(currentInternetStrength);
                            previousNetworkStrength = currentInternetStrength;
                        } catch { }
                    }
                }
                else
                {
                    stopPeriodicThread = true;
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception in CheckInternetStrengthPeriodically: " +exception.Message);
            }
        }
        #endregion
    }
}
