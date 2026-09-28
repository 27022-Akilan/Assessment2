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


        public async Task<string> Start()
        {
            if (_boiler.GetInterLockState() == InterLockState.Open)
            {
                return "Boiler is open please close then start the boiler";
            }

            if (_boiler.GetBoilerState() == BoilerState.Ready && _boiler.GetInterLockState() == InterLockState.Close)
            {
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => StartSequence(_cts.Token));
                return "Boiler stated its processing";
            }
            return "Boiler is already running";
        }

        private async Task StartSequence(CancellationToken ct)
        {
            try
            {
                _boiler.ChangeState(BoilerState.PrePurge);
                Console.WriteLine($"Task Started and in Pre gauge in the state {_boiler.GetBoilerState()} : {DateTime.Now}");
                await Task.Delay(5000, ct);
                _boiler.ChangeState(BoilerState.Ignition);
                Console.WriteLine($"Pre gauge completed and in {_boiler.GetBoilerState()} : {DateTime.Now}");
                _boiler.ChangeState(BoilerState.OperationalState);
                await Task.Delay(5000, ct);
                Console.WriteLine($"Went into {_boiler.GetBoilerState()}  :  {DateTime.Now}");
            }

            catch (OperationCanceledException)
            {
                _cts.Dispose();
                _boiler.ChangeState(BoilerState.Lockout);
            }
        }

        public string Stop()
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
                return $"Stopped Boiler and changed {currentBoilerState} into lockout State";
            }

            if (currentBoilerState != BoilerState.OperationalState || currentBoilerState == BoilerState.Ready)
            {
                _boiler.ChangeState(BoilerState.Lockout);
                return $"Changed {currentBoilerState} to Lockout state ";
            }
            return "";
        }

        public void ThrowError()
        {
            throw new InvalidOperationException();
        }
        public string ToggleInterLockState()
        {
            InterLockState currentInterLockState = _boiler.ToggleInterLockState();

            BoilerState currentBoilerState = _boiler.GetBoilerState();

            if (currentBoilerState == BoilerState.Lockout && currentInterLockState == InterLockState.Close)
            {
                _boiler.ChangeState(BoilerState.Ready);
                return "Staus changed to close and boiler is ready";
            }

            // If its Ready and changed to open then its lock out state.
            if (currentBoilerState == BoilerState.Ready && currentInterLockState == InterLockState.Open)
            {
                return "Changed to open state";
                // _boiler.ChangeState(BoilerState.Lockout);
            }

            // If its in operationalState and Open then gets into LockOut state
            if (currentBoilerState == BoilerState.OperationalState && currentInterLockState == InterLockState.Open)
            {
                _boiler.ChangeState(BoilerState.Lockout);
            }

            // To Handle inbetween cases inginition and pre

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
            }
            _boiler.ChangeState(BoilerState.Lockout);
            return "Reseted Boiler State";
        }
    }
}
