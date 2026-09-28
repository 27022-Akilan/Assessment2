namespace BoilerApplication.Models.Enums
{
    public enum BoilerState
    {
        Lockout = 1,

        Ready,

        PrePurge,

        Ignition,

        OperationalState,
    }
}