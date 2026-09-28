using BoilerApplication.Models.Enums;

namespace BoilerApplication.Models
{
    public class Boiler
    {
        private static object _stateLock = new object();
        public BoilerState BoilerState { get; private set; } = BoilerState.Lockout;

        public InterLockState InterLockState { get; private set; } = InterLockState.Open;


        public void ChangeState(BoilerState boilerState)
        {
            lock (_stateLock)
            {
                BoilerState = boilerState;
            }
        }

        public InterLockState ToggleInterLockState()
        {
            lock (_stateLock)
            {
                InterLockState = InterLockState == InterLockState.Close ? InterLockState.Open : InterLockState.Close;
                return InterLockState;
            }
        }

        public BoilerState GetBoilerState()
        {
            lock (_stateLock)
            {
                return BoilerState;
            }
        }

        public InterLockState GetInterLockState()
        {
            lock (_stateLock)
            {
                return InterLockState;
            }
        }
    }
}
