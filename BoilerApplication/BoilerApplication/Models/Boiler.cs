using BoilerApplication.Models.Enums;

namespace BoilerApplication.Models
{
    public class Boiler
    {
        /// <summary>
        /// Lock to access the states of the boiler.
        /// </summary>
        private static object _stateLock = new object();

        /// <summary>
        /// Represent the boiler state.
        /// </summary>
        private BoilerState BoilerState = BoilerState.Lockout;

        /// <summary>
        /// Represents the interlock state. 
        /// </summary>
        private InterLockState InterLockState = InterLockState.Open;


        /// <summary>
        /// Changes the state to the required state.
        /// </summary>
        /// <param name="boilerState">State that should be changed to.</param>
        public void ChangeState(BoilerState boilerState)
        {
            lock (_stateLock)
            {
                BoilerState = boilerState;
            }
        }

        /// <summary>
        /// Toggles the interlock to the opposite states.
        /// </summary>
        /// <returns>Interlock State after the change.</returns>
        public InterLockState ToggleInterLockState()
        {
            lock (_stateLock)
            {
                InterLockState = InterLockState == InterLockState.Close ? InterLockState.Open : InterLockState.Close;
                return InterLockState;
            }
        }

        /// <summary>
        /// Gets the current boiler state of the boiler.
        /// </summary>
        /// <returns>Current boiler state.</returns>
        public BoilerState GetBoilerState()
        {
            lock (_stateLock)
            {
                return BoilerState;
            }
        }

        /// <summary>
        /// Gets the InterLock of the boiler state.
        /// </summary>
        /// <returns>Current InterLock state of the Boiler.</returns>
        public InterLockState GetInterLockState()
        {
            lock (_stateLock)
            {
                return InterLockState;
            }
        }
    }
}
