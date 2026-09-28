using BoilerApplication.Models.Enums;

namespace BoilerApplication.Models
{
    public class Boiler
    {
        private static object _stateLock = new object();

        private BoilerState BoilerState = BoilerState.Lockout;

        private InterLockState InterLockState = InterLockState.Open;


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
