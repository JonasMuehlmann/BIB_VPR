namespace Messenger.Core.Helpers
{
    /// <summary>
    /// Holds status values of login attempts
    /// </summary>
    public enum LoginResultType
    {
        Success,
        Unauthorized,
        CancelledByUser,
        NoNetworkAvailable,
        UnknownError
    }
}
