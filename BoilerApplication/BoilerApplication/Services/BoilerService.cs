using BoilerApplication.ConstantData;
using BoilerApplication.Models;
using BoilerApplication.Models.Enums;
using BoilerApplication.Repository;

namespace BoilerApplication.Services
{
    /// <summary>
    /// Represents the services provided by the Boiler.
    /// </summary>
    public class BoilerService
    {
        private Boiler _boiler;

        private ILogger _logger;

        private CancellationTokenSource _cts = default;
        public BoilerService(Boiler boiler, ILogger logger)
        {
            _logger = logger;
            _boiler = boiler;
        }

        /// <summary>
        /// An event which is to be triggered on while the boiler is processing.
        /// </summary>
        public event Action<string, BoilerState> OnProcessing;

        /// <summary>
        /// Starts the Boiler .
        /// Just started the processing and returned to the caller.
        /// </summary>
        /// <returns>Operation's result message</returns>
        public async Task<string> StartAsync()
        {
            if (_boiler.GetInterLockState() == InterLockState.Open)
            {
                return "Boiler is open, please close the boiler inter lock then start the boiler";
            }

            if (_boiler.GetBoilerState() == BoilerState.Ready && _boiler.GetInterLockState() == InterLockState.Close)
            {
                _cts = new CancellationTokenSource();

                // Knowingly didnt awaited here backend processing. 
                _ = Task.Run(() => StartSequenceAsync(_cts.Token));
                return "Boiler stated its processing";
            }

            return "Boiler is already running";
        }

        /// <summary>
        /// Makes the boiler to progress through Pre-purge, ignition and operational stages.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token which is used to cancel the processing.</param>
        private async Task StartSequenceAsync(CancellationToken cancellationToken)
        {
            try
            {
                _boiler.ChangeState(BoilerState.PrePurge);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.PrePurge}");

                // Loop to invoke the event each second for 10 seconds while in pre-purge state.
                for (int i = Constants.TimeForPrePurge; i >= 0; i--)
                {
                    OnProcessing?.Invoke($"Time Left :{i} s", _boiler.GetBoilerState());

                    // delaying for 1 second so totaly it makes 10 seconds
                    await Task.Delay(1000, cancellationToken);
                }

                _boiler.ChangeState(BoilerState.Ignition);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Ignition}");

                // Loop to invoke the event each second for 10 seconds while in ignition state.
                for (int i = Constants.TimeForIngnition; i >= 0; i--)
                {
                    OnProcessing?.Invoke($"Time Left :{i} s", _boiler.GetBoilerState());

                    // delaying for 1 second so totaly it makes 10 seconds
                    await Task.Delay(1000, cancellationToken);
                }

                // Finally changing to operational state.
                _boiler.ChangeState(BoilerState.OperationalState);
                OnProcessing?.Invoke("Completed", _boiler.GetBoilerState());
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.OperationalState}");

            }

            // Catches the when the cacellation token is recieved then this exception is trown and handled gracefull exiting.
            catch (OperationCanceledException)
            {
                _cts.Dispose();
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Lockout}");
                if (_boiler.GetInterLockState() == InterLockState.Close)
                {
                    _boiler.ToggleInterLockState();
                    await _logger.AppendLogAsync(DateTime.Now, "InterLock change", $"Boiler Interlock changed to : {_boiler.GetInterLockState}");
                }
            }
        }

        /// <summary>
        /// Stops the Boiler.
        /// </summary>
        /// <returns>Operation's result message</returns>
        public async Task<string> StopAsync()
        {
            BoilerState currentBoilerState = _boiler.GetBoilerState();
            if (currentBoilerState == BoilerState.Lockout)
            {
                return "The boiler is in the Initial state only. \nSo Can't stop it.";
            }
            if (currentBoilerState == BoilerState.Ready)
            {
                return "THe boiler is already in the Ready state hence cant be stopped";
            }
            if (currentBoilerState == BoilerState.PrePurge || currentBoilerState == BoilerState.Ignition)
            {
                // To stop the processing.
                _cts.Cancel();
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Stopped the boiler and state changed to : {BoilerState.Lockout}");
                return $"Stopped Boiler and changed {currentBoilerState} into lockout State";
            }

            if (currentBoilerState != BoilerState.OperationalState || currentBoilerState == BoilerState.Ready)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Lockout}");
                return $"Changed {currentBoilerState} to Lockout state ";
            }
            return "";
        }

        /// <summary>
        /// Throws an exception .
        /// </summary>
        /// <exception cref="InvalidOperationException">Invalid operation exception just for throwing an exception.</exception>
        public void ThrowError()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Toggles the state of the boiler and changes the Boiler state according to the interlock state.
        /// </summary>
        /// <returns>Operation's result message</returns>
        public async Task<string> ToggleInterLockStateAsync()
        {
            InterLockState currentInterLockState = _boiler.ToggleInterLockState();
            await _logger.AppendLogAsync(DateTime.Now, "Inter Lock State change", $" Changed to : {BoilerState.Lockout}");
            BoilerState currentBoilerState = _boiler.GetBoilerState();

            if (currentBoilerState == BoilerState.Lockout && currentInterLockState == InterLockState.Close)
            {
                _boiler.ChangeState(BoilerState.Ready);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Ready}");
                return "Staus changed to close and boiler is ready";
            }

            // If its Ready and changed to open then its lock out state.
            if (currentBoilerState == BoilerState.Ready && currentInterLockState == InterLockState.Open)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Lockout}");
                return "Changed InterLock state to open state and so Boiler state went into LockOut ";
            }

            // If its in operationalState and Open then gets into LockOut state
            if (currentBoilerState == BoilerState.OperationalState && currentInterLockState == InterLockState.Open)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLogAsync(DateTime.Now, "State change", $"Changed to : {BoilerState.Lockout}");
                return $"You opened the Inter Lock in the operation state hence its moving to the {BoilerState.Lockout}";
            }

            // To Handle inbetween cases inginition and pre - purge
            ResetBoiler();
            await _logger.AppendLogAsync(DateTime.Now, "Inter Lock State change", $"Changed to : {currentInterLockState}");
            return $"Status Chnaged to {currentInterLockState}";
        }


        /// <summary>
        /// Resetting the boiler to the initial state (LockOut).
        /// </summary>
        /// <returns>Operation's result message</returns>
        public string ResetBoiler()
        {
            BoilerState currentBoilerState = _boiler.GetBoilerState();
            if (currentBoilerState == BoilerState.Lockout)
            {
                return "Already in the initial state";
            }

            if (currentBoilerState == BoilerState.PrePurge || currentBoilerState == BoilerState.Ignition)
            {
                _cts.Cancel();
                return "Reseted Boiler Intial State";
            }

            if (currentBoilerState == BoilerState.OperationalState)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                _logger.AppendLogAsync(DateTime.Now, "Change State", $"{BoilerState.Lockout}");
                return $"Reseted the Boiler to {BoilerState.Lockout}";
            }

            else
            {
                return "You are already in the ready state \nCant be resetted.";
            }
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <returns>Array of string representing the logs.</returns>
        public async Task<string[]> LoadFromFileAsync()
        {
            return await _logger.LoadFromFileAsync();
        }
    }
}
