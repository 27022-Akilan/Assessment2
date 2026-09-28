using BoilerApplication.Models;
using BoilerApplication.Models.Enums;
using BoilerApplication.Repository;

namespace BoilerApplication
{
    public class Service
    {
        private Boiler _boiler;

        private ILogger _logger;

        private CancellationTokenSource _cts = null;
        public Service(Boiler boiler, ILogger logger)
        {
            _logger = logger;
            this._boiler = boiler;
        }

        public event Action<string, BoilerState> ReflectTime;
        public async Task<string> Start()
        {
            if (_boiler.GetInterLockState() == InterLockState.Open)
            {
                return "Boiler is open please close then start the boiler";
            }

            if (_boiler.GetBoilerState() == BoilerState.Ready && _boiler.GetInterLockState() == InterLockState.Close)
            {
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => StartSequence(10, _cts.Token));
                return "Boiler stated its processing";
            }

            return "Boiler is already running";
        }

        private async Task StartSequence(int time, CancellationToken ct)
        {
            try
            {
                _boiler.ChangeState(BoilerState.PrePurge);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.PrePurge}");
                for (int i = time; i > 0; i--)
                {
                    ReflectTime?.Invoke($"Time Left :{i} s", _boiler.GetBoilerState());
                    await Task.Delay(1000, ct);
                }

                _boiler.ChangeState(BoilerState.Ignition);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Ignition}");

                for (int i = time; i > 0; i--)
                {
                    ReflectTime?.Invoke($"Time Left :{i} s", _boiler.GetBoilerState());
                    await Task.Delay(1000, ct);
                }

                _boiler.ChangeState(BoilerState.OperationalState);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.OperationalState}");

            }

            catch (OperationCanceledException)
            {
                _cts.Dispose();
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Lockout}");
                if (_boiler.GetInterLockState() == InterLockState.Close)
                {
                    _boiler.ToggleInterLockState();
                    await _logger.AppendLog(DateTime.Now, "InterLock change", $"Boiler Interlock changed to : {_boiler.GetInterLockState}");
                }
            }
        }

        public async Task<string> Stop()
        {
            BoilerState currentBoilerState = _boiler.GetBoilerState();
            if (currentBoilerState == BoilerState.Lockout)
            {
                return "The boiler itself is in the stop state";
            }

            if (currentBoilerState == BoilerState.PrePurge || currentBoilerState == BoilerState.Ignition)
            {
                _cts.Cancel();
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLog(DateTime.Now, "State change", $"Stopped the boiler and state changed to : {BoilerState.Lockout}");
                return $"Stopped Boiler and changed {currentBoilerState} into lockout State";
            }

            if (currentBoilerState != BoilerState.OperationalState || currentBoilerState == BoilerState.Ready)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Lockout}");
                return $"Changed {currentBoilerState} to Lockout state ";
            }
            return "";
        }

        public void ThrowError()
        {
            throw new InvalidOperationException();
        }

        public async Task<string> ToggleInterLockState()
        {
            InterLockState currentInterLockState = _boiler.ToggleInterLockState();
            await _logger.AppendLog(DateTime.Now, "Inter Lock State change", $" Changed to : {BoilerState.Lockout}");
            BoilerState currentBoilerState = _boiler.GetBoilerState();

            if (currentBoilerState == BoilerState.Lockout && currentInterLockState == InterLockState.Close)
            {
                _boiler.ChangeState(BoilerState.Ready);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Ready}");
                return "Staus changed to close and boiler is ready";
            }

            // If its Ready and changed to open then its lock out state.
            if (currentBoilerState == BoilerState.Ready && currentInterLockState == InterLockState.Open)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLog(DateTime.Now, "State change", $"Boiler state changed to : {BoilerState.Lockout}");
                return "Changed InterLock state to open state and so Boiler state went into LockOut ";
            }

            // If its in operationalState and Open then gets into LockOut state
            if (currentBoilerState == BoilerState.OperationalState && currentInterLockState == InterLockState.Open)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                await _logger.AppendLog(DateTime.Now, "State change", $"Changed to : {BoilerState.Lockout}");
                return $"You opened the Inter Lock in the operation state hence its moving to the {BoilerState.Lockout}";
            }

            // To Handle inbetween cases inginition and pre - purge
            await _logger.AppendLog(DateTime.Now, "Inter Lock State change", $"Changed to : {currentInterLockState}");
            return $"Status Chnaged to {currentInterLockState}";
        }

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
            else
            {
                return "";
            }
        }

        public async Task<string[]> LoadFromFile()
        {
            return await _logger.LoadFromFile();
        }
    }
}
